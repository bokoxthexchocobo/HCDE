namespace HCDE.Gamedata;

/// <summary><c>DoInclude</c> path parsing from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedIncludeDirective
{
    public static bool TryParseNotextFlag(ref string line2, out bool noText)
    {
        noText = false;
        if (line2.Length < 6) return true;
        if (!line2.StartsWith("notext", StringComparison.OrdinalIgnoreCase)) return true;
        var rest = line2[6..];
        if (rest.Length > 0 && !char.IsWhiteSpace(rest[0])) return true;
        noText = true;
        line2 = rest.TrimStart();
        return true;
    }

    public static bool TryUnquoteIncludePath(string line2, out string path)
    {
        path = line2.Trim();
        if (path.Length >= 2 && path[0] == '"')
        {
            var end = path.IndexOf('"', 1);
            if (end > 0)
            {
                path = path[1..end];
                return path.Length > 0;
            }
        }

        return path.Length > 0;
    }

    public static string? TryCombinePatchDirectory(string patchName, string includeFile)
    {
        var slash = patchName.LastIndexOf('/');
        if (slash < 0) return null;
        return patchName[..(slash + 1)] + includeFile;
    }
}
