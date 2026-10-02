namespace HCDE.MapLoader.Tests;

public class LevelBuilderTests
{
    [Fact]
    public void DoomBinaryOffsetsPopulateEveryTexturePart()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(BinaryMapDecoder.TryReadMap(wad, "MAP01", out var map, out _, out var error), error);
        var surface = new BinaryMapSurface([new MapSidedefRecord(-8, 12, "TOP", "BOTTOM", "MID", 0)],
            map.Surface.Subsectors);
        var level = LevelBuilder.FromBinary(new BinaryMap(map.Core, map.Geometry, surface, map.Collision, map.Behavior), "MAP01");
        var side = Assert.Single(level.Sides);
        Assert.Equal(-8, side.TopTextureOffsetX); Assert.Equal(-8, side.MidTextureOffsetX); Assert.Equal(-8, side.BottomTextureOffsetX);
        Assert.Equal(12, side.TopTextureOffsetY); Assert.Equal(12, side.MidTextureOffsetY); Assert.Equal(12, side.BottomTextureOffsetY);
    }

    [Fact]
    public void TryFromWad_BinaryMap_BuildsBlockingLineAndPlayerStart()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);

        Assert.Equal("MAP01", level.MapName);
        Assert.Equal(2, level.Vertices.Count);
        Assert.Equal(100, level.Vertices[1].X);
        Assert.Single(level.Sectors);
        Assert.Equal(160, level.Sectors[0].LightLevel);
        Assert.Equal("FLOOR1_1", level.Sectors[0].FloorPic);
        Assert.Equal("STARTAN2", level.Sides[0].MidTexture);
        Assert.Equal(LevelLine.NoSide, level.Lines[0].SideBack);
        Assert.True(level.Lines[0].BlocksMovement);
        Assert.Equal(0, level.Lines[0].X1);
        Assert.Equal(100, level.Lines[0].X2);
        var thing = Assert.Single(level.Things);
        Assert.Equal(100, thing.X);
        Assert.Equal(200, thing.Y);
        Assert.Equal(90, thing.Angle);
        Assert.Equal(1, thing.Type);
    }

    [Fact]
    public void TryFromWad_EmptyUdmf_BuildsAnEmptyLevel()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", udmfTextMap: true);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Empty(level.Vertices);
        Assert.Empty(level.Things);
    }

    [Fact]
    public void FromUdmf_BlockingAndOpenLines()
    {
        const string text = """
            namespace = "Doom";
            vertex { x = 0.0; y = 0.0; }
            vertex { x = 0.0; y = 128.0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; blocking = true; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; }
            sidedef { sector = 0; texturemiddle = "STARTAN2"; }
            sidedef { sector = 0; }
            sector { heightfloor = 0; heightceiling = 128; lightlevel = 160; texturefloor = "FLOOR1_1"; }
            thing { x = 32.0; y = 64.0; type = 3001; angle = 180.0; }
            """;

        Assert.True(UdmfTextMapParser.TryParse(text, out var udmf, out var error), error);
        var level = LevelBuilder.FromUdmf(udmf, "MAP02");

        Assert.Equal(2, level.Lines.Count);
        Assert.True(level.Lines[0].BlocksMovement);
        Assert.False(level.Lines[1].BlocksMovement);
        Assert.Equal(128, level.Lines[0].Y2);
        Assert.Equal(3001, level.Things[0].Type);
        Assert.Equal(160, level.Sectors[0].LightLevel);
    }
}
