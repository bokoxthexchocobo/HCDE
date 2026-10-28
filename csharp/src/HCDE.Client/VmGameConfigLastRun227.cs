namespace HCDE.Client;

/// <summary><c>FGameConfigFile</c> ctor migration when <c>LastRun.Version</c> &lt; 227.</summary>
public static class VmGameConfigLastRun227
{
    public static void EnsureSearchPathsForLegacyConfig(VmConfigFile config, double lastRunVersion, IReadOnlyList<string> defaultSearchPaths)
    {
        if (lastRunVersion >= 227) return;

        EnsurePaths(config, "IWADSearch.Directories", defaultSearchPaths, null);
        EnsurePaths(config, "FileSearch.Directories", defaultSearchPaths, null);
        EnsurePaths(config, "SoundfontSearch.Directories", defaultSearchPaths, suffix =>
        [
            $"{suffix}/soundfonts",
            $"{suffix}/fm_banks"
        ]);
    }

    private static void EnsurePaths(
        VmConfigFile config,
        string sectionName,
        IReadOnlyList<string> defaultSearchPaths,
        Func<string, IEnumerable<string>>? pathFactory)
    {
        if (!config.SetSection(sectionName)) return;
        foreach (var path in defaultSearchPaths)
        {
            if (pathFactory is null)
                config.EnsureValueForKey("Path", path);
            else
            {
                foreach (var expanded in pathFactory(path))
                    config.EnsureValueForKey("Path", expanded);
            }
        }
    }
}
