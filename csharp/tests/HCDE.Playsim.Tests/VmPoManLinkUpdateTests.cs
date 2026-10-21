namespace HCDE.Playsim.Tests;

public class VmPoManLinkUpdateTests
{
    [Fact]
    public void CalcCenter_AveragesVertices()
    {
        VmPoManLinkUpdate.CalcCenter(
            new List<(double, double)> { (0, 0), (4, 6) },
            out var cx,
            out var cy);
        Assert.Equal(2, cx);
        Assert.Equal(3, cy);
    }

    [Fact]
    public void UpdateLinks_MovesDisplacementGroupOncePerPortalGroup()
    {
        var moves = new List<int>();
        var ctx = new VmPoManLinkUpdate.UpdateLinksContext
        {
            PortalLinkMode = 2,
            MoveDisplacementGroup = (g, _, _) => moves.Add(g),
        };
        ctx.Portals.Add(new VmPoManLinkUpdate.PortalDisplacement
        {
            IsLinkedPortal = true,
            OldDx = 0,
            OldDy = 0,
            NewDx = 2,
            NewDy = 0,
            DestPortalGroup = 5,
        });
        ctx.Portals.Add(new VmPoManLinkUpdate.PortalDisplacement
        {
            IsLinkedPortal = true,
            OldDx = 0,
            OldDy = 0,
            NewDx = 4,
            NewDy = 0,
            DestPortalGroup = 5,
        });
        VmPoManLinkUpdate.UpdateLinks(ctx);
        Assert.Equal(new[] { 5 }, moves);
    }

    [Fact]
    public void UpdateBBox_AdjustsLinesAndCenter()
    {
        var adjusted = new List<int>();
        var ctx = new VmPoManLinkUpdate.UpdateBBoxContext
        {
            AdjustLine = i => adjusted.Add(i),
        };
        ctx.LineIndices.Add(2);
        ctx.Vertices.Add((0, 0));
        ctx.Vertices.Add((2, 2));
        VmPoManLinkUpdate.UpdateBBox(ctx);
        Assert.Equal(new[] { 2 }, adjusted);
        Assert.Equal(1, ctx.CenterX);
    }

    [Fact]
    public void LinkPolyobj_AssignsCellsInComputedBox()
    {
        var cells = new List<int>();
        var ctx = new VmPoManLinkUpdate.LinkPolyobjContext
        {
            BlockWidth = 8,
            BlockHeight = 8,
            BlockX = x => (int)x,
            BlockY = y => (int)y,
            AssignPolyToCell = (idx, _) => cells.Add(idx),
        };
        ctx.SideVertices.Add((1, 1));
        ctx.SideVertices.Add((3, 2));
        VmPoManLinkUpdate.LinkPolyobj(ctx, 9);
        Assert.Contains(9, cells);
        Assert.True(ctx.BoxLeft <= 1 && ctx.BoxRight >= 3);
    }

    [Fact]
    public void LinkPolyobj_SkipsCellsOutsideMap()
    {
        var cells = new List<int>();
        var ctx = new VmPoManLinkUpdate.LinkPolyobjContext
        {
            BlockWidth = 2,
            BlockHeight = 2,
            BlockX = x => (int)x,
            BlockY = y => (int)y,
            AssignPolyToCell = (idx, _) => cells.Add(idx),
        };
        ctx.SideVertices.Add((10, 10));
        VmPoManLinkUpdate.LinkPolyobj(ctx, 1);
        Assert.Empty(cells);
    }

    [Fact]
    public void RecalcActorFloorCeil_SkipsActorsOutsideBounds()
    {
        var touched = new List<int>();
        var ctx = new VmPoManLinkUpdate.RecalcActorFloorCeilContext
        {
            BoundsLeft = 0,
            BoundsRight = 10,
            BoundsBottom = 0,
            BoundsTop = 10,
            ActorsInBounds = () => new[]
            {
                new VmPoManLinkUpdate.RecalcActor(20, 5, 1),
                new VmPoManLinkUpdate.RecalcActor(5, 5, 1),
            },
            FindFloorCeiling = a => touched.Add((int)a.X),
        };
        VmPoManLinkUpdate.RecalcActorFloorCeil(ctx);
        Assert.Equal(new[] { 5 }, touched);
    }
}
