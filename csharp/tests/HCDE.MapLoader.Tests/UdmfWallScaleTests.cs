namespace HCDE.MapLoader.Tests;

public class UdmfWallScaleTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    public void ImportsIndependentPartsWithNativeZeroNormalization(string ns, bool enabled)
    {
        var text = $$"""
            namespace = "{{ns}}";
            sidedef { scalex_top = 2; scaley_top = -0.5; scalex_mid = 0;
                scaley_mid = 3; scalex_bottom = -4; scaley_bottom = 0.25; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var side = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sides);
        Assert.Equal(enabled ? 2 : 1, side.TopTextureScaleX);
        Assert.Equal(enabled ? -0.5 : 1, side.TopTextureScaleY);
        Assert.Equal(1, side.MidTextureScaleX);
        Assert.Equal(enabled ? 3 : 1, side.MidTextureScaleY);
        Assert.Equal(enabled ? -4 : 1, side.BottomTextureScaleX);
        Assert.Equal(enabled ? 0.25 : 1, side.BottomTextureScaleY);
    }

    [Fact]
    public void AbsentScalesDefaultToOne()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; sidedef {}", out var map, out var error), error);
        var side = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sides);
        Assert.Equal(1, side.TopTextureScaleX);
        Assert.Equal(1, side.TopTextureScaleY);
        Assert.Equal(1, side.MidTextureScaleX);
        Assert.Equal(1, side.MidTextureScaleY);
        Assert.Equal(1, side.BottomTextureScaleX);
        Assert.Equal(1, side.BottomTextureScaleY);
    }
}
