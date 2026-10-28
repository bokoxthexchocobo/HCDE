namespace HCDE.Playsim.Tests;

public class VmGGameLookAdjustTests
{
    [Fact]
    public void ScaleMovementAxis_ShiftsEightBits()
    {
        Assert.Equal(256, VmGGameBuildTiccmdTail.ScaleMovementAxis(1));
    }

    [Fact]
    public void FormatInputTrace_IncludesButtonsHex()
    {
        var s = VmGGameInputTrace.FormatInputTrace(1, 0, 0, 0, 0x10);
        Assert.Contains("buttons=0x00000010", s);
    }

    [Fact]
    public void ScaleLook_AppliesFovThenLookScale()
    {
        var v = VmGGameLookAdjust.ScaleLook(10, 2.0, 0.5, true);
        Assert.Equal(10, v);
    }

    [Fact]
    public void ClampPitchUp_StopsAtClamp()
    {
        Assert.Equal(VmGGameLookAdjust.PitchClamp, VmGGameLookAdjust.ClampPitchUp(int.MaxValue, 10));
    }

    [Fact]
    public void IsSpyingDisallowed_WhenDf2Set()
    {
        Assert.True(VmGGameChangeSpy.IsSpyingDisallowed(VmGGameChangeSpy.Df2DisallowSpying));
    }

    [Fact]
    public void StepSpyIndex_WrapsWithinMaxPlayers()
    {
        Assert.Equal(0, VmGGameChangeSpy.StepSpyIndex(7, 1));
    }

    [Fact]
    public void IsEligibleSpyTarget_AllowsTeammate()
    {
        Assert.True(VmGGameChangeSpy.IsEligibleSpyTarget(true, true, true, false, false));
    }
}
