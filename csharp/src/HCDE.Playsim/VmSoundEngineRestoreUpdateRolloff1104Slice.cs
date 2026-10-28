namespace HCDE.Playsim;

/// <summary>
/// Restore-evict recursion, `UpdateSounds` 3D scan, and `GetRolloff` from <c>s_sound.cpp</c> (**1201–1350**). Chunk **1104**.
/// </summary>
public static class VmSoundEngineRestoreUpdateRolloff1104Slice
{
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int ChanfForgettable = 8;
    public const int ChanfJustStarted = 0x80;
    public const int ChanfIs3D = 0x30;
    public const int ChanfArea = 4;
    public const int RolloffLog = 1;
    public const int RolloffLinear = 2;
    public const int RolloffCustom = 3;

    public sealed class RestoreEvictedContext
    {
        public int ChanFlags { get; set; }
        public bool HasSysChannel { get; init; }
        public bool IsLooping { get; init; }
        public Action? RestartChannel { get; init; }
        public Action? ReturnChannel { get; init; }
    }

    public static void RunRestoreEvictedNode(RestoreEvictedContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.ChanFlags & ChanfEvicted) == 0)
        {
            if (!ctx.HasSysChannel
                && (ctx.ChanFlags & (ChanfForgettable | ChanfLoop)) == ChanfForgettable)
            {
                ctx.ReturnChannel?.Invoke();
            }

            return;
        }

        ctx.RestartChannel?.Invoke();
        if (ctx.IsLooping)
            return;

        if ((ctx.ChanFlags & ChanfEvicted) != 0)
        {
            ctx.ReturnChannel?.Invoke();
            return;
        }

        if ((ctx.ChanFlags & ChanfJustStarted) == 0)
            ctx.ChanFlags |= ChanfForgettable;
    }

    public sealed class UpdateSoundsContext
    {
        public IReadOnlyList<UpdateChannelRow>? Channels { get; init; }
        public Action<UpdateChannelRow>? Update3D { get; init; }
        public Action? UpdateListener { get; init; }
        public Action? UpdateBackend { get; init; }
        public int Time { get; init; }
        public int RestartEvictionsAt { get; set; }
        public Action? RestoreEvicted { get; init; }
    }

    public sealed class UpdateChannelRow
    {
        public int ChanFlags { get; set; }
    }

    public static void RunUpdateSounds(UpdateSoundsContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var channels = ctx.Channels;
        if (channels is not null)
        {
            for (var i = 0; i < channels.Count; i++)
            {
                var row = channels[i];
                if ((row.ChanFlags & (ChanfEvicted | ChanfIs3D)) == ChanfIs3D)
                    ctx.Update3D?.Invoke(row);
                row.ChanFlags &= ~ChanfJustStarted;
            }
        }

        ctx.UpdateListener?.Invoke();
        ctx.UpdateBackend?.Invoke();
        if (ctx.Time >= ctx.RestartEvictionsAt)
        {
            ctx.RestartEvictionsAt = 0;
            ctx.RestoreEvicted?.Invoke();
        }
    }

    public sealed class RolloffInfo
    {
        public float MinDistance { get; init; }
        public float MaxDistance { get; init; }
        public float RolloffFactor { get; init; }
        public int RolloffType { get; init; }
    }

    public static float GetRolloff(RolloffInfo? rolloff, float distance, IReadOnlyList<byte>? soundCurve)
    {
        if (rolloff is null)
            return 0f;
        if (distance <= rolloff.MinDistance)
            return 1f;

        if (rolloff.RolloffType == RolloffLog)
        {
            return rolloff.MinDistance
                   / (rolloff.MinDistance + rolloff.RolloffFactor * (distance - rolloff.MinDistance));
        }

        if (distance >= rolloff.MaxDistance)
            return 0f;

        var volume = (rolloff.MaxDistance - distance) / (rolloff.MaxDistance - rolloff.MinDistance);
        if (rolloff.RolloffType == RolloffLinear)
            return volume;

        if (rolloff.RolloffType == RolloffCustom && soundCurve is { Count: > 0 })
        {
            var index = (int)(soundCurve.Count * (1f - volume));
            index = Math.Clamp(index, 0, soundCurve.Count - 1);
            return soundCurve[index] / 127f;
        }

        return ((float)Math.Pow(10, volume) - 1f) / 9f;
    }
}
