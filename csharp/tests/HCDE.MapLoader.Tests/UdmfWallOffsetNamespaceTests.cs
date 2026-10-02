namespace HCDE.MapLoader.Tests;

public class UdmfWallOffsetNamespaceTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    public void PerPartOffsetsRespectNamespaceAndKeepCommonOffsets(string ns, bool enabled)
    {
        var text = $$"""
            namespace = "{{ns}}";
            sidedef { offsetx = 10; offsety = 20; offsetx_top = 1; offsety_top = 2;
                offsetx_mid = 3; offsety_mid = 4; offsetx_bottom = 5; offsety_bottom = 6; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var side = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sides);
        Assert.Equal(enabled ? 11 : 10, side.TopTextureOffsetX);
        Assert.Equal(enabled ? 22 : 20, side.TopTextureOffsetY);
        Assert.Equal(enabled ? 13 : 10, side.MidTextureOffsetX);
        Assert.Equal(enabled ? 24 : 20, side.MidTextureOffsetY);
        Assert.Equal(enabled ? 15 : 10, side.BottomTextureOffsetX);
        Assert.Equal(enabled ? 26 : 20, side.BottomTextureOffsetY);
    }
}
