namespace HCDE.MapLoader.Tests;

public class SideOffsetTests
{
    [Theory]
    [InlineData(-32768)]
    [InlineData(0)]
    [InlineData(32767)]
    public void BinaryRowOffsetSurvivesLevelBuilding(short offset)
    {
        var map = new BinaryMap(new BinaryMapRecords([], [], []), new BinaryMapGeometry([], [], []),
            new BinaryMapSurface([new MapSidedefRecord(15, offset, "TOP", "BOTTOM", "MID", 0)], []),
            default, BinaryMapBehavior.Absent);
        Assert.Equal(offset, Assert.Single(LevelBuilder.FromBinary(map, "MAP01").Sides).MidTextureOffsetY);
    }

    [Theory]
    [InlineData("ZDoom", -12.75)]
    [InlineData("zdoom", -12.75)]
    [InlineData("ZDoomTranslated", -12.75)]
    [InlineData("Vavoom", -12.75)]
    [InlineData("Doom", -10)]
    [InlineData("Hexen", -10)]
    [InlineData("Strife", -10)]
    public void UdmfCombinesBaseAndNamespaceSupportedMiddleOffset(string ns, double expected)
    {
        var text = "namespace = \"" + ns + "\"; sidedef { sector = 0; offsetx = 99; offsety = -10; offsety_mid = -2.75; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sides).MidTextureOffsetY);
    }
}
