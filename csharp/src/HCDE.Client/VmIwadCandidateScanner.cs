namespace HCDE.Client;

public static class VmIwadCandidateScanner
{
    public static IReadOnlyList<string> Scan(string directory, IReadOnlyList<string> knownNames, bool recursive = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(knownNames);
        var names = knownNames.ToArray();
        foreach (var name in names) ArgumentNullException.ThrowIfNull(name);
        var candidates = new List<string>();
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.ReparsePoint,
        };
        foreach (var path in Directory.EnumerateFiles(Path.GetFullPath(directory), "*", options))
        {
            var leaf = Path.GetFileName(path);
            var extension = Path.GetExtension(leaf);
            if (extension.Equals(".iwad", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ipk3", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ipk7", StringComparison.OrdinalIgnoreCase)) candidates.Add(path);
            // Native discovery can add a special-extension file again through NAMES.
            foreach (var name in names)
                if (name.Equals(leaf, StringComparison.OrdinalIgnoreCase)) candidates.Add(path);
        }
        return candidates.AsReadOnly();
    }
}
