namespace HCDE.Playsim.Tests;

public class VmLight1267SliceTests
{
    const string CameraLog =
"""
cam camera lev=-1 fixed=real real=spec1 extra=-2147483648
cam camera lev=1280 fixed=inverse real=null extra=0
cam camera lev=0 fixed=full real=null extra=7
cam actor lev=-1 fixed=real real=spec0 extra=4
cam actor lev=-1 fixed=null real=null extra=9
cam none lev=-1 fixed=inverse real=null extra=0

""";

    const string DistanceLog =
"""
early cur=204.700000 wall=9.000000 fade=1
vis cur=204.700000 base=204.700000 wall=2.730583 floor=780.166667 tilt=32.751999 maxw=0.032767 maxf=2.438021 fade=0 aspect=500
vis cur=-204.700000 base=-204.700000 wall=-142.223854 floor=-181.955556 tilt=-1965.119995 maxw=75495.168000 maxf=368.628750 fade=1 aspect=667
cmap fixedlev base=src num=0
cam camera lev=-1 fixed=null real=null extra=3
cmap sprite base=src num=27 vis=0.682646 shade=1835008
cmap sprite base=src num=4 vis=0.000000 shade=262144
cmap sprite base=src num=4 vis=0.000000 shade=262144
cmap sprite base=src num=31 vis=-18204.653333 shade=2359296
lights 00aabbcc 00000000 7 1
cmap fixedlev base=dyn1 num=0
lights 00aabbcc 00ffffff 7 2
cmap full base=dyn2 num=0
cmap full base=full num=0
lights 00aabbcc a0eeddcc 7 3
cmap map base=inverse num=0

""";

    [Fact]
    public void Camera_UsesTheCameraPlayerAndClearsExtralightOnlyWhenUnmapped()
    {
        VmLight1267.ResetHost();
        Assert.Equal(0, VmLight1267.Camera.FixedLightLev);
        var viewpoint = new VmLightViewpoint1267();
        Assert.Throws<NullReferenceException>(() => VmLight1267.Camera.SetCamera(null, viewpoint));

        var actor = new VmLightActor1267();
        var actorPlayer = new VmLightPlayer1267 { Mo = actor, FixedColormap = 9, FixedLightLevel = 4 };
        actor.Player = actorPlayer;
        var cameraActor = new VmLightActor1267();
        var cameraPlayer = new VmLightPlayer1267 { Mo = cameraActor, FixedColormap = 1, FixedLightLevel = 9 };
        cameraActor.Player = cameraPlayer;
        VmLight1267.Camera.Specials.Add(new VmSwColormap1267("spec0"));
        VmLight1267.Camera.Specials.Add(new VmSwColormap1267("spec1"));
        VmLight1267.Camera.Specials.Add(new VmSwColormap1267("spec2"));
        viewpoint.Camera = cameraActor;
        viewpoint.ExtraLight = int.MinValue;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Equal(int.MinValue, viewpoint.ExtraLight);
        Assert.Same(VmLight1267.Camera.Specials[1], VmLight1267.Camera.RealFixedColormap);

        cameraPlayer.FixedColormap = 5;
        cameraPlayer.FixedLightLevel = 5;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Equal(0, viewpoint.ExtraLight);
        Assert.Equal(1280, VmLight1267.Camera.FixedLightLev);
        Assert.Same(VmLight1267.InverseMap, VmLight1267.Camera.FixedColormap);

        VmLight1267.Camera.FullbrightIgnore = true;
        cameraPlayer.FixedColormap = -1;
        cameraPlayer.FixedLightLevel = 0;
        viewpoint.ExtraLight = 7;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Equal(7, viewpoint.ExtraLight);
        Assert.Same(VmLight1267.FullNormal, VmLight1267.Camera.FixedColormap);
        Assert.Equal(0, VmLight1267.Camera.FixedLightLev);

        viewpoint.Camera = null;
        actorPlayer.Mo = null;
        actorPlayer.FixedColormap = 0;
        viewpoint.ExtraLight = 4;
        VmLight1267.Camera.FullbrightIgnore = false;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Equal(4, viewpoint.ExtraLight);
        Assert.Same(VmLight1267.RealColormaps, VmLight1267.Camera.FixedColormap);

        var other = new VmLightActor1267();
        viewpoint.Camera = other;
        actorPlayer.Mo = actor;
        viewpoint.ExtraLight = 9;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Null(VmLight1267.Camera.FixedColormap);
        Assert.Equal(-1, VmLight1267.Camera.FixedLightLev);

        actor.Player = null;
        viewpoint.Camera = null;
        viewpoint.ExtraLight = int.MinValue;
        VmLight1267.Camera.SetCamera(actor, viewpoint);
        Assert.Equal(0, viewpoint.ExtraLight);
        Assert.Equal(CameraLog, string.Join("\n", VmLight1267.Trace) + "\n");
    }

    [Fact]
    public void Distances_ClampEachPlaneAndHalveParticleVisibility()
    {
        VmLight1267.ResetHost();
        var early = new VmLight1267 { WallVisibility = 9, NoLightFade = true };
        early.SetVisibility(new VmLightView1267 { FocalTangent = 0, FocalLengthY = 50, ScreenWidth = 320, ScreenHeight = 200, ViewWidth = 320, ViewHeight = 200 }, 300, false);
        Assert.Equal(204.7, early.CurrentVisibility);
        Assert.Equal(0, early.BaseVisibility);
        Assert.True(early.NoLightFade);

        var wide = new VmLight1267();
        wide.SetVisibility(new VmLightView1267
        {
            ScreenWidth = 320,
            ScreenHeight = 200,
            ViewWidth = 320,
            ViewHeight = 168,
            InvZtoScale = 10000,
            FocalTangent = 0.01,
            FocalLengthY = 0.5,
            WidescreenRatio = 1.6f,
        }, 204.7, false);

        var tall = new VmLight1267();
        tall.SetVisibility(new VmLightView1267
        {
            ScreenWidth = 640,
            ScreenHeight = 480,
            ViewWidth = 640,
            ViewHeight = 400,
            InvZtoScale = 1.25,
            FocalTangent = 1.2,
            FocalLengthY = 180,
            WidescreenRatio = 1.2f,
        }, -204.7, true);
        Assert.Equal(0d, tall.SpriteVis(2, false));
        Assert.Equal(-18204.653333, tall.SpriteVis(-4, true), 6);

        VmLight1267.Camera.ExtraCvar = 16;
        Assert.Equal(1785856, wide.ShadeImpl(128, false, 0));
        VmLight1267.Camera.ExtraCvar = 0;
        Assert.Equal(0, wide.ShadeImpl(300, false, VmLight1267.NoLightFadeFlag));
        Assert.Equal(720896, wide.ShadeImpl(200, true, VmLight1267.NoLightFadeFlag));
        Assert.Equal(450560, wide.ShadeImpl(200, false, VmLight1267.NoLightFadeFlag));
        VmLight1267.Camera.ExtraCvar = -64;
        Assert.Equal(2424832, wide.ShadeImpl(128, false, 0));
        Assert.Equal(3997696, wide.ShadeImpl(0, false, 0));
        VmLight1267.Camera.ExtraCvar = 0;

        var src = new VmSwColormap1267("src") { Color = 0x00AABBCCu, Fade = 0xA0112233u, Desaturate = 7 };
        var view = new VmLightView1267 { ExtraLight = 2 };
        var colors = new VmLight1267();
        colors.SetColormap(wide, view, 2, 100, false, src, false, false, false, false, false);
        Assert.Equal(0, colors.ColormapNum);

        var actor = new VmLightActor1267();
        var player = new VmLightPlayer1267 { Mo = new VmLightActor1267() };
        actor.Player = player;
        var viewpoint = new VmLightViewpoint1267 { Camera = actor, ExtraLight = 3 };
        VmLight1267.Camera.SetCamera(actor, viewpoint);

        colors.SetColormap(wide, view, 2, 100, false, src, false, false, false, false, true);
        colors.SetColormap(wide, view, 2, 100, false, src, false, false, false, true, false);
        colors.SetColormap(wide, view, 2, 100, false, src, false, false, false, true, true);
        colors.SetColormap(tall, view, -4, 100, true, src, false, false, false, false, false);

        VmLight1267.Camera.FixedLightLev = 0;
        colors.SetColormap(wide, view, 2, 100, false, src, false, false, true, false, false);
        Assert.Equal(0u, colors.BaseColormap!.Fade);
        Assert.Equal(0x00AABBCCu, colors.BaseColormap.Color);

        VmLight1267.Camera.FixedLightLev = -1;
        colors.SetColormap(wide, view, 2, 100, false, src, true, true, true, false, false);
        Assert.Equal(0x00FFFFFFu, colors.BaseColormap!.Fade);

        VmLight1267.Camera.FullbrightIgnore = true;
        colors.SetColormap(wide, view, 2, 100, false, src, true, false, false, false, false);
        Assert.Same(VmLight1267.FullNormal, colors.BaseColormap);

        VmLight1267.Camera.FullbrightIgnore = false;
        VmLight1267.Camera.FixedColormap = VmLight1267.InverseMap;
        colors.SetColormap(wide, view, 2, 100, false, src, true, true, false, false, false);
        Assert.Same(VmLight1267.InverseMap, colors.BaseColormap);
        Assert.Equal(0xA0EEDDCCu, VmLight1267.InversePacked(0xA0112233u));

        Assert.Throws<NullReferenceException>(() =>
            colors.SetColormap(wide, view, 2, 100, false, null, false, false, true, false, false));
        Assert.Equal(DistanceLog, string.Join("\n", VmLight1267.Trace) + "\n");
    }
}
