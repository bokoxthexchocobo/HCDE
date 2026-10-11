namespace HCDE.Client;

public static class VmIwadRequiredResource
{
    public static string? ResolveWad(VmIwadInfoEntry selected, VmIwadInfoCatalog catalog,
        string selectedPath, string? explicitPath = null)
        => Resolve(selected, catalog, selectedPath, explicitPath);

    public static string? Resolve(VmIwadInfoEntry selected, VmIwadInfoCatalog catalog,
        string selectedPath, string? explicitPath = null,
        IReadOnlyList<string>? searchDirectories = null, IReadOnlyList<string>? recursiveDirectories = null)
    {
        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        var required = selected.Properties.TryGetValue("required", out var values) ? values[0] : "";
        if (required.Length == 0)
        {
            if (explicitPath is not null) throw new InvalidDataException("Selected IWAD has no Required dependency");
            return null;
        }
        var definitions = catalog.Entries.Select(entry => entry.Definition).ToArray();
        var directory = Path.GetDirectoryName(Path.GetFullPath(selectedPath))!;
        var candidates = explicitPath is not null
            ? new[] { explicitPath }
            : FindCandidates().Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in candidates)
        {
            if (explicitPath is null && !File.Exists(candidate)) continue;
            var bytes = File.ReadAllBytes(candidate);
            VmIwadDefinition? identified;
            try
            {
                var archive = new VmScriptArchive(candidate, bytes);
                var extension = Path.GetExtension(candidate).ToLowerInvariant();
                if (extension is ".iwad" or ".ipk3" or ".ipk7")
                {
                    identified = VmIwadInfoParser.ReadEmbeddedArchive(archive).Definition;
                    // Native CheckIWADInfo reuses an existing catalog record with the same name.
                    identified = definitions.FirstOrDefault(definition => definition.Name.Equals(identified.Name, StringComparison.Ordinal)) ?? identified;
                }
                else identified = VmIwadIdentifier.IdentifyArchive(definitions, archive);
            }
            catch (InvalidDataException error) when (explicitPath is null)
            {
                Console.Error.WriteLine(candidate + ": " + error.Message);
                continue;
            }
            // Native Required compares display names case-sensitively, not Autoname or filename.
            if (identified?.Name.Equals(required, StringComparison.Ordinal) == true)
                return Path.GetFullPath(candidate);
        }
        throw new InvalidDataException("Required IWAD '" + required + "' was not identified");

        IEnumerable<string> FindCandidates()
        {
            foreach (var candidate in VmIwadCandidateScanner.Scan(directory, catalog.Names)) yield return candidate;
            foreach (var path in searchDirectories ?? [])
                foreach (var candidate in VmIwadCandidateScanner.Scan(path, catalog.Names)) yield return candidate;
            foreach (var path in recursiveDirectories ?? [])
                foreach (var candidate in VmIwadCandidateScanner.Scan(path, catalog.Names, recursive: true)) yield return candidate;
        }
    }
}
