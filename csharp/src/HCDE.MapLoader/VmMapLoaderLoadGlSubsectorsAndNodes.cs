namespace HCDE.MapLoader;

/// <summary>
/// <c>MapLoader::LoadGLSubsectors</c> and <c>LoadNodes</c> from <c>glnodes.cpp</c> (424–643).
/// </summary>
public static class VmMapLoaderLoadGlSubsectorsAndNodes
{
    public const int NfSubsector = 0x8000;
    public const int Gl5NfSubsector = 1 << 31;
    private const int FracBits = 16;
    private static readonly byte[] GlNode3Magic = "gNd3"u8.ToArray();

    public sealed class GlSubsector
    {
        public int NumLines { get; set; }
        public int FirstSegIndex { get; set; }
    }

    public readonly struct BspChild
    {
        public bool IsSubsector { get; init; }
        public int Index { get; init; }
    }

    public sealed class BspNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }
        public float[][] Bbox { get; } = { new float[4], new float[4] };
        public BspChild[] Children { get; } = { default, default };
    }

    public sealed class GlSubsectorSeg
    {
        public int V1Index { get; set; }
        public int V2Index { get; set; }
        public VmMapLoaderLoadGlSegs.MapLine? Linedef { get; set; }
        public VmMapLoaderLoadGlSegs.MapSector? FrontSector { get; set; }
        public VmMapLoaderLoadGlSegs.MapSector? BackSector { get; set; }
    }

    public sealed class LoadGlSubsectorsLevel
    {
        public List<GlSubsectorSeg> Segs { get; } = new();
        public List<GlSubsector> Subsectors { get; } = new();
    }

    public sealed class LoadGlNodesLevel
    {
        public int SubsectorCount { get; set; }
        public List<BspNode> Nodes { get; } = new();
    }

    public static bool LoadGlSubsectors(ReadOnlySpan<byte> lumpData, bool format5, LoadGlSubsectorsLevel level)
    {
        if (lumpData.Length < 4)
            return false;

        var numSegs = (uint)level.Segs.Count;

        if (!format5 && !StartsWithMagic(lumpData, GlNode3Magic))
            return LoadClassicGlSubsectors(lumpData, level, numSegs);

        return LoadGl3Subsectors(lumpData, format5, level, numSegs);
    }

    static bool LoadClassicGlSubsectors(ReadOnlySpan<byte> data, LoadGlSubsectorsLevel level, uint numSegs)
    {
        const int recordSize = 4;
        var numSubsectors = data.Length / recordSize;
        if (numSubsectors == 0)
            return false;

        level.Subsectors.Clear();
        for (var i = 0; i < numSubsectors; i++)
            level.Subsectors.Add(new GlSubsector());

        for (var i = 0; i < numSubsectors; i++)
        {
            var o = i * recordSize;
            var numLines = ReadUInt16(data, o);
            if (numLines == 0)
                return false;

            var firstSeg = ReadUInt16(data, o + 2);
            if (firstSeg >= numSegs)
                return false;

            level.Subsectors[i].NumLines = numLines;
            level.Subsectors[i].FirstSegIndex = firstSeg;
        }

        return FinalizeSubsectorSegSectors(level);
    }

    static bool LoadGl3Subsectors(ReadOnlySpan<byte> data, bool format5, LoadGlSubsectorsLevel level, uint numSegs)
    {
        var payload = format5 ? data : (data.Length >= 4 ? data[4..] : ReadOnlySpan<byte>.Empty);
        const int recordSize = 8;
        var numSubsectors = payload.Length / recordSize;
        if (numSubsectors == 0)
            return false;

        level.Subsectors.Clear();
        for (var i = 0; i < numSubsectors; i++)
            level.Subsectors.Add(new GlSubsector());

        for (var i = 0; i < numSubsectors; i++)
        {
            var o = i * recordSize;
            var numLines = ReadInt32(payload, o);
            if (numLines == 0)
                return false;

            var firstSeg = ReadUInt32(payload, o + 4);
            if (firstSeg >= numSegs)
                return false;

            level.Subsectors[i].NumLines = numLines;
            level.Subsectors[i].FirstSegIndex = (int)firstSeg;
        }

        return FinalizeSubsectorSegSectors(level);
    }

    static bool FinalizeSubsectorSegSectors(LoadGlSubsectorsLevel level)
    {
        foreach (var sub in level.Subsectors)
        {
            for (var j = 0; j < sub.NumLines; j++)
            {
                var segIndex = sub.FirstSegIndex + j;
                var seg = level.Segs[segIndex];
                if (seg.Linedef is null)
                {
                    var anchor = level.Segs[sub.FirstSegIndex];
                    seg.FrontSector = anchor.FrontSector;
                    seg.BackSector = anchor.FrontSector;
                }
            }

            var firstSeg = level.Segs[sub.FirstSegIndex];
            var lastSeg = level.Segs[sub.FirstSegIndex + sub.NumLines - 1];
            if (lastSeg.V2Index != firstSeg.V1Index)
                return false;
        }

        return true;
    }

    public static bool LoadGlNodes(ReadOnlySpan<byte> lumpData, bool format5, LoadGlNodesLevel level)
    {
        if (!format5)
            return LoadClassicGlNodes(lumpData, level);

        return LoadGl5GlNodes(lumpData, level);
    }

    static bool LoadClassicGlNodes(ReadOnlySpan<byte> data, LoadGlNodesLevel level)
    {
        const int recordSize = 28;
        var numNodes = data.Length / recordSize;
        if (numNodes == 0)
            return false;

        var used = new int[numNodes];
        level.Nodes.Clear();
        for (var i = 0; i < numNodes; i++)
            level.Nodes.Add(new BspNode());

        for (var i = 0; i < numNodes; i++)
        {
            var o = i * recordSize;
            var node = level.Nodes[i];
            node.X = ReadInt16(data, o) << FracBits;
            node.Y = ReadInt16(data, o + 2) << FracBits;
            node.Dx = ReadInt16(data, o + 4) << FracBits;
            node.Dy = ReadInt16(data, o + 6) << FracBits;

            for (var j = 0; j < 2; j++)
            {
                var child = ReadUInt16(data, o + 24 + j * 2);
                var resolution = VmMapLoaderClassicNodes.ResolveChild(
                    child,
                    NfSubsector,
                    level.SubsectorCount,
                    (int)numNodes,
                    i,
                    used);

                switch (resolution)
                {
                    case VmMapLoaderClassicNodes.ChildResolution.InvalidSubsector:
                    case VmMapLoaderClassicNodes.ChildResolution.InvalidNode:
                    case VmMapLoaderClassicNodes.ChildResolution.NodeAlreadyUsed:
                        return false;
                    case VmMapLoaderClassicNodes.ChildResolution.SubsectorChild:
                        node.Children[j] = new BspChild
                        {
                            IsSubsector = true,
                            Index = child & ~NfSubsector,
                        };
                        break;
                    case VmMapLoaderClassicNodes.ChildResolution.NodeChild:
                        node.Children[j] = new BspChild { IsSubsector = false, Index = child };
                        VmMapLoaderClassicNodes.MarkNodeUsed(used, child, i, j);
                        break;
                }

                for (var k = 0; k < 4; k++)
                    node.Bbox[j][k] = ReadInt16(data, o + 8 + j * 8 + k * 2);
            }
        }

        return true;
    }

    static bool LoadGl5GlNodes(ReadOnlySpan<byte> data, LoadGlNodesLevel level)
    {
        const int recordSize = 32;
        var numNodes = data.Length / recordSize;
        if (numNodes == 0)
            return false;

        var used = new int[numNodes];
        level.Nodes.Clear();
        for (var i = 0; i < numNodes; i++)
            level.Nodes.Add(new BspNode());

        for (var i = 0; i < numNodes; i++)
        {
            var o = i * recordSize;
            var node = level.Nodes[i];
            node.X = ReadInt16(data, o) << FracBits;
            node.Y = ReadInt16(data, o + 2) << FracBits;
            node.Dx = ReadInt16(data, o + 4) << FracBits;
            node.Dy = ReadInt16(data, o + 6) << FracBits;

            for (var j = 0; j < 2; j++)
            {
                var child = ReadInt32(data, o + 24 + j * 4);
                var resolution = VmMapLoaderClassicNodes.ResolveChild(
                    child,
                    Gl5NfSubsector,
                    level.SubsectorCount,
                    (int)numNodes,
                    i,
                    used);

                switch (resolution)
                {
                    case VmMapLoaderClassicNodes.ChildResolution.InvalidSubsector:
                    case VmMapLoaderClassicNodes.ChildResolution.InvalidNode:
                    case VmMapLoaderClassicNodes.ChildResolution.NodeAlreadyUsed:
                        return false;
                    case VmMapLoaderClassicNodes.ChildResolution.SubsectorChild:
                        node.Children[j] = new BspChild
                        {
                            IsSubsector = true,
                            Index = child & ~Gl5NfSubsector,
                        };
                        break;
                    case VmMapLoaderClassicNodes.ChildResolution.NodeChild:
                        node.Children[j] = new BspChild { IsSubsector = false, Index = child };
                        VmMapLoaderClassicNodes.MarkNodeUsed(used, child, i, j);
                        break;
                }

                for (var k = 0; k < 4; k++)
                    node.Bbox[j][k] = ReadInt16(data, o + 8 + j * 8 + k * 2);
            }
        }

        return true;
    }

    static bool StartsWithMagic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> magic)
    {
        if (data.Length < magic.Length)
            return false;
        for (var i = 0; i < magic.Length; i++)
        {
            if (data[i] != magic[i])
                return false;
        }

        return true;
    }

    static short ReadInt16(ReadOnlySpan<byte> data, int offset) =>
        (short)(data[offset] | (data[offset + 1] << 8));

    static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));

    static int ReadInt32(ReadOnlySpan<byte> data, int offset) =>
        data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);

    static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => (uint)ReadInt32(data, offset);
}
