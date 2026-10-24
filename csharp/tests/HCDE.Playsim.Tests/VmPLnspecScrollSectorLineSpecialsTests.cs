namespace HCDE.Playsim.Tests;

public class VmPLnspecScrollSectorLineSpecialsTests
{
    [Fact]
    public void SectorLink_SkipsControlTagZero()
    {
        Assert.False(VmPLnspecScrollSectorLineSpecials.SectorSetLinkRequiresControlTag(0));
    }

    [Fact]
    public void ScrollTextureBoth_NormalizesNegativeId()
    {
        var scroll = VmPLnspecScrollSectorLineSpecials.ScrollTextureBoth(-3, 0, 0, 0, 0);
        Assert.Equal(3, scroll.Id);
        Assert.Equal(1, scroll.SideChoice);
    }

    [Fact]
    public void Damage_LegacyLeakinessForLowAmount()
    {
        var patch = VmPLnspecScrollSectorLineSpecials.NormalizeLegacyLeakiness(10, 0, 0);
        Assert.Equal(32, patch.Interval);
    }

    [Fact]
    public void Gravity_ClampsFraction()
    {
        Assert.Equal(99, VmPLnspecScrollSectorLineSpecials.ClampGravityFraction(120));
    }

    [Fact]
    public void Panning_CombinesIntAndFrac()
    {
        var (x, _) = VmPLnspecScrollSectorLineSpecials.CeilingPanning(1, 50, 0, 0);
        Assert.Equal(1.5, x);
    }

    [Fact]
    public void FloorScale_InvertsNonZero()
    {
        var scale = VmPLnspecScrollSectorLineSpecials.InvertScaleComponent(2, 0);
        Assert.Equal(0.5, scale);
    }

    [Fact]
    public void Scale2_Uses65536Factors()
    {
        var inv = VmPLnspecScrollSectorLineSpecials.InvertScaleFactor65536(65536);
        Assert.Equal(1.0, inv);
    }

    [Fact]
    public void ScrollFloor_SplitsCarryAndScroller()
    {
        var (carryX, _) = VmPLnspecScrollSectorLineSpecials.ScrollFloorCarry(1, 1.0, 0);
        Assert.Equal(1.0, carryX);
    }

    [Fact]
    public void CeilingScale_MatchesFloorInvert()
    {
        var floor = VmPLnspecScrollSectorLineSpecials.SectorSetFloorScale(1, 2, 0, 2, 0);
        var ceiling = VmPLnspecScrollSectorLineSpecials.SectorSetCeilingScale(1, 2, 0, 2, 0);
        Assert.Equal(floor.XScale, ceiling.XScale);
    }
}
