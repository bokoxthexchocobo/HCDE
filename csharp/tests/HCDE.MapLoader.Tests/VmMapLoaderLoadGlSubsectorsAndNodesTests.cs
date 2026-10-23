namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadGlSubsectorsAndNodesTests
{
    [Fact]
    public void LoadGlSubsectors_Classic_PopulatesFirstSegIndex()
    {
        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectorsLevel();
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg { V1Index = 0, V2Index = 1 });
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg { V1Index = 1, V2Index = 0 });

        Span<byte> lump = stackalloc byte[4];
        WriteUInt16(lump, 0, 2);
        WriteUInt16(lump, 2, 0);

        Assert.True(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectors(lump, format5: false, level));
        Assert.Single(level.Subsectors);
        Assert.Equal(2, level.Subsectors[0].NumLines);
        Assert.Equal(0, level.Subsectors[0].FirstSegIndex);
    }

    [Fact]
    public void LoadGlSubsectors_Classic_FailsWhenSubsectorNotClosed()
    {
        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectorsLevel();
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg { V1Index = 0, V2Index = 1 });
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg { V1Index = 2, V2Index = 3 });

        Span<byte> lump = stackalloc byte[4];
        WriteUInt16(lump, 0, 2);
        WriteUInt16(lump, 2, 0);

        Assert.False(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectors(lump, format5: false, level));
    }

    [Fact]
    public void LoadGlSubsectors_Miniseg_InheritsFrontSectorFromAnchor()
    {
        var sector = new VmMapLoaderLoadGlSegs.MapSector();
        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectorsLevel();
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg
        {
            V1Index = 0,
            V2Index = 1,
            Linedef = new VmMapLoaderLoadGlSegs.MapLine(),
            FrontSector = sector,
        });
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg
        {
            V1Index = 1,
            V2Index = 0,
            Linedef = null,
        });

        Span<byte> lump = stackalloc byte[4];
        WriteUInt16(lump, 0, 2);
        WriteUInt16(lump, 2, 0);

        Assert.True(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectors(lump, format5: false, level));
        Assert.Equal(sector, level.Segs[1].FrontSector);
        Assert.Equal(sector, level.Segs[1].BackSector);
    }

    [Fact]
    public void LoadGlNodes_Classic_ResolvesSubsectorChild()
    {
        var lump = new byte[28];
        WriteInt16(lump, 0, 1);
        WriteInt16(lump, 2, 2);
        WriteInt16(lump, 4, 3);
        WriteInt16(lump, 6, 4);
        WriteUInt16(lump, 24, (ushort)(VmMapLoaderLoadGlSubsectorsAndNodes.NfSubsector | 1));

        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlNodesLevel { SubsectorCount = 4 };
        Assert.True(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlNodes(lump, format5: false, level));
        Assert.Single(level.Nodes);
        Assert.True(level.Nodes[0].Children[0].IsSubsector);
        Assert.Equal(1, level.Nodes[0].Children[0].Index);
        Assert.Equal(1 << 16, level.Nodes[0].X);
    }

    [Fact]
    public void LoadGlNodes_Classic_FailsWhenNodeChildReused()
    {
        var lump = new byte[56];
        WriteUInt16(lump, 24, 1);
        WriteUInt16(lump, 52, 1);

        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlNodesLevel { SubsectorCount = 1 };
        Assert.False(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlNodes(lump, format5: false, level));
    }

    [Fact]
    public void LoadGlSubsectors_Gl3_SkipsMagicHeader()
    {
        var level = new VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectorsLevel();
        level.Segs.Add(new VmMapLoaderLoadGlSubsectorsAndNodes.GlSubsectorSeg { V1Index = 0, V2Index = 0 });

        var lump = new byte[4 + 8];
        "gNd3"u8.CopyTo(lump);
        WriteInt32(lump, 4, 1);
        WriteInt32(lump, 8, 0);

        Assert.True(VmMapLoaderLoadGlSubsectorsAndNodes.LoadGlSubsectors(lump, format5: false, level));
        Assert.Single(level.Subsectors);
        Assert.Equal(1, level.Subsectors[0].NumLines);
    }

    static void WriteUInt16(Span<byte> data, int offset, ushort value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    static void WriteInt16(byte[] data, int offset, short value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    static void WriteInt32(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }
}
