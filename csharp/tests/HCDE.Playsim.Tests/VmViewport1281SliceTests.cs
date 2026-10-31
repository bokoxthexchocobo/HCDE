using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmViewport1281SliceTests
{
    [Fact]
    public void Viewport_ScalesTheWideRatioAndMirrorsTheSkyAngles()
    {
        var view = new VmViewport1281
        {
            ViewWidth = 320,
            ViewHeight = 200,
            ViewCenterX = 160,
            ViewCenterY = 90,
            FocalTangent = 1,
            WidescreenRatio = 1.6f,
            Visibility = 8,
            RenderPitch = 320,
            Flags3 = 8,
            PixelStretch = 2.4,
            FieldOfViewDegrees = 90,
            Sin = 0.6,
            Cos = 0.8,
            PosX = 10,
            PosY = 20,
            PosZ = 30,
        };
        view.SetViewport(320, 200, 1.6f);
        Assert.Equal(
            "viewsize 320 x 200\nfill 320 200\nvisibility 8 1\nfuzz 320\nparticles",
            string.Join('\n', view.Log));
        Assert.Equal(198, view.FuzzViewHeight);
        Assert.Equal(160.0, view.CenterX);
        Assert.Equal(90.0, view.CenterY);
        Assert.Equal("1.20300752", VmViewport1281.Num(view.BaseYaspectMul));
        Assert.Equal("2.40601504", VmViewport1281.Num(view.YaspectMul));
        Assert.Equal("0.415625", VmViewport1281.Num(view.IYaspectMul));
        Assert.Equal("0.00259765625", VmViewport1281.Num(view.WallTMapScale2));
        Assert.Equal("384.962406", VmViewport1281.Num(view.InvZtoScale));
        Assert.Equal("160", VmViewport1281.Num(view.FocalLengthX));
        Assert.Equal("384.962406", VmViewport1281.Num(view.FocalLengthY));
        Assert.Equal(65536, view.ViewingRangeRecip);
        Assert.Equal(320, view.ScreenHeight.Length);
        Assert.Equal((short)200, view.ScreenHeight[0]);
        Assert.Equal((short)200, view.ScreenHeight[319]);
        Assert.Equal(0u, view.XToViewAngle[160]);
        Assert.Equal(4290695068u, view.XToViewAngle[161]);
        Assert.Equal(3758096384u, view.XToViewAngle[320]);
        Assert.Equal(534728082u, view.XToViewAngle[0]);
        Assert.Equal(unchecked(0u - view.XToViewAngle[319]), view.XToViewAngle[0]);
        Assert.Equal(641, view.GetDest(1, 2));
        view.Bgra = true;
        Assert.Equal(2564, view.GetDest(1, 2));
        view.ViewWindowX = 3;
        view.ViewWindowY = 4;
        view.RenderPitch = 10;
        view.Bgra = false;
        Assert.Equal(42, view.GetDest(-1, 0));
        view.ViewWindowX = 0;
        view.ViewWindowY = 0;
        view.RenderPitch = 8;
        Assert.Equal(-3, view.GetDest(-3, 0));
        var seen = view.WorldToView3(10, 0, 5);
        Assert.Equal("16 -25 -12", VmViewport1281.Num(seen.X) + " " + VmViewport1281.Num(seen.Y) + " " + VmViewport1281.Num(seen.Z));
        var screen = view.WorldToScreen(10, 0, 5);
        Assert.Equal("-53.3333333 -712.005013 -12", VmViewport1281.Num(screen.X) + " " + VmViewport1281.Num(screen.Y) + " " + VmViewport1281.Num(screen.Z));
        var scaled = view.ScaleView(4, 8, 2, false);
        Assert.Equal("320 640", VmViewport1281.Num(scaled.X) + " " + VmViewport1281.Num(scaled.Y));
        Assert.False(VmViewport1281.Taller(1.333f));
        Assert.True(VmViewport1281.Taller(1.332f));
        Assert.Equal(40, VmViewport1281.Multiplier(1.6f));
        Assert.Equal(48, VmViewport1281.Multiplier(1.333f));
        Assert.Equal(48, VmViewport1281.Multiplier(1.332f));

        view.Log.Clear();
        view.RenderingToCanvas = true;
        view.ViewWidth = 4;
        view.ViewHeight = 40000;
        view.ViewCenterX = 2;
        view.FocalTangent = 1;
        view.WidescreenRatio = 1.333f;
        view.PixelStretch = null;
        view.Flags3 = 0;
        view.Visibility = 0.5;
        view.RenderPitch = 10;
        view.SetViewport(320, 200, 1.0f);
        Assert.Equal("fill 4 -25536\nvisibility 0.5 0\nfuzz 10\nparticles", string.Join('\n', view.Log));
        Assert.DoesNotContain("viewsize", string.Join('\n', view.Log));
        Assert.Equal(39998, view.FuzzViewHeight);
        Assert.Equal("0.75", VmViewport1281.Num(view.BaseYaspectMul));
        Assert.Equal("1", VmViewport1281.Num(view.YaspectMul));
        Assert.Equal("1", VmViewport1281.Num(view.IYaspectMul));
        Assert.Equal("0.5", VmViewport1281.Num(view.WallTMapScale2));
        Assert.Equal((short)-25536, view.ScreenHeight[0]);
        Assert.Equal(4, view.ScreenHeight.Length);
        Assert.Equal(316933406u, view.XToViewAngle[0]);
        Assert.Equal(0u, view.XToViewAngle[1]);
        Assert.Equal(0u, view.XToViewAngle[2]);
        Assert.Equal(3978033890u, view.XToViewAngle[3]);
        Assert.Equal(3758096384u, view.XToViewAngle[4]);

        view.Log.Clear();
        view.RenderingToCanvas = false;
        view.ViewWidth = 123456;
        view.ViewHeight = 123456789;
        view.SetViewport(8, 8, 1.0f);
        Assert.Equal("viewsize 123456 x 123456", view.Log[0]);
    }

    [Fact]
    public void Freelook_MultipliesTheClipMatrixAlreadyStored()
    {
        var view = new VmViewport1281
        {
            ViewHeight = 200,
            FocalLengthY = 100,
            InvZtoScale = 50,
            PitchDegrees = 30,
            HasCamera = false,
        };
        view.SetupFreelook();
        Assert.Equal("100", VmViewport1281.Num(view.CenterY));
        Assert.Equal(100, view.ViewCenterY);
        Assert.Equal("-2", VmViewport1281.Num(view.GlobalUClip));
        Assert.Equal("2", VmViewport1281.Num(view.GlobalDClip));
        view.HasCamera = true;
        view.SetupFreelook();
        Assert.Equal("42.2649731", VmViewport1281.Num(view.CenterY));
        Assert.Equal(42, view.ViewCenterY);
        Assert.Equal("-0.845299462", VmViewport1281.Num(view.GlobalUClip));
        Assert.Equal("3.15470054", VmViewport1281.Num(view.GlobalDClip));
        view.PitchDegrees = 45;
        view.ViewHeight = 5;
        view.SetupFreelook();
        Assert.Equal("-97.5", VmViewport1281.Num(view.CenterY));
        Assert.Equal(-97, view.ViewCenterY);

        view.CenterX = 160;
        view.CenterY = 100;
        view.FocalLengthX = 160;
        view.FocalLengthY = 200;
        view.ViewHeight = 200;
        view.Sin = 0.6;
        view.Cos = 0.8;
        view.PosX = 10;
        view.PosY = 20;
        view.PosZ = 30;
        view.MirrorFlags = 0;
        view.WorldToClip.LoadIdentity();
        view.SetupPolyViewport();
        const string world =
            "0.600000024 0 -0.800000012 0 -0.800000012 0 -0.600000024 0 0 1 0 0 -10 -20 -30 1";
        const string clip =
            "1 0 0 0 0 2 0 0 0 0 -1.00015259 -1 0 0 -2.00015259 0";
        Assert.Equal(world, view.WorldToView.Text());
        Assert.Equal(clip, view.ViewToClip.Text());
        Assert.Equal(clip, view.WorldToClip.Text());
        view.SetupPolyViewport();
        Assert.Equal(
            "1 0 0 0 0 4 0 0 0 0 3.00045776 1.00015259 0 0 2.00045776 2.00015259",
            view.WorldToClip.Text());
        view.MirrorFlags = 1;
        view.SetupPolyViewport();
        Assert.Equal(
            "-0.600000024 -0 0.800000012 -0 -0.800000012 0 -0.600000024 0 0 1 0 0 -10 -20 -30 1",
            view.WorldToView.Text());
        Assert.NotEqual(world, view.WorldToView.Text());
    }
}
