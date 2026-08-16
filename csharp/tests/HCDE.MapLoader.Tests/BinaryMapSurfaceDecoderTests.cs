namespace HCDE.MapLoader.Tests;

public class BinaryMapSurfaceDecoderTests
{
    [Fact]
    public void TryDecode_ReadsSidedefAndSubsectorRecords()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapSurfaceDecoder.TryDecode(wad, catalog, out var surface, out _));

        Assert.Single(surface.Sidedefs);
        Assert.Equal(0, surface.Sidedefs[0].TextureOffset);
        Assert.Equal(0, surface.Sidedefs[0].RowOffset);
        Assert.Equal("-", surface.Sidedefs[0].TopTexture);
        Assert.Equal("-", surface.Sidedefs[0].BottomTexture);
        Assert.Equal("STARTAN2", surface.Sidedefs[0].MidTexture);
        Assert.Equal(0, surface.Sidedefs[0].Sector);

        Assert.Single(surface.Subsectors);
        Assert.Equal(1, surface.Subsectors[0].NumSegs);
        Assert.Equal(0, surface.Subsectors[0].FirstSeg);
    }

    [Fact]
    public void TryDecode_RejectsUdmfMap()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", udmfTextMap: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.False(BinaryMapSurfaceDecoder.TryDecode(wad, catalog, out _, out var reason));
        Assert.Equal("map-not-binary", reason);
    }

    [Fact]
    public void MapSidedefCodec_RejectsSizeMismatch()
    {
        var lump = new byte[MapSidedefRecord.RecordSize + 1];
        Assert.False(MapSidedefCodec.TryReadAll(lump, out _, out var reason));
        Assert.Equal("sidedefs-lump-size-mismatch", reason);
    }

    [Fact]
    public void MapSubsectorCodec_RejectsSizeMismatch()
    {
        var lump = new byte[MapSubsectorRecord.RecordSize + 1];
        Assert.False(MapSubsectorCodec.TryReadAll(lump, out _, out var reason));
        Assert.Equal("ssectors-lump-size-mismatch", reason);
    }
}
