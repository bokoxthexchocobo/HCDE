namespace HCDE.Playsim.Tests;

public class VmPLnspecScrollSector928SliceTests
{
    [Fact]
    public void SectorLink_SkipsControlTagZero()
    {
        Assert.False(VmPLnspecScrollSector928Slice.SectorSetLinkRequiresControlTag(0));
    }

    [Fact]
    public void ScrollTextureBoth_NormalizesNegativeId()
    {
        var scroll = VmPLnspecScrollSector928Slice.ScrollTextureBoth(-3, 0, 0, 0, 0);
        Assert.Equal(3, scroll.Id);
        Assert.Equal(1, scroll.SideChoice);
    }

    [Fact]
    public void Damage_LegacyLeakinessForLowAmount()
    {
        var patch = VmPLnspecScrollSector928Slice.NormalizeLegacyLeakiness(10, 0, 0);
        Assert.Equal(32, patch.Interval);
    }

    [Fact]
    public void Gravity_ClampsFraction()
    {
        Assert.Equal(99, VmPLnspecScrollSector928Slice.ClampGravityFraction(120));
    }

    [Fact]
    public void ScrollFloor_SplitsCarryAndScroller()
    {
        var (carryX, _) = VmPLnspecScrollSector928Slice.ScrollFloorCarry(1, 1.0, 0);
        Assert.Equal(1.0, carryX);
    }

    [Fact]
    public void PointPush_AlwaysFails()
    {
        Assert.True(VmPLnspecScrollSector928Slice.PointPushAlwaysFails());
    }
}
