namespace HCDE.Client;

/// <summary>Debug/restart CCMDs and window title helpers from <c>d_main.cpp</c> (4890–5108).</summary>
public static class VmDMainDebugCcmds
{
    public const char TextColorEscape = '\x1c';

    public static bool ShouldIgnoreDebugRestart(bool netGame, bool demoPlayback) =>
        netGame && !demoPlayback;

    public static bool ShouldSetWantToRestart(bool netGame, bool demoPlayback) =>
        !ShouldIgnoreDebugRestart(netGame, demoPlayback);

    public static bool ShouldLogCleanupDuringNetgame(bool netGame, bool demoPlayback) =>
        netGame && !demoPlayback;

    public static int ResolveFriendlyTitleCase(int friendlyTitle, bool levelNameNonEmpty) =>
        friendlyTitle switch
        {
            1 when levelNameNonEmpty => 1,
            1 => 2,
            2 => 2,
            _ => 0
        };

    public static bool ShouldStripColorEscape(char c) => c != TextColorEscape;

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

    public static bool ShouldMarkFsDirEntryHidden(int findFileIndex, int entryIndex) =>
        findFileIndex != entryIndex;

    public static bool ShouldPrintTypeLump(int argc) => argc >= 2;

    public static int ResolveTypeLumpIndex(int lumpNum) => lumpNum;

    public static int AppendRemainingArgv(int argc) => argc > 1 ? argc - 1 : 0;

    public static bool ShouldForceOptionalFilesNonOptional(bool optionalRequest, int addedCount) =>
        optionalRequest && addedCount > 0;
}
