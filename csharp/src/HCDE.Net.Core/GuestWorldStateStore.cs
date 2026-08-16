using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public sealed class GuestPlayerState
{
    public byte PlayerNum { get; init; }
    public short Health { get; set; }
    public byte PlayerState { get; set; }
    public bool OnGround { get; set; }
}

public sealed class GuestSectorState
{
    public ushort SectorIndex { get; init; }
    public float Floor { get; set; }
    public float Ceiling { get; set; }
    public short LightLevel { get; set; }
    public short Special { get; set; }
}

public sealed class GuestActorState
{
    public uint ActorId { get; init; }
    public ushort ClassId { get; set; }
    public short Health { get; set; }
    public byte Category { get; set; }
    public byte Flags { get; set; }
}

public sealed class GuestWorldStateStore : IWorldDeltaApplySink, IActorDeltaApplySink
{
    private readonly Dictionary<byte, GuestPlayerState> _players = new();
    private readonly Dictionary<ushort, GuestSectorState> _sectors = new();
    private readonly Dictionary<uint, GuestActorState> _actors = new();

    public IReadOnlyDictionary<byte, GuestPlayerState> Players => _players;
    public IReadOnlyDictionary<ushort, GuestSectorState> Sectors => _sectors;
    public IReadOnlyDictionary<uint, GuestActorState> Actors => _actors;

    public bool ApplyPose(int recipientClientSlot, PlayerPoseWorldDelta pose, int sequenceAck)
    {
        _ = recipientClientSlot;
        _ = sequenceAck;
        if ((pose.Flags & LiveConstants.ServerWorldDeltaPoseHasActor) == 0)
            return false;

        if (!_players.TryGetValue(pose.PlayerNum, out var player))
        {
            player = new GuestPlayerState { PlayerNum = pose.PlayerNum };
            _players[pose.PlayerNum] = player;
        }

        player.Health = pose.Health;
        player.OnGround = (pose.Flags & LiveConstants.ServerWorldDeltaPoseOnGround) != 0;
        return true;
    }

    public bool ApplySector(SectorWorldDelta sector)
    {
        if (!_sectors.TryGetValue(sector.SectorIndex, out var state))
        {
            state = new GuestSectorState { SectorIndex = sector.SectorIndex };
            _sectors[sector.SectorIndex] = state;
        }

        state.Floor = sector.Floor;
        state.Ceiling = sector.Ceiling;
        return true;
    }

    public void SeedMapSector(
        ushort sectorIndex,
        short floorHeight,
        short ceilingHeight,
        short lightLevel,
        short special)
    {
        if (!_sectors.TryGetValue(sectorIndex, out var state))
        {
            state = new GuestSectorState { SectorIndex = sectorIndex };
            _sectors[sectorIndex] = state;
        }

        state.Floor = floorHeight;
        state.Ceiling = ceilingHeight;
        state.LightLevel = lightLevel;
        state.Special = special;
    }

    public bool TryApply(int recipientClientSlot, ActorDeltaRecord record)
    {
        _ = recipientClientSlot;
        if (!_actors.TryGetValue(record.ActorId, out var actor))
        {
            actor = new GuestActorState { ActorId = record.ActorId };
            _actors[record.ActorId] = actor;
        }

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldCategory) != 0)
            actor.Category = record.Category;
        if ((record.FieldMask & LiveConstants.ActorDeltaFieldFlags) != 0)
            actor.Flags = record.Flags;
        if ((record.FieldMask & LiveConstants.ActorDeltaFieldHealth) != 0)
            actor.Health = record.Health;

        actor.ClassId = record.ClassId;
        return true;
    }
}
