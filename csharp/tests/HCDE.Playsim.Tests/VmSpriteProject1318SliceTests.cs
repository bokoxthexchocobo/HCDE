namespace HCDE.Playsim.Tests;

public class VmSpriteProject1318SliceTests
{
    private static VmSpriteProject1318.View East() => new()
    {
        Cos = 1,
        TanCos = 1,
        CenterX = 160,
        WindowRight = 320,
        ViewWidth = 320
    };

    private static VmSpriteProject1318.Actor ActorAt(double x) => new()
    {
        X = x,
        CenterZ = 32,
        Sector = new VmSpriteProject1318.Sector(),
        Section = new VmSpriteProject1318.Section()
    };

    private static VmSpriteProject1318.Sprite? Project(
        VmSpriteProject1318 host,
        VmSpriteProject1318.Actor actor,
        VmSpriteProject1318.View view,
        int flags = 0,
        VmSpriteProject1318.WaterSide side = VmSpriteProject1318.WaterSide.Center,
        bool foggy = false,
        VmSpriteProject1318.Colormap? map = null,
        bool shadow = false,
        VmSpriteProject1318.Texture? tex = null,
        int light = 128)
    {
        return host.Project(
            actor, view, tex ?? new VmSpriteProject1318.Texture(), 1, 1, flags, side,
            "floor", "ceiling", light, foggy, map ?? new VmSpriteProject1318.Colormap(), shadow);
    }

    [Fact]
    public void Project_PlacesTheBillboardAndRejectsWater()
    {
        var host = new VmSpriteProject1318();
        var view = East();
        VmSpriteProject1318.Sprite vis = Project(host, ActorAt(200), view)!;
        Assert.Equal(160, vis.Wall.Sx1);
        Assert.Equal(186, vis.Wall.Sx2);
        Assert.Equal(64, vis.Gzt);
        Assert.Equal(0, vis.Gzb);
        Assert.Equal(160, vis.X1);
        Assert.Equal(186, vis.X2);
        Assert.Equal(1, vis.SpriteScale);
        Assert.Equal(64, vis.TextureMid);
        Assert.Same(vis, host.Sprites[0]);
        Assert.Equal("floor", vis.FakeFloor);
        Assert.Equal("ceiling", vis.FakeCeiling);

        var behind = Project(host, ActorAt(-40), view);
        Assert.Null(behind);

        var sunk = ActorAt(200);
        sunk.Sector.HeightSec = new VmSpriteProject1318.Sector { FloorAt = 80, CeilingAt = 100 };
        Assert.Null(host.Project(sunk, view, new VmSpriteProject1318.Texture(), 1, 1, 0,
            VmSpriteProject1318.WaterSide.Center, null, null, 0, false, new VmSpriteProject1318.Colormap(), false));
        Assert.Null(host.Project(sunk, view, new VmSpriteProject1318.Texture(), 1, 1, 0,
            VmSpriteProject1318.WaterSide.AboveCeiling, null, null, 0, false, new VmSpriteProject1318.Colormap(), false));
        sunk.Sector.HeightSec.FloorAt = 0;
        Assert.Null(host.Project(sunk, view, new VmSpriteProject1318.Texture(), 1, 1, 0,
            VmSpriteProject1318.WaterSide.BelowFloor, null, null, 0, false, new VmSpriteProject1318.Colormap(), false));

        var fake = ActorAt(200);
        fake.Sector.HeightSec = new VmSpriteProject1318.Sector
        {
            FloorAt = -10,
            CeilingAt = -1,
            MoreFlags = VmSpriteProject1318.FakeFloorOnly
        };
        VmSpriteProject1318.Sprite kept = Project(host, fake, view)!;
        Assert.Equal(64, kept.Gzt);
        fake.Sector.HeightSec.MoreFlags = 0;
        Assert.Null(host.Project(fake, view, new VmSpriteProject1318.Texture(), 1, 1, 0,
            VmSpriteProject1318.WaterSide.Center, null, null, 0, false, new VmSpriteProject1318.Colormap(), false));
    }

    [Fact]
    public void Project_FlipsScalesStylesAndFadesTheShadow()
    {
        var host = new VmSpriteProject1318
        {
            HqResizeMode = 1,
            HqResizeMult = 2,
            HqResizeTargets = VmSpriteProject1318.HqSprites,
            VanillaTrans = 2,
            FullbrightOverrides = true
        };
        host.AutoTrans.Add("Zombieman");
        var view = East();
        view.MirrorFlags = VmSpriteProject1318.XFlip;
        var actor = ActorAt(200);
        actor.ActorFlags = VmSpriteProject1318.SpriteFlip | VmSpriteProject1318.ZDoomTrans;
        actor.TypeName = "zombieman";
        actor.RenderStyle = new VmSpriteProject1318.Style(1, VmSpriteProject1318.InvertSource | VmSpriteProject1318.FadeToBlack);
        actor.FloorClip = 4;
        actor.RenderFlags2 = VmSpriteProject1318.InterpolateAlpha;
        actor.PrevAlpha = 0;
        actor.Alpha = 1;
        view.TicFrac = 0.25;
        actor.SpriteOffsetX = 8;
        actor.RenderFlags2 |= VmSpriteProject1318.FlipSpriteOffsetX;
        var map = new VmSpriteProject1318.Colormap { Color = 3, Fade = 5, Desaturate = 1 };
        var tex = new VmSpriteProject1318.Texture { Fullbright = true };

        VmSpriteProject1318.Sprite vis = Project(host, actor, view, VmSpriteProject1318.YFlip, map: map, tex: tex)!;
        Assert.Equal(0, vis.RenderFlags & VmSpriteProject1318.XFlip);
        Assert.Equal(VmSpriteProject1318.YFlip, vis.RenderFlags & VmSpriteProject1318.YFlip);
        Assert.Equal(0.5, vis.SpriteScale);
        Assert.Equal(4, vis.FloorClip);
        Assert.Equal(60, vis.TextureMid);
        Assert.Equal(0.25f, vis.Alpha);
        Assert.Equal(VmSpriteProject1318.Style.Normal, vis.RenderStyle);
        Assert.False(vis.Light.Invert);
        Assert.False(vis.Light.FadeToBlack);
        Assert.Equal(5, map.Fade);
        Assert.Equal(5, vis.Light.Map!.Fade);
        Assert.True(vis.Light.Fullbright);
        Assert.NotEqual(160, vis.Wall.Sx1);

        actor.ActorFlags = 0;
        actor.TypeName = "";
        actor.RenderFlags2 = 0;
        actor.Alpha = 1;
        actor.SpriteOffsetX = 0;
        actor.FloorClip = 0;
        actor.RenderStyle = VmSpriteProject1318.Style.Add;
        host.VanillaTrans = 0;
        host.HqResizeMode = 0;
        var faded = new VmSpriteProject1318.Colormap { Fade = 9, Color = 4, Desaturate = 2 };
        VmSpriteProject1318.Sprite add = Project(host, actor, view, VmSpriteProject1318.FullBright, foggy: true, map: faded)!;
        Assert.Equal(0, add.Light.Map!.Fade);
        Assert.Equal(9, faded.Fade);
        Assert.False(add.Light.Fullbright);
        Assert.Equal(1, add.SpriteScale);

        actor.Flags5 = VmSpriteProject1318.Bright;
        VmSpriteProject1318.Sprite bright = Project(host, actor, view, foggy: true)!;
        Assert.Equal(VmSpriteProject1318.FullBright, bright.RenderFlags & VmSpriteProject1318.FullBright);
        Assert.False(bright.Light.Fullbright);

        tex.FullbrightDisabled = true;
        actor.Flags5 = 0;
        VmSpriteProject1318.Sprite blocked = Project(host, actor, view, VmSpriteProject1318.FullBright, tex: tex)!;
        Assert.False(blocked.Light.Fullbright);

        host.ShadowAlpha = 0.5;
        host.ShadowFadeHeight = 100;
        actor.Z = 0;
        actor.FloorZ = 0;
        VmSpriteProject1318.Sprite shade = Project(host, actor, view, shadow: true)!;
        Assert.Equal(VmSpriteProject1318.Style.Stencil, shade.RenderStyle);
        Assert.Equal(0u, shade.FillColor);
        Assert.Equal(0.5f, shade.Alpha);
        actor.Z = -10;
        VmSpriteProject1318.Sprite raised = Project(host, actor, view, shadow: true)!;
        Assert.Equal(0.6f, raised.Alpha);
    }

    [Fact]
    public void Project_AccumulatesLightsAndRenderClipsColumns()
    {
        var host = new VmSpriteProject1318 { DynLights = true };
        var actor = ActorAt(200);
        var level = new VmSpriteProject1318.Level();
        level.FlatLights.Add(new List<VmSpriteProject1318.DynLight?>
        {
            new() { X = 200, Y = 0, Z = 32, Radius = 100, Red = 255 },
            new() { X = 0, Y = 0, Z = 0, Radius = 1, Red = 255 },
            null,
            new() { X = 200, Y = 0, Z = 32, Radius = 50, Green = 255, LightsActor = false }
        });
        actor.Sector.Level = level;
        VmSpriteProject1318.Sprite vis = Project(host, actor, East())!;
        Assert.Equal(255u << 16, vis.DynLight);

        host.LightSprites = false;
        Assert.Equal(0u, Project(host, actor, East())!.DynLight);

        var spr = host.Sprites[0];
        var floor = new short[320];
        var ceiling = new short[320];
        for (int i = 0; i < 320; i++)
        {
            floor[i] = 10;
            ceiling[i] = 100;
        }
        host.ClipColumn = (x, _) => x == spr.X1;
        host.Render(spr, floor, ceiling);
        VmSpriteProject1318.Draw draw = host.Draws[0];
        Assert.Equal(100, draw.PortalFloor[spr.X1]);
        Assert.Equal(10, draw.PortalFloor[spr.X1 + 1]);
        Assert.Equal(spr.Gzt - spr.FloorClip, draw.Y);
        Assert.False(draw.XFlip);
        Assert.Equal(vis.DynLight, draw.DynLight);

        host.AcceptStyle = _ => false;
        host.Render(spr, floor, ceiling);
        Assert.Single(host.Draws);
    }
}
