using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public readonly struct SnapshotChecksumPlayerSample
{
    public SnapshotChecksumPlayerSample(int playerIndex, int health, int playerState, bool onGround)
    {
        PlayerIndex = playerIndex;
        Health = health;
        PlayerState = playerState;
        OnGround = onGround;
    }

    public int PlayerIndex { get; }
    public int Health { get; }
    public int PlayerState { get; }
    public bool OnGround { get; }
}

public readonly struct SnapshotChecksumSectorSample
{
    public SnapshotChecksumSectorSample(
        int sectorIndex,
        double floorHeight,
        double ceilingHeight,
        int lightLevel,
        int special)
    {
        SectorIndex = sectorIndex;
        FloorHeight = floorHeight;
        CeilingHeight = ceilingHeight;
        LightLevel = lightLevel;
        Special = special;
    }

    public int SectorIndex { get; }
    public double FloorHeight { get; }
    public double CeilingHeight { get; }
    public int LightLevel { get; }
    public int Special { get; }
}

public readonly struct SnapshotChecksumMoverSample
{
    public SnapshotChecksumMoverSample(
        int sectorIndex,
        double floorHeight,
        double ceilingHeight,
        bool floorMoving,
        bool ceilingMoving)
    {
        SectorIndex = sectorIndex;
        FloorHeight = floorHeight;
        CeilingHeight = ceilingHeight;
        FloorMoving = floorMoving;
        CeilingMoving = ceilingMoving;
    }

    public int SectorIndex { get; }
    public double FloorHeight { get; }
    public double CeilingHeight { get; }
    public bool FloorMoving { get; }
    public bool CeilingMoving { get; }
}

public readonly struct SnapshotChecksumActorSample
{
    public SnapshotChecksumActorSample(ReadOnlySpan<byte> className, int health)
    {
        ClassName = className.Length == 0 ? Array.Empty<byte>() : className.ToArray();
        Health = health;
    }

    public byte[] ClassName { get; }
    public int Health { get; }
}

public readonly struct SnapshotChecksumInputs
{
    public SnapshotChecksumInputs(
        ReadOnlySpan<SnapshotChecksumPlayerSample> players,
        ReadOnlySpan<SnapshotChecksumSectorSample> sectors,
        ReadOnlySpan<SnapshotChecksumMoverSample> movers,
        ReadOnlySpan<SnapshotChecksumActorSample> actors,
        int rngSeed,
        int gameTic,
        uint lineSpecRollingHash = 0,
        uint coopDeadSpawnRollingHash = 0)
    {
        Players = players.Length == 0 ? Array.Empty<SnapshotChecksumPlayerSample>() : players.ToArray();
        Sectors = sectors.Length == 0 ? Array.Empty<SnapshotChecksumSectorSample>() : sectors.ToArray();
        Movers = movers.Length == 0 ? Array.Empty<SnapshotChecksumMoverSample>() : movers.ToArray();
        Actors = actors.Length == 0 ? Array.Empty<SnapshotChecksumActorSample>() : actors.ToArray();
        RngSeed = rngSeed;
        GameTic = gameTic;
        LineSpecRollingHash = lineSpecRollingHash;
        CoopDeadSpawnRollingHash = coopDeadSpawnRollingHash;
    }

    public SnapshotChecksumPlayerSample[] Players { get; }
    public SnapshotChecksumSectorSample[] Sectors { get; }
    public SnapshotChecksumMoverSample[] Movers { get; }
    public SnapshotChecksumActorSample[] Actors { get; }
    public int RngSeed { get; }
    public int GameTic { get; }
    public uint LineSpecRollingHash { get; }
    public uint CoopDeadSpawnRollingHash { get; }
}

public static class SnapshotChecksumMixer
{
    public static uint MixU32(uint hash, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return Crc32.Add(hash, bytes);
    }

    public static uint MixDouble(uint hash, double value)
    {
        if (!double.IsFinite(value))
            return MixU32(hash, 0);

        var clamped = Math.Max(-2147483648.0, Math.Min(2147483392.0, value * 256.0));
        var scaled = (int)clamped;
        return MixU32(hash, unchecked((uint)scaled));
    }

    public static uint HashClassName(ReadOnlySpan<byte> className) =>
        className.Length == 0 ? 0u : Crc32.Calc(className);

    public static uint ComputePlayers(ReadOnlySpan<SnapshotChecksumPlayerSample> players)
    {
        var hash = 0u;
        foreach (var player in players)
        {
            hash = MixU32(hash, unchecked((uint)player.PlayerIndex));
            hash = MixU32(hash, unchecked((uint)player.Health));
            hash = MixU32(hash, unchecked((uint)player.PlayerState));
            hash = MixU32(hash, player.OnGround ? 1u : 0u);
        }

        return hash;
    }

    public static uint ComputeSectors(ReadOnlySpan<SnapshotChecksumSectorSample> sectors)
    {
        var hash = 0u;
        foreach (var sector in sectors)
        {
            hash = MixU32(hash, unchecked((uint)sector.SectorIndex));
            hash = MixDouble(hash, sector.FloorHeight);
            hash = MixDouble(hash, sector.CeilingHeight);
            hash = MixU32(hash, unchecked((uint)sector.LightLevel));
            hash = MixU32(hash, unchecked((uint)sector.Special));
        }

        return hash;
    }

    public static uint ComputeMovers(ReadOnlySpan<SnapshotChecksumMoverSample> movers)
    {
        var hash = 0u;
        foreach (var mover in movers)
        {
            hash = MixU32(hash, unchecked((uint)mover.SectorIndex));
            hash = MixDouble(hash, mover.FloorHeight);
            hash = MixDouble(hash, mover.CeilingHeight);
            hash = MixU32(hash, mover.FloorMoving ? 1u : 0u);
            hash = MixU32(hash, mover.CeilingMoving ? 1u : 0u);
        }

        return hash;
    }

    public static uint ComputeActors(ReadOnlySpan<SnapshotChecksumActorSample> actors)
    {
        uint accum = 0;
        uint count = 0;
        foreach (var actor in actors)
        {
            count++;
            var entry = MixU32(0u, HashClassName(actor.ClassName));
            entry = MixU32(entry, unchecked((uint)actor.Health));
            accum += entry;
        }

        var hash = MixU32(0u, count);
        return MixU32(hash, accum);
    }

    public static uint ComputeRng(int rngSeed, int gameTic)
    {
        var hash = MixU32(0u, unchecked((uint)rngSeed));
        return MixU32(hash, unchecked((uint)gameTic));
    }

    public static uint[] ComputeAll(SnapshotChecksumInputs inputs, byte categoryMask = SnapshotChecksumRing.DefaultEnabledCategoryMask)
    {
        var hashes = new uint[LiveConstants.SnapshotChecksumCategoryCount];
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.Players)) != 0)
            hashes[(int)SnapshotChecksumCategory.Players] = ComputePlayers(inputs.Players);
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.Sectors)) != 0)
            hashes[(int)SnapshotChecksumCategory.Sectors] = ComputeSectors(inputs.Sectors);
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.Movers)) != 0)
            hashes[(int)SnapshotChecksumCategory.Movers] = ComputeMovers(inputs.Movers);
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.Actors)) != 0)
        {
            var actorsHash = ComputeActors(inputs.Actors);
            if (inputs.CoopDeadSpawnRollingHash != 0)
                actorsHash = MixU32(actorsHash, inputs.CoopDeadSpawnRollingHash);
            hashes[(int)SnapshotChecksumCategory.Actors] = actorsHash;
        }
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.Rng)) != 0)
            hashes[(int)SnapshotChecksumCategory.Rng] = ComputeRng(inputs.RngSeed, inputs.GameTic);
        if ((categoryMask & (1 << (int)SnapshotChecksumCategory.LineSpec)) != 0)
            hashes[(int)SnapshotChecksumCategory.LineSpec] = inputs.LineSpecRollingHash;
        return hashes;
    }
}

public sealed class SnapshotChecksumSession
{
    private readonly SnapshotChecksumRing _ring = new();
    private readonly uint[] _categoryHashes = new uint[LiveConstants.SnapshotChecksumCategoryCount];
    private int _lastComputedTic = int.MinValue;
    private uint _lineSpecRollingHash;

    public SnapshotChecksumRing Ring => _ring;

    public void Reset()
    {
        _ring.Reset();
        Array.Clear(_categoryHashes);
        _lastComputedTic = int.MinValue;
        _lineSpecRollingHash = 0;
    }

    public void NoteLineSpec(int lineIndex, int special, bool success)
    {
        _lineSpecRollingHash = SnapshotChecksumMixer.MixU32(_lineSpecRollingHash, unchecked((uint)lineIndex));
        _lineSpecRollingHash = SnapshotChecksumMixer.MixU32(_lineSpecRollingHash, unchecked((uint)special));
        _lineSpecRollingHash = SnapshotChecksumMixer.MixU32(_lineSpecRollingHash, success ? 1u : 0u);
    }

    public ReadOnlySpan<uint> CategoryHashes => _categoryHashes;

    public void ComputeIfStale(int gameTic, SnapshotChecksumInputs inputs, byte categoryMask = SnapshotChecksumRing.DefaultEnabledCategoryMask)
    {
        if (_lastComputedTic == gameTic)
            return;

        var mergedInputs = new SnapshotChecksumInputs(
            inputs.Players,
            inputs.Sectors,
            inputs.Movers,
            inputs.Actors,
            inputs.RngSeed,
            inputs.GameTic,
            _lineSpecRollingHash);
        var computed = SnapshotChecksumMixer.ComputeAll(mergedInputs, categoryMask);
        computed.CopyTo(_categoryHashes, 0);
        _lastComputedTic = gameTic;
        _ring.Store(gameTic, _categoryHashes);
    }

    public int WriteServerChunk(Span<byte> output, int gameTic)
    {
        if (_lastComputedTic != gameTic)
            return 0;

        return SnapshotChecksumCodec.Write(output, unchecked((uint)gameTic), _categoryHashes);
    }
}
