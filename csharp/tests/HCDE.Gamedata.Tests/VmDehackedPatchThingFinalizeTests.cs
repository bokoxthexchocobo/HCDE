namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingFinalizeTests
{
    [Fact]
    public void ValidateHeightPair_RequiresBoth()
    {
        Assert.False(VmDehackedPatchThingFinalize.ValidateHeightPair(true, false));
        Assert.True(VmDehackedPatchThingFinalize.ValidateHeightPair(true, true));
    }

    [Fact]
    public void NormalizeSpeed_ScalesLargeFixed()
    {
        Assert.Equal(2, VmDehackedPatchThingFinalize.NormalizeSpeed(131072), 3);
    }
}
