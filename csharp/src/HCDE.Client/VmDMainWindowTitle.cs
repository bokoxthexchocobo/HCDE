namespace HCDE.Client;

/// <summary><c>I_UpdateWindowTitle</c>, <c>fs_dir</c>, and <c>type</c> from <c>d_main.cpp</c>.</summary>
public static partial class VmDMainDebugCcmds
{
    public const char TextColorEscape = '\x1c';

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

    public static bool ShouldSkipDefaultWindowTitle(int friendlyTitle) =>
        ResolveFriendlyTitleCase(friendlyTitle, false) == 0;

    public static bool ShouldMarkFsDirEntryHidden(int findFileIndex, int entryIndex) =>
        findFileIndex != entryIndex;

    public static bool ShouldPrintTypeLump(int argc) => argc >= 2;

    public static int ResolveTypeLumpIndex(int lumpNum) => lumpNum;
}
