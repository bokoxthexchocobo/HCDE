namespace HCDE.Client;

/// <summary><c>FileSystemPrintf</c> and <c>D_InitGame</c> header from <c>d_main.cpp</c> (3820–3996).</summary>
public static class VmDMainInitFs
{
    public enum FsLevel
    {
        Error,
        Warning,
        Attention,
        Message,
        DebugWarn,
        DebugNotify,
    }

    public const int GameRaven = 1;
    public const int GameDoomStrifeChex = 2;
    public const int GameDoomChex = 4;
    public const int GameAny = 0;

    public readonly record struct FsPrintfRoute(bool UseRed, bool UseYellow, bool UseBlue, bool PlainPrintf, bool DebugWarn, bool DebugNotify);

    public static FsLevel MapFsLevel(int raw) => (FsLevel)Math.Clamp(raw, 0, 5);

    public static FsPrintfRoute RouteFsPrintf(FsLevel level) => level switch
    {
        FsLevel.Error => new(true, false, false, false, false, false),
        FsLevel.Warning => new(false, true, false, false, false, false),
        FsLevel.Attention => new(false, false, true, false, false, false),
        FsLevel.Message => new(false, false, false, true, false, false),
        FsLevel.DebugWarn => new(false, false, false, false, true, false),
        FsLevel.DebugNotify => new(false, false, false, false, false, true),
        _ => new(false, false, false, true, false, false),
    };

    public static bool FsPrintfError(FsLevel level) => level == FsLevel.Error;

    public static bool FsPrintfWarning(FsLevel level) => level == FsLevel.Warning;

    public static bool FsPrintfAttention(FsLevel level) => level == FsLevel.Attention;

    public static int FsPrintfReturnLength(int textLen) => textLen;

    public static bool DInitRequiresIwad(object? iwad) => iwad != null;

    public static bool DedicatedSkipsClientFramebuffer(bool dedicated) => dedicated;

    public static bool DebugRunDisablesVmJit(bool debug) => debug;

    public static bool RestartSkipsEarlyFramebuffer(bool restart) => restart;

    public static bool ShouldInitClientFramebuffer(bool restart, bool dedicated) =>
        !restart && !dedicated;

    public static bool RngSeedFromArg(string? arg) => arg != null;

    public static int ParseRngSeed(string arg) => int.TryParse(arg, out var v) ? v : 0;

    public static bool UseStaticRng(bool fromArg) => fromArg;

    public static bool BatchRunQuiet(bool batch) => batch;

    public static bool NoAutoexec(bool flag) => flag;

    public static bool ExecListChained(object? prior, object? next) => prior != null || next != null;

    public static bool AllowDuplicateWads(bool flag) => flag;

    public static bool WadInitSucceeded(bool ok) => ok;

    public static bool WadInitFatal(bool ok) => !ok;

    public readonly record struct BlanketFilter(int Match, string Name);

    public static BlanketFilter[] DefaultBlanketFilters() =>
    [
        new(GameRaven, "game-Raven"),
        new(GameDoomStrifeChex, "game-DoomStrifeChex"),
        new(GameDoomChex, "game-DoomChex"),
        new(GameAny, string.Empty),
    ];

    public static bool BlanketMatches(int gameType, int match) => match == GameAny || (gameType & match) != 0;

    public static IReadOnlyList<string> BuildGameTypeFilters(int gameType, string gameTypeName, string lumpFilterIwad)
    {
        var list = new List<string>();
        foreach (var inf in DefaultBlanketFilters())
        {
            if (BlanketMatches(gameType, inf.Match) && inf.Name.Length > 0)
                list.Add(inf.Name);
        }
        list.Add(GameTypeFilterName(gameTypeName));
        list.Add(lumpFilterIwad);
        if (DoomIdDoomWorkaround(lumpFilterIwad, out var replacement))
            list.Add(replacement);
        return list;
    }

    public static string GameTypeFilterName(string gameTypeName) => $"game-{gameTypeName}";

    public static bool DoomIdDoomWorkaround(string iwadFilter, out string replacement)
    {
        if (iwadFilter.Contains("doom.id.doom", StringComparison.Ordinal))
        {
            replacement = "doom.doom" + iwadFilter.Substring(12);
            return true;
        }
        replacement = string.Empty;
        return false;
    }

    public static bool PostProcessRenames(bool hasFs) => hasFs;

    public static bool ClearAllWadsAfterInit(bool ok) => ok;

    public static bool StartupStatusWadScan(bool batch) => !batch;

    public static bool PrintWInit(bool batch) => !batch;

    public static string DedicatedSoundInitBanner(bool dedicated) =>
        dedicated ? "S_Init: Setting up server sound tables.\n" : "S_Init: Setting up sound.\n";

    public static int StartupTextureProgressGuess(int texGuess)
    {
        if (texGuess > 0)
            return Math.Max(texGuess * 4, 4096);
        return 4096;
    }

    public static bool NorunFromArgs(bool norun) => norun;

    public static bool NoStartScreen(
        bool batch,
        bool restart,
        bool join,
        bool dedicatedJoin,
        bool host,
        bool dedicated,
        bool norun) =>
        batch || restart || join || dedicatedJoin || host || dedicated || norun;

    public static bool MergePwadsIntoAll(bool ok) => ok;

    public static bool InitMultipleFilesFatal(bool ok) => !ok;

    public static bool ParseCvarsAfterWadLoad(bool ok) => ok;

    public static bool GrabCvarDefaultsAfterMount(bool ok) => ok;

    public static bool InitPaletteAfterCvars(bool ok) => ok;
}
