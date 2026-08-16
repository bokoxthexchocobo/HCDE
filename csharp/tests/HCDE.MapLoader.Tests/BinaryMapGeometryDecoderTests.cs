namespace HCDE.MapLoader.Tests;

public class BinaryMapGeometryDecoderTests
{
    [Fact]
    public void TryDecode_ReadsVertexSegAndNodeRecords()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapGeometryDecoder.TryDecode(wad, catalog, out var geometry, out _));

        Assert.Equal(2, geometry.Vertices.Length);
        Assert.Equal(0, geometry.Vertices[0].X);
        Assert.Equal(0, geometry.Vertices[0].Y);
        Assert.Equal(100, geometry.Vertices[1].X);
        Assert.Equal(0, geometry.Vertices[1].Y);

        Assert.Single(geometry.Segs);
        Assert.Equal(0, geometry.Segs[0].V1);
        Assert.Equal(1, geometry.Segs[0].V2);
        Assert.Equal(0, geometry.Segs[0].Linedef);

        Assert.Single(geometry.Nodes);
        Assert.Equal(50, geometry.Nodes[0].X);
        Assert.Equal(0, geometry.Nodes[0].Y);
        Assert.Equal(100, geometry.Nodes[0].DeltaX);
        Assert.False(geometry.Nodes[0].IsChildASubsector);
        Assert.False(geometry.Nodes[0].IsChildBSubsector);
    }

    [Fact]
    public void TryDecode_RejectsUdmfMap()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", udmfTextMap: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.False(BinaryMapGeometryDecoder.TryDecode(wad, catalog, out _, out var reason));
        Assert.Equal("map-not-binary", reason);
    }

    [Fact]
    public void MapNodeCodec_FlagsSubsectorChildren()
    {
        var lump = TestWadBuilder.BuildNodeLump(
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            (ushort)(MapNodeRecord.SubsectorFlag | 3),
            (ushort)(MapNodeRecord.SubsectorFlag | 7));

        Assert.True(MapNodeCodec.TryReadAll(lump, out var nodes, out _));
        Assert.Single(nodes);
        Assert.True(nodes[0].IsChildASubsector);
        Assert.True(nodes[0].IsChildBSubsector);
        Assert.Equal(3, nodes[0].ChildAIndex);
        Assert.Equal(7, nodes[0].ChildBIndex);
    }
}

public class MapSectorBootstrapTests
{
    [Fact]
    public void TryReadSectors_ReturnsDecodedSectorRecords()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(MapSectorBootstrap.TryReadSectors(wad, catalog, out var sectors, out _));

        Assert.Single(sectors);
        Assert.Equal(0, sectors[0].FloorHeight);
        Assert.Equal(128, sectors[0].CeilingHeight);
        Assert.Equal(160, sectors[0].LightLevel);
    }

    [Fact]
    public void CountSectors_MatchesDecodedRecords()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapLumpDecoder.TryDecode(wad, catalog, out var records, out _));
        Assert.Equal(1, MapSectorBootstrap.CountSectors(records));
    }
}
