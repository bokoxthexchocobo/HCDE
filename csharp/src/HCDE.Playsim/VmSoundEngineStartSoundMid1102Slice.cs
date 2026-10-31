namespace HCDE.Playsim;

/// <summary>
/// <c>StartSound</c> mid-body: rolloff/singular/limit gates, load/priority/overlap/stop scan, pause/pitch, and
/// post-start flag merge from <c>s_sound.cpp</c> (**451–600**). Chunk **1102**.
/// </summary>
public static class VmSoundEngineStartSoundMid1102Slice
{
    public const int ChanfSingular = 0x20;
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int ChanfOverlap = 4;
    public const int ChanfUi = 8;
    public const int ChanfNopause = 0x10;
    public const int ChanfForce = 0x100;
    public const int ChanfIs3D = 0x30;
    public const int ChanfListenerZ = 0x60;
    public const int ChanfJustStarted = 0x80;

    public static float ApplySfxAttenuation(float attenuation, float sfxAttenuation) =>
        attenuation * sfxAttenuation;

    public static float ResolveRolloffMinDistance(
        bool forcedPresent,
        float forcedMin,
        float currentMin,
        float globalMin)
    {
        if (forcedPresent && forcedMin != 0f)
            return forcedMin;
        return currentMin == 0f ? globalMin : currentMin;
    }

    public sealed class SingularLimitContext
    {
        public bool SfxSingular { get; init; }
        public int IncomingFlags { get; init; }
        public int ChanFlags { get; set; }
        public int Type { get; init; }
        public bool SourceIsListener { get; init; }
        public int NearLimit { get; set; }
        public Func<int, bool>? CheckSingular { get; init; }
        public int SoundId { get; init; }
        public Func<bool>? CheckSoundLimit { get; init; }
    }

    public static void RunSingularAndLimitGates(SingularLimitContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.SfxSingular || (ctx.IncomingFlags & ChanfSingular) != 0)
            && ctx.CheckSingular?.Invoke(ctx.SoundId) == true)
        {
            ctx.ChanFlags |= ChanfEvicted;
        }

        if (ctx.Type == StartSoundContext.SourceNone || ctx.SourceIsListener)
            ctx.NearLimit = 0;

        if (ctx.NearLimit > 0 && ctx.CheckSoundLimit?.Invoke() == true)
            ctx.ChanFlags |= ChanfEvicted;
    }

    public static bool ShouldAbortEvictedNonLoop(int chanFlags) =>
        (chanFlags & (ChanfEvicted | ChanfLoop)) == ChanfEvicted;

    public static int ResolveBasePriority(int type, bool sourceIsListener) =>
        type == StartSoundContext.SourceNone || sourceIsListener ? 80 : 0;

    public static bool ShouldApplyOverlapForAutoChannel(object? source, int channel) =>
        source != null && channel == StartSoundContext.ChanAuto;

    public sealed class StopChannelScanContext
    {
        public int ChanFlags { get; init; }
        public int Type { get; init; }
        public object? Source { get; init; }
        public int Channel { get; init; }
        public bool ChannelInUse { get; init; }
        public float TargetX { get; init; }
        public float TargetY { get; init; }
        public float TargetZ { get; init; }
        public IReadOnlyList<ChannelSlot>? ActiveChannels { get; init; }
        public Action<ChannelSlot>? StopChannel { get; init; }
    }

    public sealed class ChannelSlot
    {
        public int SourceType { get; init; }
        public int EntChannel { get; init; }
        public object? Source { get; init; }
        public float PointX { get; init; }
        public float PointY { get; init; }
        public float PointZ { get; init; }
    }

    public static bool ShouldScanForStop(StopChannelScanContext ctx)
    {
        if ((ctx.ChanFlags & ChanfOverlap) != 0 || ctx.Type == StartSoundContext.SourceNone)
            return false;
        if (ctx.Source is null && ctx.Channel != StartSoundContext.ChanAuto)
            return true;
        return ctx.Source is not null && ctx.ChannelInUse;
    }

    public static void RunStopMatchingChannels(StopChannelScanContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ShouldScanForStop(ctx))
            return;

        var channels = ctx.ActiveChannels;
        if (channels is null)
            return;

        for (var i = 0; i < channels.Count; i++)
        {
            var slot = channels[i];
            if (slot.SourceType != ctx.Type || slot.EntChannel != ctx.Channel)
                continue;

            var found = ctx.Type == StartSoundContext.SourceUnattached
                ? slot.PointX == ctx.TargetX && slot.PointZ == ctx.TargetZ && slot.PointY == ctx.TargetY
                : ReferenceEquals(slot.Source, ctx.Source);

            if (found)
                ctx.StopChannel?.Invoke(slot);
        }
    }

    public static bool ShouldAbortPausedNonLoop(int chanFlags, bool soundPaused) =>
        (chanFlags & ChanfLoop) == 0
        && (chanFlags & (ChanfUi | ChanfNopause | ChanfForce)) == 0
        && soundPaused;

    public static float ResolveStartPitch(float spitch, float defPitch, float defPitchMax, Func<int, float, float, float>? calcPitch)
    {
        if (spitch > 0f)
            return spitch;
        return calcPitch?.Invoke(0, defPitch, defPitchMax) ?? 1f;
    }

    public static bool ShouldSkipBackendStart(int chanFlags) => (chanFlags & ChanfEvicted) != 0;

    public static int MergePostStartChannelFlags(int chanFlags, float attenuation, int type)
    {
        if (attenuation > 0f && type != StartSoundContext.SourceNone)
            return chanFlags | ChanfIs3D | ChanfJustStarted;
        return chanFlags | ChanfListenerZ | ChanfJustStarted;
    }

    public static bool LoadSoundShouldRejectEmpty(int lumpNum, int emptyLump) => lumpNum == emptyLump;
}
