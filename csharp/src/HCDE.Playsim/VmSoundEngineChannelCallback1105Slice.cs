namespace HCDE.Playsim;

/// <summary>
/// `ChannelEnded` evicted classification, `SoundDone`, virtual flag, and `StopChannel` from
/// <c>s_sound.cpp</c> (**1351–1500**). Chunk **1105**.
/// </summary>
public static class VmSoundEngineChannelCallback1105Slice
{
    public const int ChanfForgettable = 8;
    public const int ChanfLoop = 2;
    public const int ChanfEvicted = 1;
    public const int ChanfJustStarted = 0x80;
    public const int ChanfVirtual = 0x400;

    public sealed class ChannelEndedContext
    {
        public int ChanFlags { get; set; }
        public bool Forgettable { get; init; }
        public bool Looping { get; init; }
        public uint PlaybackPosition { get; init; }
        public uint SampleLength { get; init; }
        public bool HasSysChannel { get; set; }
    }

    public static bool ResolveChannelEndedEvicted(ChannelEndedContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.Forgettable)
            return false;
        if (ctx.Looping || (ctx.ChanFlags & ChanfEvicted) != 0)
            return true;
        if (ctx.PlaybackPosition == 0)
            return (ctx.ChanFlags & ChanfJustStarted) != 0;
        return ctx.PlaybackPosition < ctx.SampleLength;
    }

    public static void ApplyChannelEndedFlags(ChannelEndedContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var evicted = ResolveChannelEndedEvicted(ctx);
        if (!evicted)
        {
            ctx.ChanFlags &= ~ChanfEvicted;
            return;
        }

        ctx.ChanFlags |= ChanfEvicted;
        ctx.HasSysChannel = false;
    }

    public static void RunSoundDone(Action? returnChannel) => returnChannel?.Invoke();

    public static int ApplyVirtualFlag(int chanFlags, bool isVirtual) =>
        isVirtual ? chanFlags | ChanfVirtual : chanFlags & ~ChanfVirtual;

    public sealed class StopChannelContext
    {
        public int ChanFlags { get; set; }
        public bool HasSysChannel { get; init; }
        public bool BackendPresent { get; init; }
        public Action? StopBackend { get; init; }
        public Action? ReturnChannel { get; init; }
    }

    public static void RunStopChannel(StopChannelContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasSysChannel)
        {
            ctx.ReturnChannel?.Invoke();
            return;
        }

        if ((ctx.ChanFlags & ChanfEvicted) == 0)
            ctx.ChanFlags |= ChanfForgettable;

        if (ctx.BackendPresent)
            ctx.StopBackend?.Invoke();
    }

    public static bool UnloadAllShouldWalk(int sfxCount) => sfxCount > 0;

    public static int CountUnloadIndices(int sfxCount, Func<int, bool>? shouldUnload)
    {
        var count = 0;
        for (var i = 0; i < sfxCount; i++)
        {
            if (shouldUnload?.Invoke(i) == true)
                count++;
        }

        return count;
    }
}
