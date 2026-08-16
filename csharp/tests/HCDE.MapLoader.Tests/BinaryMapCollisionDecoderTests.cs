namespace HCDE.MapLoader.Tests;

public class BinaryMapCollisionDecoderTests
{
    [Fact]
    public void TryDecode_ReadsBlockmapAndRejectForMinimalMap()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapLumpDecoder.TryDecode(wad, catalog, out var records, out _));
        Assert.True(BinaryMapCollisionDecoder.TryDecode(wad, catalog, records.Sectors.Length, out var collision, out _));

        Assert.Equal(0, collision.Blockmap.Header.OriginX);
        Assert.Equal(0, collision.Blockmap.Header.OriginY);
        Assert.Equal(1, collision.Blockmap.Header.Width);
        Assert.Equal(1, collision.Blockmap.Header.Height);
        Assert.Equal(5, collision.Blockmap.Cells[0]);
        Assert.Equal(MapBlockmapCodec.Terminator, collision.Blockmap.Cells[1]);

        Assert.Equal(1, collision.Reject.SectorCount);
        Assert.Single(collision.Reject.Bytes);
        Assert.Equal(0, collision.Reject.Bytes[0]);
    }

    [Fact]
    public void MapRejectCodec_RejectsWrongSize()
    {
        Assert.False(MapRejectCodec.TryReadForSectorCount(new byte[2], sectorCount: 1, out _, out var reason));
        Assert.Equal("reject-lump-size-mismatch", reason);
    }

    [Fact]
    public void MapBlockmapCodec_RejectsOddSizedLump()
    {
        Assert.False(MapBlockmapCodec.TryRead(new byte[9], out _, out var reason));
        Assert.Equal("blockmap-lump-size-mismatch", reason);
    }
}
