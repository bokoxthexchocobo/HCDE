namespace HCDE.Playsim.Tests;

public class VmWall1272SliceTests
{
    [Fact]
    public void Stripes_PingPongTheBandsAndKeepTheLastColormap()
    {
        var wall = new VmWall1272 { Mask = true, Alpha = VmWall1272.Opaque };
        wall.Planes.Add(new VmWallPlane1272 { Cull = 0, Name = "L0", Screen = [9, 10, -3] });
        wall.Planes.Add(new VmWallPlane1272 { Cull = VmWall1272.OutsideAbove, Name = "L1", Screen = [0, 0, 0] });
        wall.Planes.Add(new VmWallPlane1272 { Cull = 2, Name = "L2", Screen = [3, 8, 6] });
        wall.Render(0, 3, [10, 5, 10], [4, 8, 4]);
        Assert.Equal(
"""
draw base vary 0 0 1 0 65536 pal 28 0 null 0 0 16 16 10,5,10 9,8,4
draw L1 vary 0 0 1 0 65536 pal 28 0 null 0 0 16 16 9,8,4 4,8,4
draw L2 vary 0 0 1 0 65536 pal 28 0 null 0 0 16 16 4,8,4 4,8,4
""",
            string.Join('\n', wall.Log));
    }

    [Fact]
    public void Styles_PromoteOpaqueAndPreferAFixedLight()
    {
        var solid = new VmWall1272
        {
            Mask = true,
            Alpha = 65536,
            FixedLightLevel = 40,
            IgnoreSectorColor = true,
            Shade = 12,
            HeightBits = 0,
            CenterX = 40,
            CenterY = 7,
            LightListCount = 4,
            SideIndex = 1,
            LightListId = 7,
            PicWidth = 3,
            PicHeight = 1,
        };
        solid.Render(2, 4, [0, 0, 8, 9, 0], [0, 0, 1, 2, 0]);
        solid.CenterX = 99;
        Assert.Equal(
"""
draw full fixed 12 0 1 0 65536 pal 0 0 null 40 7 3 1 8,9 1,2
""",
            string.Join('\n', solid.Log));

        var dim = new VmWall1272
        {
            Mask = true,
            Additive = true,
            Alpha = 65536,
            PicMasked = true,
            Bgra = true,
            MipmapCvar = true,
            PicMipmapped = true,
            HasFixedColormap = true,
            FixedLightLevel = -1,
            LightListCount = 2,
            SideIndex = 0,
            LightListId = 7,
        };
        dim.Render(0, 1, [4], [5]);
        Assert.Contains("draw fixed map 0 0 1 1 65536 bgra 32 1 null", dim.Log[0], StringComparison.Ordinal);

        var both = new VmWall1272 { FixedLightLevel = 0, HasFixedColormap = true, Shade = 3, BaseColormap = "sector" };
        both.Render(0, 1, [1], [2]);
        Assert.StartsWith("draw sector fixed 3 ", both.Log[0], StringComparison.Ordinal);

        var listed = new VmWall1272 { LightListCount = 2, SideIndex = 1, LightListId = 7, Foggy = true, LightLevel = 80 };
        listed.Render(0, 1, [1], [2]);
        Assert.StartsWith("draw base vary 80 1 0 0 0 pal 28 0 7 ", listed.Log[0], StringComparison.Ordinal);

        var missing = new VmWall1272 { HasCurline = false, LightListCount = 5, SideIndex = 0, LightListId = 7 };
        missing.Render(0, 1, [1], [2]);
        Assert.Contains(" null ", missing.Log[0], StringComparison.Ordinal);

        var none = new VmWall1272 { PicPresent = false };
        none.Render(0, 1, [1], [2]);
        Assert.Empty(none.Log);
        var dead = new VmWall1272 { ThreadPresent = false };
        Assert.Throws<NullReferenceException>(() => dead.Render(0, 1, [1], [2]));

        var low = new VmWall1272 { Mask = true, Alpha = 65535 };
        low.Render(0, 1, [1], [2]);
        Assert.Contains(" 1 0 65535 ", low.Log[0], StringComparison.Ordinal);
    }
}
