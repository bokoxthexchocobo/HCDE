namespace HCDE.Client.Tests;

public class VmGameConfigSearchPathsTests
{
    [Fact]
    public void Collect_IncludesLinuxSharePaths()
    {
        var paths = VmGameConfigSearchPaths.Collect(null, "/data", "/usr/share", "/usr/local/share");
        Assert.Contains("$PROGDIR", paths);
        Assert.Contains("/data/games/gzdoom", paths);
        Assert.Contains("/usr/share/games/doom", paths);
    }
}
