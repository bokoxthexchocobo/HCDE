namespace HCDE.Playsim;

/// <summary>
/// Volume/pitch setters, playing-info queries, channel-used scan, and evict loop head from
/// <c>s_sound.cpp</c> (**1051–1200**). Chunk **1103**.
/// </summary>
public static class VmSoundEngineVolumePitchQuery1103Slice
{
    public const int ChanfEvicted = 1;
    public const int ChanfAbstime = 0x200;
    public const int SourceAny = -1;
    public const int InvalidSound = -1;

    public static float ClampUnitVolume(float volume)
    {
        if (volume < 0f)
            return 0f;
        return volume > 1f ? 1f : volume;
    }

    public sealed class SetVolumeContext
    {
        public float RequestedVolume { get; init; }
        public Action<float>? ApplyBackend { get; init; }
        public Action<float>? AssignChannelVolume { get; init; }
    }

    public static void RunSetVolume(SetVolumeContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var volume = ClampUnitVolume(ctx.RequestedVolume);
        ctx.ApplyBackend?.Invoke(volume);
        ctx.AssignChannelVolume?.Invoke(volume);
    }

    public sealed class ChangePitchScanContext
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int Channel { get; init; }
        public int OrgSoundId { get; init; }
        public bool MatchBySoundId { get; init; }
        public double Pitch { get; init; }
        public IReadOnlyList<PitchChannelRow>? Channels { get; init; }
        public Action<PitchChannelRow, float>? SetPitch { get; init; }
    }

    public sealed class PitchChannelRow
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int EntChannel { get; init; }
        public int OrgId { get; init; }
    }

    public static void RunChangeSoundPitchScan(ChangePitchScanContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var channels = ctx.Channels;
        if (channels is null)
            return;

        for (var i = 0; i < channels.Count; i++)
        {
            var row = channels[i];
            if (row.SourceType != ctx.SourceType || row.Source != ctx.Source)
                continue;

            var channelMatch = ctx.MatchBySoundId
                ? row.OrgId == ctx.OrgSoundId
                : row.EntChannel == ctx.Channel;
            if (!channelMatch)
                continue;

            var pitch = Math.Max(0.0001f, (float)ctx.Pitch);
            ctx.SetPitch?.Invoke(row, pitch);
        }
    }

    public static float ClampPitchForBackend(float pitch) => Math.Max(0.0001f, pitch);

    public sealed class PlayingInfoContext
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int ChannelFilter { get; init; }
        public int OrgSoundId { get; init; }
        public bool ValidSoundId { get; init; }
        public IReadOnlyList<PlayingInfoRow>? Channels { get; init; }
    }

    public sealed class PlayingInfoRow
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int EntChannel { get; init; }
        public int OrgId { get; init; }
    }

    public static int CountPlayingInfo(PlayingInfoContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var channels = ctx.Channels;
        if (channels is null)
            return 0;

        var count = 0;
        for (var i = 0; i < channels.Count; i++)
        {
            var row = channels[i];
            if (ctx.ChannelFilter != -1 && ctx.ChannelFilter != row.EntChannel)
                continue;

            if (ctx.ValidSoundId)
            {
                if (row.OrgId != ctx.OrgSoundId)
                    continue;
                if (ctx.SourceType != SourceAny
                    && (row.SourceType != ctx.SourceType || row.Source != ctx.Source))
                {
                    continue;
                }
            }
            else if (ctx.SourceType != SourceAny
                     && (row.SourceType != ctx.SourceType || row.Source != ctx.Source))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    public sealed class ChannelUsedContext
    {
        public int SourceType { get; init; }
        public object? Actor { get; init; }
        public int Channel { get; init; }
        public int SeenMask { get; set; }
        public IReadOnlyList<ChannelUsedRow>? Channels { get; init; }
    }

    public sealed class ChannelUsedRow
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int EntChannel { get; init; }
    }

    public static bool IsChannelUsed(ChannelUsedContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.SeenMask & (1 << ctx.Channel)) != 0)
            return true;

        var channels = ctx.Channels;
        if (channels is null)
            return false;

        for (var i = 0; i < channels.Count; i++)
        {
            var row = channels[i];
            if (row.SourceType != ctx.SourceType || row.Source != ctx.Actor)
                continue;

            ctx.SeenMask |= 1 << row.EntChannel;
            if (row.EntChannel == ctx.Channel)
                return true;
        }

        return false;
    }

    public sealed class SourcePlayingContext
    {
        public int SourceType { get; init; }
        public object? Actor { get; init; }
        public int Channel { get; init; }
        public int OrgSoundId { get; init; }
        public bool AnySound { get; init; }
        public IReadOnlyList<SourcePlayingRow>? Channels { get; init; }
    }

    public sealed class SourcePlayingRow
    {
        public int SourceType { get; init; }
        public object? Source { get; init; }
        public int EntChannel { get; init; }
        public int OrgId { get; init; }
    }

    public static bool IsSourcePlayingSomething(SourcePlayingContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var channels = ctx.Channels;
        if (channels is null)
            return false;

        for (var i = 0; i < channels.Count; i++)
        {
            var row = channels[i];
            if (row.SourceType != ctx.SourceType)
                continue;
            if (ctx.SourceType != StartSoundContext.SourceNone
                && ctx.SourceType != StartSoundContext.SourceUnattached
                && row.Source != ctx.Actor)
            {
                continue;
            }

            var channelOk = ctx.Channel == 0 || row.EntChannel == ctx.Channel;
            var soundOk = ctx.AnySound || row.OrgId == ctx.OrgSoundId;
            if (channelOk && soundOk)
                return true;
        }

        return false;
    }

    public sealed class EvictChannelContext
    {
        public int ChanFlags { get; init; }
        public bool HasSysChannel { get; init; }
        public bool BackendPresent { get; init; }
        public float StartTime { get; set; }
        public int ChanFlagsOut { get; set; }
        public Func<float>? ReadPosition { get; init; }
        public Action? StopChannel { get; init; }
    }

    public static bool ShouldMarkEvicted(int chanFlags) => (chanFlags & ChanfEvicted) == 0;

    public static void RunEvictChannel(EvictChannelContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.ChanFlagsOut = ctx.ChanFlags | ChanfEvicted;
        if (!ctx.HasSysChannel)
            return;

        if ((ctx.ChanFlags & ChanfAbstime) == 0)
        {
            ctx.StartTime = ctx.BackendPresent ? ctx.ReadPosition?.Invoke() ?? 0f : 0f;
            ctx.ChanFlagsOut |= ChanfAbstime;
        }

        ctx.StopChannel?.Invoke();
    }
}
