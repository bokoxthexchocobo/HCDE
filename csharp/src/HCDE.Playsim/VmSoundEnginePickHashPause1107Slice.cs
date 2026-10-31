namespace HCDE.Playsim;

/// <summary>
/// `GetMSLength` tail seam, `PickReplacement`, hash/random registration, reset, cache CCMD, and pause/resume
/// from <c>s_sound.cpp</c> (**1651–1800**). Chunk **1107**.
/// </summary>
public static class VmSoundEnginePickHashPause1107Slice
{
    public const int NoSound = -1;

    public sealed class PickReplacementContext
    {
        public int RefId { get; set; }
        public Func<int, bool>? IsRandomHeader { get; init; }
        public Func<int, int>? RandomListIndex { get; init; }
        public Func<int, int>? ChoiceCount { get; init; }
        public Func<int, int, int>? ChoiceAt { get; init; }
        public Func<int, int>? RandomIndex { get; init; }
    }

    public static int RunPickReplacement(PickReplacementContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        while (ctx.IsRandomHeader?.Invoke(ctx.RefId) == true)
        {
            var listIndex = ctx.RandomListIndex?.Invoke(ctx.RefId) ?? 0;
            var count = ctx.ChoiceCount?.Invoke(listIndex) ?? 0;
            if (count <= 0)
                break;
            var pick = ctx.RandomIndex?.Invoke(count) ?? 0;
            ctx.RefId = ctx.ChoiceAt?.Invoke(listIndex, pick) ?? ctx.RefId;
        }

        return ctx.RefId;
    }

    public sealed class HashSoundsContext
    {
        public int SfxCount { get; init; }
        public Func<int, string>? ReadName { get; init; }
        public Func<int, int>? ReadResourceId { get; init; }
        public Action<int, string>? InsertName { get; init; }
        public Action<int, int>? InsertResource { get; init; }
        public Action? ShrinkMaps { get; init; }
    }

    public static void RunHashSounds(HashSoundsContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.ShrinkMaps?.Invoke();
        for (var i = 1; i < ctx.SfxCount; i++)
        {
            var name = ctx.ReadName?.Invoke(i) ?? string.Empty;
            ctx.InsertName?.Invoke(i, name);
            var resid = ctx.ReadResourceId?.Invoke(i) ?? NoSound;
            if (resid != NoSound)
                ctx.InsertResource?.Invoke(i, resid);
        }
    }

    public sealed class AddRandomSoundContext
    {
        public int OwnerIndex { get; init; }
        public IReadOnlyList<int> Choices { get; init; } = Array.Empty<int>();
        public Action<int, IReadOnlyList<int>>? ReserveList { get; init; }
        public Action<int, int, bool>? MarkOwnerRandom { get; init; }
    }

    public static void RunAddRandomSound(AddRandomSoundContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.ReserveList?.Invoke(ctx.OwnerIndex, ctx.Choices);
        ctx.MarkOwnerRandom?.Invoke(ctx.OwnerIndex, ctx.Choices.Count, true);
    }

    public sealed class SoundResetContext
    {
        public Action? StopMusic { get; init; }
        public Action? ResetEngine { get; init; }
        public Action? RestartMusic { get; init; }
    }

    public static void RunSoundReset(SoundResetContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.StopMusic?.Invoke();
        ctx.ResetEngine?.Invoke();
        ctx.RestartMusic?.Invoke();
    }

    public sealed class CacheSoundCommandContext
    {
        public int ArgCount { get; init; }
        public Func<int, string>? ReadArg { get; init; }
        public Func<string, int>? FindSound { get; init; }
        public Action<int>? CacheSound { get; init; }
        public Action<string>? PrintUsage { get; init; }
    }

    public static void RunCacheSoundCommand(CacheSoundCommandContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.ArgCount < 2)
        {
            ctx.PrintUsage?.Invoke("Usage: cachesound <sound> ...\n");
            return;
        }

        for (var i = 1; i < ctx.ArgCount; i++)
        {
            var arg = ctx.ReadArg?.Invoke(i) ?? string.Empty;
            var sfx = ctx.FindSound?.Invoke(arg) ?? NoSound;
            if (sfx != NoSound)
                ctx.CacheSound?.Invoke(sfx);
        }
    }

    public static void RunPauseSound(bool notMusic, bool notSfx, Action? pauseMusic, Action? setPaused, Action? setSfxPaused)
    {
        if (!notMusic)
            pauseMusic?.Invoke();
        if (!notSfx)
        {
            setPaused?.Invoke();
            setSfxPaused?.Invoke();
        }
    }

    public static void RunResumeSound(bool notSfx, Action? resumeMusic, Action? setPaused, Action? setSfxPaused)
    {
        resumeMusic?.Invoke();
        if (!notSfx)
        {
            setPaused?.Invoke();
            setSfxPaused?.Invoke();
        }
    }
}
