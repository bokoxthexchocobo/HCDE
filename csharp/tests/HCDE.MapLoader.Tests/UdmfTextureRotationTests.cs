namespace HCDE.MapLoader.Tests;

public class UdmfTextureRotationTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Strife", false)]
    public void PlaneTransformsRespectNativeNamespace(string ns, bool enabled)
    {
        var text = $$"""
            namespace = "{{ns}}";
            sector { rotationfloor = 90; rotationceiling = 180;
                xpanningfloor = 2; ypanningfloor = 3; xpanningceiling = 4; ypanningceiling = 5; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(enabled ? 0x40000000u : 0u, sector.FloorTextureAngle);
        Assert.Equal(enabled ? 0x80000000u : 0u, sector.CeilingTextureAngle);
        Assert.Equal(enabled ? 2 : 0, sector.FloorTextureOffsetX);
        Assert.Equal(enabled ? 3 : 0, sector.FloorTextureOffsetY);
        Assert.Equal(enabled ? 4 : 0, sector.CeilingTextureOffsetX);
        Assert.Equal(enabled ? 5 : 0, sector.CeilingTextureOffsetY);
    }

    [Theory]
    [InlineData(90, 0x40000000u)]
    [InlineData(-90, 0xc0000000u)]
    [InlineData(450, 0x40000000u)]
    [InlineData(-450, 0xc0000000u)]
    [InlineData(22.5, 0x10000000u)]
    [InlineData(360, 0u)]
    public void ImportsFloorAndCeilingAngles(double degrees, uint expected)
    {
        var text = FormattableString.Invariant($"namespace = \"ZDoom\"; sector {{ rotationfloor = {degrees}; rotationceiling = {degrees}; }}");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(expected, sector.FloorTextureAngle);
        Assert.Equal(expected, sector.CeilingTextureAngle);
    }

    [Fact]
    public void MissingRotationDefaultsToZero()
    {
        Assert.True(UdmfTextMapParser.TryParse("sector {}", out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(0u, sector.FloorTextureAngle);
        Assert.Equal(0u, sector.CeilingTextureAngle);
    }
}
