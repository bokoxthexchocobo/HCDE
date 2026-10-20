namespace HCDE.Client;

/// <summary>Path helpers from <c>findfile.cpp</c> (<c>D_AddFile</c> / wildcard filter).</summary>
public static class VmFindFileWildcard
{
    public static string NormalizeArchivePath(string file) =>
        file.Replace('\\', '/');

    public static bool ContainsDuplicate(IReadOnlyList<string> existingNames, string normalizedPath) =>
        existingNames.Any(e => string.Equals(e, normalizedPath, StringComparison.OrdinalIgnoreCase));

    public static string ApplyWildcardExtensionFilter(string name, string? extension)
    {
        if (string.IsNullOrEmpty(extension) || string.IsNullOrEmpty(name))
            return name;
        if (name == "*" || name == "*.*")
            return extension;
        if (!name.Contains('.'))
        {
            var dot = extension.IndexOf('.');
            return name + (dot >= 0 ? extension[dot..] : extension);
        }

        return name;
    }
}
