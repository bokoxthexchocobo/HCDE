namespace HCDE.MapLoader.Tests;

public class BinaryMapLumpDecoderTests
{
    [Fact]
    public void TryDecode_ReadsThingLinedefAndSectorRecords()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapLumpDecoder.TryDecode(wad, catalog, out var records, out _));

        Assert.Single(records.Things);
        Assert.Equal(100, records.Things[0].X);
        Assert.Equal(200, records.Things[0].Y);
        Assert.Equal(90, records.Things[0].Angle);
        Assert.Equal(1, records.Things[0].Type);

        Assert.Single(records.Linedefs);
        Assert.Equal(0, records.Linedefs[0].V1);
        Assert.Equal(1, records.Linedefs[0].V2);

        Assert.Single(records.Sectors);
        Assert.Equal(0, records.Sectors[0].FloorHeight);
        Assert.Equal(128, records.Sectors[0].CeilingHeight);
        Assert.Equal("FLOOR1_1", records.Sectors[0].FloorPic);
        Assert.Equal(160, records.Sectors[0].LightLevel);
    }
}
