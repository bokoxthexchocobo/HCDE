namespace HCDE.Client;

public static class VmIwadCompanionResources
{
    public static IReadOnlyList<string> Resolve(VmIwadInfoEntry entry, string iwadPath,
        Func<string, bool, string?>? baseFileSearch = null, Action<string>? warning = null,
        IReadOnlyList<string>? searchDirectories = null, IReadOnlyList<string>? recursiveDirectories = null,
        IReadOnlyList<VmFileSearchPath>? orderedSearchPaths = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(iwadPath);
        if (!entry.Properties.TryGetValue("load", out var loads)) return Array.Empty<string>();
        var directory = Path.GetDirectoryName(Path.GetFullPath(iwadPath))!;
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var load in loads)
        {
            if (string.IsNullOrEmpty(load)) throw new InvalidDataException("IWADINFO Load entry is empty");
            var programFirst = load[0] == ':';
            var requested = programFirst ? load[1..] : directory + "/" + load.Replace('\\', '/');
            if (requested.Length == 0) throw new InvalidDataException("IWADINFO Load entry has no resource name");
            var found = baseFileSearch is null ? Search(requested, programFirst, searchDirectories, recursiveDirectories, orderedSearchPaths) : baseFileSearch(requested, programFirst);
            if (found is null && !programFirst && Path.GetExtension(requested).Length == 0)
                found = baseFileSearch is null ? Search(requested + ".wad", false, searchDirectories, recursiveDirectories, orderedSearchPaths) : baseFileSearch(requested + ".wad", false);
            if (found is null)
            {
                var message = "Can't find IWADINFO Load resource '" + requested + "'";
                if (warning is null) Console.Error.WriteLine(message); else warning(message);
                continue;
            }
            var path = Path.GetFullPath(found);
            if (seen.Add(path.Replace('\\', '/'))) result.Add(path);
        }
        return result.AsReadOnly();
    }

    private static string? Search(string requested, bool programFirst,
        IReadOnlyList<string>? searchDirectories, IReadOnlyList<string>? recursiveDirectories,
        IReadOnlyList<VmFileSearchPath>? orderedSearchPaths)
    {
        static bool Exists(string path) => File.Exists(path) || Directory.Exists(path);
        if (programFirst)
        {
            var programPath = AppContext.BaseDirectory.Replace('\\', '/').TrimEnd('/') + "/" + requested;
            if (Exists(programPath)) return programPath;
        }
        if (Exists(requested)) return requested;
        var paths = (searchDirectories ?? []).Select(path => new VmFileSearchPath(VmFileSearchPathKind.Path, path))
            .Concat((recursiveDirectories ?? []).Select(path => new VmFileSearchPath(VmFileSearchPathKind.RecursivePath, path)))
            .Concat(orderedSearchPaths ?? []);
        foreach (var entry in paths)
        {
            var directory = entry.Value;
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);
            // Native concatenation retains the complete requested path, including an IWAD directory prefix.
            var candidate = directory.Replace('\\', '/').TrimEnd('/') + "/" + requested.Replace('\\', '/');
            if (entry.Kind == VmFileSearchPathKind.Path)
            {
                if (Exists(candidate)) return candidate;
                continue;
            }
            if (entry.Kind != VmFileSearchPathKind.RecursivePath)
                throw new ArgumentException("Search paths must be expanded before lookup", nameof(orderedSearchPaths));
            if (File.Exists(candidate)) return candidate;
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true, IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint,
            };
            foreach (var child in Directory.EnumerateDirectories(directory, "*", options))
            {
                candidate = child.Replace('\\', '/').TrimEnd('/') + "/" + requested.Replace('\\', '/');
                if (File.Exists(candidate)) return candidate;
            }
        }
        return null;
    }
}
