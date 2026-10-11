namespace HCDE.Client;

public static class VmIwadSupportResource
{
    public static string? Resolve(VmIwadInfoEntry selected, IReadOnlyList<string> searchDirectories,
        bool enabled = true, bool networkGame = false, IReadOnlyList<string>? recursiveDirectories = null)
    {
        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(searchDirectories);
        // Native automatic support loading is disabled for host/join launches.
        if (!enabled || networkGame || !selected.Properties.TryGetValue("supportwad", out var values)
            || values[0].Length == 0) return null;
        foreach (var directory in searchDirectories)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);
            var candidate = FindInDirectory(directory);
            if (candidate is not null) return candidate;
        }
        foreach (var directory in recursiveDirectories ?? [])
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);
            var candidate = FindInDirectory(directory);
            if (candidate is not null) return candidate;
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint,
            };
            foreach (var child in Directory.EnumerateDirectories(directory, "*", options))
            {
                candidate = FindInDirectory(child);
                if (candidate is not null) return candidate;
            }
        }
        return null;

        string? FindInDirectory(string directory)
        {
            var candidate = directory.Replace('\\', '/').TrimEnd('/') + "/" + values[0].Replace('\\', '/');
            return File.Exists(candidate) ? Path.GetFullPath(candidate) : null;
        }
    }
}
