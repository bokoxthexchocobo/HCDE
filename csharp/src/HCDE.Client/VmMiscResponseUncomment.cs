namespace HCDE.Client;

/// <summary><c>M_FindResponseFile</c> helper <c>Uncomment</c> from <c>m_misc.cpp</c>.</summary>
public static class VmMiscResponseUncomment
{
    private const string CommentToken = "#";

    [Flags]
    public enum UncommentFlags
    {
        None = 0,
        StripWhitespace = 1,
        SkipEmpty = 2,
        AllowTrailing = 4,
    }

    public static string Uncomment(string from, UncommentFlags flags = UncommentFlags.None)
    {
        var stripWhitespace = flags.HasFlag(UncommentFlags.StripWhitespace);
        var skipEmpty = flags.HasFlag(UncommentFlags.SkipEmpty);
        var allowTrailing = flags.HasFlag(UncommentFlags.AllowTrailing);
        var builder = new System.Text.StringBuilder();
        foreach (var line in from.Split('\n'))
        {
            var cidx = line.IndexOf(CommentToken, StringComparison.Ordinal);
            if (cidx == 0) continue;
            if (!allowTrailing && cidx >= 0)
            {
                var start = 0;
                while (start < cidx && char.IsWhiteSpace(line[start])) start++;
                if (start != cidx) cidx = -1;
            }

            var substring = cidx >= 0 ? line[..cidx] : line;
            var end = substring.Length;
            if (skipEmpty && end == 0) continue;
            var trimStart = 0;
            if (stripWhitespace)
            {
                while (trimStart < end && char.IsWhiteSpace(substring[trimStart])) trimStart++;
                while (end > trimStart && char.IsWhiteSpace(substring[end - 1])) end--;
            }

            var trimmed = substring[trimStart..end];
            if (skipEmpty && trimmed.Length == 0) continue;
            if (trimmed.Length > 0) builder.Append(trimmed).Append(' ');
        }

        return builder.ToString();
    }
}
