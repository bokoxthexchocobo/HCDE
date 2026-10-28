namespace HCDE.Client;

/// <summary><c>debug_restart</c> and cleanup warn from <c>d_main.cpp</c>.</summary>
public static partial class VmDMainDebugCcmds
{
    public static readonly string[] DebugRestartStripArgKinds =
    {
        "iwad",
        "deh",
        "bex",
        "playdemo",
        "file",
        "optfile",
        "altdeath",
        "deathmatch",
        "coop",
        "skill",
        "savedir",
        "xlat",
        "oldsprites"
    };

    public static bool ShouldIgnoreDebugRestart(bool netGame, bool demoPlayback) =>
        netGame && !demoPlayback;

    public static bool ShouldSetWantToRestart(bool netGame, bool demoPlayback) =>
        !ShouldIgnoreDebugRestart(netGame, demoPlayback);

    public static bool ShouldLogCleanupDuringNetgame(bool netGame, bool demoPlayback) =>
        netGame && !demoPlayback;

    public static IReadOnlyList<string> CollectRestartArgvTail(IReadOnlyList<string> argv)
    {
        if (argv.Count <= 1)
            return Array.Empty<string>();
        var tail = new string[argv.Count - 1];
        for (var i = 1; i < argv.Count; i++)
            tail[i - 1] = argv[i];
        return tail;
    }

    public static List<string> StripArgsMatchingKinds(
        IReadOnlyList<string> argv,
        IReadOnlyList<string> stripKinds)
    {
        var kept = new List<string> { argv[0] };
        for (var i = 1; i < argv.Count; i++)
        {
            var token = argv[i];
            if (stripKinds.Any(k => token.Equals(k, StringComparison.OrdinalIgnoreCase)))
            {
                if (i + 1 < argv.Count)
                    i++;
                continue;
            }

            kept.Add(token);
        }

        return kept;
    }
}
