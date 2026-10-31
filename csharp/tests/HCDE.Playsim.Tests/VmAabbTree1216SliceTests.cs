namespace HCDE.Playsim.Tests;

public class VmAabbTree1216SliceTests
{
    static VmAabbNode1216 Node(float left, float top, float right, float bottom, int leftNode, int rightNode, int line) => new()
    {
        Left = left,
        Top = top,
        Right = right,
        Bottom = bottom,
        LeftNode = leftNode,
        RightNode = rightNode,
        LineIndex = line,
    };

    static VmAabbLine1216 Line(float x, float y, float dx, float dy) => new()
    {
        X = x,
        Y = y,
        Dx = dx,
        Dy = dy,
    };

    static readonly VmAabbLine1216[] Lines =
    {
        Line(2f, 0f, 0f, 1f),
        Line(4f, 0f, 0f, 1f),
        Line(0f, 2f, 10f, 0f),
    };

    static VmAabbNode1216[] Pair()
    {
        return new[]
        {
            Node(0f, 0f, 3f, 1f, -1, -1, 0),
            Node(3f, 0f, 10f, 1f, -1, -1, 1),
            Node(0f, 0f, 10f, 1f, 0, 1, -1),
        };
    }

    [Fact]
    public void FindNodePath_ReturnsTheLeafThenItsParents()
    {
        var tree = Pair();
        Assert.Equal(new[] { 0, 2 }, VmAabbTree1216Slice.FindNodePath(tree, Lines, 0, 2));
        Assert.Equal(new[] { 1, 2 }, VmAabbTree1216Slice.FindNodePath(tree, Lines, 1, 2));
        Assert.Empty(VmAabbTree1216Slice.FindNodePath(tree, Lines, 2, 2));
        Assert.Empty(VmAabbTree1216Slice.FindNodePath(tree, Lines, 1, 0));
        var edge = new[] { Node(2f, 0f, 2f, 0f, -1, -1, 0) };
        Assert.Equal(new[] { 0 }, VmAabbTree1216Slice.FindNodePath(edge, Lines, 0, 0));
        var outside = new[] { Node(2f, 0f, 2f, -0.1f, -1, -1, 0) };
        Assert.Empty(VmAabbTree1216Slice.FindNodePath(outside, Lines, 0, 0));
        var swapped = new[]
        {
            Node(3f, 0f, 10f, 1f, -1, -1, 1),
            Node(0f, 0f, 3f, 1f, -1, -1, 0),
            Node(0f, 0f, 10f, 1f, 0, 1, -1),
        };
        Assert.Equal(new[] { 1, 2 }, VmAabbTree1216Slice.FindNodePath(swapped, Lines, 0, 2));
    }

    static void Ray(VmAabbNode1216[] nodes, double sx, double sy, double sz, double ex, double ey, double ez, ulong bits)
    {
        double hit = VmAabbTree1216Slice.RayTest(nodes, Lines, sx, sy, sz, ex, ey, ez);
        Assert.Equal(bits, BitConverter.DoubleToUInt64Bits(hit));
    }

    [Fact]
    public void RayTest_KeepsTheNearestHitAndSkipsATooDeepTree()
    {
        const ulong Fifth = 0x3fc999999999999a;
        const ulong TwoFifths = 0x3fd999999999999a;
        const ulong Miss = 0x3ff0000000000000;
        var tree = Pair();
        Ray(tree, 0, 0.5, 0, 10, 0.5, 0, Fifth);
        Ray(tree, 0, 0.5, 9, 10, 0.5, -4, Fifth);
        Ray(tree, 0, 0.5, 0, 0.5, 0.5, 0, Miss);
        Ray(tree, 0, 0.5, 0, 1, 0.5, 0, Miss);
        Ray(tree, 5, 0.5, 0, 10, 0.5, 0, Miss);
        Ray(new[] { Node(0f, 0f, 10f, 3f, -1, -1, 2) }, 0, 0.5, 0, 10, 0.5, 0, Miss);
        Ray(new[] { Node(0f, 0f, 3f, 1f, -1, -1, 0), Node(20f, 20f, 21f, 21f, 0, 0, -1) }, 0, 0.5, 0, 10, 0.5, 0, Miss);
        var deep = new VmAabbNode1216[34];
        deep[0] = Node(100f, 100f, 101f, 101f, -1, -1, 2);
        deep[1] = Node(0f, 0f, 10f, 1f, -1, -1, 0);
        for (int i = 0; i < 32; i++)
            deep[2 + i] = Node(0f, 0f, 10f, 1f, 0, i == 0 ? 1 : 2 + i - 1, -1);
        Ray(deep, 0, 0.5, 0, 10, 0.5, 0, Miss);
        Ray(new[] { deep[0], deep[1], Node(0f, 0f, 10f, 1f, 0, 1, -1) }, 0, 0.5, 0, 10, 0.5, 0, Fifth);
        Ray(new[] { Node(3f, 0f, 10f, 1f, -1, -1, 1) }, 0, 0.5, 0, 10, 0.5, 0, TwoFifths);
        var swapped = new[]
        {
            Node(3f, 0f, 10f, 1f, -1, -1, 1),
            Node(0f, 0f, 3f, 1f, -1, -1, 0),
            Node(0f, 0f, 10f, 1f, 0, 1, -1),
        };
        Ray(swapped, 0, 0.5, 0, 10, 0.5, 0, Fifth);
        Assert.Throws<IndexOutOfRangeException>(() => VmAabbTree1216Slice.RayTest(Array.Empty<VmAabbNode1216>(), Lines, 0, 0, 0, 10, 0, 0));
    }
}
