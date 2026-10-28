namespace HCDE.Client.Tests;

public class VmCmdLibPathsTests
{
    [Fact]
    public void FixPathSeparator_ReplacesBackslashes()
    {
        Assert.Equal("foo/bar/baz", VmCmdLibPaths.FixPathSeparator(@"foo\bar\baz"));
    }

    [Fact]
    public void DefaultExtension_AppendsWhenMissing()
    {
        var path = "textures/brick";
        VmCmdLibPaths.DefaultExtension(ref path, ".png");
        Assert.Equal("textures/brick.png", path);
    }

    [Fact]
    public void DefaultExtension_SkipsWhenExtensionPresent()
    {
        var path = "textures/brick.png";
        VmCmdLibPaths.DefaultExtension(ref path, ".tga");
        Assert.Equal("textures/brick.png", path);
    }

    [Fact]
    public void ExtractFilePath_ReturnsDirectoryPrefix()
    {
        Assert.Equal("textures/", VmCmdLibPaths.ExtractFilePath("textures/brick.png"));
    }

    [Fact]
    public void ExtractFileBase_StripsDirectoryAndExtension()
    {
        Assert.Equal("brick", VmCmdLibPaths.ExtractFileBase("textures/brick.png"));
    }

    [Fact]
    public void SplitPath_SplitsDirectoryBaseAndExtension()
    {
        VmCmdLibPaths.SplitPath("textures/brick.png", out var dir, out var baseName, out var ext);
        Assert.Equal("textures/", dir);
        Assert.Equal("brick", baseName);
        Assert.Equal(".png", ext);
    }
}
