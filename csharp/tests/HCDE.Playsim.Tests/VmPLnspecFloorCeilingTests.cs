namespace HCDE.Playsim.Tests;

public class VmPLnspecFloorCeilingTests
{
    [Fact]
    public void Speed_DividesByEight()
    {
        Assert.Equal(2.0, VmPLnspecArgMacros.Speed(16));
    }

    [Fact]
    public void FloorLowerByValueTimes8_ScalesHeight()
    {
        var invoke = VmPLnspecFloorLineSpecials.LowerByValueTimes8(1, 8, 3, 2);
        Assert.Equal(24, invoke.Height);
    }

    [Fact]
    public void GenericFloor_SelectsRaiseToHighest()
    {
        var kind = VmPLnspecFloorLineSpecials.ResolveGenericFloorKind(1, raise: true);
        Assert.Equal(VmPLnspecFloorLineSpecials.FloorKind.RaiseToHighest, kind);
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
