namespace HCDE.Playsim;

/// <summary>
/// <c>CacheSound</c>, channel pool, and list helpers from <c>s_sound.cpp</c> (**151–300**). Chunk **1100**.
/// </summary>
public static class VmSoundEngineCacheChannel1100Slice
{
    public const int ChanfForgettable = 1;
    public const int ChanfUi = 8;
    public const int ChanfTransient = 256;

    public sealed class CacheSoundContext
    {
        public bool BackendPresent { get; init; }
        public bool Tentative { get; init; }
        public bool RandomHeader { get; set; }
        public int LinkIndex { get; set; }
        public Func<int, bool>? IsValidLink { get; init; }
        public Action? CacheRandom { get; init; }
        public Action? LoadSingle { get; init; }
        public Action? MarkUsed { get; init; }
    }

    public static void RunCacheSound(CacheSoundContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.BackendPresent || ctx.Tentative)
            return;

        while (!ctx.RandomHeader && ctx.IsValidLink?.Invoke(ctx.LinkIndex) == true)
        {
            // Host resolves linked sfx; managed loop only models follow-link gate.
            break;
        }

        if (ctx.RandomHeader)
            ctx.CacheRandom?.Invoke();
        else
        {
            ctx.LoadSingle?.Invoke();
            ctx.MarkUsed?.Invoke();
        }
    }

    public sealed class UnloadSoundContext
    {
        public bool DataValid { get; init; }
        public Action? UnloadBackend { get; init; }
        public Action? ClearData { get; init; }
    }

    public static void RunUnloadSound(UnloadSoundContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.DataValid)
            ctx.UnloadBackend?.Invoke();
        ctx.ClearData?.Invoke();
    }

    public enum GetChannelSource
    {
        FreeList,
        Allocate
    }

    public static GetChannelSource ResolveGetChannelSource(object? freeHead) =>
        freeHead != null ? GetChannelSource.FreeList : GetChannelSource.Allocate;

    public static bool ShouldPersistActiveChannel(int chanFlags) =>
        (chanFlags & (ChanfForgettable | ChanfUi | ChanfTransient)) == 0;

    public static int CountPersistedActiveChannels(IEnumerable<int> activeFlags) =>
        activeFlags.Count(ShouldPersistActiveChannel);

    public static void UnlinkChannel(object chan, object? next, Action<object?, object?> assignNext, Action<object?, object?> assignPrev)
    {
        assignNext(null, next);
        if (next != null)
            assignPrev(next, null);
    }

    public static void LinkChannel(object chan, object? head, Action<object?, object?> setNext, Action<object?, object?> setPrevOnHead)
    {
        setNext(chan, head);
        if (head != null)
            setPrevOnHead(head, chan);
    }

    public static bool ListChannelsShouldInclude(int chanFlags) =>
        (chanFlags & VmSoundEngineRestartChannel.ChanfEvicted) == 0;

    public static bool CacheSoundShouldAbort(bool backendPresent, bool tentative) =>
        !backendPresent || tentative;

    public static int SumPersistedChannels(IReadOnlyList<int> flags)
    {
        var total = 0;
        for (var i = 0; i < flags.Count; i++)
        {
            if (ShouldPersistActiveChannel(flags[i]))
                total++;
        }

        return total;
    }
}
