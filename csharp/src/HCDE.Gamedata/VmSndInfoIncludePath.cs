namespace HCDE.Gamedata;

/// <summary><c>S_ResolveIncludePath</c> from <c>s_advsound.cpp</c>.</summary>
public static class VmSndInfoIncludePath
{
    public static string Resolve(string includerFullPath, string includedFile)
    {
        var included = includedFile;
        if (included.StartsWith("./", StringComparison.Ordinal))
            included = included[2..];

        var incDir = "";
        var slash = includerFullPath.LastIndexOf('/');
        if (slash >= 0)
            incDir = includerFullPath[..slash];

        if (included.StartsWith("../", StringComparison.Ordinal))
        {
            var pathOk = true;
            var relative = included;
            while (relative.StartsWith("../", StringComparison.Ordinal))
            {
                relative = relative[3..];
                var parentSlash = incDir.LastIndexOf('/');
                if (parentSlash >= 0)
                    incDir = incDir[..parentSlash];
                else if (incDir.Length > 0)
                    incDir = "";
                else
                {
                    pathOk = false;
                    break;
                }
            }

            if (pathOk)
                return incDir.Length > 0 ? $"{incDir}/{relative}" : relative;
            return includedFile;
        }

        return incDir.Length > 0 ? $"{incDir}/{included}" : included;
    }
}
