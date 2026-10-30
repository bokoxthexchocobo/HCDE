using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPostProcess1280SliceTests
{
    [Fact]
    public void Scene_PassesTheCallbackBetweenTheTwoPasses()
    {
        var scene = new VmPostProcess1280
        {
            SceneWidth = 320,
            SceneHeight = 200,
            DeltaTime = 0.5,
            FrameTime = 1500,
            WorldTimer = 36,
        };
        int calls = 0;
        scene.PostProcessScene(4, 0.25f, () =>
        {
            calls++;
            scene.Log.Add("after");
        });
        Assert.Equal(1, calls);
        Assert.Equal(
            "time 0.5 1.5 1.02857149\npass1 4 320 200\nbind fb\nafter\npass2 4 0.25 320 200",
            string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.PostProcessScene(0, 0f, null);
        Assert.Equal(
            "time 0.5 1.5 1.02857149\npass1 0 320 200\nbind fb\npass2 0 0 320 200",
            string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.FrameTime = 9007199254740993UL;
        scene.WorldTimer = 0;
        scene.DeltaTime = -2;
        scene.PostProcessScene(1, 1f, null);
        Assert.StartsWith("time -2 9.00719968E+12 0\n", string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.AmbientOccludeScene(-1.5f);
        scene.EyeCount = 3;
        scene.BlurScene(0.125f);
        scene.ClearTonemapPalette();
        Assert.Equal(
            "ssao -1.5 320 200\nblur 0.125 320 200\nnext eye 3\nblur 0.125 320 200\nnext eye 3\nblur 0.125 320 200\ntonemap clear",
            string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.EyeCount = 1;
        scene.BlurScene(1f);
        Assert.Equal("blur 1 320 200", Assert.Single(scene.Log));

        scene.Log.Clear();
        scene.EyeCount = 2;
        scene.Flush();
        Assert.Equal("draw2d\nnext eye 2\ndraw2d\ntwod clear\npresent stereo", string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.EyeCount = 0;
        scene.Flush();
        Assert.Equal("twod clear\npresent stereo", string.Join('\n', scene.Log));
    }

    [Fact]
    public void Present_ClampsGammaAndSkipsBordersWhenTheClientIsEmpty()
    {
        var scene = new VmPostProcess1280
        {
            EyeCount = 1,
            ClientWidth = 320,
            ClientHeight = 200,
            Letterbox = new VmPostRect1280 { Left = 10, Top = 20, Width = 300, Height = 160 },
            ScreenWidth = 640,
            ScreenHeight = 400,
            BufferWidth = 320,
            BufferHeight = 200,
            Gamma = 0.05f,
            Contrast = 9f,
            Saturation = -20f,
            BlackPoint = 2f,
            WhitePoint = -1f,
            SatFormula = 3,
            DitherBpc = 8,
            HdrActive = true,
            Fullscreen = true,
        };
        scene.Flush();
        Assert.Equal(
            "draw2d\ntwod clear\nshader screen\n" +
            "viewport 0 0 320 200\nclear 0 0 0 1\nscissor on\n" +
            "scissor 0 0 320 20\nscissor 0 180 320 20\nscissor 0 20 10 160\nscissor 310 20 10 160\n" +
            "scissor off\nbind tex 0\nviewport 10 20 300 160\ndither 1\nfilter nearest\n" +
            "uniform 10 3 -15 1 0 gray 3 hdr 1 scale 255 xy 2 2 off 0 0\nquad 8 3",
            string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.Uniforms.GrayFormula = 7;
        scene.HardwareGamma = true;
        scene.DrawPresentTexture(new VmPostRect1280 { Left = 1, Top = 2, Width = 3, Height = 4 }, true);
        Assert.Equal(7, scene.Uniforms.GrayFormula);
        Assert.Contains("uniform 1 1 1 0 1 gray 7 hdr 1 scale 255 xy 2 2 off 0 0", scene.Log);
        Assert.Contains("filter nearest", scene.Log);

        scene.Log.Clear();
        scene.LinearScale = true;
        scene.HardwareGamma = false;
        scene.DrawPresentTexture(scene.Letterbox, false);
        Assert.Equal(7, scene.Uniforms.GrayFormula);
        Assert.Contains("filter linear", scene.Log);
        Assert.Contains("uniform 1 1 1 0 1 gray 7 hdr 1 scale 255 xy 2 2 off 0 0", scene.Log);

        scene.Log.Clear();
        scene.HdrActive = true;
        scene.Fullscreen = false;
        scene.DitherBpc = -1;
        scene.DrawPresentTexture(scene.Letterbox, false);
        Assert.Contains("hdr 0 scale 255", string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.HdrActive = false;
        scene.Fullscreen = true;
        scene.DitherBpc = 10;
        scene.DrawPresentTexture(scene.Letterbox, false);
        Assert.Contains("hdr 0 scale 1023", string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.HdrActive = true;
        scene.Fullscreen = true;
        scene.DitherBpc = -1;
        scene.DrawPresentTexture(scene.Letterbox, false);
        Assert.Contains("hdr 1 scale 1023", string.Join('\n', scene.Log));

        scene.Gamma = 4f;
        scene.Contrast = 0.1f;
        scene.Saturation = 15f;
        scene.BlackPoint = 0f;
        scene.WhitePoint = 5f;
        scene.Log.Clear();
        scene.DrawPresentTexture(new VmPostRect1280 { Left = 4, Top = 5, Width = 6, Height = 7 }, true);
        Assert.Contains("uniform 0.25 0.100000001 15 0 5 gray 3 hdr 1 scale 1023", string.Join('\n', scene.Log));

        scene.ScreenWidth = 100;
        scene.BufferWidth = 3;
        scene.ScreenHeight = 1;
        scene.BufferHeight = 2;
        scene.Log.Clear();
        scene.DrawPresentTexture(scene.Letterbox, false);
        Assert.Contains("xy 33.3333321 0.5 off 0 0", string.Join('\n', scene.Log));

        scene.Log.Clear();
        scene.ClientWidth = 0;
        scene.ClientHeight = 200;
        scene.CopyToBackbuffer(null, false);
        Assert.DoesNotContain("scissor", string.Join('\n', scene.Log));
        Assert.Contains("viewport 10 20 300 160", scene.Log);

        scene.Log.Clear();
        scene.ClientWidth = 100;
        scene.ClientHeight = 80;
        scene.Letterbox = new VmPostRect1280 { Width = 100, Height = 80 };
        scene.ClearBorders();
        Assert.Equal("viewport 0 0 100 80\nclear 0 0 0 1\nscissor on\nscissor off", string.Join('\n', scene.Log));

        scene.Log.Clear();
        var bounds = new VmPostRect1280 { Left = 4, Top = 5, Width = 6, Height = 7 };
        scene.CopyToBackbuffer(bounds, false);
        string joined = string.Join('\n', scene.Log);
        Assert.DoesNotContain("scissor", joined);
        Assert.Contains("viewport 4 5 6 7", joined);
        Assert.Contains("quad 8 3", joined);
    }
}
