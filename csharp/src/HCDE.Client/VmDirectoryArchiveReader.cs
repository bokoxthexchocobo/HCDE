namespace HCDE.Client;

internal static class VmDirectoryArchiveReader
{
    public static IReadOnlyList<(string Name, string FilePath)> Read(string path, Func<string, string, bool>? filenameCheck = null,
        Action<string>? warning = null)
    {
        var root = Path.GetFullPath(path);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
        var entries = new List<(string, string)>();
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.ReparsePoint
        };
        foreach (var file in Directory.EnumerateFiles(root, "*", options))
        {
            var name = Path.GetFileName(file).ToLowerInvariant();
            if (name.Contains(".orig", StringComparison.Ordinal) || name.Contains(".bak", StringComparison.Ordinal) ||
                name.Contains(".cache", StringComparison.Ordinal)) continue;
            if (filenameCheck is not null && !filenameCheck(name, file)) continue;
            if (new FileInfo(file).Length > int.MaxValue)
            {
                var message = $"{file} is larger than 2GB and will be ignored";
                if (warning is not null) warning(message);
                else Console.Error.WriteLine(message);
                continue;
            }
            entries.Add((Path.GetRelativePath(root, file), file));
        }
        return entries.AsReadOnly();
    }
}
