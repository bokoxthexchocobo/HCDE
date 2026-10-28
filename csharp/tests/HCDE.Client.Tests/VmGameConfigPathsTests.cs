namespace HCDE.Client.Tests;

public class VmGameConfigPathsTests
{
    [Fact]
    public void GetConfigPath_UsesConfigArg()
    {
        var args = new VmCommandLineArgs(["hcde", "-config", "/tmp/custom.ini"]);
        var path = VmGameConfigPaths.GetConfigPath(args, true, _ => "/default");
        Assert.Equal("/tmp/custom.ini", path);
    }
}
