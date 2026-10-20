namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingLoopSecondaryTests
{
    [Fact]
    public void ResolveTranslucentAlpha_Half() =>
        Assert.Equal(0.5, VmDehackedPatchThingLoopSecondary.ResolveTranslucentAlpha(2));

    [Fact]
    public void HeightPairInvalid_WhenMismatched() =>
        Assert.True(VmDehackedPatchThingLoopSecondary.HeightPairInvalid(true, false));

    [Fact]
    public void NormalizeSpeed_ScalesLarge() =>
        Assert.Equal(1.0, VmDehackedPatchThingLoopSecondary.NormalizeSpeed(65536));
}
