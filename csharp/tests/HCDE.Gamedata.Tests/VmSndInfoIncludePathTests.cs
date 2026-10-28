namespace HCDE.Gamedata.Tests;

public class VmSndInfoIncludePathTests
{
    [Fact]
    public void Resolve_StripsLeadingDotSlash()
    {
        var path = VmSndInfoIncludePath.Resolve("sndinfo/foo/bar.txt", "./extra.sndinfo");
        Assert.Equal("sndinfo/foo/extra.sndinfo", path);
    }

    [Fact]
    public void Resolve_ParentWalkReturnsOriginalWhenTooManyDots()
    {
        var path = VmSndInfoIncludePath.Resolve("sndinfo/a.txt", "../../../escape.sndinfo");
        Assert.Equal("../../../escape.sndinfo", path);
    }
}
