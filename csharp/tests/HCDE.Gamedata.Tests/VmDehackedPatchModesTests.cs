namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchModesTests
{
    [Fact]
    public void IsKnownSection_RejectsUnknown()
    {
        Assert.True(VmDehackedPatchModes.IsKnownSection("Thing"));
        Assert.False(VmDehackedPatchModes.IsKnownSection("NotASection"));
    }
}
