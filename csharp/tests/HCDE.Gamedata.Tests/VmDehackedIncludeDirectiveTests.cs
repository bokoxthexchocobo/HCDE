namespace HCDE.Gamedata.Tests;

public class VmDehackedIncludeDirectiveTests
{
    [Fact]
    public void TryCombinePatchDirectory_PrefixesFolder()
    {
        Assert.Equal("patches/foo.deh", VmDehackedIncludeDirective.TryCombinePatchDirectory("patches/base.deh", "foo.deh"));
    }
}
