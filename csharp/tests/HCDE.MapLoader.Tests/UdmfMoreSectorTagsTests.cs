namespace HCDE.MapLoader.Tests;

public class UdmfMoreSectorTagsTests
{
    private static LevelSector Load(string ns = "ZDoom", int primary = 7, string more = "8 9")
    {
        var text = $$"""
            namespace = "{{ns}}";
            sector { id = {{primary}}; moreids = "{{more}}"; heightceiling = 128; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
    }

    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Heretic", false)]
    public void NamespaceGatesAdditionalTags(string ns, bool enabled)
    {
        var sector = Load(ns);
        Assert.True(sector.HasTag(7)); Assert.Equal(enabled, sector.HasTag(8));
    }

    [Fact]
    public void NegativeTagsAreValidAndZeroAndDuplicatesAreOmitted()
    {
        var sector = Load(more: "7 8 -1 0 8");
        Assert.Equal(new[] { 8, -1 }, sector.AdditionalTags);
        Assert.True(sector.HasTag(-1)); Assert.False(sector.HasTag(0));
    }

    [Fact]
    public void FullWidthPrimaryAndAdditionalTagsArePreserved()
    {
        var sector = Load(primary: 70000, more: "90000 -70000");
        Assert.Equal(70000, sector.Tag); Assert.True(sector.HasTag(90000)); Assert.True(sector.HasTag(-70000));
        Assert.False(sector.HasTag(unchecked((short)70000)));
    }

    [Fact]
    public void ZeroSelectorOnlyMatchesSectorsWithoutAnyTags()
    {
        Assert.True(Load(primary: 0, more: "0").MatchesTag(0));
        Assert.False(Load(primary: 0, more: "8").MatchesTag(0));
        Assert.True(Load(primary: 0, more: "8").MatchesTag(8));
    }
}
