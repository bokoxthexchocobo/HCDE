namespace HCDE.Client.Tests;

public class VmAutomapCvarModelTests
{
    [Fact]
    public void ClampEmptySpaceMargin_Bounds()
    {
        Assert.Equal(0, VmAutomapCvarModel.ClampEmptySpaceMargin(-1));
        Assert.Equal(90, VmAutomapCvarModel.ClampEmptySpaceMargin(120));
    }

    [Fact]
    public void ShouldBlockAutomapCheat_WhenNetNoCheats()
    {
        Assert.True(VmAutomapCvarModel.ShouldBlockAutomapCheat(true, false, 1));
        Assert.False(VmAutomapCvarModel.ShouldBlockAutomapCheat(false, false, 1));
    }
}
