namespace HCDE.Client.Tests;

public class VmAutomapRestoreColorsTests
{
    [Fact]
    public void ShouldResetStandardSlot_InRange()
    {
        Assert.True(VmAutomapRestoreColors.ShouldResetStandardSlot(0, 16));
    }
}
