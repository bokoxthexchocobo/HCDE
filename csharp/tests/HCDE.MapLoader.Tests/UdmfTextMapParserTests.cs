namespace HCDE.MapLoader.Tests;

public class UdmfTextMapParserTests
{
    [Fact]
    public void TryParse_ReadsDoomNamespaceSquare()
    {
        const string text = """
            namespace = "Doom";
            // player start
            vertex { x = 0.0; y = 0.0; }
            vertex { x = 128; y = 0; }
            vertex { x = 128.5; y = 64; }
            vertex { x = 0; y = 64; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; blocking = true; }
            linedef { v1 = 1; v2 = 2; sidefront = 0; sideback = 1; twosided = true; special = 1; arg0 = 2; }
            sidedef { sector = 0; texturemiddle = "STARTAN2"; offsetx = 8; }
            sector { heightfloor = 0; heightceiling = 128; texturefloor = "FLOOR0_1"; textureceiling = "CEIL1_1"; lightlevel = 160; }
            thing { x = 32; y = 32; type = 1; angle = 90; skill1 = true; skill2 = true; skill3 = true; single = true; coop = true; }
            """;

        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal("Doom", map.Namespace);
        Assert.Equal(4, map.Vertices.Count);
        Assert.Equal(128.5, map.Vertices[2].X);
        Assert.Equal(64, map.Vertices[2].Y);
        Assert.Equal(2, map.Linedefs.Count);
        Assert.True(map.Linedefs[0].Blocking);
        Assert.Equal(-1, map.Linedefs[0].SideBack);
        Assert.True(map.Linedefs[1].TwoSided);
        Assert.Equal(1, map.Linedefs[1].SideBack);
        Assert.Equal(1, map.Linedefs[1].Special);
        Assert.Equal(2, map.Linedefs[1].Arg0);
        Assert.Equal("STARTAN2", map.Sidedefs[0].TextureMiddle);
        Assert.Equal(8, map.Sidedefs[0].OffsetX);
        Assert.Equal(128, map.Sectors[0].HeightCeiling);
        Assert.Equal(160, map.Sectors[0].LightLevel);
        Assert.Equal(1, map.Things[0].Type);
        Assert.Equal(90, map.Things[0].Angle);
        Assert.True(map.Things[0].Coop);
        Assert.False(map.Things[0].Dm);
    }

    [Fact]
    public void TryParse_SkipsUnknownKeysAndBlocks()
    {
        const string text = """
            namespace = "ZDoomTranslated";
            /* custom */
            vertex { x = 1; y = 2; zfloor = 3; }
            dialogue { id = 1; }
            thing { id = 7; x = 4; y = 5; type = 3001; comment = "imp"; }
            """;

        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal("ZDoomTranslated", map.Namespace);
        Assert.Single(map.Vertices);
        Assert.Equal(1, map.Vertices[0].X);
        Assert.Empty(map.Linedefs);
        Assert.Single(map.Things);
        Assert.Equal(3001, map.Things[0].Type);
        Assert.Equal(7, map.Things[0].Id);
    }

    [Fact]
    public void TryParse_RejectsUnterminatedBlock()
    {
        Assert.False(UdmfTextMapParser.TryParse("namespace = \"Doom\"; vertex { x = 1;", out _, out var error));
        Assert.NotNull(error);
    }
}
