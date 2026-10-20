namespace HCDE.Gamedata.Tests;

public class VmDehackedIncludeDirectiveTests
{
    [Fact]
    public void TryCombinePatchDirectory_PrefixesFolder()
    {
        Assert.Equal("patches/foo.deh", VmDehackedIncludeDirective.TryCombinePatchDirectory("patches/base.deh", "foo.deh"));
    }

    [Fact]
    public void TryParseNotextFlag_StripsPrefix()
    {
        var line = "notext patch.deh";
        Assert.True(VmDehackedIncludeDirective.TryParseNotextFlag(ref line, out var noText));
        Assert.True(noText);
        Assert.Equal("patch.deh", line);
    }
}
