namespace HCDE.Client;

/// <summary>Debug/restart CCMDs and window title helpers from <c>d_main.cpp</c> (2275–2309, 4890–5108).</summary>
public static class VmDMainDebugCcmds
{
    public const char TextColorEscape = '\x1c';

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

    public static int ResolveFriendlyTitleCase(int friendlyTitle, bool levelNameNonEmpty) =>
        friendlyTitle switch
        {
            1 when levelNameNonEmpty => 1,
            1 => 2,
            2 => 2,
            _ => 0
        };

    public static string? ComposeTitleString(int friendlyTitle, string levelName, string gameName)
    {
        return ResolveFriendlyTitleCase(friendlyTitle, levelName.Length > 0) switch
        {
            1 => $"{levelName} - {gameName}",
            2 => gameName,
            _ => null
        };
    }

    public static string StripTitleColorEscapes(string title)
    {
        Span<char> buffer = stackalloc char[title.Length];
        var dst = 0;
        var src = 0;
        while (src < title.Length)
        {
            if (title[src] != TextColorEscape)
            {
                buffer[dst++] = title[src++];
                continue;
            }

            if (src + 1 < title.Length && title[src + 1] == '[')
            {
                src += 2;
                while (src < title.Length && title[src] != ']')
                    src++;
                if (src < title.Length && title[src] == ']')
                    src++;
                continue;
            }

            if (src + 1 < title.Length)
                src += 2;
            else
                break;
        }

        return new string(buffer[..dst]);
    }

    public static string FormatBrandedWindowTitle(string strippedTitle) =>
        strippedTitle.Length > 0 ? $"HCDE - {strippedTitle}" : "HCDE";

    public static string BuildWindowTitle(int friendlyTitle, string levelName, string gameName)
    {
        var raw = ComposeTitleString(friendlyTitle, levelName, gameName);
        if (raw == null)
            return string.Empty;
        var stripped = StripTitleColorEscapes(raw);
        return FormatBrandedWindowTitle(stripped);
    }

    public static bool ShouldMarkFsDirEntryHidden(int findFileIndex, int entryIndex) =>
        findFileIndex != entryIndex;

    public static bool ShouldPrintTypeLump(int argc) => argc >= 2;

    public static int ResolveTypeLumpIndex(int lumpNum) => lumpNum;

    public static IEnumerable<string> MultiExecFileList(IReadOnlyList<string> argList)
    {
        for (var i = 0; i < argList.Count; i++)
            yield return argList[i];
    }

    public static int GetCmdLineFilesStartIndex(bool optional, int existingWadCount) =>
        optional ? 0 : existingWadCount;

    public static bool ShouldClearOptionalFlagAfterAdd(bool optional, int addedCount) =>
        optional && addedCount > 0;

    public static int CountNewWadSlots(int sizeBefore, int sizeAfter) => sizeAfter - sizeBefore;
}
