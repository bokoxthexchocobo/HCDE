namespace HCDE.Playsim.Tests;

public class VmPLnspecLineCameraTests
{
    [Fact]
    public void AutomapFlags_RejectsZeroId()
    {
        Assert.True(VmPLnspecLineCameraLineSpecials.AutomapFlagsRejectZeroId(0));
    }

    [Fact]
    public void AutomapStyle_RejectsOutOfRange()
    {
        Assert.False(VmPLnspecLineCameraLineSpecials.AutomapStyleInRange(99, 5));
    }

    [Fact]
    public void ChangeCamera_SetsRevertCheatWhenArg2()
    {
        Assert.NotEqual(0, VmPLnspecLineCameraLineSpecials.ApplyRevertCheat(0, true));
    }

    [Fact]
    public void ThingMove_InvertsFogWhenArg2Zero()
    {
        Assert.True(VmPLnspecLineCameraLineSpecials.ThingMoveFogEnabled(0));
    }

    [Fact]
    public void PuzzleItem_RejectsNullActivator()
    {
        Assert.True(VmPLnspecLineCameraLineSpecials.PuzzleRequiresActivator(null));
    }
}
