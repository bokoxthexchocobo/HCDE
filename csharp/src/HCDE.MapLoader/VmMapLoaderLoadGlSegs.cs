namespace HCDE.MapLoader;

/// <summary>
/// <c>MapLoader::checkGLVertex</c>, <c>checkGLVertex3</c>, and <c>LoadGLSegs</c> from <c>glnodes.cpp</c> (241–415).
/// </summary>
public static class VmMapLoaderLoadGlSegs
{
    public const ushort MinisegLinedef = 0xffff;
    public const ushort InvalidPartner16 = 0xffff;
    public const uint InvalidPartner32 = 0xffffffffu;
    private static readonly byte[] GlNode3Magic = "gNd3"u8.ToArray();

    public static int CheckGlVertex(int num, int firstGlVertex)
    {
        if ((num & 0x8000) != 0)
            num = (num & 0x7FFF) + firstGlVertex;
        return num;
    }

    public static int CheckGlVertex3(int num, int firstGlVertex)
    {
        if ((num & 0xc0000000) != 0)
            num = (int)((num & 0x3FFFFFFF) + firstGlVertex);
        return num;
    }

    public sealed class MapSide
    {
        public MapSector? Sector { get; set; }
    }

    public sealed class MapLine
    {
        public ushort Flags { get; set; }
        public MapSide?[] Sidedef { get; } = new MapSide?[2];
    }

    public sealed class MapSector
    {
        public int Index { get; set; }
    }

    public sealed class GlSeg
    {
        public int V1Index { get; set; }
        public int V2Index { get; set; }
        public MapLine? Linedef { get; set; }
        public MapSide? Sidedef { get; set; }
        public MapSector? FrontSector { get; set; }
        public MapSector? BackSector { get; set; }
        public int? PartnerIndex { get; set; }
    }

    public sealed class LoadGlSegsLevel
    {
        public int VertexCount { get; set; }
        public List<MapLine> Lines { get; } = new();
        public List<GlSeg> Segs { get; } = new();
    }

    public static bool LoadGlSegs(
        ReadOnlySpan<byte> lumpData,
        bool format5,
        int firstGlVertex,
        LoadGlSegsLevel level)
    {
        if (lumpData.Length < 4)
            return false;

        var numVertexes = (uint)level.VertexCount;
        var numLines = (uint)level.Lines.Count;

        if (!format5 && !StartsWithMagic(lumpData, GlNode3Magic))
            return LoadClassicGlSegs(lumpData, firstGlVertex, level, numVertexes, numLines);

        return LoadGl3Segs(lumpData, format5, firstGlVertex, level, numVertexes, numLines);
    }

    static bool LoadClassicGlSegs(
        ReadOnlySpan<byte> data,
        int firstGlVertex,
        LoadGlSegsLevel level,
        uint numVertexes,
        uint numLines)
    {
        var recordSize = 10; // glseg_t
        var numSegs = data.Length / recordSize;
        if (numSegs == 0)
            return false;

        level.Segs.Clear();
        for (var i = 0; i < numSegs; i++)
        {
            var o = i * recordSize;
            var v1Raw = ReadUInt16(data, o);
            var v2Raw = ReadUInt16(data, o + 2);
            var linedefRaw = ReadUInt16(data, o + 4);
            var side = ReadUInt16(data, o + 6);
            var partnerRaw = ReadUInt16(data, o + 8);

            var v1idx = (uint)CheckGlVertex(v1Raw, firstGlVertex);
            if (v1idx >= numVertexes)
                return false;
            var v2idx = (uint)CheckGlVertex(v2Raw, firstGlVertex);
            if (v2idx >= numVertexes)
                return false;

            var seg = new GlSeg { V1Index = (int)v1idx, V2Index = (int)v2idx };
            if (partnerRaw == InvalidPartner16)
                seg.PartnerIndex = null;
            else if (partnerRaw >= numSegs)
                return false;
            else
                seg.PartnerIndex = partnerRaw;

            if (linedefRaw != MinisegLinedef)
            {
                if (linedefRaw >= numLines)
                    return false;
                var ldef = level.Lines[linedefRaw];
                seg.Linedef = ldef;
                if (side > 1)
                    return false;
                seg.Sidedef = ldef.Sidedef[side];
                if (ldef.Sidedef[side] is not null)
                    seg.FrontSector = ldef.Sidedef[side]!.Sector;
                else
                    seg.FrontSector = null;

                var backSide = side ^ 1;
                if ((ldef.Flags & MapLoaderLineFlags.TwoSided) != 0 && ldef.Sidedef[backSide] is not null)
                    seg.BackSector = ldef.Sidedef[backSide]!.Sector;
                else
                {
                    ldef.Flags &= unchecked((ushort)~MapLoaderLineFlags.TwoSided);
                    seg.BackSector = null;
                }
            }
            else
            {
                seg.Linedef = null;
                seg.Sidedef = null;
                seg.FrontSector = null;
                seg.BackSector = null;
            }

            level.Segs.Add(seg);
        }

        return true;
    }

    static bool LoadGl3Segs(
        ReadOnlySpan<byte> data,
        bool format5,
        int firstGlVertex,
        LoadGlSegsLevel level,
        uint numVertexes,
        uint numLines)
    {
        var payload = data;
        if (!format5)
            payload = data.Length >= 4 ? data[4..] : ReadOnlySpan<byte>.Empty;

        var recordSize = 16; // glseg3_t
        var numSegs = payload.Length / recordSize;
        if (numSegs == 0)
            return false;

        level.Segs.Clear();
        for (var i = 0; i < numSegs; i++)
        {
            var o = i * recordSize;
            var v1Raw = ReadInt32(payload, o);
            var v2Raw = ReadInt32(payload, o + 4);
            var linedefRaw = ReadUInt16(payload, o + 8);
            var side = ReadUInt16(payload, o + 10);
            var partnerRaw = ReadUInt32(payload, o + 12);

            var v1idx = (uint)CheckGlVertex3(v1Raw, firstGlVertex);
            if (v1idx >= numVertexes)
                return false;
            var v2idx = (uint)CheckGlVertex3(v2Raw, firstGlVertex);
            if (v2idx >= numVertexes)
                return false;

            var seg = new GlSeg { V1Index = (int)v1idx, V2Index = (int)v2idx };
            if (partnerRaw == InvalidPartner32)
                seg.PartnerIndex = null;
            else if (partnerRaw >= (uint)numSegs)
                return false;
            else
                seg.PartnerIndex = (int)partnerRaw;

            if (linedefRaw != MinisegLinedef)
            {
                if (linedefRaw >= numLines)
                    return false;
                var ldef = level.Lines[linedefRaw];
                seg.Linedef = ldef;
                if (side > 1)
                    return false;
                seg.Sidedef = ldef.Sidedef[side];
                if (ldef.Sidedef[side] is not null)
                    seg.FrontSector = ldef.Sidedef[side]!.Sector;
                else
                    seg.FrontSector = null;

                var backSide = side ^ 1;
                if ((ldef.Flags & MapLoaderLineFlags.TwoSided) != 0 && ldef.Sidedef[backSide] is not null)
                    seg.BackSector = ldef.Sidedef[backSide]!.Sector;
                else
                {
                    ldef.Flags &= unchecked((ushort)~MapLoaderLineFlags.TwoSided);
                    seg.BackSector = null;
                }
            }
            else
            {
                seg.Linedef = null;
                seg.Sidedef = null;
                seg.FrontSector = null;
                seg.BackSector = null;
            }

            level.Segs.Add(seg);
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

    static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));

    static int ReadInt32(ReadOnlySpan<byte> data, int offset) =>
        data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);

    static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => (uint)ReadInt32(data, offset);
}
