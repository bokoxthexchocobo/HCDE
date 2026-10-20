namespace HCDE.Playsim.Tests;

public class VmPSpecPredictLineTests
{
    [Fact]
    public void SpecialIsPredictable_TeleportLine() =>
        Assert.True(VmPSpecPredictLine.SpecialIsPredictable(215));

    [Fact]
    public void RejectWhenLocked() =>
        Assert.True(VmPSpecPredictLine.RejectWhenLocked(1));

    [Fact]
    public void RequiresPredictCvar() =>
        Assert.True(VmPSpecPredictLine.RequiresPredictCvar(true));
}
