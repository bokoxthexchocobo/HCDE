namespace HCDE.Playsim.Tests;

public class VmPoManSpatialTests
{
    [Fact]
    public void ClosestPoint_PicksNearestSegmentPoint()
    {
        var ctx = new VmPoManSpatial.ClosestPointContext { QueryX = 1, QueryY = 1 };
        ctx.Sides.Add(new VmPoManSpatial.PolySide { V1X = 0, V1Y = 0, V2X = 4, V2Y = 0, SideId = 3 });
        var (x, y, side) = VmPoManSpatial.ClosestPoint(ctx);
        Assert.Equal(3, side);
        Assert.Equal(1, x, 3);
        Assert.Equal(0, y, 3);
    }

    [Fact]
    public void PoBusy_TrueOnlyWhenSpecialDataPresent()
    {
        Assert.True(VmPoManSpatial.PoBusy(true, true));
        Assert.False(VmPoManSpatial.PoBusy(true, false));
    }

    [Fact]
    public void ClearSubsectorLinks_UnhooksHeadList()
    {
        var link = new VmPoManSpatial.SubsectorLink();
        var node = new VmPoManSpatial.SubsectorLinkNode { Owner = link };
        link.Head = node;
        VmPoManSpatial.ClearSubsectorLinks(link);
        Assert.Null(link.Head);
        Assert.Equal(-1, node.State);
    }
}
