namespace HCDE.MapLoader.Tests;

public class UdmfTextureScaleTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    public void ScaleImportIsDirectAndNamespaceScoped(string ns, bool enabled)
    {
        var text = $$"""
            namespace = "{{ns}}";
            sector { xscalefloor = 2; yscalefloor = -0.5; xscaleceiling = 0; yscaleceiling = 3; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(enabled ? 2 : 1, sector.FloorTextureScaleX);
        Assert.Equal(enabled ? -0.5 : 1, sector.FloorTextureScaleY);
        Assert.Equal(enabled ? 0 : 1, sector.CeilingTextureScaleX);
        Assert.Equal(enabled ? 3 : 1, sector.CeilingTextureScaleY);
    }

    [Fact]
    public void MissingScaleDefaultsToOne()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; sector {}", out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(1, sector.FloorTextureScaleX);
        Assert.Equal(1, sector.FloorTextureScaleY);
        Assert.Equal(1, sector.CeilingTextureScaleX);
        Assert.Equal(1, sector.CeilingTextureScaleY);
    }
}
