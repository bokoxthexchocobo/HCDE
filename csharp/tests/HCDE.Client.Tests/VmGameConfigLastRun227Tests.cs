namespace HCDE.Client.Tests;

public class VmGameConfigLastRun227Tests
{
    [Fact]
    public void EnsureSearchPaths_AddsMissingPathsForLegacyVersion()
    {
        var config = new VmConfigFile();
        config.LoadFromText("[LastRun]\nVersion=226\n[IWADSearch.Directories]\nPath=.\n");
        VmGameConfigLastRun227.EnsureSearchPathsForLegacyConfig(config, 226, ["$PROGDIR"]);
        Assert.True(config.SetSection("IWADSearch.Directories"));
        var paths = new List<string>();
        while (config.NextInSection(out var key, out var value) && key == "Path") paths.Add(value);
        Assert.Contains("$PROGDIR", paths);
    }
}
