using HCDE.Net.Core;
using HCDE.Playsim;
using System.Buffers.Binary;

namespace HCDE.Server;

public static class SimSnapshotPublisher
{
    public static void Publish(AuthoritySimulation simulation, GuestWorldStateStore store)
    {
        var present = simulation.Actors.Select(actor => actor.Id).ToHashSet();
        // Keep tombstones in full snapshots so a missed removal is corrected later.
        foreach (var actor in store.Actors.Values)
        {
            if (present.Contains(actor.ActorId)) continue;
            actor.Flags = (byte)(actor.Flags & ~LiveConstants.ActorDeltaFlagLive);
            actor.Health = 0;
        }
        foreach (var sector in simulation.Level.Sectors)
        {
            if (sector.Index is < 0 or > ushort.MaxValue)
                continue;
            store.SeedMapSector(
                (ushort)sector.Index,
                simulation.FloorOf(sector.Index),
                simulation.CeilingOf(sector.Index),
                simulation.LightOf(sector.Index),
                sector.Special);
        }

        foreach (var player in simulation.Players)
        {
            store.SetPlayerPose(
                player.PlayerNum,
                (short)Math.Clamp(player.Health, 0, short.MaxValue),
                onGround: player.OnGround,
                (float)player.X.ToDouble(),
                (float)player.Y.ToDouble(),
                player.Angle.Raw,
                (float)player.Z.ToDouble(),
                (float)player.VelocityX.ToDouble(),
                (float)player.VelocityY.ToDouble(),
                (float)player.VelocityZ.ToDouble(),
                (short)Math.Clamp(player.Inventory.Armor, 0, short.MaxValue),
                BamAngle.FromDegrees(player.PitchDegrees).Raw);
        }

        foreach (var actor in simulation.Actors)
        {
            store.SeedActor(actor.Id, (ushort)actor.DoomEdNum, (short)Math.Clamp(actor.Health, 0, short.MaxValue),
                category: actor is PlayerPawn ? (byte)ReplicatedActorCategory.Player
                    : actor is ProjectileActor ? (byte)ReplicatedActorCategory.Projectile
                    : PickupCatalog.IsPickup(actor.DoomEdNum) ? (byte)ReplicatedActorCategory.Pickup
                    : actor.DoomEdNum is InvasionDirector.SpawnSpotType or LineSpecials.TeleportDestType ? (byte)ReplicatedActorCategory.Map
                    : (byte)ReplicatedActorCategory.Monster,
                flags: !actor.IsDead && !actor.Destroyed ? LiveConstants.ActorDeltaFlagLive : (byte)0);
            var pose = store.Actors[actor.Id];
            pose.HasPose = true;
            pose.PosX = actor.X.ToDouble();
            pose.PosY = actor.Y.ToDouble();
            pose.PosZ = actor.Z.ToDouble();
            pose.YawBams = actor.Angle.Raw;
        }
    }
}

public sealed class SimulationCommandSink : IClientInputCommandSink
{
    private readonly AuthoritySimulation _simulation;

    public SimulationCommandSink(AuthoritySimulation simulation) => _simulation = simulation;

    public bool ApplyCommand(
        int clientSlot,
        byte playerNum,
        int sequence,
        UserCmd command,
        ReadOnlyMemory<byte> eventRecords)
    {
        _ = clientSlot;
        _ = sequence;
        if (!TryReadWeaponSelections(eventRecords.Span, out var selections))
            return false;
        return _simulation.QueueCommand(playerNum, new PlayerCommand
        {
            WeaponSelections = selections,
            ForwardMove = command.ForwardMove,
            SideMove = command.SideMove,
            YawDelta = command.Yaw,
            PitchDelta = command.Pitch,
            Attack = (command.Buttons & 1u) != 0, // BT_ATTACK in src/d_event.h
            Jump = (command.Buttons & 4u) != 0, // BT_JUMP
            Use = (command.Buttons & 2u) != 0, // BT_USE
        });
    }

    private static bool TryReadWeaponSelections(ReadOnlySpan<byte> block, out byte[] selections)
    {
        selections = [];
        if (block.IsEmpty) return true; // Direct callers may omit the empty canonical block.
        var end = 0;
        if (!EventRecordsCodec.TryRead(block, ref end, out var count, out _) || end != block.Length)
            return false;
        var slots = new List<byte>();
        var cursor = EventRecordsCodec.EmptyBlockSize;
        for (var i = 0; i < count; i++)
        {
            var type = block[cursor++];
            var length = BinaryPrimitives.ReadUInt16BigEndian(block[cursor..]);
            cursor += 2;
            if (type == (byte)DemoCommand.WeapSelect)
            {
                if (length != 1) return false;
                slots.Add(block[cursor]);
            }
            cursor += length;
        }
        selections = slots.ToArray();
        return true;
    }
}
