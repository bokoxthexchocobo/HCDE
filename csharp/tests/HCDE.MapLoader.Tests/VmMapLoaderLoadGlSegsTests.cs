namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadGlSegsTests
{
    [Fact]
    public void CheckGlVertex_RemapsHighBitThroughFirstGlVertex()
    {
        Assert.Equal(105, VmMapLoaderLoadGlSegs.CheckGlVertex(0x8005, 100));
        Assert.Equal(3, VmMapLoaderLoadGlSegs.CheckGlVertex(3, 100));
    }

    [Fact]
    public void CheckGlVertex3_RemapsSignBitThroughFirstGlVertex()
    {
        Assert.Equal(110, VmMapLoaderLoadGlSegs.CheckGlVertex3(unchecked((int)0xC000000A), 100));
    }

    [Fact]
    public void LoadGlSegs_ClassicMiniseg_PopulatesSegWithoutLinedef()
    {
        var level = new VmMapLoaderLoadGlSegs.LoadGlSegsLevel { VertexCount = 4 };
        Span<byte> lump = stackalloc byte[10];
        WriteUInt16(lump, 0, 1);
        WriteUInt16(lump, 2, 2);
        WriteUInt16(lump, 4, VmMapLoaderLoadGlSegs.MinisegLinedef);
        WriteUInt16(lump, 6, 0);
        WriteUInt16(lump, 8, VmMapLoaderLoadGlSegs.InvalidPartner16);

        Assert.True(VmMapLoaderLoadGlSegs.LoadGlSegs(lump, format5: false, firstGlVertex: 0, level));
        Assert.Single(level.Segs);
        Assert.Null(level.Segs[0].Linedef);
        Assert.Equal(1, level.Segs[0].V1Index);
        Assert.Equal(2, level.Segs[0].V2Index);
    }

    [Fact]
    public void LoadGlSegs_ClassicLinedef_ClearsTwoSidedWhenBackMissing()
    {
        var frontSector = new VmMapLoaderLoadGlSegs.MapSector();
        var line = new VmMapLoaderLoadGlSegs.MapLine { Flags = MapLoaderLineFlags.TwoSided };
        line.Sidedef[0] = new VmMapLoaderLoadGlSegs.MapSide { Sector = frontSector };
        line.Sidedef[1] = null;

        var level = new VmMapLoaderLoadGlSegs.LoadGlSegsLevel { VertexCount = 2 };
        level.Lines.Add(line);

        Span<byte> lump = stackalloc byte[10];
        WriteUInt16(lump, 0, 0);
        WriteUInt16(lump, 2, 1);
        WriteUInt16(lump, 4, 0);
        WriteUInt16(lump, 6, 0);
        WriteUInt16(lump, 8, VmMapLoaderLoadGlSegs.InvalidPartner16);

        Assert.True(VmMapLoaderLoadGlSegs.LoadGlSegs(lump, format5: false, firstGlVertex: 0, level));
        Assert.Equal(0, line.Flags & MapLoaderLineFlags.TwoSided);
        Assert.Null(level.Segs[0].BackSector);
        Assert.Equal(frontSector, level.Segs[0].FrontSector);
    }

    [Fact]
    public void LoadGlSegs_Gl3Header_ParsesAfterMagicSkip()
    {
        var level = new VmMapLoaderLoadGlSegs.LoadGlSegsLevel { VertexCount = 2 };
        var lump = new byte[4 + 16];
        "gNd3"u8.CopyTo(lump);
        var seg = lump.AsSpan(4);
        WriteInt32(seg, 0, 0);
        WriteInt32(seg, 4, 1);
        WriteUInt16(seg, 8, VmMapLoaderLoadGlSegs.MinisegLinedef);
        WriteUInt16(seg, 10, 0);
        WriteUInt32(seg, 12, VmMapLoaderLoadGlSegs.InvalidPartner32);

        Assert.True(VmMapLoaderLoadGlSegs.LoadGlSegs(lump, format5: false, firstGlVertex: 0, level));
        Assert.Single(level.Segs);
    }

    [Fact]
    public void LoadGlSegs_ReturnsFalseWhenVertexOutOfRange()
    {
        var level = new VmMapLoaderLoadGlSegs.LoadGlSegsLevel { VertexCount = 1 };
        Span<byte> lump = stackalloc byte[10];
        WriteUInt16(lump, 0, 5);
        WriteUInt16(lump, 2, 0);
        WriteUInt16(lump, 4, VmMapLoaderLoadGlSegs.MinisegLinedef);
        WriteUInt16(lump, 6, 0);
        WriteUInt16(lump, 8, VmMapLoaderLoadGlSegs.InvalidPartner16);

        Assert.False(VmMapLoaderLoadGlSegs.LoadGlSegs(lump, format5: false, firstGlVertex: 0, level));
    }

    static void WriteUInt16(Span<byte> data, int offset, ushort value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    static void WriteUInt32(Span<byte> data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    static void WriteInt32(Span<byte> data, int offset, int value) => WriteUInt32(data, offset, (uint)value);
}
