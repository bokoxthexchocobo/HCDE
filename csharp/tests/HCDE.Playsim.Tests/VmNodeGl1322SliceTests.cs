namespace HCDE.Playsim.Tests;

public class VmNodeGl1322SliceTests
{
    [Fact]
    public void Intersection_InsertsOnceAndBuildsAMiniseg()
    {
        var host = new VmNodeGl1322();
        var node = new VmNodeGl1322.Node { X = 0, Y = 0, Dx = 2, Dy = 0 };
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 5, Y = 0 });
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 5, Y = 7 });
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 1, Y = 0 });
        Assert.Equal(10, host.AddIntersection(node, 0));
        Assert.Equal(10, host.AddIntersection(node, 1));
        Assert.Equal(2, host.AddIntersection(node, 2));

        VmEventNode1218? first = host.Events.GetMinimum();
        Assert.NotNull(first);
        Assert.Equal(2, first!.Vertex);
        Assert.Equal(VmNodeGl1322.None, first.FrontSeg);
        VmEventNode1218 second = host.Events.Successor(first);
        Assert.Equal(0, second.Vertex);
        Assert.True(ReferenceEquals(host.Events.Successor(second), host.Events.NilNode()));

        Assert.Equal(0u, VmNodeGl1322.PointToAngle(1, 0));
        Assert.Equal(0x40000000u, VmNodeGl1322.PointToAngle(0, 1));
        Assert.Equal(0x80000000u, VmNodeGl1322.PointToAngle(-1, 0));
        Assert.Equal(0xC0000000u, VmNodeGl1322.PointToAngle(0, -1));
        Assert.Equal(-1, VmNodeGl1322.PointOnSide(0, -8, 0, 0, 10, 0));
        Assert.Equal(1, VmNodeGl1322.PointOnSide(0, 8, 0, 0, 10, 0));
        Assert.Equal(0, VmNodeGl1322.PointOnSide(0, 1, 0, 0, 100, 0));
        Assert.Equal(1, VmNodeGl1322.PointOnSide(0, 10, 0, 0, 100, 0));

        host.Segs.Add(new VmNodeGl1322.Seg { Next = 9, Plane = 3 });
        uint id = host.AddMiniseg(0, 1, VmNodeGl1322.None, 0, VmNodeGl1322.None);
        Assert.Equal(1u, id);
        Assert.Equal(VmNodeGl1322.NoSide, host.Segs[1].Side);
        Assert.Equal(-1, host.Segs[1].Line);
        Assert.Equal(-1, host.Segs[1].Plane);
        Assert.True(host.Segs[1].PlaneFront);
        Assert.Equal(9u, host.Segs[1].Next);
        Assert.Equal(VmNodeGl1322.None, host.Segs[1].Partner);
        Assert.Equal(1u, host.Vertices[0].Segs);
        Assert.Equal(1u, host.Vertices[1].Segs2);
        Assert.Equal(VmNodeGl1322.None, host.Segs[1].NextForVert);
        Assert.Equal(VmNodeGl1322.None, host.Segs[1].NextForVert2);
    }

    [Fact]
    public void Sharer_SplitsBothWaysAndClosesTheLoop()
    {
        var node = new VmNodeGl1322.Node { Dx = 1 };
        var host = new VmNodeGl1322();
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 0, Y = 0, Segs = 0, Segs2 = 1 });
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 64, Y = 0 });
        host.Vertices.Add(new VmNodeGl1322.Vert { X = 128, Y = 0, Segs = 1, Segs2 = 0 });
        host.Segs.Add(new VmNodeGl1322.Seg { V1 = 0, V2 = 2, Partner = 1 });
        host.Segs.Add(new VmNodeGl1322.Seg { V1 = 2, V2 = 0, Partner = 0 });
        host.AddIntersection(node, 0);
        host.AddIntersection(node, 1);
        host.AddIntersection(node, 2);
        host.SplitSharers.Add(new VmNodeGl1322.Sharer { Distance = 50, Seg = 0, Forward = true });
        host.SplitSharers.Add(new VmNodeGl1322.Sharer { Distance = 128, Seg = 0, Forward = true });
        host.FixSplitSharers(node);
        Assert.Equal(2, host.Segs.Count);

        host.SplitSharers.Clear();
        host.SplitSharers.Add(new VmNodeGl1322.Sharer { Distance = 0, Seg = 0, Forward = true });
        host.FixSplitSharers(node);
        Assert.Equal(4, host.Segs.Count);
        Assert.Equal(1, host.Segs[0].V2);
        Assert.Equal(2u, host.Segs[0].Next);
        Assert.Equal(3u, host.Segs[0].Partner);
        Assert.Equal(1, host.Segs[2].V1);
        Assert.Equal(2, host.Segs[2].V2);
        Assert.Equal(VmNodeGl1322.None, host.Segs[2].Next);
        Assert.Equal(1u, host.Segs[2].Partner);
        Assert.Equal(1, host.Segs[1].V2);
        Assert.Equal(3u, host.Segs[1].Next);
        Assert.Equal(2u, host.Segs[1].Partner);
        Assert.Equal(1, host.Segs[3].V1);
        Assert.Equal(0, host.Segs[3].V2);
        Assert.Equal(0u, host.Segs[3].Partner);
        Assert.Equal(0u, host.Vertices[0].Segs);
        Assert.Equal(3u, host.Vertices[0].Segs2);
        Assert.Equal(3u, host.Vertices[1].Segs);
        Assert.Equal(1u, host.Vertices[1].Segs2);
        Assert.Equal(2u, host.Vertices[2].Segs2);

        var back = new VmNodeGl1322();
        back.Vertices.Add(new VmNodeGl1322.Vert { X = 0 });
        back.Vertices.Add(new VmNodeGl1322.Vert { X = 64 });
        back.Vertices.Add(new VmNodeGl1322.Vert { X = 128 });
        back.Segs.Add(new VmNodeGl1322.Seg { V1 = 2, V2 = 0 });
        back.AddIntersection(node, 0);
        back.AddIntersection(node, 1);
        back.AddIntersection(node, 2);
        back.SplitSharers.Add(new VmNodeGl1322.Sharer { Distance = 128, Seg = 0, Forward = false });
        back.FixSplitSharers(node);
        Assert.Equal(2, back.Segs.Count);
        Assert.Equal(1, back.Segs[0].V2);
        Assert.Equal(1u, back.Segs[0].Next);
        Assert.Equal(1, back.Segs[1].V1);
        Assert.Equal(0, back.Segs[1].V2);

        var loop = new VmNodeGl1322();
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 0, Y = 0, Segs = 1, Segs2 = 0 });
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 40, Y = 0, Segs = 3, Segs2 = 2 });
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 10, Y = -4 });
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 10, Y = 4 });
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 30, Y = 4 });
        loop.Vertices.Add(new VmNodeGl1322.Vert { X = 30, Y = -4 });
        var frontSector = new VmNodeGl1322.Sector { Id = 4 };
        var frontBack = new VmNodeGl1322.Sector { Id = 5 };
        var backSector = new VmNodeGl1322.Sector { Id = 6 };
        var backBack = new VmNodeGl1322.Sector { Id = 7 };
        loop.Segs.Add(new VmNodeGl1322.Seg { V1 = 2, V2 = 0, Front = frontSector, Back = frontBack });
        loop.Segs.Add(new VmNodeGl1322.Seg { V1 = 0, V2 = 3 });
        loop.Segs.Add(new VmNodeGl1322.Seg { V1 = 4, V2 = 1, Front = backSector, Back = backBack });
        loop.Segs.Add(new VmNodeGl1322.Seg { V1 = 1, V2 = 5 });
        loop.Segs.Add(new VmNodeGl1322.Seg { Plane = 7 });
        Assert.Equal(0u, loop.CheckLoopStart(1, 0, 0, 1));
        Assert.Equal(2u, loop.CheckLoopStart(-1, 0, 1, 0));
        Assert.Equal(3u, loop.CheckLoopEnd(1, 0, 1));
        Assert.Equal(1u, loop.CheckLoopEnd(-1, 0, 0));
        loop.Segs[1].V2 = 1;
        Assert.Equal(VmNodeGl1322.None, loop.CheckLoopStart(1, 0, 0, 1));
        loop.Segs[1].V2 = 3;
        loop.AddIntersection(node, 0);
        loop.AddIntersection(node, 1);
        uint fset = VmNodeGl1322.None;
        uint bset = VmNodeGl1322.None;
        loop.AddMinisegs(node, 4, ref fset, ref bset);
        Assert.Equal(5u, fset);
        Assert.Equal(6u, bset);
        Assert.Equal(frontSector, loop.Segs[5].Front);
        Assert.Equal(backSector, loop.Segs[5].Back);
        Assert.Equal(backSector, loop.Segs[6].Front);
        Assert.Equal(frontSector, loop.Segs[6].Back);
        Assert.Equal(6u, loop.Segs[5].Partner);
        Assert.Equal(5u, loop.Segs[6].Partner);
        Assert.Equal(7, loop.Segs[5].Plane);
        Assert.Equal(0, loop.Segs[5].V1);
        Assert.Equal(1, loop.Segs[5].V2);
        Assert.Equal(1u, loop.Segs[5].NextForVert);
        Assert.Equal(2u, loop.Segs[5].NextForVert2);
        Assert.Equal(VmNodeGl1322.None, loop.Segs[5].Next);
        Assert.Equal(5u, loop.Vertices[0].Segs);
        Assert.Equal(6u, loop.Vertices[0].Segs2);
        Assert.Equal(6u, loop.Vertices[1].Segs);
        Assert.Equal(5u, loop.Vertices[1].Segs2);
        Assert.Equal(new[] { "0:1" }, loop.Warnings);

        loop.Segs[0].Front = frontSector;
        loop.Segs[0].Back = frontSector;
        loop.Segs[2].Front = frontSector;
        loop.Warnings.Clear();
        uint againF = VmNodeGl1322.None;
        uint againB = VmNodeGl1322.None;
        loop.AddMinisegs(node, 4, ref againF, ref againB);
        Assert.Empty(loop.Warnings);
    }
}
