namespace HCDE.Client.Tests;

public class VmSearchConfigurationDiscoveryTests
{
    [Fact]
    public void ExistingConfigurationPriorityIsPortableUserThenTemplate()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var program = Directory.CreateDirectory(Path.Combine(root, "program")).FullName;
        var documents = Directory.CreateDirectory(Path.Combine(root, "documents")).FullName;
        try
        {
            Assert.Null(VmSearchConfigurationDiscovery.FindWindows(program, documents));
            var template = Path.Combine(program, "hcde.ini");
            File.WriteAllText(template, "");
            Assert.Equal(template, VmSearchConfigurationDiscovery.FindWindows(program, documents));
            var userDirectory = Directory.CreateDirectory(Path.Combine(documents, "My Games", "HCDE")).FullName;
            var user = Path.Combine(userDirectory, "hcde.ini");
            File.WriteAllText(user, "");
            Assert.Equal(user, VmSearchConfigurationDiscovery.FindWindows(program, documents));
            var portable = Path.Combine(program, "hcde_portable.ini");
            File.WriteAllText(portable, "");
            Assert.Equal(portable, VmSearchConfigurationDiscovery.FindWindows(program, documents));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void DiscoveryOptionRoundTripsWithExplicitOverride()
    {
        Assert.True(LauncherPlan.TryParse(["--iwad", "game.wad", "--discover-search-config", "--search-config", "explicit.ini"], out var selection, out _));
        Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
        Assert.True(copy.DiscoverSearchPathConfig);
        Assert.Equal("explicit.ini", copy.SearchPathConfigPath);
        Assert.False(new LauncherSelection().DiscoverSearchPathConfig);
    }
}
