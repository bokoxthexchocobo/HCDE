namespace HCDE.Playsim.Tests;

public class VmPLnspecCeilingPlatBlockTests
{
    [Fact]
    public void GenericCrusher_SlowdownWhenSpeedsLow()
    {
        Assert.Equal(
            VmPLnspecCeilingPlatBlock.CrushMode.Slowdown,
            VmPLnspecCeilingPlatBlock.CrusherModeForSpeeds(16, 16, false));
    }

    [Fact]
    public void GenericCrusher2_AlwaysHexen()
    {
        var c = VmPLnspecCeilingPlatBlock.PackGenericCrusher2(32, 32, 0, 10);
        Assert.Equal(VmPLnspecCeilingPlatBlock.CrushMode.Hexen, c.Mode);
    }

    [Fact]
    public void PlatStop_RemoveDefaultsHexen()
    {
        Assert.True(VmPLnspecCeilingPlatBlock.PlatStopRemove(0, VmPLnspecCeilingPlatBlock.GameHexen));
        Assert.False(VmPLnspecCeilingPlatBlock.PlatStopRemove(1, VmPLnspecCeilingPlatBlock.GameHexen));
    }

    [Fact]
    public void RaiseAndStay_DefaultHereticLockout()
    {
        Assert.Equal(
            VmPLnspecCeilingPlatBlock.PlatType.RaiseAndStayLockout,
            VmPLnspecCeilingPlatBlock.ResolveRaiseAndStayType(0, VmPLnspecCeilingPlatBlock.GameHeretic));
    }

    [Fact]
    public void GenericLift_TargetCase2NearestFloor()
    {
        Assert.Equal(
            VmPLnspecCeilingPlatBlock.PlatType.DownToNearestFloor,
            VmPLnspecCeilingPlatBlock.ResolveGenericLiftTarget(2));
    }

    [Fact]
    public void GenericCeiling_RaiseModeFromArg4Bit8()
    {
        var plan = VmPLnspecCeilingPlatBlock.PlanGenericCeiling(1, 8);
        Assert.True(plan.RaiseMode);
        Assert.Equal(-1, plan.CrushOrModel);
    }

    [Fact]
    public void DownByValue_ScalesHeightTimes8()
    {
        var p = VmPLnspecCeilingPlatBlock.PackDownByValue(1, 8, 4, 3);
        Assert.Equal(24, p.HeightUnits);
    }
}
