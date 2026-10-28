namespace HCDE.Gamedata.Tests;

public class VmDehackedLumpLoaderTests
{
    [Fact]
    public void IsRootDehOrBex_RejectsNestedPaths()
    {
        Assert.False(VmDehackedLumpLoader.IsRootDehOrBex("mods/x.deh"));
        Assert.True(VmDehackedLumpLoader.IsRootDehOrBex("patch.bex"));
    }

    [Fact]
    public void IsSupportedDehackedHeader_AcceptsV3()
    {
        Assert.True(VmDehackedLumpLoader.IsSupportedDehackedHeader("Patch File for DeHackEd v3.1.0"));
    }
}
