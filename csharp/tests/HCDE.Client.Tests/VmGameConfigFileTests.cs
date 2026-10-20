namespace HCDE.Client.Tests;

public class VmGameConfigFileTests
{
    [Fact]
    public void Constructor_CreatesDefaultIwadSearchPaths()
    {
        var config = new VmGameConfigFile(["$PROGDIR", "/games/doom"]);
        Assert.True(config.SetSection("IWADSearch.Directories"));
        Assert.Equal(".", config.GetValueForKey("Path"));
    }

    [Fact]
    public void WriteGameHeader_IncludesVersion()
    {
        var config = new VmGameConfigFile([]);
        Assert.Contains("HCDE", config.WriteGameHeader("1.0", "2026-01-01"));
    }
}
