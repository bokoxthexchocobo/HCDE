namespace HCDE.Playsim;

/// <summary>
/// ListSoundChannels tail, <c>ResolveSound</c>, <c>CalcPitch</c>, and <c>StartSound</c> prologue from
/// <c>s_sound.cpp</c> (**301–450**). Chunk **1101**.
/// </summary>
public static class VmSoundEngineListResolveStart1101Slice
{
    public const int DefaultPitch = 128;

    public sealed class ListSoundChannelsContext
    {
        public IReadOnlyList<int>? ChannelFlags { get; init; }
        public Action<int>? AppendListedChannel { get; init; }
        public Action<int, string>? AppendSummary { get; init; }
    }

    public static int RunListSoundChannels(ListSoundChannelsContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var count = 0;
        var flags = ctx.ChannelFlags;
        if (flags is null)
            return 0;

        for (var i = 0; i < flags.Count; i++)
        {
            if (!VmSoundEngineCacheChannel1100Slice.ListChannelsShouldInclude(flags[i]))
                continue;
            ctx.AppendListedChannel?.Invoke(i);
            count++;
        }

        ctx.AppendSummary?.Invoke(count, $"{count} sounds playing\n");
        return count;
    }

    public sealed class ResolveSoundContext
    {
        public bool RandomHeader { get; init; }
        public float Attenuation { get; set; }
        public float HeaderAttenuation { get; init; }
        public int SfxLink { get; init; }
        public Func<int, int>? PickReplacement { get; init; }
        public int CurrentSoundId { get; init; }
    }

    public static int RunResolveSound(ResolveSoundContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.RandomHeader)
        {
            ctx.Attenuation *= ctx.HeaderAttenuation;
            return ctx.PickReplacement?.Invoke(ctx.CurrentSoundId) ?? 0;
        }

        return ctx.SfxLink;
    }

    public static float CalcPitch(
        int pitchmask,
        float defpitch,
        float defpitchmax,
        bool sndPitched,
        Func<int>? randomPitch = null)
    {
        Func<int> pitchRandom = randomPitch ?? DefaultPitchRandom;
        if (defpitch > 0f)
        {
            if (defpitchmax > 0f && Math.Abs(defpitch - defpitchmax) > float.Epsilon)
            {
                var sample = pitchRandom();
                defpitch = sample * (defpitchmax - defpitch) + defpitch;
            }

            return defpitch;
        }

        if (pitchmask != 0 && sndPitched)
        {
            var a = pitchRandom() & pitchmask;
            var b = pitchRandom() & pitchmask;
            return (DefaultPitch - a + b) / (float)DefaultPitch;
        }

        return 1f;
    }

    private static int DefaultPitchRandom() => Environment.TickCount & 0x7FFF;

    public sealed class StartSoundPrologueContext
    {
        public int Type { get; set; }
        public bool HasPoint { get; set; }
        public int SoundId { get; set; }
        public int OrgSoundId { get; set; }
        public float Volume { get; set; }
        public float SfxVolume { get; set; }
        public bool NoSfx { get; init; }
        public bool BlockNewSounds { get; init; }
        public Func<int, bool>? IsValidSoundId { get; init; }
        public Func<bool>? SoundEnabled { get; init; }
        public Action? CalcPosVel { get; init; }
        public Func<bool>? ValidatePosVel { get; init; }
    }

    /// <summary>Native early-out through scaled volume check (**410–430**).</summary>
    public static bool TryRunStartSoundPrologue(StartSoundPrologueContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.IsValidSoundId?.Invoke(ctx.SoundId) != true || ctx.Volume <= 0 || ctx.NoSfx
            || ctx.SoundEnabled?.Invoke() != true || ctx.BlockNewSounds)
        {
            return false;
        }

        if (ctx.Type == StartSoundContext.SourceUnattached && !ctx.HasPoint)
            ctx.Type = StartSoundContext.SourceNone;

        ctx.OrgSoundId = ctx.SoundId;
        ctx.CalcPosVel?.Invoke();
        if (ctx.ValidatePosVel?.Invoke() != true)
            return false;

        ctx.Volume = Math.Min(ctx.Volume * ctx.SfxVolume, 1f);
        return ctx.Volume > 0;
    }

    public sealed class SfxLinkStepContext
    {
        public int NearLimit { get; set; }
        public float LimitRange { get; set; }
        public float DefPitch { get; set; }
        public float DefPitchMax { get; set; }
        public float RolloffMinDistance { get; set; }
        public int CurrentSoundId { get; set; }
        public int ResolvedSoundId { get; set; }
        public bool SameSfxAfterResolve { get; set; }
        public Func<SfxLinkStepContext, bool>? ResolveLinkedSound { get; init; }
        public Func<int, bool>? IsValidSoundId { get; init; }
        public Action<SfxLinkStepContext, int>? ApplyLinkedSfxLimits { get; init; }
    }

    /// <summary>One iteration of the link-resolution loop (**441–450**).</summary>
    public static bool TryAdvanceSfxLink(SfxLinkStepContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.ResolveLinkedSound?.Invoke(ctx) != true)
            return false;

        if (ctx.IsValidSoundId?.Invoke(ctx.ResolvedSoundId) != true)
            return false;

        if (ctx.SameSfxAfterResolve)
            return false;

        ctx.ApplyLinkedSfxLimits?.Invoke(ctx, ctx.ResolvedSoundId);
        ctx.CurrentSoundId = ctx.ResolvedSoundId;
        return true;
    }

    public static void ApplyNearLimitFromLinkedSfx(SfxLinkStepContext ctx, int nearLimit, float limitRange, float defPitch, float defPitchMax)
    {
        if (ctx.NearLimit < 0)
        {
            ctx.NearLimit = nearLimit;
            ctx.LimitRange = limitRange;
            ctx.DefPitch = defPitch;
            ctx.DefPitchMax = defPitchMax;
        }
    }

    public static void ApplyRolloffFromLinkedSfx(SfxLinkStepContext ctx, float linkedRolloffMin)
    {
        if (ctx.RolloffMinDistance == 0f)
            ctx.RolloffMinDistance = linkedRolloffMin;
    }
}
