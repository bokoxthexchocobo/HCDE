namespace HCDE.Client;

/// <summary><c>HCDEApplyInvasionSnapshot</c> from <c>d_net.cpp</c> (3351–3510).</summary>
public static class VmDNetInvasionSnapshotApply
{
    public const byte InvsDisabled = 0;
    public const byte InvsCountdown = 2;
    public const byte InvsFailure = 4;
    public const byte InvasionSnapshotProtocolVersion = 2;
    public const byte BossWaveFlag = 1;
    public const int InvWaveFBoss = 1;
    public const byte SpawnFallbackFlag = 1;
    public const byte InvspawnNone = 0;
    public const byte InvspawnPlayerStart = 2;

    public const int InvasionSnapshotMagicOffset = 0;

    public static ReadOnlySpan<byte> DefaultSnapshotMagic => "HCIV"u8;

    public static bool ExpectActorDeltas(bool actorDelta, bool actorRegistry) => actorDelta && actorRegistry;

    public static bool HeaderBytesAvailable(int bodyCursor, int bodyBytes, int headerSize) =>
        bodyCursor <= bodyBytes && bodyBytes - bodyCursor >= headerSize;

    public static bool MagicMatches(ReadOnlySpan<byte> body, int offset, ReadOnlySpan<byte> magic) =>
        body.Length >= offset + magic.Length && body.Slice(offset, magic.Length).SequenceEqual(magic);

    public static bool MagicMatches(ReadOnlySpan<byte> body, int offset, byte[] magic) =>
        MagicMatches(body, offset, magic.AsSpan());

    public static bool InvasionModeDisabledLocally(bool enabled) => !enabled;

    public static bool AcceptSnapshotWhenGametypeZero(byte stateRaw) =>
        stateRaw != InvsDisabled && stateRaw <= InvsFailure;

    public static bool VersionHasSpawnMetadata(byte version) => version >= 2;

    public static int SnapshotHeaderSize(byte version) =>
        version >= 2 ? InvasionSnapshotHeaderV2Size() : InvasionSnapshotHeaderV1Size();

    public static bool VersionAllowed(byte version) =>
        version == 1 || version == InvasionSnapshotProtocolVersion;

    public static bool ReservedZero(byte reserved) => reserved == 0;

    public static bool FlagsAllowed(byte flags) => (flags & ~BossWaveFlag) == 0;

    public static bool StateRawAllowed(byte stateRaw) => stateRaw <= InvsFailure;

    public static bool SpawnFlagsAllowed(byte spawnFlags) => (spawnFlags & ~SpawnFallbackFlag) == 0;

    public static bool SpawnFallbackSourceAllowed(byte source) => source <= InvspawnPlayerStart;

    public static int ClampNonNegative(int value) => Math.Max(value, 0);

    public static int BossWaveFlags(byte flags) => (flags & BossWaveFlag) != 0 ? InvWaveFBoss : 0;

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

    public static int PendingWaveFromState(byte stateRaw, int wave) =>
        stateRaw == InvsCountdown ? ClampNonNegative(wave) : 0;

    public static bool ValidateSnapshotHeaderBytes(byte version, byte flags, byte stateRaw, byte reserved) =>
        VersionAllowed(version)
        && ReservedZero(reserved)
        && FlagsAllowed(flags)
        && StateRawAllowed(stateRaw);

    public static bool ValidateSpawnMetadataBlock(byte spawnFlags, byte spawnFallbackSource, byte reserved2, byte reserved3) =>
        SpawnFlagsAllowed(spawnFlags)
        && SpawnFallbackSourceAllowed(spawnFallbackSource)
        && ReservedSpawnBytesZero(reserved2, reserved3);

    public static bool RunInvasionDisabledGametypeGate(byte stateRaw, bool invasionEnabled, Action? traceGametypeOverride)
    {
        if (!InvasionModeDisabledLocally(invasionEnabled))
            return true;
        if (!AcceptSnapshotWhenGametypeZero(stateRaw))
            return false;
        traceGametypeOverride?.Invoke();
        return true;
    }

    public sealed class SnapshotHeaderView
    {
        public byte Version { get; set; }
        public byte Flags { get; set; }
        public byte StateRaw { get; set; }
        public byte Reserved { get; set; }
    }

    public static bool TryReadSnapshotHeader(ReadOnlySpan<byte> body, int bodyCursor, out SnapshotHeaderView header)
    {
        header = new SnapshotHeaderView();
        if (body.Length < bodyCursor + InvasionSnapshotStateTicsOffset())
            return false;
        var baseOff = bodyCursor;
        header.Version = body[baseOff + InvasionSnapshotVersionOffset()];
        header.Flags = body[baseOff + InvasionSnapshotFlagsOffset()];
        header.StateRaw = body[baseOff + InvasionSnapshotStateOffset()];
        header.Reserved = body[baseOff + InvasionSnapshotReservedOffset()];
        return true;
    }

    public sealed class InvasionSnapshotPayload
    {
        public byte Version { get; set; }
        public byte Flags { get; set; }
        public byte StateRaw { get; set; }
        public int StateTics { get; set; }
        public int Wave { get; set; }
        public int MaxWaves { get; set; }
        public int WaveBudget { get; set; }
        public InvasionWaveMerge WaveMerge { get; set; }
        public int ActiveMonsters { get; set; }
        public int SpawnSpotCount { get; set; }
        public int ActiveSpawnSpotCount { get; set; }
        public int SpawnPlanBudget { get; set; }
        public int SpawnActiveTag { get; set; }
        public byte SpawnFlags { get; set; }
        public byte SpawnFallbackSource { get; set; }
        public int WaveFlags { get; set; }
        public int PendingWave { get; set; }
        public bool UsingFallbackSpawn { get; set; }
    }

    public static InvasionSnapshotPayload BuildPayloadFromFields(
        byte version,
        byte flags,
        byte stateRaw,
        int stateTics,
        int wave,
        int maxWaves,
        int waveBudget,
        int waveSpawned,
        int waveCleared,
        int activeMonsters,
        int spawnSpotCount,
        int activeSpawnSpotCount,
        int spawnPlanBudget,
        int spawnActiveTag,
        byte spawnFlags,
        byte spawnFallbackSource)
    {
        var merge = new InvasionWaveMerge(WaveDirectorSpawned(waveSpawned), WaveDirectorCleared(waveCleared));
        return new InvasionSnapshotPayload
        {
            Version = version,
            Flags = flags,
            StateRaw = stateRaw,
            StateTics = ClampNonNegative(stateTics),
            Wave = ClampNonNegative(wave),
            MaxWaves = ClampNonNegative(maxWaves),
            WaveBudget = ClampNonNegative(waveBudget),
            WaveMerge = merge,
            ActiveMonsters = ReplicatedActiveMonsters(activeMonsters),
            SpawnSpotCount = spawnSpotCount,
            ActiveSpawnSpotCount = activeSpawnSpotCount,
            SpawnPlanBudget = SpawnDirectoryBudget(spawnPlanBudget),
            SpawnActiveTag = SpawnDirectoryTag(spawnActiveTag),
            SpawnFlags = spawnFlags,
            SpawnFallbackSource = spawnFallbackSource,
            WaveFlags = BossWaveFlags(flags),
            PendingWave = PendingWaveFromState(stateRaw, wave),
            UsingFallbackSpawn = UsingFallback(spawnFlags),
        };
    }

    public sealed class ApplyInvasionSnapshotContext
    {
        public bool ValidClient { get; set; }
        public bool HasInvasionSnapshotCap { get; set; }
        public bool ExpectActorDeltas { get; set; }
        public bool ExpectAuthorityEvents { get; set; }
        public bool InvasionModeEnabled { get; set; }
        public byte[] Body { get; set; } = Array.Empty<byte>();
        public int BodyBytes { get; set; }
        public int BodyCursor { get; set; }
        public int StartCursor { get; set; }
        public byte[] SnapshotMagic { get; set; } = "HCIV"u8.ToArray();
        public byte[] AuthorityEventsMagic { get; set; } = Array.Empty<byte>();
        public byte[] ActorDeltasMagic { get; set; } = Array.Empty<byte>();
        public int AuthorityHeaderSize { get; set; }
        public int ActorDeltasHeaderSize { get; set; }
        public int AuthorityMagicOffset { get; set; }
        public int ActorDeltasMagicOffset { get; set; }
        public InvasionSnapshotPayload? Payload { get; set; }
        public byte PreviousStateRaw { get; set; }
        public int PreviousWave { get; set; }
        public Action? TraceGametypeOverride { get; set; }
        public Action<byte, byte>? LogStateTransition { get; set; }
        public Func<ApplyInvasionSnapshotContext, bool>? ReadCoreFields { get; set; }
        public Func<ApplyInvasionSnapshotContext, bool>? ReadSpawnMetadata { get; set; }
        public Action<InvasionSnapshotPayload, byte, int>? ApplyMirror { get; set; }
        public Func<ApplyInvasionSnapshotContext, bool>? ApplyAuthorityEvents { get; set; }
        public Func<ApplyInvasionSnapshotContext, bool>? ApplyActorDeltas { get; set; }
        public Action<int, int>? RecordLaneRx { get; set; }
        public Action? TickAnnouncements { get; set; }
        public Action<int, long>? RecordProfile { get; set; }
    }

    public static bool RunClientCapabilityGate(ApplyInvasionSnapshotContext ctx) =>
        ctx.ValidClient && ctx.HasInvasionSnapshotCap;

    public static bool RunInitialHeaderValidation(ApplyInvasionSnapshotContext ctx)
    {
        if (!HeaderBytesAvailable(ctx.BodyCursor, ctx.BodyBytes, InvasionSnapshotHeaderV1Size()))
            return false;
        var magicOff = ctx.BodyCursor + InvasionSnapshotMagicOffset;
        return MagicMatches(ctx.Body.AsSpan(), magicOff, ctx.SnapshotMagic);
    }

    public static bool RunSnapshotHeaderValidation(ApplyInvasionSnapshotContext ctx, SnapshotHeaderView header)
    {
        if (!HeaderBytesAvailable(ctx.BodyCursor, ctx.BodyBytes, SnapshotHeaderSize(header.Version)))
            return false;
        return ValidateSnapshotHeaderBytes(header.Version, header.Flags, header.StateRaw, header.Reserved);
    }

    public static bool RunHeaderValidationPipeline(ApplyInvasionSnapshotContext ctx, out SnapshotHeaderView header)
    {
        header = new SnapshotHeaderView();
        if (!RunInitialHeaderValidation(ctx))
            return false;
        if (!TryReadSnapshotHeader(ctx.Body.AsSpan(), ctx.BodyCursor, out header))
            return false;
        if (!RunInvasionDisabledGametypeGate(header.StateRaw, ctx.InvasionModeEnabled, ctx.TraceGametypeOverride))
            return false;
        return RunSnapshotHeaderValidation(ctx, header);
    }

    public static bool RunSpawnMetadataPipeline(ApplyInvasionSnapshotContext ctx, byte version)
    {
        if (!VersionHasSpawnMetadata(version))
            return true;
        return ctx.ReadSpawnMetadata?.Invoke(ctx) ?? false;
    }

    public static bool RunEmbeddedReplayPipeline(ApplyInvasionSnapshotContext ctx)
    {
        var authMagic = ctx.AuthorityEventsMagic.Length > 0
            && MagicMatches(ctx.Body.AsSpan(), ctx.BodyCursor + ctx.AuthorityMagicOffset, ctx.AuthorityEventsMagic);
        if (ShouldApplyAuthorityEvents(ctx.ExpectAuthorityEvents, ctx.BodyCursor, ctx.BodyBytes, authMagic))
        {
            if (ctx.BodyBytes - ctx.BodyCursor < ctx.AuthorityHeaderSize)
                return false;
            if (ctx.ApplyAuthorityEvents?.Invoke(ctx) != true)
                return false;
        }

        var actorMagic = ctx.ActorDeltasMagic.Length > 0
            && MagicMatches(ctx.Body.AsSpan(), ctx.BodyCursor + ctx.ActorDeltasMagicOffset, ctx.ActorDeltasMagic);
        if (ShouldApplyActorDeltas(ctx.ExpectActorDeltas, ctx.BodyCursor, ctx.BodyBytes, actorMagic))
        {
            if (ctx.BodyBytes - ctx.BodyCursor < ctx.ActorDeltasHeaderSize)
                return false;
            if (ctx.ApplyActorDeltas?.Invoke(ctx) != true)
                return false;
        }

        return true;
    }

    public static bool RunApplyInvasionSnapshotPipeline(ApplyInvasionSnapshotContext ctx)
    {
        if (!RunClientCapabilityGate(ctx))
            return false;
        ctx.StartCursor = ctx.BodyCursor;
        if (!RunHeaderValidationPipeline(ctx, out var header))
            return false;
        if (ctx.ReadCoreFields?.Invoke(ctx) != true)
            return false;
        if (!RunSpawnMetadataPipeline(ctx, header.Version))
            return false;
        if (ctx.Payload == null)
            return false;

        if (PreviousStateDiffers(ctx.PreviousStateRaw, header.StateRaw))
            ctx.LogStateTransition?.Invoke(ctx.PreviousStateRaw, header.StateRaw);

        ctx.ApplyMirror?.Invoke(ctx.Payload, header.StateRaw, ctx.PreviousWave);

        var snapshotBytes = SnapshotHeaderSize(header.Version);
        ctx.RecordLaneRx?.Invoke(ctx.BodyCursor, snapshotBytes);

        if (!RunEmbeddedReplayPipeline(ctx))
            return false;

        var bytesReceived = ctx.BodyCursor - ctx.StartCursor;
        ctx.RecordProfile?.Invoke(ctx.Payload.Version, bytesReceived);
        ctx.TickAnnouncements?.Invoke();
        return true;
    }

    public static bool ApplyInvasionSnapshot(ApplyInvasionSnapshotContext ctx) =>
        RunApplyInvasionSnapshotPipeline(ctx);
}
