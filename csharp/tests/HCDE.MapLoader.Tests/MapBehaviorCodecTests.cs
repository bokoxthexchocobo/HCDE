namespace HCDE.MapLoader.Tests;

public class MapBehaviorCodecTests
{
    [Theory]
    [InlineData(MapBehaviorFormat.AcsOld)]
    [InlineData(MapBehaviorFormat.AcsEnhanced)]
    [InlineData(MapBehaviorFormat.AcsLittleEnhanced)]
    public void TryProbe_AcceptsKnownAcsFormats(MapBehaviorFormat format)
    {
        var lump = TestWadBuilder.BuildBehaviorLump(format);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.Equal(format, record.Format);
        Assert.Equal(24u, record.DirectoryOffset);
    }

    [Fact]
    public void TryProbe_RejectsInvalidMagic()
    {
        var lump = new byte[MapBehaviorRecord.MinLumpSize];
        Assert.False(MapBehaviorCodec.TryProbe(lump, out _, out var reason));
        Assert.Equal("behavior-magic-mismatch", reason);
    }
}

public class BinaryMapBehaviorDecoderTests
{
    [Fact]
    public void TryDecode_ReadsHexenBehaviorLump()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapBehaviorDecoder.TryDecode(wad, catalog, out var behavior, out _));
        Assert.True(behavior.IsPresent);
        Assert.Equal(MapBehaviorFormat.AcsOld, behavior.Format);
        Assert.Equal(24u, behavior.DirectoryOffset);
        Assert.Single(behavior.Scripts);
    }

    [Fact]
    public void TryDecode_AbsentBehavior_IsNotAnError()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapBehaviorDecoder.TryDecode(wad, catalog, out var behavior, out _));
        Assert.False(behavior.IsPresent);
    }
}
