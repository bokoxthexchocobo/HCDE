namespace HCDE.Client;

/// <summary>Native <c>DoAutoloadSetup</c> / <c>CreateStandardAutoExec</c> / <c>AddAutoexec</c>.</summary>
public static class VmGameConfigAutoload
{
    private static readonly (double MaxVersion, string Old, string New)[] LegacyRenames211 =
    [
        (211, "Chex3.Autoload", "chex.chex3.Autoload"),
        (211, "Doom2.Autoload", "doom.id.doom2.commercial.Autoload"),
        (211, "Freedoom.Autoload", "doom.freedoom.Autoload"),
    ];

    private static readonly (double MaxVersion, string Old, string New)[] LegacyRenames218 =
    [
        (218, "doom.doom2.commercial.Autoload", "doom.id.doom2.commercial.Autoload"),
    ];

    public static void ApplyLegacyRenames(VmConfigFile config, double lastRunVersion)
    {
        foreach (var (max, oldName, newName) in LegacyRenames211)
        {
            if (lastRunVersion < max) config.RenameSection(oldName, newName);
        }

        foreach (var (max, oldName, newName) in LegacyRenames218)
        {
            if (lastRunVersion < max) config.RenameSection(oldName, newName);
        }
    }

    public static void EnsureAutoloadHierarchy(VmConfigFile config, IEnumerable<string> autonames)
    {
        foreach (var autoname in autonames)
        {
            var work = autoname;
            while (!string.IsNullOrEmpty(work))
            {
                config.CreateSectionAtStart($"{work}.Autoload");
                var dot = work.LastIndexOf('.');
                if (dot < 0) break;
                work = work[..dot];
            }
        }

        config.CreateSectionAtStart("Global.Autoload");
    }

    public static void CreateStandardAutoExec(VmConfigFile config, string section, string defaultPath, bool moveToStart)
    {
        if (!config.SetSection(section))
        {
            config.SetSection(section, allowCreate: true);
            config.SetValueForKey("Path", defaultPath);
        }

        if (moveToStart) config.MoveSectionToStart(section);
    }

    public static IReadOnlyList<string> CollectAutoexecPaths(VmConfigFile config, string game, Func<string, bool> fileExists)
    {
        var section = $"{game}.AutoExec";
        var paths = new List<string>();
        if (!config.SetSection(section)) return paths;
        if (config.SectionIsEmpty()) return paths;
        while (config.NextInSection(out var key, out var value))
        {
            if (string.Equals(key, "Path", StringComparison.OrdinalIgnoreCase) && value.Length > 0 && fileExists(value))
                paths.Add(value);
        }

        return paths;
    }
}
