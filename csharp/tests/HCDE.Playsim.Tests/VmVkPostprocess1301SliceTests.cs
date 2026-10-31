namespace HCDE.Playsim.Tests;

public class VmVkPostprocess1301SliceTests
{
    [Fact]
    public void Scene_ResolvesAboveOneSampleAndSkipsAMismatchedCopy()
    {
        var host = new VmVkPostprocess1301.Host
        {
            DeltaTime = 0.25,
            FrameTime = 1500,
            WorldTimer = 70,
            TicRate = 35,
        };
        var pp = new VmVkPostprocess1301(host)
        {
            SceneWidth = 320,
            SceneHeight = 200,
            BufferWidth = 640,
            BufferHeight = 400,
            SceneSamples = 4,
            CurrentPipelineImage = 1,
        };
        pp.SceneColor.Width = 320;
        pp.SceneColor.Height = 200;
        pp.Pipeline.Add(new VmVkPostprocess1301.Image("Pipeline0", 640, 400));
        pp.Pipeline.Add(new VmVkPostprocess1301.Image("Pipeline1", 800, 600));
        pp.Depth.Width = 640;
        pp.Depth.Height = 400;

        Assert.Equal(16, pp.Uniforms.Size);
        Assert.Equal(VmVkPostprocess1301.UsageUniform, pp.Uniforms.Usage);
        Assert.Equal(VmVkPostprocess1301.MemoryCpuToGpu, pp.Uniforms.Memory);
        Assert.Equal("AutomaticUniformsBuffer", pp.Uniforms.Name);
        Assert.True(pp.Uniforms.Alive);
        Assert.False(VmVkPostprocess1301.DeltaUsesCurrentArgument);

        int seenPipeline = -1;
        int endsDuring = -1;
        pp.PostProcessScene(2, 0.5f, () =>
        {
            seenPipeline = pp.CurrentPipelineImage;
            endsDuring = pp.Ends;
        });

        Assert.Equal(1, seenPipeline);
        Assert.Equal(0, endsDuring);
        Assert.Equal(1, pp.CurrentPipelineImage);
        Assert.Equal(0.25f, pp.TimeDelta);
        Assert.Equal(1.5f, pp.Time);
        Assert.Equal(2f, pp.TimeGame);
        Assert.False(pp.Uniforms.MappedOpen);
        Assert.Equal(0, pp.Uniforms.MapOffset);
        Assert.Equal(16, pp.Uniforms.MapSize);
        Assert.Equal(new[] { 0.25f, 1.5f, 2f, 0f }, pp.Uniforms.Mapped);
        Assert.Equal(
            new[] { "map", "pass1", "transition", "render-target", "callback", "pass2" },
            pp.Trace);
        Assert.Equal(new VmVkPostprocess1301.Pass("pass1", 2, null, 320, 200), pp.Passes[0]);
        Assert.Equal(new VmVkPostprocess1301.Pass("pass2", 2, 0.5f, 320, 200), pp.Passes[1]);
        VmVkPostprocess1301.TransitionBatch active = Assert.Single(pp.Transitions);
        Assert.Equal("draw", active.Queue);
        Assert.Equal(
            new[]
            {
                new VmVkPostprocess1301.ImageStep("Pipeline1", VmVkPostprocess1301.LayoutColor, false),
                new VmVkPostprocess1301.ImageStep("PipelineDepthStencil", VmVkPostprocess1301.LayoutDepth, false),
            },
            active.Steps);
        Assert.Equal(
            new VmVkPostprocess1301.Target("Pipeline1", "PipelineDepthStencil.View", 640, 400, VmVkPostprocess1301.FormatRgba16f, 1),
            Assert.Single(pp.Targets));

        pp.BlitSceneToPostprocess();
        Assert.Equal(0, pp.CurrentPipelineImage);
        Assert.Equal("resolve", pp.Regions[0].Kind);
        Assert.Equal(320, pp.Regions[0].SrcWidth);
        Assert.Equal(200, pp.Regions[0].SrcHeight);
        Assert.Equal(320, pp.Regions[0].DstWidth);
        Assert.Equal(200, pp.Regions[0].DstHeight);
        Assert.Equal(1, pp.Regions[0].Depth);
        Assert.Equal(-1, pp.Regions[0].Filter);
        Assert.Equal(VmVkPostprocess1301.AspectColor, pp.Regions[0].Aspect);
        Assert.Equal(1, pp.Regions[0].LayerCount);
        Assert.Equal(0, pp.Regions[0].Mip);
        Assert.Equal(0, pp.Regions[0].Layer);
        Assert.Equal("SceneColor", pp.Regions[0].Src);
        Assert.Equal("Pipeline0", pp.Regions[0].Dst);
        Assert.Equal(VmVkPostprocess1301.LayoutTransferSrc, pp.SceneColor.Layout);
        Assert.False(pp.SceneColor.Undefined);
        Assert.Equal(VmVkPostprocess1301.LayoutTransferDst, pp.Pipeline[0].Layout);
        Assert.True(pp.Pipeline[0].Undefined);

        pp.SceneSamples = 1;
        pp.SceneColor.Width = 300;
        pp.SceneColor.Height = 180;
        pp.BlitSceneToPostprocess();
        Assert.Equal("blit", pp.Regions[1].Kind);
        Assert.Equal(VmVkPostprocess1301.FilterNearest, pp.Regions[1].Filter);
        Assert.Equal(300, pp.Regions[1].SrcWidth);
        Assert.Equal(180, pp.Regions[1].SrcHeight);
        Assert.Equal(300, pp.Regions[1].DstWidth);
        Assert.Equal(180, pp.Regions[1].DstHeight);
        Assert.Equal(640, pp.Pipeline[0].Width);

        pp.SceneSamples = 0;
        pp.BlitSceneToPostprocess();
        Assert.Equal("resolve", pp.Regions[2].Kind);
        Assert.Equal(300, pp.Regions[2].SrcWidth);
        Assert.Equal(180, pp.Regions[2].DstHeight);

        var shot = new VmVkPostprocess1301.Image("Shot", 100, 180);
        int transitions = pp.Transitions.Count;
        int regions = pp.Regions.Count;
        int layout = pp.Pipeline[0].Layout;
        pp.CopyCurrentToImage(shot);
        Assert.Equal(transitions, pp.Transitions.Count);
        Assert.Equal(regions, pp.Regions.Count);
        Assert.Equal(layout, pp.Pipeline[0].Layout);
        Assert.Equal(-1, shot.Layout);
        Assert.Equal("end-pass", pp.Trace[^1]);

        shot.Width = 640;
        pp.CopyCurrentToImage(shot);
        Assert.Equal(transitions, pp.Transitions.Count);
        Assert.Equal(-1, shot.Layout);

        shot.Height = 400;
        pp.CopyCurrentToImage(shot, VmVkPostprocess1301.LayoutShaderRead);
        Assert.Equal("copy", pp.Regions[^1].Kind);
        Assert.Equal(640, pp.Regions[^1].SrcWidth);
        Assert.Equal(400, pp.Regions[^1].SrcHeight);
        Assert.Equal(1, pp.Regions[^1].Depth);
        Assert.Equal(VmVkPostprocess1301.AspectColor, pp.Regions[^1].Aspect);
        Assert.Equal(1, pp.Regions[^1].LayerCount);
        Assert.Equal(VmVkPostprocess1301.LayoutColor, pp.Pipeline[0].Layout);
        Assert.False(pp.Pipeline[0].Undefined);
        Assert.Equal(VmVkPostprocess1301.LayoutShaderRead, shot.Layout);
        Assert.False(shot.Undefined);
        Assert.Equal(6, pp.Ends);

        int ends = pp.Ends;
        pp.ImageTransitionScene(true);
        Assert.Equal(ends, pp.Ends);
        VmVkPostprocess1301.TransitionBatch scene = pp.Transitions[^1];
        Assert.Equal("draw", scene.Queue);
        Assert.Equal(
            new[]
            {
                new VmVkPostprocess1301.ImageStep("SceneColor", VmVkPostprocess1301.LayoutColor, true),
                new VmVkPostprocess1301.ImageStep("SceneFog", VmVkPostprocess1301.LayoutColor, true),
                new VmVkPostprocess1301.ImageStep("SceneNormal", VmVkPostprocess1301.LayoutColor, true),
                new VmVkPostprocess1301.ImageStep("SceneDepthStencil", VmVkPostprocess1301.LayoutDepth, true),
            },
            scene.Steps);

        pp.Destroy();
        Assert.False(pp.Uniforms.Alive);
        Assert.True(pp.MembersReleased);
        Assert.Equal("release:AutomaticUniformsBuffer", pp.Trace[^1]);
        pp.Destroy();
        Assert.Equal("release:AutomaticUniformsBuffer", pp.Trace[^1]);
    }

    [Fact]
    public void Present_ClampsGammaAndAdvancesEveryEyeButTheLast()
    {
        var host = new VmVkPostprocess1301.Host
        {
            ViewportWidth = 320,
            ViewportHeight = 240,
            LinearScale = false,
            DitherBpc = -1,
            ColorSpace = VmVkPostprocess1301.ColorSpaceExtendedSrgbLinear,
            Gamma = 0.05f,
            Contrast = 9f,
            Saturation = -20f,
            BlackPoint = -1f,
            WhitePoint = 9f,
            SatFormula = 2,
        };
        var pp = new VmVkPostprocess1301(host)
        {
            SceneWidth = 320,
            SceneHeight = 200,
            BufferWidth = 640,
            BufferHeight = 480,
            CurrentPipelineImage = 4,
        };
        pp.Pipeline.Add(new VmVkPostprocess1301.Image("Pipeline0", 640, 480));
        var box = new VmVkPostprocess1301.Box(8, 12, 100, 80);

        pp.DrawPresentTexture(box, false, false);
        VmVkPostprocess1301.PresentDraw plain = Assert.Single(pp.Presents);
        Assert.Equal(
            new[]
            {
                "screen", "uniforms", "clear", "shader", "bind-uniforms", "viewport",
                "input-current", "input-dither", "output-swapchain", "noblend", "draw",
            },
            plain.Steps);
        Assert.Equal("screen", plain.ScreenShader);
        Assert.Equal("present", plain.Shader);
        Assert.Equal(1f, plain.Uniforms.InvGamma);
        Assert.Equal(1f, plain.Uniforms.Contrast);
        Assert.Equal(1f, plain.Uniforms.Saturation);
        Assert.Equal(0f, plain.Uniforms.BlackPoint);
        Assert.Equal(1f, plain.Uniforms.WhitePoint);
        Assert.Null(plain.Uniforms.GrayFormula);
        Assert.Null(plain.Uniforms.WindowPositionParity);
        Assert.Equal(255f, plain.Uniforms.ColorScale);
        Assert.Equal(0.5f, plain.Uniforms.ScaleX);
        Assert.Equal(-0.5f, plain.Uniforms.ScaleY);
        Assert.Equal(0f, plain.Uniforms.OffsetX);
        Assert.Equal(1f, plain.Uniforms.OffsetY);
        Assert.Equal(0, plain.Uniforms.HdrMode);
        Assert.Equal(VmVkPostprocess1301.FilterNearest, plain.InputFilter);
        Assert.Equal("dither", plain.Dither);
        Assert.Equal(VmVkPostprocess1301.FilterNearest, plain.DitherFilter);
        Assert.Equal(VmVkPostprocess1301.WrapRepeat, plain.DitherWrap);
        Assert.Equal("swapchain", plain.Output);
        Assert.True(plain.NoBlend);
        Assert.Equal(box, plain.Viewport);

        host.LinearScale = true;
        host.DitherBpc = 4;
        pp.DrawPresentTexture(box, true, true);
        VmVkPostprocess1301.PresentDraw shot = pp.Presents[1];
        Assert.Equal(
            new[]
            {
                "uniforms", "clear", "shader", "bind-uniforms", "viewport",
                "input-current", "input-dither", "output-next", "noblend", "draw",
            },
            shot.Steps);
        Assert.Null(shot.ScreenShader);
        Assert.Equal(10f, shot.Uniforms.InvGamma);
        Assert.Equal(3f, shot.Uniforms.Contrast);
        Assert.Equal(-15f, shot.Uniforms.Saturation);
        Assert.Equal(0f, shot.Uniforms.BlackPoint);
        Assert.Equal(5f, shot.Uniforms.WhitePoint);
        Assert.Equal(2, shot.Uniforms.GrayFormula);
        Assert.Null(shot.Uniforms.WindowPositionParity);
        Assert.Equal(15f, shot.Uniforms.ColorScale);
        Assert.Equal(0.5f, shot.Uniforms.ScaleX);
        Assert.Equal(0.5f, shot.Uniforms.ScaleY);
        Assert.Equal(0f, shot.Uniforms.OffsetY);
        Assert.Equal(0, shot.Uniforms.HdrMode);
        Assert.Equal(VmVkPostprocess1301.FilterLinear, shot.InputFilter);
        Assert.Equal("next", shot.Output);

        host.Gamma = 8f;
        host.Contrast = 0.01f;
        host.Saturation = 20f;
        host.BlackPoint = 2f;
        host.WhitePoint = -1f;
        host.SatFormula = -3;
        host.DitherBpc = 8;
        host.LinearScale = false;
        pp.DrawPresentTexture(box, true, false);
        VmVkPostprocess1301.PresentDraw hdr = pp.Presents[2];
        Assert.Equal("screen", hdr.Steps[0]);
        Assert.Equal(0.25f, hdr.Uniforms.InvGamma);
        Assert.Equal(0.1f, hdr.Uniforms.Contrast);
        Assert.Equal(15f, hdr.Uniforms.Saturation);
        Assert.Equal(1f, hdr.Uniforms.BlackPoint);
        Assert.Equal(0f, hdr.Uniforms.WhitePoint);
        Assert.Equal(-3, hdr.Uniforms.GrayFormula);
        Assert.Null(hdr.Uniforms.WindowPositionParity);
        Assert.Equal(255f, hdr.Uniforms.ColorScale);
        Assert.Equal(-0.5f, hdr.Uniforms.ScaleY);
        Assert.Equal(1f, hdr.Uniforms.OffsetY);
        Assert.Equal(1, hdr.Uniforms.HdrMode);
        Assert.Equal("swapchain", hdr.Output);

        host.ColorSpace = 0;
        host.DitherBpc = -1;
        pp.DrawPresentTexture(box, true, false);
        VmVkPostprocess1301.PresentDraw sdr = pp.Presents[3];
        Assert.Equal(0, sdr.Uniforms.HdrMode);
        Assert.Equal(255f, sdr.Uniforms.ColorScale);
        Assert.Null(sdr.Uniforms.WindowPositionParity);

        pp.Services.EyeCount = 0;
        pp.BlurScene(0.4f);
        Assert.Empty(pp.Blooms);
        Assert.Empty(pp.EyeAdvances);
        pp.Services.EyeCount = 1;
        pp.BlurScene(0.4f);
        Assert.Equal(new VmVkPostprocess1301.BloomCall(320, 200, 0.4f), Assert.Single(pp.Blooms));
        Assert.Empty(pp.EyeAdvances);
        pp.Services.EyeCount = 3;
        pp.BlurScene(0.4f);
        Assert.Equal(4, pp.Blooms.Count);
        Assert.Equal(new[] { 3, 3 }, pp.EyeAdvances);
        Assert.Equal(4, pp.CurrentPipelineImage);

        int transitions = pp.Transitions.Count;
        pp.UpdateShadowMap();
        Assert.Equal(0, pp.ShadowUpdates);
        Assert.Equal(0, pp.ShadowFinishes);
        Assert.Equal(transitions, pp.Transitions.Count);
        pp.Services.ShadowCollects = true;
        pp.Shadowmap.Width = 1024;
        pp.UpdateShadowMap();
        Assert.Equal(1, pp.ShadowUpdates);
        Assert.Equal(1, pp.ShadowFinishes);
        VmVkPostprocess1301.TransitionBatch shadow = pp.Transitions[^1];
        Assert.Equal(
            new VmVkPostprocess1301.ImageStep("Shadowmap", VmVkPostprocess1301.LayoutShaderRead, false),
            Assert.Single(shadow.Steps));
        Assert.Equal("draw", shadow.Queue);
        Assert.Equal(new[] { "shadow-update", "transition", "shadow-finish" }, pp.Trace);

        pp.AmbientOccludeScene(0.25f);
        Assert.Equal(new VmVkPostprocess1301.SsaoCall(0.25f, 320, 200), Assert.Single(pp.Ssao));
        Assert.Equal(
            new[]
            {
                new VmVkPostprocess1301.ImageStep("SceneColor", VmVkPostprocess1301.LayoutColor, false),
                new VmVkPostprocess1301.ImageStep("SceneFog", VmVkPostprocess1301.LayoutColor, false),
                new VmVkPostprocess1301.ImageStep("SceneNormal", VmVkPostprocess1301.LayoutColor, false),
                new VmVkPostprocess1301.ImageStep("SceneDepthStencil", VmVkPostprocess1301.LayoutDepth, false),
            },
            pp.Transitions[^1].Steps);
        Assert.Equal(0, pp.Ends);

        pp.ClearTonemapPalette();
        Assert.Equal(1, pp.TonemapClears);
        Assert.Equal("tonemap-clear", pp.Trace[^1]);

        var dest = new VmVkPostprocess1301.Image("Export", 320, 200);
        pp.CurrentPipelineImage = 0;
        pp.BlitCurrentToImage(dest);
        Assert.Equal(1, pp.Ends);
        Assert.Equal("blit", pp.Regions[^1].Kind);
        Assert.Equal(640, pp.Regions[^1].SrcWidth);
        Assert.Equal(480, pp.Regions[^1].SrcHeight);
        Assert.Equal(320, pp.Regions[^1].DstWidth);
        Assert.Equal(200, pp.Regions[^1].DstHeight);
        Assert.Equal(VmVkPostprocess1301.FilterNearest, pp.Regions[^1].Filter);
        Assert.Equal(VmVkPostprocess1301.LayoutTransferSrc, pp.Pipeline[0].Layout);
        Assert.False(pp.Pipeline[0].Undefined);
        Assert.Equal(VmVkPostprocess1301.LayoutShaderRead, dest.Layout);
        Assert.False(dest.Undefined);
        Assert.Equal("Pipeline0", pp.Regions[^1].Src);
        Assert.Equal("Export", pp.Regions[^1].Dst);
    }
}
