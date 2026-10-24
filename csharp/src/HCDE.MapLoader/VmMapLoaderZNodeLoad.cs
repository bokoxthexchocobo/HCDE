namespace HCDE.MapLoader;

/// <summary>
/// <c>LoadVertexes</c>, <c>LoadZSegs</c>, <c>LoadGLZSegs</c>, and <c>LoadZNodes</c> from <c>maploader.cpp</c> (403–664).
/// </summary>
public static class VmMapLoaderZNodeLoad
{
    public readonly struct VertexRecord
    {
        public VertexRecord(short x, short y)
        {
            X = x;
            Y = y;
        }

        public short X { get; }
        public short Y { get; }
    }

    public static int CountVertexes(int lumpBytes, int recordSize = 4)
    {
        if (recordSize <= 0)
            return 0;
        return lumpBytes / recordSize;
    }

    public static void EnsureHasVertexes(int count)
    {
        if (count == 0)
            throw new InvalidOperationException("Map has no vertices.");
    }

    public static VertexRecord[] ReadVertexRecords(ReadOnlySpan<byte> lump)
    {
        var count = lump.Length / 4;
        var result = new VertexRecord[count];
        for (var i = 0; i < count; i++)
        {
            var o = i * 4;
            var x = (short)(lump[o] | (lump[o + 1] << 8));
            var y = (short)(lump[o + 2] | (lump[o + 3] << 8));
            result[i] = new VertexRecord(x, y);
        }

        return result;
    }

    public readonly struct ZSegRecord
    {
        public ZSegRecord(uint v1, uint v2, ushort line, byte side)
        {
            V1 = v1;
            V2 = v2;
            Line = line;
            Side = side;
        }

        public uint V1 { get; }
        public uint V2 { get; }
        public ushort Line { get; }
        public byte Side { get; }
    }

    public static ZSegRecord ReadZSegRecord(ReadOnlySpan<byte> data, ref int offset)
    {
        var v1 = ReadUInt32(data, ref offset);
        var v2 = ReadUInt32(data, ref offset);
        var line = ReadUInt16(data, ref offset);
        var side = data[offset++];
        return new ZSegRecord(v1, v2, line, side);
    }

    public static bool IsTwoSided(ushort lineFlags, bool backSideDefPresent) =>
        (lineFlags & MapLoaderLineFlags.TwoSided) != 0 && backSideDefPresent;

    public static void ValidateGlPartner(uint partner, uint segCount, int subsectorIndex, int segIndex)
    {
        if (partner != 0xffffffffu && partner >= segCount)
            throw new InvalidOperationException(
                $"partner seg index out of range for subsector {subsectorIndex}, seg {segIndex}");
    }

    public static uint ReadGlLineIndex(ReadOnlySpan<byte> data, ref int offset, int glNodeType)
    {
        if (glNodeType >= 2)
            return ReadUInt32(data, ref offset);

        var line = ReadUInt16(data, ref offset);
        return line == 0xffff ? 0xffffffffu : line;
    }

    public static void EnsureVertexNodeCounts(uint orgVerts, int levelVertexCount)
    {
        if (orgVerts > levelVertexCount)
            throw new InvalidOperationException("Incorrect number of vertexes in nodes.");
    }

    public static void EnsureSegCountMatches(uint numSegs, uint currSeg)
    {
        if (numSegs != currSeg)
            throw new InvalidOperationException("Incorrect number of segs in nodes.");
    }

    public static int ComputeNewVertexReserve(uint orgVerts, uint newVerts, int levelVertexCount)
    {
        EnsureVertexNodeCounts(orgVerts, levelVertexCount);
        return orgVerts + newVerts == levelVertexCount ? 0 : (int)newVerts;
    }

    public static uint ReadUInt32(ReadOnlySpan<byte> data, ref int offset)
    {
        var v = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
        offset += 4;
        return v;
    }

    public static ushort ReadUInt16(ReadOnlySpan<byte> data, ref int offset)
    {
        var v = (ushort)(data[offset] | (data[offset + 1] << 8));
        offset += 2;
        return v;
    }

    public static ushort ClearTwoSidedIfNoBack(ushort lineFlags, bool hasBackSide)
    {
        if (!IsTwoSided(lineFlags, hasBackSide))
            return (ushort)(lineFlags & ~MapLoaderLineFlags.TwoSided);
        return lineFlags;
    }

    public static uint AccumulateSubsectorSegs(uint currSeg, uint numSegsInSub) => currSeg + numSegsInSub;

    public static bool GlSegHasLinedef(uint lineIndex) => lineIndex != 0xffffffffu;

    public static int ReadNodeChildIsSubsector(uint child) => (child & 0x80000000) != 0 ? 1 : 0;

    public static int ReadNodeSubsectorIndex(uint child) => (int)(child & 0x7fffffff);
}

public static class MapLoaderLineFlags
{
    public const ushort TwoSided = 0x0004;
}
