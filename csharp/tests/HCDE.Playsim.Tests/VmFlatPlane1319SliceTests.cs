namespace HCDE.Playsim.Tests;

public class VmFlatPlane1319SliceTests
{
    private static VmFlatPlane1319.View View() => new()
    {
        Z = 64,
        YawDegrees = 90,
        FocalLengthX = 160,
        FocalLengthY = 100,
        CenterX = 160,
        CenterY = 100
    };

    [Fact]
    public void Render_SkipsZeroAlphaAndBuildsTheSteps()
    {
        var host = new VmFlatPlane1319();
        var plane = new VmFlatPlane1319.Plane { Left = 10, Right = 20, ZAt0 = 0, Light = 128 };
        plane.Rows.Add(new VmFlatPlane1319.Row { Y = 120, X1 = 12, X2 = 18 });
        var view = View();
        host.Render(plane, view, 1, 1, 0, false, false, new VmFlatPlane1319.Colormap(), new VmFlatPlane1319.Texture());
        Assert.True(host.Skipped);
        Assert.Empty(host.Spans);

        view.FadeTo = 0;
        view.LevelFlags = VmFlatPlane1319.HasFadeTable;
        host.Render(plane, view, 1, 1, 1, true, true, new VmFlatPlane1319.Colormap(), new VmFlatPlane1319.Texture());
        Assert.False(host.Skipped);
        Assert.Equal(1.0 / 160.0, plane.XStep, 12);
        Assert.Equal(0, plane.YStep, 12);
        Assert.Equal(0, plane.ViewX);
        Assert.Equal(0, plane.ViewY);
        Assert.Equal(64, plane.Height);
        Assert.True(plane.Foggy);
        Assert.True(plane.Shade);
        Assert.Equal(128, plane.LightLevel);
        Assert.Equal(3, plane.SolidColor);
        Assert.True(plane.Masked);
        Assert.True(plane.Additive);
        Assert.Equal((plane.RightXFrac - plane.LeftXFrac) / 10.0, plane.XStepScale, 12);
        Assert.Single(host.Spans);

        view.MirrorFlags = VmFlatPlane1319.XFlip;
        view.FixedLightLevel = 0;
        view.LevelFlags = 0;
        host.Render(plane, view, 1, 1, 1, false, false, new VmFlatPlane1319.Colormap { Fade = 4 }, new VmFlatPlane1319.Texture());
        Assert.Equal(-1.0 / 160.0, plane.XStep, 12);
        Assert.False(plane.Shade);
        Assert.True(plane.Foggy);

        var rotated = new VmFlatPlane1319.Plane
        {
            Angle = Math.PI / 2,
            XOff = 1,
            YOff = 2,
            BaseYOff = 5,
            Left = 4,
            Right = 4,
            ZAt0 = 64
        };
        view.X = 3;
        view.Y = 4;
        view.MirrorFlags = 0;
        view.FixedLightLevel = -1;
        view.FixedColormap = true;
        host.Render(rotated, view, 2, 3, 1, false, false, new VmFlatPlane1319.Colormap(), new VmFlatPlane1319.Texture());
        Assert.Equal(-6, rotated.ViewX, 9);
        Assert.Equal(12, rotated.ViewY, 9);
        Assert.Equal(0, rotated.XStepScale);
        Assert.Equal(0, rotated.Height);
        Assert.False(rotated.Shade);
    }

    [Fact]
    public void RenderLine_LightsTheSpanAndRejectsABadRange()
    {
        var host = new VmFlatPlane1319 { DynLights = true, LodBias = 1 };
        var view = View();
        view.IsBgra = true;
        view.FloorVisibility = 2;
        view.InvZtoScale = 160;
        var tex = new VmFlatPlane1319.Texture { Width = 64, Height = 32 };
        double zspan = 64.0 * 160.0 / 20.5;
        var plane = new VmFlatPlane1319.Plane
        {
            Left = 10,
            Right = 20,
            ZAt0 = 0,
            Tex = tex,
            Lights = new VmFlatPlane1319.LightNode
            {
                Light = new VmFlatPlane1319.DynLight { X = zspan, Z = 0, Radius = 10, Red = 255, Attenuated = false },
                Next = new VmFlatPlane1319.LightNode
                {
                    Light = new VmFlatPlane1319.DynLight { Active = false, Radius = 10, Blue = 255 }
                }
            }
        };
        plane.Rows.Add(new VmFlatPlane1319.Row { Y = 120, X1 = 12, X2 = 18 });
        host.Render(plane, view, 1, 1, 1, false, false, new VmFlatPlane1319.Colormap(), tex);
        VmFlatPlane1319.Span span = host.Spans[0];
        double distance = view.PlaneDepth(120, plane.Height);
        double curX = plane.BaseXFrac + plane.XStepScale * (12 - plane.MinX);
        Assert.Equal((distance * curX + plane.ViewX) / 64.0, span.U, 9);
        Assert.True(span.Lod > -1000);
        Assert.Equal(2.0 / 64.0 * 20.0, span.Light, 6);
        Assert.Single(span.Lights);
        Assert.Equal(255 << 16, span.Lights[0].Color);
        Assert.Equal(256f / 10f, span.Lights[0].Radius);
        Assert.Equal(1f, span.Lights[0].NormalZ);

        view.NoLightFade = true;
        host.Render(plane, view, 1, 1, 1, false, false, new VmFlatPlane1319.Colormap(), tex);
        Assert.Equal(0, host.Spans[0].Light);

        var missed = new VmFlatPlane1319.Plane
        {
            Left = 10,
            Right = 20,
            ZAt0 = 0,
            Tex = tex,
            Lights = new VmFlatPlane1319.LightNode
            {
                Light = new VmFlatPlane1319.DynLight { Radius = 1_000_000, Attenuated = true, Green = 9, Z = 10_000 }
            }
        };
        missed.Rows.Add(new VmFlatPlane1319.Row { Y = 80, X1 = 12, X2 = 18 });
        host.Render(missed, view, 1, 1, 1, false, false, new VmFlatPlane1319.Colormap(), tex);
        Assert.Empty(host.Spans[0].Lights);

        host.RangeCheck = true;
        Assert.Throws<InvalidOperationException>(() => host.RenderLine(plane, view, 120, 18, 12));

        var colored = new VmFlatPlane1319();
        plane.Rows.Add(new VmFlatPlane1319.Row { Y = 121, X1 = 1, X2 = 2 });
        colored.RenderColored(plane, view);
        Assert.Equal(2, colored.Colored.Count);
        Assert.Equal(121, colored.Colored[1].Y);
        Assert.Empty(colored.Spans);
    }
}
