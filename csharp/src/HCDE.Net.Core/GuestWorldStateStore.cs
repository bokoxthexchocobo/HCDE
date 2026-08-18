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

public sealed class GuestWorldStateStore : IWorldDeltaApplySink, IActorDeltaApplySink, ICoopDeadSpawnsApplySink
{
    private readonly Dictionary<byte, GuestPlayerState> _players = new();
    private readonly Dictionary<ushort, GuestSectorState> _sectors = new();
    private readonly Dictionary<uint, GuestActorState> _actors = new();
    private readonly List<uint> _pendingCoopDeadSpawns = new();
    private readonly HashSet<uint> _retiredCoopDeadSpawns = new();
    private readonly List<AuthorityEventRecord> _pendingAuthorityEvents = new();
    private uint _authorityEventRollingHash;
    private uint _actorDeltaRollingHash;
    private uint _presentationEchoRollingHash;
    private uint _lineSpecRollingHash;

    public IReadOnlyDictionary<byte, GuestPlayerState> Players => _players;
    public IReadOnlyDictionary<ushort, GuestSectorState> Sectors => _sectors;
    public IReadOnlyDictionary<uint, GuestActorState> Actors => _actors;
    public IReadOnlyCollection<uint> RetiredCoopDeadSpawns => _retiredCoopDeadSpawns;
    public bool HasPendingCoopDeadSpawns => _pendingCoopDeadSpawns.Count > 0;
    public bool HasPendingAuthorityEvents => _pendingAuthorityEvents.Count > 0;
    public uint AuthorityEventRollingHash => _authorityEventRollingHash;
    public uint ActorDeltaRollingHash => _actorDeltaRollingHash;
    public uint PresentationEchoRollingHash => _presentationEchoRollingHash;
    public uint LineSpecRollingHash => _lineSpecRollingHash;

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
        if ((sector.Flags & LiveConstants.ServerWorldDeltaSectorHasLight) != 0)
            state.LightLevel = sector.LightLevel;
        if ((sector.Flags & LiveConstants.ServerWorldDeltaSectorHasSpecial) != 0)
            state.Special = sector.Special;
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

    public void SeedActor(uint actorId, ushort classId, short health = 100, byte category = 0, byte flags = 0)
    {
        if (!_actors.TryGetValue(actorId, out var actor))
        {
            actor = new GuestActorState { ActorId = actorId };
            _actors[actorId] = actor;
        }

        actor.ClassId = classId;
        actor.Health = health;
        actor.Category = category;
        actor.Flags = flags;
    }

    public void SeedPlayer(byte playerNum, short health = 100, bool onGround = true)
    {
        if (!_players.TryGetValue(playerNum, out var player))
        {
            player = new GuestPlayerState { PlayerNum = playerNum };
            _players[playerNum] = player;
        }

        player.Health = health;
        player.OnGround = onGround;
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

    public void QueueCoopDeadSpawn(uint spawnIndex)
    {
        if (_retiredCoopDeadSpawns.Contains(spawnIndex))
            return;

        if (!_pendingCoopDeadSpawns.Contains(spawnIndex))
            _pendingCoopDeadSpawns.Add(spawnIndex);
    }

    public bool TryRetireSpawnIndex(uint spawnIndex)
    {
        if (_retiredCoopDeadSpawns.Contains(spawnIndex))
            return false;

        _retiredCoopDeadSpawns.Add(spawnIndex);
        _pendingCoopDeadSpawns.Remove(spawnIndex);
        return true;
    }

    public uint[] TakePendingCoopDeadSpawnsForTail()
    {
        if (_pendingCoopDeadSpawns.Count == 0)
            return Array.Empty<uint>();

        var pending = _pendingCoopDeadSpawns.ToArray();
        _pendingCoopDeadSpawns.Clear();
        foreach (var spawnIndex in pending)
            _retiredCoopDeadSpawns.Add(spawnIndex);
        return pending;
    }

    public void QueueAuthorityEvent(AuthorityEventRecord record) =>
        _pendingAuthorityEvents.Add(record);

    public AuthorityEventRecord[] TakePendingAuthorityEventsForTail()
    {
        if (_pendingAuthorityEvents.Count == 0)
            return Array.Empty<AuthorityEventRecord>();

        var pending = _pendingAuthorityEvents.ToArray();
        _pendingAuthorityEvents.Clear();
        foreach (var record in pending)
            _authorityEventRollingHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(_authorityEventRollingHash, record);
        return pending;
    }

    public void CommitAppliedAuthorityEvents(ReadOnlySpan<AuthorityEventRecord> records)
    {
        foreach (var record in records)
            _authorityEventRollingHash = SnapshotChecksumAuthorityEventPolicy.MixRecord(_authorityEventRollingHash, record);
    }

    public void MixShippedActorDeltas(IReadOnlyList<ActorDeltaRecord> records)
    {
        foreach (var record in records)
            _actorDeltaRollingHash = SnapshotChecksumActorDeltaPolicy.MixRecord(_actorDeltaRollingHash, record);
    }

    public void CommitAppliedActorDeltas(IReadOnlyList<ActorDeltaRecord> records)
    {
        foreach (var record in records)
            _actorDeltaRollingHash = SnapshotChecksumActorDeltaPolicy.MixRecord(_actorDeltaRollingHash, record);
    }

    public void CommitAppliedPresentationEcho(PresentationEchoBlock block) =>
        _presentationEchoRollingHash = SnapshotChecksumPresentationEchoPolicy.MixBlock(_presentationEchoRollingHash, block);

    public void NoteLineSpec(int lineIndex, int special, bool success) =>
        _lineSpecRollingHash = SnapshotChecksumLineSpecPolicy.MixRecord(_lineSpecRollingHash, lineIndex, special, success);
}
