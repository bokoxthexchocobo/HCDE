namespace HCDE.MapLoader.Tests;

public class MapBehaviorDirectoryCodecTests
{
    [Fact]
    public void TryReadScripts_OldFormat_ParsesPackedScriptNumber()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(MapBehaviorFormat.AcsOld, scriptCount: 1, scriptNumber: 1, scriptType: 1);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));

        Assert.Single(scripts);
        Assert.Equal(1, scripts[0].Number);
        Assert.Equal(1, scripts[0].Type);
        Assert.Equal(2, scripts[0].ArgCount);
        Assert.True(scripts[0].Address > 0);
    }

    [Theory]
    [InlineData(MapBehaviorFormat.AcsEnhanced)]
    [InlineData(MapBehaviorFormat.AcsLittleEnhanced)]
    public void TryReadScripts_EnhancedFormat_ParsesSptrChunk(MapBehaviorFormat format)
    {
        var lump = TestWadBuilder.BuildBehaviorLump(format, scriptCount: 2, scriptNumber: 4, scriptType: 0);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));

        Assert.Equal(2, scripts.Count);
        Assert.All(scripts, script =>
        {
            Assert.Equal(4, script.Number);
            Assert.Equal(0, script.Type);
            Assert.Equal(1, script.ArgCount);
        });
    }

    [Fact]
    public void TryDecode_IncludesScriptDirectory()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapBehaviorDecoder.TryDecode(wad, catalog, out var behavior, out _));
        Assert.True(behavior.IsPresent);
        Assert.NotEmpty(behavior.Scripts);
    }
}
