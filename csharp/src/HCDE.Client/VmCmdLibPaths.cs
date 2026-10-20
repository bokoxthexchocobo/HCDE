namespace HCDE.Client;

/// <summary>Native <c>cmdlib.cpp</c> path helpers.</summary>
public static class VmCmdLibPaths
{
    public static void FixPathSeparator(Span<char> path)
    {
        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] == '\\')
                path[index] = '/';
        }
    }

    public static string FixPathSeparator(string path)
    {
        var chars = path.ToCharArray();
        FixPathSeparator(chars);
        return new string(chars);
    }

    public static void DefaultExtension(ref string path, string extension, bool forceBackslash = false)
    {
        ArgumentNullException.ThrowIfNull(extension);
        var index = path.Length - 1;
        while (index >= 0 && !IsSeparator(path[index], forceBackslash))
        {
            if (path[index] == '.')
                return;
            index--;
        }

        path += extension;
    }

    public static string ExtractFilePath(string path, bool forceBackslash = false)
    {
        var index = path.Length;
        while (index > 0 && !IsSeparator(path[index - 1], forceBackslash))
            index--;
        return path[..index];
    }

    public static string ExtractFileBase(string path, bool includeExtension = false, bool forceBackslash = false)
    {
        var end = path.Length;
        var start = end;
        while (start > 0 && !IsSeparator(path[start - 1], forceBackslash))
            start--;
        if (start == 0 && end > 0 && end > 1 && path[1] == ':')
            start = 2;
        var file = path[start..];
        if (!includeExtension)
        {
            var dot = file.LastIndexOf('.');
            if (dot >= 0)
                return file[..dot];
        }

        return file;
    }

    public static void SplitPath(string path, out string directory, out string baseName, out string extension, bool forceBackslash = false)
    {
        directory = ExtractFilePath(path, forceBackslash);
        var file = ExtractFileBase(path, includeExtension: true, forceBackslash);
        var dot = file.LastIndexOf('.');
        if (dot < 0)
        {
            baseName = file;
            extension = "";
        }
        else
        {
            baseName = file[..dot];
            extension = file[dot..];
        }
    }

    private static bool IsSeparator(char character, bool forceBackslash) =>
        character == '/' || character == '\\' && (OperatingSystem.IsWindows() || forceBackslash);
}
