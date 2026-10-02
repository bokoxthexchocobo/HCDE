namespace HCDE.MapLoader.Tests;

public class UdmfAdditionalIdScannerTests
{
    [Theory]
    [InlineData("8/* comment */9", new[] { 8, 9 })]
    [InlineData("8 // comment\n9", new[] { 8, 9 })]
    [InlineData("8 ; comment\n9", new[] { 8, 9 })]
    [InlineData("#region label\n8\n#endregion\n9", new[] { 8, 9 })]
    [InlineData("\"8\" \"0x10\" \"010\"", new[] { 8, 16 })]
    [InlineData("8 \"bad\" 9", new[] { 8 })]
    [InlineData("8,9 10", new int[0])]
    [InlineData("8|9", new[] { 8 })]
    [InlineData("8/ 9", new[] { 8 })]
    [InlineData("8/9 10", new int[0])]
    [InlineData("8 /* unfinished", new[] { 8 })]
    [InlineData("-9223372036854775808 9", new[] { 9 })]
    [InlineData("-9223372036854775809 9", new[] { 9 })]
    [InlineData("4294967298 9", new[] { 2, 9 })]
    [InlineData("9223372036854775808x 9", new int[0])]
    [InlineData("\" 8\" 9", new[] { 8, 9 })]
    [InlineData("\"8 \" 9", new int[0])]
    [InlineData("MAXINT maxint 9", new[] { int.MaxValue })]
    [InlineData("8 \"9\\\"0\" 10", new[] { 8 })]
    [InlineData("8 \"9", new[] { 8 })]
    [InlineData("8 \"9\n", new[] { 8 })]
    [InlineData("8 \"MAXINT", new[] { 8 })]
    [InlineData("8 \"9\0", new[] { 8 })]
    [InlineData("8 \"9\"", new[] { 8, 9 })]
    [InlineData("\uFEFF8 9", new[] { 8, 9 })]
    [InlineData("8 9\0", new[] { 8, 9 })]
    [InlineData("8 // final comment", new[] { 8 })]
    [InlineData("8 \"9\r\n\" 10", new[] { 8 })]
    [InlineData("\"\r\n8\" 9", new[] { 8, 9 })]
    public void NestedScannerGrammarAppliesToLinesAndSectors(string ids, int[] expected)
    {
        var level = Load(ids);
        Assert.Equal(expected, Assert.Single(level.Lines).AdditionalIds);
        Assert.Equal(expected, Assert.Single(level.Sectors).AdditionalTags);
    }

    [Theory]
    [InlineData("9223372036854775808 9")]
    [InlineData("0xffffffffffffffff 9")]
    public void PositiveOverflowSaturatesBeforeIntCastAndTagFiltering(string ids)
    {
        var level = Load(ids);
        Assert.Equal(new[] { 9 }, Assert.Single(level.Lines).AdditionalIds);
        Assert.Equal(new[] { -1, 9 }, Assert.Single(level.Sectors).AdditionalTags);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EscapedNestedQuotesPassThroughTextMapParsing(bool unfinished)
    {
        var nested = unfinished ? "8 \"9" : "8 \"9\"";
        var encoded = nested.Replace("\"", "\\\"");
        var text = $$"""
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 64; y = 0; }
            sector { id = 7; moreids = "{{encoded}}"; } sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; id = 7; moreids = "{{encoded}}"; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        var expected = unfinished ? new[] { 8 } : new[] { 8, 9 };
        Assert.Equal(expected, Assert.Single(level.Lines).AdditionalIds);
        Assert.Equal(expected, Assert.Single(level.Sectors).AdditionalTags);
    }

    private static PlayLevel Load(string ids)
    {
        const string text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 64; y = 0; }
            sector { id = 7; heightceiling = 128; } sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; id = 7; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        map.Linedefs[0].MoreIds = ids;
        map.Sectors[0].MoreIds = ids;
        return LevelBuilder.FromUdmf(map, "MAP01");
    }
}
