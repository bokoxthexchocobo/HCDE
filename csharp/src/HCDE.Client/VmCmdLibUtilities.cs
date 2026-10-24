namespace HCDE.Client;

/// <summary>Remaining native <c>cmdlib.cpp</c> string/path helpers.</summary>
public static class VmCmdLibUtilities
{
    public static string StripExtension(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        var index = path.Length - 1;
        while (index >= 0 && !VmCmdLibPaths.IsPathSeparator(path[index]))
        {
            if (path[index] == '.') return path[..index];
            index--;
        }

        return path;
    }

    public static bool IsNum(string str)
    {
        if (string.IsNullOrEmpty(str)) return true;
        foreach (var ch in str)
        {
            if (ch is not (>= '0' and <= '9') and not '-') return false;
        }

        return true;
    }

    public static bool CheckWildcards(string? pattern, string? text) =>
        CheckWildcardsRecursive(pattern, text);

    private static bool CheckWildcardsRecursive(string? pattern, string? text)
    {
        if (pattern is null || text is null) return true;
        var p = 0;
        var t = 0;
        while (p < pattern.Length)
        {
            if (pattern[p] == '*')
            {
                p++;
                var stop = p < pattern.Length ? char.ToLowerInvariant(pattern[p]) : '\0';
                while (t < text.Length && char.ToLowerInvariant(text[t]) != stop) t++;
                if (t < text.Length && char.ToLowerInvariant(text[t]) == stop)
                {
                    if (CheckWildcardsRecursive(pattern[p..], text[t..])) return true;
                    p--;
                }
            }
            else if (pattern[p] == '?' || t < text.Length && char.ToLowerInvariant(pattern[p]) == char.ToLowerInvariant(text[t]))
            {
                p++;
                t++;
            }
            else return false;
        }

        return p >= pattern.Length && t >= text.Length;
    }

    public static bool IsAbsPath(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (name[0] == '/' || name[0] == '\\') return true;
        return OperatingSystem.IsWindows() && name.Length > 1 && char.IsAsciiLetter(name[0]) && name[1] == ':';
    }

    public static string NormalizeFileName(string path)
    {
        path = VmCmdLibPaths.FixPathSeparator(path);
        var splits = path.Split('/', StringSplitOptions.None).ToList();
        for (var i = 1; i < splits.Count; i++)
        {
            if (splits[i] == "." || splits[i].Length == 0)
            {
                splits.RemoveAt(i);
                i--;
            }
            else if (splits[i] == ".." && splits[i - 1] != "..")
            {
                splits.RemoveAt(i);
                splits.RemoveAt(i - 1);
                i -= 2;
                if (i < 0) i = 0;
            }
        }

        return string.Join('/', splits);
    }

    public static void UpperCopy(Span<char> to, ReadOnlySpan<char> from)
    {
        var length = Math.Min(to.Length, from.Length);
        for (var i = 0; i < length; i++)
            to[i] = char.ToUpperInvariant(from[i]);
    }
}
