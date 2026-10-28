namespace HCDE.Playsim;

/// <summary>
/// Sound engine lifecycle prologue from <c>s_sound.cpp</c> (**1–150**): enable gate, init/clear/shutdown,
/// mark/cache/unload sweep. Chunk **1099**; I/O and channel allocation remain host-injected.
/// </summary>
public static class VmSoundEngineLifecycle1099Slice
{
    public const int DefaultPitch = 128;
    public const int SfxNoLink = -1;

    public sealed class CacheMarkedSoundsContext
    {
        public int SfxCount { get; init; }
        public Func<int, bool>? IsUsed { get; init; }
        public Func<int, int>? ReadLink { get; init; }
        public Action<int>? CacheUsed { get; init; }
        public Action<int>? UnloadUnused { get; init; }
        public Func<object?>? ChannelHead { get; init; }
        public Func<object?, bool>? MarkPlayingSound { get; init; }
        public Func<object?, object?>? NextChannel { get; init; }
    }

    public static bool ResolveSoundEnabled(bool sndEnabledCvar, bool noSound, bool noSfx) =>
        sndEnabledCvar && !noSound && !noSfx;

    public static bool ShouldContinueInitDrain(object? channelsHead) => channelsHead != null;

    public static bool ShutdownShouldWalkFreeList(object? freeHead) => freeHead != null;

    public static bool IsValidSoundIndex(int index, int count) =>
        index > 0 && (uint)index < (uint)count;

    public static bool ShouldUnloadAfterCachePass(bool used, int link) =>
        !used && link == SfxNoLink;

    public static void RunCacheMarkedSounds(CacheMarkedSoundsContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        for (var chan = ctx.ChannelHead?.Invoke(); chan != null; chan = ctx.NextChannel?.Invoke(chan))
            ctx.MarkPlayingSound?.Invoke(chan);

        for (var i = 1; i < ctx.SfxCount; i++)
        {
            if (ctx.IsUsed?.Invoke(i) == true)
                ctx.CacheUsed?.Invoke(i);
        }

        for (var i = 1; i < ctx.SfxCount; i++)
        {
            var used = ctx.IsUsed?.Invoke(i) == true;
            var link = ctx.ReadLink?.Invoke(i) ?? SfxNoLink;
            if (ShouldUnloadAfterCachePass(used, link))
                ctx.UnloadUnused?.Invoke(i);
        }
    }

    public static bool MarkUsedShouldSetFlag(int index, int count) => IsValidSoundIndex(index, count);

    public sealed class InitContext
    {
        public object? ActiveHead { get; set; }
        public Func<object?, object?>? ReturnChannel { get; init; }
        public Action<IReadOnlyList<byte>>? AssignCurve { get; init; }
        public IReadOnlyList<byte>? IncomingCurve { get; init; }
    }

    public static void RunInit(InitContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        while (ShouldContinueInitDrain(ctx.ActiveHead))
        {
            var head = ctx.ActiveHead;
            ctx.ActiveHead = ctx.ReturnChannel?.Invoke(head);
        }

        if (ctx.IncomingCurve is { Count: > 0 })
            ctx.AssignCurve?.Invoke(ctx.IncomingCurve);
    }

    public static int CountShutdownDeletes(object? freeHead, Func<object?, object?>? next, Action<object>? delete)
    {
        var count = 0;
        for (var node = freeHead; node != null; node = next?.Invoke(node))
        {
            delete?.Invoke(node);
            count++;
        }

        return count;
    }
}
