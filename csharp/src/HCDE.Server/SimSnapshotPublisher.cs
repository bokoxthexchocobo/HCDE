using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server;

public static class SimSnapshotPublisher
{
    public static void Publish(AuthoritySimulation simulation, GuestWorldStateStore store)
    {
        foreach (var sector in simulation.Level.Sectors)
        {
            if (sector.Index is < 0 or > ushort.MaxValue)
                continue;
            store.SeedMapSector(
                (ushort)sector.Index,
                sector.FloorHeight,
                sector.CeilingHeight,
                sector.LightLevel,
                sector.Special);
        }

        foreach (var player in simulation.Players)
        {
            store.SetPlayerPose(
                player.PlayerNum,
                (short)player.Health,
                onGround: true,
                (float)player.X.ToDouble(),
                (float)player.Y.ToDouble(),
                player.Angle.Raw);
        }

        foreach (var actor in simulation.Actors)
        {
            store.SeedActor(actor.Id, (ushort)actor.DoomEdNum, (short)actor.Health);
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
        _ = eventRecords;
        _simulation.QueueCommand(playerNum, new PlayerCommand
        {
            ForwardMove = command.ForwardMove,
            SideMove = command.SideMove,
            YawDelta = command.Yaw,
        });
        return true;
    }
}
