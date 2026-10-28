namespace HCDE.Playsim.Tests;

public class VmADynLightCollect957SliceTests
{
    [Fact]
    public void DistToSegSquared_OnSegmentEndpoint()
    {
        Assert.Equal(0, VmADynLightCollect957Slice.DistToSegSquared(0, 0, 0, 0, 10, 0));
    }

    [Fact]
    public void DistToSegSquared_PerpendicularOffset()
    {
        var dist = VmADynLightCollect957Slice.DistToSegSquared(5, 5, 0, 0, 10, 0);
        Assert.Equal(25, dist);
    }

    [Fact]
    public void SegmentWithinLightRadius_UsesSquaredCompare()
    {
        Assert.True(VmADynLightCollect957Slice.SegmentWithinLightRadius(
            0, 0, 0, 0, 3, 4, 25f));
    }

    [Fact]
    public void LightNodeListNeedsResize_WhenIndexAtSize()
    {
        Assert.True(VmADynLightCollect957Slice.LightNodeListNeedsResize(5, 5));
    }

    [Fact]
    public void EvaluateLightListCapacity_ResizesWhenNeeded()
    {
        var (needs, size) = VmADynLightCollect957Slice.EvaluateLightListCapacity(3, 3);
        Assert.True(needs);
        Assert.Equal(4, size);
    }

    [Fact]
    public void LinkedPortalEligible_OnlyLinkedType()
    {
        Assert.True(VmADynLightCollect957Slice.LinkedPortalEligible(VmADynLightCollect957Slice.PortTypeLinked));
        Assert.False(VmADynLightCollect957Slice.LinkedPortalEligible(0));
    }

    [Fact]
    public void ShouldAddWallLightNode_DelegatesCrossProduct()
    {
        Assert.True(VmADynLightCollect957Slice.ShouldAddWallLightNode(0, 0, 0, 0, 10, 0));
    }

    [Fact]
    public void ShouldSkipCollectWithinRadius_WhenSectionNull()
    {
        Assert.True(VmADynLightCollect957Slice.ShouldSkipCollectWithinRadius(null));
    }

    [Fact]
    public void AddLightNodeTargetsWall_WhenOnlySidedef()
    {
        Assert.True(VmADynLightCollect957Slice.AddLightNodeTargetsWall(null, new object()));
    }
}
