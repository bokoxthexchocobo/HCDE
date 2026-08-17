namespace HCDE.MapLoader.Tests;

public class BinaryMapDecoderTests
{
    [Fact]
    public void TryReadMap_DecodesAllLumpGroups()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(BinaryMapDecoder.TryReadMap(wad, "MAP01", out var map, out var catalog, out _));

        Assert.Equal(MapDataFormat.DoomBinary, catalog.Format);
        Assert.Single(map.Core.Things);
        Assert.Single(map.Core.Linedefs);
        Assert.Single(map.Core.Sectors);
        Assert.Equal(2, map.Geometry.Vertices.Length);
        Assert.Single(map.Geometry.Segs);
        Assert.Single(map.Geometry.Nodes);
        Assert.Single(map.Surface.Sidedefs);
        Assert.Single(map.Surface.Subsectors);
        Assert.Equal(1, map.Collision.Reject.SectorCount);
        Assert.Equal(1, map.Collision.Blockmap.Header.Width);
        Assert.False(map.Behavior.IsPresent);
    }

    [Fact]
    public void TryReadMap_DecodesHexenBehaviorWhenPresent()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true);
        Assert.True(BinaryMapDecoder.TryReadMap(wad, "MAP01", out var map, out _, out _));
        Assert.True(map.Behavior.IsPresent);
        Assert.Equal(MapBehaviorFormat.AcsOld, map.Behavior.Format);
    }

    [Fact]
    public void TryDecode_RejectsUdmfMap()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", udmfTextMap: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.False(BinaryMapDecoder.TryDecode(wad, catalog, out _, out var reason));
        Assert.Equal("map-not-binary", reason);
    }
}
