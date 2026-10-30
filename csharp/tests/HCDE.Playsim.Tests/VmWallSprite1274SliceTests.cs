namespace HCDE.Playsim.Tests;

public class VmWallSprite1274SliceTests
{
    [Fact]
    public void Project_TurnsTheYawAndClampsTheWindow()
    {
        var pass = View();
        Assert.Throws<NullReferenceException>(() => pass.Project(Sprite(1, s => s.PicPresent = false)));
        var dead = View();
        dead.ThreadPresent = false;
        Assert.Throws<NullReferenceException>(() => dead.Project(Sprite(2)));
        pass.Project(Sprite(3, s => s.ProjectReject = true));
        pass.Project(Sprite(4, s => { s.Sx1 = 8; s.Sx2 = 12; }));
        pass.Project(Sprite(5, s => { s.Sx1 = -4; s.Sx2 = 0; }));
        pass.Project(Sprite(6, s => { s.Sx1 = -2; s.Sx2 = 12; }));
        pass.Project(Sprite(7, s =>
        {
            s.Flags5 = VmWallSpritePass1274.BrightActor;
            s.FullbrightDisabled = true;
        }));
        pass.Project(Sprite(8, s =>
        {
            s.Flags5 = VmWallSpritePass1274.BrightActor;
            s.Foggy = true;
        }));
        pass.Project(Sprite(9, s =>
        {
            s.RenderFlags = VmWallSpritePass1274.FullBright;
            s.FullbrightDisabled = true;
        }));
        pass.Project(Sprite(10, s =>
        {
            s.TexPresent = false;
            s.TextureFullbright = true;
        }));
        pass.FullbrightOverrides = true;
        pass.Project(Sprite(11, s => s.TextureFullbright = true));
        pass.FullbrightOverrides = false;
        pass.ColormapFade = 5;
        pass.ColormapColor = 6;
        pass.ColormapDesaturate = 2;
        pass.Project(Sprite(12, s =>
        {
            s.Style = VmWallSpritePass1274.StyleAdd;
            s.StyleFlags = VmWallSpritePass1274.InvertSource | VmWallSpritePass1274.InvertOverlay | VmWallSpritePass1274.FadeToBlack;
            s.RenderFlags = VmWallSpritePass1274.XFlip | VmWallSpritePass1274.YFlip;
        }));
        pass.ColormapFade = 0;
        pass.Project(Sprite(13, s =>
        {
            s.Style = VmWallSpritePass1274.StyleAdd;
            s.StyleFlags = VmWallSpritePass1274.InvertSource;
        }));
        pass.Project(Sprite(14, s => s.Yaw = 0));
        Assert.Equal(
"""
vis 6 edge 3 18 19 18 tz 27 z 13 -5 x 0 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 7 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 16 style 1 map base bright 1 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 8 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 16 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 1 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 9 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 16 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 10 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 11 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 1 map base bright 1 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 12 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 3 style 7 map special 6 0 2 bright 0 invert 0 fade 1 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 13 edge 3 18 19 18 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 7 map base bright 0 invert 1 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
vis 14 edge 9 12 9.0000000000000018 28 tz 27 z 13 -5 x 2 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
""",
            string.Join('\n', pass.Log));
        Assert.Equal(new[] { 6, 7, 8, 9, 10, 11, 12, 13, 14 }, pass.Sprites.Select(s => s.Id).ToArray());
        var bright = pass.Sprites.Single(s => s.Id == 7);
        Assert.Equal(0, bright.RenderFlags);
        Assert.Equal(VmWallSpritePass1274.FullBright, bright.VisFlags);
        var flips = pass.Sprites.Single(s => s.Id == 12);
        Assert.Equal(3, flips.RenderFlags);
        Assert.Equal(3, flips.VisFlags);

        var edge = View();
        edge.Project(Sprite(15, s => { s.Sx1 = 0; s.Sx2 = 8; }));
        edge.Project(Sprite(16, s => { s.Sx1 = 1; s.Sx2 = 4; }));
        Assert.Contains("vis 15 edge 3 18 19 18 tz 27 z 13 -5 x 0 8 ", edge.Log[0]);
        Assert.Contains("vis 16 edge 3 18 19 18 tz 27 z 13 -5 x 1 4 ", edge.Log[1]);
    }

    [Fact]
    public void Columns_SkipAHiddenStyleAndCopyPortalCeilings()
    {
        var pass = View();
        pass.Project(Sprite(6, s => { s.Sx1 = -2; s.Sx2 = 12; }));
        var sprite = pass.Sprites[0];
        var hidden = Sprite(20);
        hidden.X1 = 1;
        hidden.X2 = 1;
        hidden.Sx1 = 0;
        hidden.Sx2 = 5;
        hidden.Visible = false;
        pass.Render(hidden, Floor, Ceil);
        Assert.False(pass.StyleCalled);
        Assert.False(hidden.Drew);
        pass.PortalColumns.Add(1);
        pass.PortalColumns.Add(4);
        pass.Render(sprite, Floor, Ceil);
        Assert.Equal(
"""
vis 6 edge 3 18 19 18 tz 27 z 13 -5 x 0 8 depth 27 idepth 0.0370370373 yscale 2 flags 0 style 1 map base bright 0 invert 0 fade 0 alpha 0.333333343 delta 9 18 g 10 20 5 light 80 fog 0 portal 3 sector 4 fill 11 trans 9 side 0 null null
draw 6 0 8 9,11,7,6,14,4,3,2 ceil 10,11,12,13,14,15,16,17 z 13 y 2 flip 0 0 map base
""",
            string.Join('\n', pass.Log));
        var fog = Sprite(7);
        fog.X1 = 2;
        fog.X2 = 8;
        fog.Sx1 = 2;
        fog.Sx2 = 9;
        fog.Visible = false;
        fog.Map = "base";
        pass.StyleCalled = false;
        pass.Render(fog, Floor, Ceil);
        Assert.True(pass.StyleCalled);
        Assert.False(fog.Drew);
        Assert.Equal(2, pass.Log.Count);
        sprite.Gzt = 42;
        sprite.PosZ = 0;
        sprite.Sx1 = 3;
        sprite.Sx2 = 6;
        pass.Render(sprite, Floor, Ceil);
        Assert.Equal(
"draw 6 3 6 6,14,4 ceil 13,14,15 z 42 y 2 flip 0 0 map base",
            pass.Log[^1]);
    }

    static readonly short[] Floor = [9, 8, 7, 6, 5, 4, 3, 2, 1, 0];
    static readonly short[] Ceil = [10, 11, 12, 13, 14, 15, 16, 17, 18, 19];

    static VmWallSpritePass1274 View() => new()
    {
        ViewX = 1,
        ViewY = 2,
        TanCos = 2,
        TanSin = 0.5,
        WindowLeft = 0,
        WindowRight = 8,
        PortalUniq = 3,
    };

    static VmWallSprite1274 Sprite(int id, Action<VmWallSprite1274>? tune = null)
    {
        var sprite = new VmWallSprite1274
        {
            Id = id,
            Yaw = -90,
            PosX = 10,
            PosY = 20,
            PosZ = 5,
            ScaleX = 2,
            ScaleY = 2,
            ScaledWidth = 8,
            ScaledLeft = 3,
            ScaledTop = 4,
            ScaledHeight = 9,
            Sx1 = 2,
            Sx2 = 9,
            SectorId = 4,
            Style = 1,
            Alpha = 1.0 / 3.0,
            FillColor = 11,
            Translation = 9,
            LightLevel = 80,
        };
        tune?.Invoke(sprite);
        return sprite;
    }
}
