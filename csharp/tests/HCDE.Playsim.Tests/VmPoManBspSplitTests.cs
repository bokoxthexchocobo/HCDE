namespace HCDE.Playsim.Tests;

public class VmPoManBspSplitTests
{
    [Fact]
    public void GetIntersection_FindsPointOnBothSegments()
    {
        var seg = new VmPoManBspSplit.PolySeg { V1X = 0, V1Y = 0, V2X = 4, V2Y = 0 };
        var bsp = new VmPoManBspSplit.BspNode { X = 2, Y = -1, Dx = 0, Dy = 1 };
        Assert.True(VmPoManBspSplit.GetIntersection(seg, bsp, out var ix, out var iy));
        Assert.Equal(2, ix, 3);
        Assert.Equal(0, iy, 3);
    }

    [Fact]
    public void PartitionDistance_MeasuresPerpendicularDistance()
    {
        var node = new VmPoManBspSplit.BspNode { X = 0, Y = 0, Dx = 1, Dy = 0, Len = 1 };
        var d = VmPoManBspSplit.PartitionDistance(0, 3, node);
        Assert.Equal(3, d, 3);
    }

    [Fact]
    public void SplitPoly_PutsEpsilonSegOnCenterSide()
    {
        var sub = new VmPoManBspSplit.SubsectorHost();
        var bsp = new VmPoManBspSplit.BspNode
        {
            X = 0,
            Y = 0,
            Dx = 1,
            Dy = 0,
            Len = 1,
            Child0 = sub,
            Child1 = sub,
        };
        var host = new VmPoManBspSplit.PolySubsectorLinkHost();
        var node = VmPoManBspSplit.RentPolyNode();
        node.Poly = host;
        node.Segs.Add(new VmPoManBspSplit.PolySeg { V1X = 0, V1Y = 0, V2X = 0.1, V2Y = 0 });

        var ctx = new VmPoManBspSplit.SplitPolyContext
        {
            CenterX = 0,
            CenterY = 0,
            PointOnSide = (_, _, _) => 0,
            NewPolyNode = VmPoManBspSplit.RentPolyNode,
        };
        VmPoManBspSplit.SplitPoly(node, bsp, new float[4], ctx);
        Assert.NotNull(sub.Polys);
    }

    [Fact]
    public void SplitPoly_SplitsSegAcrossPartition()
    {
        var sub0 = new VmPoManBspSplit.SubsectorHost();
        var sub1 = new VmPoManBspSplit.SubsectorHost();
        var bsp = new VmPoManBspSplit.BspNode
        {
            X = 2,
            Y = 0,
            Dx = 0,
            Dy = 1,
            Len = 1,
            Child0 = sub0,
            Child1 = sub1,
        };
        var host = new VmPoManBspSplit.PolySubsectorLinkHost();
        var node = VmPoManBspSplit.RentPolyNode();
        node.Poly = host;
        node.Segs.Add(new VmPoManBspSplit.PolySeg { V1X = 0, V1Y = 0, V2X = 4, V2Y = 0 });

        var ctx = new VmPoManBspSplit.SplitPolyContext
        {
            CenterX = 1,
            CenterY = 0,
            PointOnSide = (x, _, n) => x < n.X ? 0 : 1,
            NewPolyNode = VmPoManBspSplit.RentPolyNode,
        };
        VmPoManBspSplit.SplitPoly(node, bsp, new float[4], ctx);
        Assert.NotNull(sub0.Polys);
        Assert.NotNull(sub1.Polys);
    }

    [Fact]
    public void CreateSubsectorLinks_CompatLinksCenterSubsector()
    {
        var sub = new VmPoManBspSplit.SubsectorHost();
        var ctx = new VmPoManBspSplit.CreateSubsectorLinksContext
        {
            CompatPolyobj = true,
            CenterSubsector = sub,
        };
        ctx.Sides.Add((0, 0, 0, 2, 0));
        VmPoManBspSplit.CreateSubsectorLinks(ctx);
        Assert.NotNull(sub.Polys);
        Assert.NotNull(ctx.Poly.SubsectorLinks);
    }

    [Fact]
    public void PolyNodePool_ReusesFreedNodes()
    {
        var node = VmPoManBspSplit.RentPolyNode();
        VmPoManBspSplit.FreePolyNode(node);
        var again = VmPoManBspSplit.RentPolyNode();
        Assert.Same(node, again);
    }

    [Fact]
    public void PoLinkToSubsectors_CreatesWhenMissing()
    {
        var ctx = new VmPoManBspSplit.CreateSubsectorLinksContext
        {
            CompatPolyobj = true,
            CenterSubsector = new VmPoManBspSplit.SubsectorHost(),
        };
        ctx.Sides.Add((0, 0, 0, 1, 1));
        VmPoManBspSplit.PoLinkToSubsectors(new[] { ctx });
        Assert.NotNull(ctx.Poly.SubsectorLinks);
    }
}
