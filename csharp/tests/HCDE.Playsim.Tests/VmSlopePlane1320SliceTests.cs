namespace HCDE.Playsim.Tests;

public class VmSlopePlane1320SliceTests
{
    private static VmSlopePlane1320.View View() => new()
    {
        Z = 32,
        Yaw = Math.PI / 2,
        Sin = 0,
        Cos = 1,
        FocalLengthX = 1,
        FocalLengthY = 1,
        IYAspect = 1,
        TiltVisibility = 1
    };

    private static VmSlopePlane1320.Plane Floor()
    {
        var plane = new VmSlopePlane1320.Plane
        {
            Normal = new VmSlopePlane1320.Vec(0, 0, 1),
            XOff = 1,
            YOff = 1,
            Light = 144
        };
        plane.Rows.Add(new VmSlopePlane1320.Row { Y = 80, X1 = 10, X2 = 20 });
        return plane;
    }

    [Fact]
    public void Render_SkipsZeroAlphaAndFixesTheOpaqueSteps()
    {
        var host = new VmSlopePlane1320();
        var view = View();
        Assert.Null(host.Render(Floor(), view, 64, 64, 0, true, true, 0, new VmSlopePlane1320.Texture()));
        Assert.True(host.Skipped);

        VmSlopePlane1320.Plane floor = host.Render(
            Floor(), view, 64, 64, 100, true, true, 0, new VmSlopePlane1320.Texture())!;
        Assert.False(host.Skipped);
        Assert.Equal(67108864, floor.PViewX);
        Assert.Equal(67108864, floor.PViewY);
        Assert.Equal(VmSlopePlane1320.Opaque, floor.StyleAlpha);
        Assert.False(floor.StyleAdditive);
        Assert.False(floor.StyleMasked);
        Assert.True(floor.IgnoredAdditive);
        Assert.True(floor.Shade);
        Assert.True(floor.Su.X < -1_000_000f);
        Assert.True(floor.Sv.Z > 1_000_000f);
        Assert.Equal(1f, floor.Sz.Y);
        Assert.Equal(-1.0 / 512.0, floor.LightFloat, 6);

        var ceiling = Floor();
        ceiling.Normal = new VmSlopePlane1320.Vec(0, 0, -1);
        ceiling.Rows.Clear();
        VmSlopePlane1320.Plane up = host.Render(ceiling, view, 64, 64, 1, false, false, 0, new VmSlopePlane1320.Texture())!;
        Assert.Equal(floor.Su, up.Su);
        Assert.Equal(-floor.LightFloat, up.LightFloat, 8);

        view.MirrorFlags = VmSlopePlane1320.XFlip;
        VmSlopePlane1320.Plane mirrored = host.Render(Floor(), view, 64, 64, 1, false, false, 0, new VmSlopePlane1320.Texture())!;
        Assert.Equal(-floor.Su.X, mirrored.Su.X);
        Assert.Equal(floor.Su.Y, mirrored.Su.Y);
        Assert.Equal(-floor.Sz.X, mirrored.Sz.X);

        view.MirrorFlags = 0;
        view.LevelFlags = VmSlopePlane1320.HasFadeTable;
        view.FixedColormap = true;
        var thin = new VmSlopePlane1320.Texture { WidthBits = 0, HeightBits = 0 };
        VmSlopePlane1320.Plane zeroed = host.Render(Floor(), view, 1, 1, 1, false, false, 0, thin)!;
        Assert.Equal(new VmSlopePlane1320.Vec(0, 0, 0), zeroed.Su);
        Assert.Equal(new VmSlopePlane1320.Vec(0, 0, 0), zeroed.Sv);
        Assert.True(zeroed.Foggy);
        Assert.False(zeroed.Shade);
    }

    [Fact]
    public void RenderLine_PacksActiveLightsAndSkipsTheRest()
    {
        var host = new VmSlopePlane1320 { DynLights = true };
        var view = View();
        var plane = Floor();
        plane.Lights = new VmSlopePlane1320.LightNode
        {
            Light = new VmSlopePlane1320.DynLight { X = 0, Y = 0, Z = 32, Radius = 8, Red = 255, Attenuated = true },
            Next = new VmSlopePlane1320.LightNode
            {
                Light = new VmSlopePlane1320.DynLight { Active = false, Radius = 8, Blue = 255 }
            }
        };
        host.Render(plane, view, 64, 64, 1, false, false, 9, new VmSlopePlane1320.Texture());
        VmSlopePlane1320.Span span = Assert.Single(host.Spans);
        Assert.True(span.Foggy);
        Assert.Equal(144, span.LightLevel);
        VmSlopePlane1320.DrawerLight light = Assert.Single(span.Lights);
        Assert.Equal(0, light.X);
        Assert.Equal(0, light.Y);
        Assert.Equal(0, light.Z);
        Assert.Equal(-32f, light.Radius);
        Assert.Equal(255 << 16, light.Color);
        VmSlopePlane1320.Vec a = view.ScreenToView(10, 80, plane.PlaneNormal, plane.PlaneD);
        VmSlopePlane1320.Vec b = view.ScreenToView(20, 80, plane.PlaneNormal, plane.PlaneD);
        a = new VmSlopePlane1320.Vec((float)(a.X / a.Z), (float)(a.Y / a.Z), (float)(1.0 / a.Z));
        b = new VmSlopePlane1320.Vec((float)(b.X / b.Z), (float)(b.Y / b.Z), (float)(1.0 / b.Z));
        Assert.Equal((b.X - a.X) / 10f, span.StepX, 5);
        Assert.Equal(a.Z, span.ViewZ, 5);

        host.DynLights = false;
        host.Render(plane, view, 64, 64, 1, false, false, 0, new VmSlopePlane1320.Texture());
        Assert.Empty(host.Spans[0].Lights);
        Assert.Equal(0, host.Spans[0].StepX);
    }
}
