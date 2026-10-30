using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAabbTree1283SliceTests
{
    static VmAabbLine1283 Line(float x1, float y1, float x2, float y2, bool back = false, bool sidedef = false, int flags = 0)
    {
        return new VmAabbLine1283
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            BackSector = back,
            Sidedef0 = sidedef,
            Flags = flags,
        };
    }

    [Fact]
    public void Build_SplitsStaticFromPolyAndHalvesATiedAxis()
    {
        var tree = new VmDoomAabb1283();
        tree.Build(new[]
        {
            Line(0, 0, 2, 0),
            Line(0, 2, 2, 2),
            Line(0, 1, 2, 1, back: true),
            Line(10, 10, 12, 10, sidedef: true, flags: VmDoomAabb1283.WallPoly),
        });
        Assert.Equal(new[] { 0, 1, 3 }, tree.MapLines);
        Assert.Equal(0, tree.DynamicStartLine);
        Assert.Equal(3, tree.DynamicStartNode);
        Assert.Equal(5, tree.Nodes.Count);
        Assert.Equal(1, tree.Nodes[0].LineIndex);
        Assert.Equal(2f, tree.Nodes[0].Top);
        Assert.Equal(2f, tree.Nodes[0].Bottom);
        Assert.Equal(0, tree.Nodes[1].LineIndex);
        Assert.Equal(0f, tree.Nodes[1].Bottom);
        Assert.Equal(-1, tree.Nodes[2].LineIndex);
        Assert.Equal(0, tree.Nodes[2].LeftNode);
        Assert.Equal(1, tree.Nodes[2].RightNode);
        Assert.Equal(2f, tree.Nodes[2].Right);
        Assert.Equal(2, tree.Nodes[3].LineIndex);
        Assert.Equal(10f, tree.Nodes[3].Left);
        Assert.Equal(2, tree.Nodes[4].LeftNode);
        Assert.Equal(3, tree.Nodes[4].RightNode);
        Assert.Equal(12f, tree.Nodes[4].Right);
        Assert.Equal(10f, tree.Nodes[4].Bottom);
        Assert.Equal(2f, tree.TreeLines[0].Dx);
        Assert.Equal(2f, tree.TreeLines[1].Y);
        Assert.Equal(10f, tree.TreeLines[2].X);

        var polyOnly = new VmDoomAabb1283();
        polyOnly.Build(new[] { Line(10, 10, 12, 10, sidedef: true, flags: VmDoomAabb1283.WallPoly) });
        Assert.Empty(polyOnly.Nodes);
        Assert.Empty(polyOnly.MapLines);
        Assert.Equal(0, polyOnly.DynamicStartNode);

        var barePolyFlag = new VmDoomAabb1283();
        barePolyFlag.Build(new[] { Line(0, 0, 1, 0, flags: VmDoomAabb1283.WallPoly) });
        Assert.Equal(new[] { 0 }, barePolyFlag.MapLines);
        var onlyNode = Assert.Single(barePolyFlag.Nodes);
        Assert.Equal(0, onlyNode.LineIndex);
        Assert.Equal(1, barePolyFlag.DynamicStartNode);

        var tied = new VmDoomAabb1283();
        tied.Build(new[]
        {
            Line(5, 5, 5, 5),
            Line(5, 5, 5, 5),
            Line(5, 5, 5, 5),
        });
        Assert.Equal(5, tied.Nodes.Count);
        Assert.Equal(5, tied.DynamicStartNode);
        Assert.Equal(0, tied.Nodes[0].LineIndex);
        Assert.Equal(1, tied.Nodes[1].LineIndex);
        Assert.Equal(2, tied.Nodes[2].LineIndex);
        Assert.Equal(1, tied.Nodes[3].LeftNode);
        Assert.Equal(2, tied.Nodes[3].RightNode);
        Assert.Equal(0, tied.Nodes[4].LeftNode);
        Assert.Equal(3, tied.Nodes[4].RightNode);
    }

    [Fact]
    public void Update_RefreshesParentsAndLeavesAPointOutsideTheBox()
    {
        var tree = new VmDoomAabb1283();
        var lines = new List<VmAabbLine1283>
        {
            Line(0, 0, 2, 0),
            Line(0, 2, 2, 2),
            Line(0, 1, 2, 1, back: true),
            Line(10, 10, 12, 10, sidedef: true, flags: VmDoomAabb1283.WallPoly),
        };
        tree.Build(lines);
        tree.TreeLines[0].X = -0f;
        Assert.True(tree.Update());
        Assert.Equal(0, BitConverter.SingleToInt32Bits(tree.TreeLines[0].X));
        Assert.Equal(2f, tree.Nodes[1].Right);

        lines[0].X2 = 4;
        Assert.True(tree.Update());
        Assert.Equal(4f, tree.TreeLines[0].Dx);
        Assert.Equal(4f, tree.Nodes[1].Right);
        Assert.Equal(0f, tree.Nodes[1].Bottom);
        Assert.Equal(4f, tree.Nodes[2].Right);
        Assert.Equal(2f, tree.Nodes[2].Bottom);
        Assert.Equal(12f, tree.Nodes[4].Right);
        Assert.Equal(10f, tree.Nodes[4].Bottom);
        Assert.False(tree.Update());

        tree.TreeLines[0].X = 1000f;
        lines[0].Y1 = 1;
        Assert.False(tree.Update());
        Assert.Equal(1000f, tree.TreeLines[0].X);
        Assert.Equal(0f, tree.TreeLines[0].Y);
        Assert.Equal(4f, tree.Nodes[1].Right);
    }
}
