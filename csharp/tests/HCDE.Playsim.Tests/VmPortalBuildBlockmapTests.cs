namespace HCDE.Playsim.Tests;

public class VmPortalBuildBlockmapTests
{
    static VmPTagManager.LineIdIterator LineIterator(int tag, params (int target, int nextTag)[] nodes)
    {
        var hash = new int[VmPortalBuildBlockmap.TagHashSize];
        for (var i = 0; i < hash.Length; i++)
            hash[i] = -1;
        if (nodes.Length == 0)
            return new VmPTagManager.LineIdIterator(tag, hash, Array.Empty<VmPTagManager.TagItem>());
        var entries = new VmPTagManager.TagItem[nodes.Length];
        for (var i = 0; i < nodes.Length; i++)
            entries[i] = new VmPTagManager.TagItem(nodes[i].target, tag, nodes[i].nextTag);
        hash[((uint)tag) % VmPortalBuildBlockmap.TagHashSize] = 0;
        return new VmPTagManager.LineIdIterator(tag, hash, entries);
    }

    [Fact]
    public void FindPortalDestination_SkipsSourceLine()
    {
        var it = LineIterator(1, (5, 1), (7, -1));
        var idx = VmPortalBuildBlockmap.FindPortalDestinationLineIndex(new VmPortalBuildBlockmap.FindPortalDestinationContext
        {
            SourceLineIndex = 5,
            Tag = 1,
            Iterator = it,
        });
        Assert.Equal(7, idx);
    }

    [Fact]
    public void FindPortalDestination_TerminatesWhenOnlySourceMatches()
    {
        var it = LineIterator(1, (5, 1), (5, -1));
        var idx = VmPortalBuildBlockmap.FindPortalDestinationLineIndex(new VmPortalBuildBlockmap.FindPortalDestinationContext
        {
            SourceLineIndex = 5,
            Tag = 1,
            Iterator = it,
        });
        Assert.Equal(-1, idx);
    }

    [Fact]
    public void LineIdIterator_NextWalksTagChainWithoutRepeatingFirstForever()
    {
        var it = LineIterator(9, (10, 1), (20, 2), (30, -1));
        Assert.Equal(10, it.Next());
        Assert.Equal(20, it.Next());
        Assert.Equal(30, it.Next());
        Assert.Equal(-1, it.Next());
    }

    [Fact]
    public void SetPortalRotation_LinkedUsesIdentity()
    {
        var st = new VmPortalBuildBlockmap.PortalRotationState();
        VmPortalBuildBlockmap.SetPortalRotation(VmPortalBuildBlockmap.PorttLinked, true, 0, 90, false, false, st);
        Assert.Equal(1, st.CosRot);
        Assert.Equal(0, st.SinRot);
    }

    [Fact]
    public void AddLineIntercepts_PushesCrossingLine()
    {
        var ctx = new VmPortalBuildBlockmap.AddLineInterceptsContext
        {
            Bx = 1,
            By = 1,
            BlockmapDx = 4,
            BlockmapDy = 4,
            PortalLinesInBlock = new[] { 3 },
            LineVerts = _ => (0, 0, 10, 10),
            LineDelta = _ => (10, 10),
            PointOnDivlineSide6 = (px, py, tx, ty, tdx, tdy) =>
                (px - tx) * tdy - (py - ty) * tdx > 0 ? 1 : 0,
            PointOnDivlineSide4 = (px, py, lx, ly) => px > lx ? 1 : 0,
            InterceptVector = (_, _, _, _, _, _) => 0.5,
            TraceX = 0,
            TraceY = 5,
            TraceDx = 10,
            TraceDy = 0,
        };
        VmPortalBuildBlockmap.AddLineIntercepts(ctx);
        Assert.Single(ctx.Intercepts);
        Assert.Equal(3, ctx.Intercepts[0].LineIndex);
    }
}
