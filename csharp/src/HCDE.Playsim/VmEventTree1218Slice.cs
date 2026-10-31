using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Splitter event tree from <c>nodebuild_events.cpp</c> (<b>1–210</b>).
/// Chunk <b>1218</b>. Equal distances go right. This file does not rebalance the tree.
/// The log line is glibc <c>%g</c> of <c>sqrt(distance / 2^32)</c>.
/// </summary>
public sealed class VmEventNode1218
{
    public VmEventNode1218? Parent;
    public VmEventNode1218? Left;
    public VmEventNode1218? Right;
    public double Distance;
    public int Vertex;
    public uint FrontSeg;
}

public sealed class VmEventTree1218 : IDisposable
{
    const double FixedScale = 4294967296.0;

    readonly VmEventNode1218 nil = new();
    VmEventNode1218 root;
    VmEventNode1218? spare;

    public VmEventTree1218()
    {
        root = nil;
    }

    internal VmEventNode1218 NilNode()
    {
        return nil;
    }

    public void Dispose()
    {
        DeleteAll();
        VmEventNode1218? probe = spare;
        while (probe != null)
        {
            VmEventNode1218? next = probe.Left;
            probe = next;
        }
        spare = null;
    }

    public void DeleteAll()
    {
        DeletionTraverser(root);
        root = nil;
    }

    void DeletionTraverser(VmEventNode1218? node)
    {
        if (node != nil && node != null)
        {
            DeletionTraverser(node.Left);
            DeletionTraverser(node.Right);
            node.Left = spare;
            spare = node;
        }
    }

    public VmEventNode1218 GetNewNode()
    {
        if (spare != null)
        {
            VmEventNode1218 node = spare;
            spare = node.Left;
            return node;
        }
        return new VmEventNode1218();
    }

    public void Insert(VmEventNode1218 node)
    {
        VmEventNode1218 parent = nil;
        VmEventNode1218 cursor = root;
        while (cursor != nil)
        {
            parent = cursor;
            if (node.Distance < cursor.Distance)
                cursor = cursor.Left!;
            else
                cursor = cursor.Right!;
        }
        node.Parent = parent;
        if (parent == nil)
            root = node;
        else if (node.Distance < parent.Distance)
            parent.Left = node;
        else
            parent.Right = node;
        node.Left = nil;
        node.Right = nil;
    }

    public VmEventNode1218 Successor(VmEventNode1218 node)
    {
        if (node.Right != nil)
        {
            node = node.Right!;
            while (node.Left != nil)
                node = node.Left!;
            return node;
        }
        VmEventNode1218 parent = node.Parent!;
        while (parent != nil && node == parent.Right)
        {
            node = parent;
            parent = node.Parent!;
        }
        return parent;
    }

    public VmEventNode1218 Predecessor(VmEventNode1218 node)
    {
        if (node.Left != nil)
        {
            node = node.Left!;
            while (node.Right != nil)
                node = node.Right!;
            return node;
        }
        VmEventNode1218 parent = node.Parent!;
        while (parent != nil && node == parent.Left)
        {
            node = parent;
            parent = node.Parent!;
        }
        return parent;
    }

    public VmEventNode1218? FindEvent(double key)
    {
        VmEventNode1218 node = root;
        while (node != nil)
        {
            if (node.Distance == key)
                return node;
            if (node.Distance > key)
                node = node.Left!;
            else
                node = node.Right!;
        }
        return null;
    }

    public VmEventNode1218? GetMinimum()
    {
        VmEventNode1218 node = root;
        if (node == nil)
            return null;
        while (node.Left != nil)
            node = node.Left!;
        return node;
    }

    public string Print()
    {
        var text = new StringBuilder();
        PrintTree(root, text);
        return text.ToString();
    }

    void PrintTree(VmEventNode1218 node, StringBuilder text)
    {
        if (node != nil)
        {
            PrintTree(node.Left!, text);
            double shown = Math.Sqrt(node.Distance / FixedScale);
            string line = " Distance " + FormatG(shown)
                + ", vertex " + node.Vertex.ToString(CultureInfo.InvariantCulture)
                + ", seg " + node.FrontSeg.ToString(CultureInfo.InvariantCulture)
                + "\n";
            if (line.Length >= 100)
                line = line.Substring(0, 99);
            text.Append(line);
            PrintTree(node.Right!, text);
        }
    }

    internal static string FormatG(double value)
    {
        if (double.IsNaN(value))
        {
            if (BitConverter.DoubleToInt64Bits(value) < 0)
                return "-nan";
            return "nan";
        }
        if (double.IsPositiveInfinity(value))
            return "inf";
        if (double.IsNegativeInfinity(value))
            return "-inf";
        return value.ToString("G6", CultureInfo.InvariantCulture).Replace('E', 'e');
    }
}
