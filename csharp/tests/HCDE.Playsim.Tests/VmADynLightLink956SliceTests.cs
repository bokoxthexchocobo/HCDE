namespace HCDE.Playsim.Tests;

public class VmADynLightLink956SliceTests
{
    [Fact]
    public void CeilingPortalAffectsLight_WhenPlaneBelowTopOfRadius()
    {
        Assert.True(VmADynLightLink956Slice.CeilingPortalAffectsLight(10, 5, 8));
    }

    [Fact]
    public void FloorPortalAffectsLight_WhenPlaneAboveBottomOfRadius()
    {
        Assert.True(VmADynLightLink956Slice.FloorPortalAffectsLight(12, 10, 5));
    }

    [Fact]
    public void ShadowMappedAfterCollection_RequiresBackHitAndShadowmapAllowed()
    {
        Assert.True(VmADynLightLink956Slice.ShadowMappedAfterCollection(true, false));
        Assert.False(VmADynLightLink956Slice.ShadowMappedAfterCollection(true, true));
    }

    [Fact]
    public void LinkLightRadiusSquared_DoublesAsSquare()
    {
        Assert.Equal(25f, VmADynLightLink956Slice.LinkLightRadiusSquared(5f));
    }

    [Fact]
    public void DistToSegClampParameter_ClampsToSegment()
    {
        Assert.Equal(0.0, VmADynLightLink956Slice.DistToSegClampParameter(-1));
        Assert.Equal(1.0, VmADynLightLink956Slice.DistToSegClampParameter(2));
    }

    [Fact]
    public void LightInFrontOfSeg_UsesCrossProductSign()
    {
        Assert.True(VmADynLightLink956Slice.LightInFrontOfSeg(0, 0, 0, 0, 10, 0));
    }

    [Fact]
    public void AssociationTriggersAttach_DelegatesTo955Slice()
    {
        Assert.True(VmADynLightLink956Slice.AssociationTriggersAttach(
            1, 2, 1, VmADynLightAttach955Slice.AnyFrameAssociation));
    }

    [Fact]
    public void ShouldAbortSetDynamicLights_WhenStateMissing()
    {
        Assert.True(VmADynLightLink956Slice.ShouldAbortSetDynamicLights(null));
    }

    [Fact]
    public void SegmentDistanceWithinRadius_ComparesSquaredDistance()
    {
        Assert.True(VmADynLightLink956Slice.SegmentDistanceWithinRadius(4, 5));
    }
}
