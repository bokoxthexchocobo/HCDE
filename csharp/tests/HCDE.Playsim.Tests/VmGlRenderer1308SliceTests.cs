namespace HCDE.Playsim.Tests;

public class VmGlRenderer1308SliceTests
{
    [Fact]
    public void Initialize_LabelsTheVaoAndBindsOffscreenOnce()
    {
        var host = new VmGlRenderer1308.Host
        {
            NextVao = 7,
            NextFramebuffer = 9,
            FramebufferBinding = 3,
            ScreenWidth = 320,
            ScreenHeight = 200,
            SceneWidth = 640,
            SceneHeight = 480,
        };
        var renderer = new VmGlRenderer1308(host);
        renderer.Initialize(1, 2);

        Assert.Equal(0, renderer.FbId);
        Assert.Equal(7, renderer.VaoId);
        Assert.Equal(0, renderer.OldFbId);
        Assert.Same(renderer.ScreenBuffers, renderer.Buffers);
        Assert.NotSame(renderer.ScreenBuffers, renderer.SaveBuffers);
        Assert.Empty(renderer.ScreenBuffers!.Setups);
        Assert.True(renderer.Present && renderer.Checker && renderer.Column && renderer.Row && renderer.Shadow);
        Assert.True(renderer.ShaderManager && renderer.SamplerManager);
        Assert.Equal(
            new[]
            {
                "gen-vao",
                "bind-vao:7",
                $"label:{VmGlRenderer1308.GlVertexArray}:7:FGLRenderer.mVAOID",
            },
            host.Steps);

        renderer.BeginFrame();
        Assert.Equal("320x200:640x480", Assert.Single(renderer.ScreenBuffers.Setups));
        Assert.Equal("216x162:216x162", Assert.Single(renderer.SaveBuffers!.Setups));

        Assert.True(renderer.StartOffscreen());
        Assert.Equal(9, renderer.FbId);
        Assert.Equal(3, renderer.OldFbId);
        host.FramebufferBinding = 4;
        Assert.True(renderer.StartOffscreen());
        Assert.Equal(9, renderer.FbId);
        Assert.Equal(4, renderer.OldFbId);
        renderer.EndOffscreen();

        Assert.Equal(
            new[]
            {
                "gen-fb",
                $"get:{VmGlRenderer1308.GlFramebufferBinding}",
                $"bind-fb:{VmGlRenderer1308.GlFramebuffer}:9",
                $"label:{VmGlRenderer1308.GlFramebuffer}:9:OffscreenFB",
                $"get:{VmGlRenderer1308.GlFramebufferBinding}",
                $"bind-fb:{VmGlRenderer1308.GlFramebuffer}:9",
                $"bind-fb:{VmGlRenderer1308.GlFramebuffer}:4",
            },
            host.Steps.Skip(3));

        renderer.Destroy();
        Assert.Equal(
            new[]
            {
                "flush-models",
                "flush-textures",
                "del:shader-manager",
                "del:sampler-manager",
                "del-fb:9",
                "bind-vao:0",
                "del-vao:7",
                "del:screen-buffers",
                "del:save-buffers",
                "del:present",
                "del:checker",
                "del:column",
                "del:row",
                "del:shadow",
            },
            host.Steps.TakeLast(14));
    }

    [Fact]
    public void Bind_UsesTheBaseLayerAndDestroyBeforeInitOnlyFlushes()
    {
        var host = new VmGlRenderer1308.Host();
        var renderer = new VmGlRenderer1308(host);
        Assert.Throws<InvalidOperationException>(() => renderer.BindToFrameBuffer(new VmGlRenderer1308.Tex()));
        Assert.Throws<InvalidOperationException>(() => renderer.BeginFrame());

        var layer = new VmGlRenderer1308.Layer();
        renderer.BindToFrameBuffer(new VmGlRenderer1308.Tex { Width = 64, Height = 32, Hardware = layer });
        Assert.Equal(
            new[]
            {
                "bind-or-create:64x32:0:0:0:0",
                "bind-fb:64x32",
            },
            layer.Calls);
        Assert.Equal(new[] { "unbind:0", "clear-material" }, host.Steps);

        renderer.Destroy();
        Assert.Equal(
            new[] { "unbind:0", "clear-material", "flush-models", "flush-textures" },
            host.Steps);
    }
}
