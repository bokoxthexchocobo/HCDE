namespace HCDE.Playsim.Tests;

public class VmPSightPathTraverseTests
{
    [Fact]
    public void SightPathTraverse_EarlyOutWhenBlockIteratorFails()
    {
        var ctx = new VmPSightPathTraverse.SightPathTraverseContext
        {
            SightStartX = 0,
            SightStartY = 0,
            SightEndX = 256,
            SightEndY = 0,
            TraceDx = 256,
            TraceDy = 0,
            BmapOrgX = 0,
            BmapOrgY = 0,
            IsValidBlock = (_, _) => true,
            SightBlockLinesIterator = (_, _) => 0,
            SightTraverseIntercepts = () => true,
            PortalGroup = 0,
            SeeingThingPortalGroup = 0,
        };
        Assert.False(VmPSightPathTraverse.SightPathTraverse(ctx));
    }

    [Fact]
    public void SightPathTraverse_QueuesCeilingPortalWhenOpen()
    {
        var ctx = new VmPSightPathTraverse.SightPathTraverseContext
        {
            SightStartX = 64,
            SightStartY = 64,
            SightEndX = 64,
            SightEndY = 64,
            TraceDx = 0,
            TraceDy = 0,
            CeilingPortalOpen = true,
            OppositePortalGroupCeiling = 3,
            IsValidBlock = (_, _) => false,
            SightTraverseIntercepts = () => true,
            PortalGroup = 0,
            SeeingThingPortalGroup = 0,
        };
        Assert.False(VmPSightPathTraverse.SightPathTraverse(ctx));
        Assert.Single(ctx.Portals);
        Assert.Equal(3, ctx.Portals[0].PortalGroup);
    }
}
