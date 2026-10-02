namespace HCDE.MapLoader.Tests;

public class UdmfMoreLineIdsTests
{
    private static LevelLine Load(string ids, string ns = "ZDoom")
    {
        var text = $$"""
            namespace = "{{ns}}";
            vertex { x = 0; y = 0; } vertex { x = 64; y = 0; }
            sector { heightceiling = 128; } sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; moreids = "{{ids}}"; id = 7; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Lines);
    }

    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Heretic", false)]
    public void NamespaceGatesExtraIds(string ns, bool enabled)
    {
        var line = Load("8 9", ns);
        Assert.True(line.HasId(7)); Assert.Equal(enabled, line.HasId(8)); Assert.Equal(enabled, line.HasId(9));
    }

    [Fact]
    public void DuplicatesZeroAndUnsetAreIgnoredWithoutChangingPrimary()
    {
        var line = Load("7 8 0 -1 8 9");
        Assert.Equal(7, line.Tag); Assert.Equal(new[] { 8, 9 }, line.AdditionalIds);
        Assert.False(line.HasId(0)); Assert.False(line.HasId(-1));
    }

    [Theory]
    [InlineData("8 invalid 9", 8)]
    [InlineData("0x10 09 11", 16)]
    [InlineData("010 3.5 9", 8)]
    public void ParsingStopsAtFirstNonintegerToken(string text, int first)
    {
        Assert.Equal(new[] { first }, Load(text).AdditionalIds);
    }

    [Fact]
    public void SignedHexOctalAndMaxIntMatchNativeNumbers()
    {
        Assert.Equal(new[] { 16, 8, -2, int.MaxValue }, Load("0x10 010 -2 MAXINT").AdditionalIds);
    }
}
