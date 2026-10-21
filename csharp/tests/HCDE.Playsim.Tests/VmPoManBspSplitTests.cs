namespace HCDE.Playsim.Tests;

public class VmPoManBspSplitTests
{
    [Fact]
    public void PartitionDistance_ProjectsOntoNode()
    {
        var d = VmPoManBspSplit.PartitionDistance(4, 0, 0, 0, 1, 0);
        Assert.Equal(4, d, 3);
    }

    [Fact]
    public void MergeBboxes_ExpandsParentExtents()
    {
        var left = 0.0;
        var right = 1.0;
        var bottom = 0.0;
        var top = 1.0;
        VmPoManBspSplit.MergeBboxes(-2, 5, -3, 4, ref left, ref right, ref bottom, ref top);
        Assert.Equal(-2, left);
        Assert.Equal(5, right);
    }

    [Fact]
    public void CreateSubsectorLinks_InvokesSplitWithVertexBounds()
    {
        double? left = null;
        var ctx = new VmPoManBspSplit.CreateSubsectorLinksContext
        {
            SplitPoly = (l, _, _, _) => left = l,
        };
        ctx.Vertices.Add((0, 0));
        ctx.Vertices.Add((5, 3));
        VmPoManBspSplit.CreateSubsectorLinks(ctx);
        Assert.Equal(0, left);
    }

    [Fact]
    public void PolyNodePool_ReusesReleasedNodes()
    {
        var pool = new VmPoManBspSplit.PolyNodePool();
        var id = pool.Rent();
        pool.Return(id);
        pool.Rent();
        Assert.Equal(1, pool.Freed);
    }
}
