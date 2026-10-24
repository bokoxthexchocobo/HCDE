namespace HCDE.Client;

/// <summary><c>HCDEApplyInvasionSnapshot</c> from <c>d_net.cpp</c> (3351–3510).</summary>
public static class VmDNetInvasionSnapshotApply
{
    public const byte InvsDisabled = 0;
    public const byte InvsFailure = 4;
    public const byte InvasionSnapshotProtocolVersion = 2;
    public const byte BossWaveFlag = 1;
    public const byte SpawnFallbackFlag = 1;
    public const byte InvspawnPlayerStart = 2;

public static bool ExpectActorDeltas(bool actorDelta, bool actorRegistry) => actorDelta && actorRegistry;

public static bool HeaderBytesAvailable(int bodyCursor, int bodyBytes, int headerSize) =>
        bodyCursor <= bodyBytes && bodyBytes - bodyCursor >= headerSize;

    public static bool MagicMatches(ReadOnlySpan<byte> body, int offset, ReadOnlySpan<byte> magic) =>
        body.Length >= offset + magic.Length && body.Slice(offset, magic.Length).SequenceEqual(magic);

    public static bool InvasionModeDisabledLocally(bool enabled) => !enabled;

    public static bool AcceptSnapshotWhenGametypeZero(byte stateRaw) =>
        stateRaw != InvsDisabled && stateRaw <= InvsFailure;

    public static bool VersionHasSpawnMetadata(byte version) => version >= 2;

    public static int SnapshotHeaderSize(byte version) =>
        version >= 2 ? 48 : 40;

    public static bool VersionAllowed(byte version) =>
        version == 1 || version == InvasionSnapshotProtocolVersion;

    public static bool ReservedZero(byte reserved) => reserved == 0;

    public static bool FlagsAllowed(byte flags) => (flags & ~BossWaveFlag) == 0;

    public static bool StateRawAllowed(byte stateRaw) => stateRaw <= InvsFailure;

    public static bool SpawnFlagsAllowed(byte spawnFlags) => (spawnFlags & ~SpawnFallbackFlag) == 0;

    public static bool SpawnFallbackSourceAllowed(byte source) => source <= InvspawnPlayerStart;

    public static int ClampNonNegative(int value) => Math.Max(value, 0);

    public static int BossWaveFlags(byte flags) => (flags & BossWaveFlag) != 0 ? 1 : 0;

    public static bool UsingFallback(byte spawnFlags) => (spawnFlags & SpawnFallbackFlag) != 0;

    public static bool ShouldApplyAuthorityEvents(bool expect, int bodyCursor, int bodyBytes, bool magicMatch) =>
        bodyCursor < bodyBytes && expect && magicMatch;

    public static bool ShouldApplyActorDeltas(bool expect, int bodyCursor, int bodyBytes, bool magicMatch) =>
        bodyCursor < bodyBytes && expect && magicMatch;

public readonly record struct InvasionWaveMerge(int IncomingSpawned, int IncomingCleared);

public static bool TraceGametypeOverride(byte stateRaw, bool invasionEnabled) =>
        InvasionModeDisabledLocally(invasionEnabled) && AcceptSnapshotWhenGametypeZero(stateRaw);

    public static int InvasionSnapshotHeaderV1Size() => 40;

    public static int InvasionSnapshotHeaderV2Size() => 48;

public static int InvasionSnapshotVersionOffset() => 4;

    public static int InvasionSnapshotFlagsOffset() => 5;

    public static int InvasionSnapshotStateOffset() => 6;

    public static int InvasionSnapshotReservedOffset() => 7;

    public static int InvasionSnapshotStateTicsOffset() => 8;

    public static bool ReadFieldsSucceeded(bool stateTics, bool wave, bool maxWaves) =>
        stateTics && wave && maxWaves;

    public static bool SpawnMetadataReadSucceeded(bool spots, bool activeSpots, bool budget) =>
        spots && activeSpots && budget;

    public static bool ReservedSpawnBytesZero(byte r2, byte r3) => r2 == 0 && r3 == 0;

    public static int ReplicatedActiveMonsters(int active) => ClampNonNegative(active);

    public static int WaveDirectorSpawned(int spawned) => ClampNonNegative(spawned);

    public static int WaveDirectorCleared(int cleared) => ClampNonNegative(cleared);

public static int SpawnDirectoryBudget(int budget) => ClampNonNegative(budget);

    public static int SpawnDirectoryTag(int tag) => ClampNonNegative(tag);

public static bool PreviousStateDiffers(byte previous, byte newState) => previous != newState;

    public static int PreviousWaveDiffers(int previousWave, int wave) =>
        previousWave != wave ? 1 : 0;
}
