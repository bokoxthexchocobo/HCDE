namespace HCDE.Playsim.Tests;

public class VmEventTree1218SliceTests
{
    static double Bits(string hex)
    {
        return BitConverter.Int64BitsToDouble(unchecked((long)Convert.ToUInt64(hex, 16)));
    }

    static VmEventNode1218 Add(VmEventTree1218 tree, double distance, int vertex, uint seg)
    {
        VmEventNode1218 node = tree.GetNewNode();
        node.Distance = distance;
        node.Vertex = vertex;
        node.FrontSeg = seg;
        tree.Insert(node);
        return node;
    }

    [Fact]
    public void Insert_PlacesEqualKeysRightAndPrintsTheSqrtDistance()
    {
        var tree = new VmEventTree1218();
        Assert.Null(tree.GetMinimum());
        Assert.Equal("", tree.Print());

        VmEventNode1218 n1 = Add(tree, 5, 1, 10);
        VmEventNode1218 n2 = Add(tree, 3, 2, 20);
        VmEventNode1218 n3 = Add(tree, 7, 3, 30);
        VmEventNode1218 n4 = Add(tree, 3, 4, 40);
        VmEventNode1218 n5 = Add(tree, 0.0, 5, 50);
        VmEventNode1218 n6 = Add(tree, -0.0, 6, 60);
        VmEventNode1218 n7 = Add(tree, Bits("7ff8000000000000"), 7, 70);

        Assert.Same(tree.NilNode(), n1.Parent);
        Assert.Same(n2, n1.Left);
        Assert.Same(n3, n1.Right);
        Assert.Same(n1, n2.Parent);
        Assert.Same(n5, n2.Left);
        Assert.Same(n4, n2.Right);
        Assert.Same(n7, n3.Right);
        Assert.Same(tree.NilNode(), n4.Left);
        Assert.Same(n6, n5.Right);
        Assert.Same(n5, n6.Parent);

        Assert.Same(n5, tree.GetMinimum());
        Assert.Same(n2, tree.FindEvent(3));
        Assert.Same(n4, tree.Successor(n2));
        Assert.Same(n6, tree.Predecessor(n2));
        Assert.Same(n5, tree.FindEvent(0.0));
        Assert.Same(n5, tree.FindEvent(-0.0));
        Assert.Null(tree.FindEvent(Bits("7ff8000000000000")));
        Assert.Null(tree.FindEvent(9));
        Assert.Same(n3, tree.Successor(n1));
        Assert.Same(n4, tree.Predecessor(n1));
        Assert.Same(n6, tree.Successor(tree.GetMinimum()!));
        Assert.Same(n3, tree.Predecessor(n7));
        Assert.Same(tree.NilNode(), tree.Successor(n7));

        Assert.Equal(
            " Distance 0, vertex 5, seg 50\n"
            + " Distance -0, vertex 6, seg 60\n"
            + " Distance 2.6429e-05, vertex 2, seg 20\n"
            + " Distance 2.6429e-05, vertex 4, seg 40\n"
            + " Distance 3.41197e-05, vertex 1, seg 10\n"
            + " Distance 4.0371e-05, vertex 3, seg 30\n"
            + " Distance nan, vertex 7, seg 70\n",
            tree.Print());

        (string hex, string text)[] samples =
        {
            ("0000000000000000", "0"),
            ("8000000000000000", "-0"),
            ("3ff0000000000000", "1"),
            ("bff0000000000000", "-1"),
            ("4024000000000000", "10"),
            ("4028b0f27bb2fec5", "12.3456"),
            ("4028b0fba8826aa9", "12.3457"),
            ("4023fffeb074a772", "9.99999"),
            ("4023ffffde7210bf", "10"),
            ("412e847e00000000", "999999"),
            ("412e848000000000", "1e+06"),
            ("3f1a36e2eb1c432d", "0.0001"),
            ("3f1a36d1bd105b06", "9.9999e-05"),
            ("3ee4f8b588e368f1", "1e-05"),
            ("3e7ad7f29abcaf48", "1e-07"),
            ("40fe240b33333333", "123457"),
            ("3ff6a09e667f3bcd", "1.41421"),
            ("3efbb67ae8584caa", "2.6429e-05"),
            ("3f01e3779b97f4a8", "3.41197e-05"),
            ("3f052a7fa9d2f8ea", "4.0371e-05"),
            ("3f75f5d32b60abe7", "0.00536139"),
            ("3ef00000c06a1ef5", "1.52588e-05"),
            ("bff8000000000000", "-1.5"),
            ("4004000000000000", "2.5"),
            ("3ff000010c6f7a0b", "1"),
            ("3fb999999999999a", "0.1"),
            ("3fbeb851eb851eb8", "0.12"),
            ("3fbf9adbb8f8da72", "0.123457"),
            ("4058fffe5c91d14e", "99.9999"),
            ("4058ffffd60e94ee", "100"),
            ("7ff8000000000000", "nan"),
            ("fff8000000000000", "-nan"),
            ("7ff0000000000000", "inf"),
            ("fff0000000000000", "-inf"),
            ("41efffffffe00000", "4.29497e+09"),
            ("4415af1d78b58c40", "1e+20"),
            ("3ddb7cdfd9d7bdbb", "1e-10"),
            ("401a36e2eb1c432d", "6.5536"),
        };
        foreach ((string hex, string text) in samples)
            Assert.Equal(text, VmEventTree1218.FormatG(Bits(hex)));
    }

    [Fact]
    public void DeleteAll_RecyclesNodesInPostOrder()
    {
        var tree = new VmEventTree1218();
        VmEventNode1218[] nodes =
        {
            Add(tree, 5, 1, 10),
            Add(tree, 3, 2, 20),
            Add(tree, 7, 3, 30),
            Add(tree, 3, 4, 40),
            Add(tree, 0.0, 5, 50),
            Add(tree, -0.0, 6, 60),
            Add(tree, Bits("7ff8000000000000"), 7, 70),
        };
        tree.DeleteAll();
        Assert.Null(tree.GetMinimum());
        Assert.Equal("", tree.Print());

        int[] recycled = { 1, 3, 7, 2, 4, 5, 6 };
        var popped = new VmEventNode1218[7];
        for (int i = 0; i < recycled.Length; i++)
        {
            popped[i] = tree.GetNewNode();
            Assert.Same(nodes[recycled[i] - 1], popped[i]);
            Assert.Equal(recycled[i], popped[i].Vertex);
        }
        VmEventNode1218 fresh = tree.GetNewNode();
        Assert.DoesNotContain(popped, node => ReferenceEquals(node, fresh));
        Assert.DoesNotContain(nodes, node => ReferenceEquals(node, fresh));

        tree.Dispose();
        VmEventNode1218 after = tree.GetNewNode();
        Assert.DoesNotContain(nodes, node => ReferenceEquals(node, after));
    }
}
