namespace HCDE.Playsim.Tests;

public class VmPLnspecFloorCeilingTests
{
    [Fact]
    public void Speed_DividesByEight()
    {
        Assert.Equal(2.0, VmPLnspecArgMacros.Speed(16));
    }

    [Fact]
    public void RaiseToCeiling_ScalesGap()
    {
        var invoke = VmPLnspecFloorRaiseToCeiling.RaiseToCeiling(1, 8, 3, 2, 4);
        Assert.Equal(4, invoke.Height);
        Assert.True(invoke.DirectionUp);
    }

    [Fact]
    public void GenericFloor_SelectsRaiseToHighest()
    {
        int? kind = null;
        _ = VmPLnspecGenericDoorFloorPillar.GenericFloor(1, 8, 0, 1, 8, new VmPLnspecGenericDoorFloorPillar.LineContext
        {
            EvDoFloor = (k, _, _, _, _, _, _) =>
            {
                kind = k;
                return true;
            },
        });
        Assert.Equal(5, kind);
    }

    [Fact]
    public void CeilingCrushStop_RemoveDefaultsHexen()
    {
        Assert.True(VmPLnspecArgMacros.CeilingCrushStopShouldRemove(0, 2));
    }

    [Fact]
    public void CeilingLowerInstant_ScalesHeight()
    {
        var invoke = VmPLnspecCeilingLineSpecials.LowerInstant(4, 3, 1, 10);
        Assert.Equal(24, invoke.Height);
    }
}
