namespace HCDE.Playsim.Tests;

public class VmPLnspecFloorCeilingTests
{
    [Fact]
    public void RaiseToCeiling_ScalesGap()
    {
        var invoke = VmPLnspecFloorRaiseToCeiling.RaiseToCeiling(1, 8, 3, 2, 4);
        Assert.Equal(4, invoke.Height);
        Assert.True(invoke.DirectionUp);
    }

    [Fact]
    public void Speed_DividesByEight() =>
        Assert.Equal(1.0, VmPLnspecArgMacros.Speed(8));
}
