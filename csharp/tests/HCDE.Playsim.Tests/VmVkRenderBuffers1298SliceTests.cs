namespace HCDE.Playsim.Tests;

public class VmVkRenderBuffers1298SliceTests
{
    [Fact]
    public void Samples_LeaveThePipelineWhenOnlyTheCountChanges()
    {
        var host = new VmVkRenderBuffers1298.Host { SampleMask = 0, Multisample = -5 };
        var buffers = new VmVkRenderBuffers1298(host);
        Assert.Equal(VmVkRenderBuffers1298.Sample1, buffers.GetBestSampleCount());
        Assert.Empty(host.DepthQueries);

        host.SampleMask = VmVkRenderBuffers1298.Sample1 | VmVkRenderBuffers1298.Sample4;
        host.Multisample = 4;
        Assert.Equal(VmVkRenderBuffers1298.Sample4, buffers.GetBestSampleCount());
        host.Multisample = 100;
        Assert.Equal(VmVkRenderBuffers1298.Sample4, buffers.GetBestSampleCount());
        host.Multisample = 0;
        Assert.Equal(VmVkRenderBuffers1298.Sample1, buffers.GetBestSampleCount());

        buffers.BeginFrame(0, 0, 10, 12);
        Assert.Equal(0, host.Waits);
        Assert.Equal(0, buffers.Executes);
        Assert.Equal(10, buffers.SceneWidth);
        Assert.Equal(12, buffers.SceneHeight);
        Assert.False(buffers.Pipeline[0].HasImage);

        host.NormalFormats.Remove(VmVkRenderBuffers1298.FormatA2R10);
        buffers.BeginFrame(320, 200, 320, 200);
        Assert.Equal(1, host.Waits);
        Assert.Equal(1, host.Resets);
        Assert.Equal(2, buffers.Executes);
        Assert.Equal(VmVkRenderBuffers1298.Sample1, buffers.Samples);
        Assert.Equal(1, buffers.Pipeline[0].Samples);
        Assert.Equal(1, buffers.PipelineDepth.Samples);
        Assert.Equal(VmVkRenderBuffers1298.FormatRgba16f, buffers.Pipeline[0].Format);
        Assert.Equal(VmVkRenderBuffers1298.UsageColor | VmVkRenderBuffers1298.UsageSampled | VmVkRenderBuffers1298.UsageTransferSrc | VmVkRenderBuffers1298.UsageTransferDst, buffers.Pipeline[0].Usage);
        Assert.Equal(VmVkRenderBuffers1298.FormatD24S8, buffers.PipelineDepthFormat);
        Assert.Equal(VmVkRenderBuffers1298.AspectDepth | VmVkRenderBuffers1298.AspectStencil, buffers.PipelineDepth.Aspect);
        Assert.Equal("VkRenderBuffers.PipelineDepthView", buffers.PipelineDepth.DepthViewName);
        Assert.Equal(VmVkRenderBuffers1298.UsageColor | VmVkRenderBuffers1298.UsageSampled | VmVkRenderBuffers1298.UsageTransferSrc, buffers.SceneColor.Usage);
        Assert.Equal(1, buffers.SceneColor.Samples);
        Assert.Equal(VmVkRenderBuffers1298.FormatRgba8, buffers.SceneFog.Format);
        Assert.Equal(VmVkRenderBuffers1298.UsageColor | VmVkRenderBuffers1298.UsageSampled, buffers.SceneFog.Usage);
        Assert.Equal(VmVkRenderBuffers1298.FormatRgba8, buffers.SceneNormalFormat);
        Assert.Equal(new[] { VmVkRenderBuffers1298.FormatA2R10 }, host.NormalQueries);
        Assert.Equal(2, buffers.Pipeline.Count(image => image.HasImage));
        int pipelineId = buffers.Pipeline[0].Id;
        int sceneId = buffers.SceneColor.Id;
        Assert.Equal(VmVkRenderBuffers1298.LayoutDepth, buffers.Transitions[0].Layout);
        Assert.True(buffers.Transitions[0].Undefined);
        Assert.Equal(7, buffers.Transitions.Count);

        buffers.BeginFrame(320, 200, 300, 180);
        Assert.Equal(1, host.Waits);
        Assert.Equal(pipelineId, buffers.Pipeline[0].Id);
        Assert.Equal(sceneId, buffers.SceneColor.Id);
        Assert.Equal(300, buffers.SceneWidth);
        Assert.Equal(180, buffers.SceneHeight);

        host.Multisample = 4;
        buffers.BeginFrame(320, 200, 300, 180);
        Assert.Equal(2, host.Waits);
        Assert.Equal(pipelineId, buffers.Pipeline[0].Id);
        Assert.NotEqual(sceneId, buffers.SceneColor.Id);
        Assert.Equal(4, buffers.SceneColor.Samples);
        Assert.Equal(1, buffers.Pipeline[0].Samples);
        Assert.Equal(VmVkRenderBuffers1298.Sample4, buffers.Samples);
        Assert.Equal(3, buffers.Executes);
    }

    [Fact]
    public void Output_CachesTheFramebufferAndDepthFallbackFatals()
    {
        var host = new VmVkRenderBuffers1298.Host
        {
            Multisample = 1,
            SampleMask = VmVkRenderBuffers1298.Sample1,
            PresentImageIndex = 1,
            SwapWidth = 640,
            SwapHeight = 480,
        };
        host.DepthFormats.Remove(VmVkRenderBuffers1298.FormatD24S8);
        var buffers = new VmVkRenderBuffers1298(host);
        buffers.BeginFrame(64, 64, 64, 64);
        Assert.Equal(VmVkRenderBuffers1298.FormatD32S8, buffers.PipelineDepthFormat);
        Assert.Equal(VmVkRenderBuffers1298.FormatD32S8, buffers.SceneDepthFormat);
        Assert.Equal(new[] { VmVkRenderBuffers1298.FormatD24S8, VmVkRenderBuffers1298.FormatD32S8, VmVkRenderBuffers1298.FormatD24S8, VmVkRenderBuffers1298.FormatD32S8 }, host.DepthQueries);
        int transitions = buffers.Transitions.Count;

        var tex = new VmVkRenderBuffers1298.Image { Name = "post", Width = 32, Height = 16 };
        var first = buffers.GetOutput(tex, VmVkRenderBuffers1298.NextPipeline, VmVkRenderBuffers1298.StencilScene, 9);
        Assert.True(first.HasFramebuffer);
        Assert.Equal(2, first.FrameAttachments);
        Assert.Equal(9, first.FramePass);
        Assert.Equal(32, first.Width);
        Assert.True(buffers.Transitions[transitions].Undefined);
        Assert.Equal(VmVkRenderBuffers1298.LayoutColor, buffers.Transitions[transitions].Layout);
        Assert.False(buffers.Transitions[transitions + 1].Undefined);
        Assert.Equal(buffers.SceneDepth.Name, buffers.Transitions[transitions + 1].Name);
        int after = buffers.Transitions.Count;
        var second = buffers.GetOutput(tex, 0, VmVkRenderBuffers1298.StencilNone, 4);
        Assert.Same(first, second);
        Assert.Equal(9, second.FramePass);
        Assert.Equal(after + 1, buffers.Transitions.Count);
        Assert.False(buffers.Transitions[^1].Undefined);
        Assert.Equal("post", buffers.Transitions[^1].Name);

        int draws = host.Draws;
        var swap = buffers.GetOutput(null, 7, VmVkRenderBuffers1298.StencilPipeline, 3);
        Assert.Equal(draws, host.Draws);
        Assert.Equal(640, swap.Width);
        Assert.Equal(480, swap.Height);
        Assert.Equal(2, swap.FrameAttachments);
        Assert.Equal("PPOutputFB", swap.Name);
        var again = buffers.GetOutput(null, 7, VmVkRenderBuffers1298.StencilNone, 8);
        Assert.Same(swap, again);
        Assert.Equal(3, again.FramePass);
        Assert.Equal(2, again.FrameAttachments);

        int kept = buffers.SceneColor.Id;
        buffers.Destroy();
        Assert.True(buffers.Destroyed);
        Assert.Equal(0, buffers.ImagesReleased);
        Assert.Equal(kept, buffers.SceneColor.Id);

        var bare = new VmVkRenderBuffers1298.Host { DepthFormats = new HashSet<int>() };
        var failing = new VmVkRenderBuffers1298(bare);
        var error = Assert.Throws<VmVkRenderBuffers1298.Failure>(() => failing.BeginFrame(8, 8, 8, 8));
        Assert.Equal("This device does not support any of the required depth stencil image formats.", error.Detail);
        Assert.Equal(VmVkRenderBuffers1298.FormatD32S8, failing.PipelineDepthFormat);
        Assert.False(failing.PipelineDepth.HasImage);
    }
}
