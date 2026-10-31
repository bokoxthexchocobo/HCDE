namespace HCDE.Playsim.Tests;

public class VmVkTexture1295SliceTests
{
    [Fact]
    public void Lightmap_ForcesAlphaAndShadowRecreatesOnWidth()
    {
        var textures = new VmVkTexture1295(256);
        Assert.Equal(1, textures.NullImage!.Width);
        Assert.Equal(VmVkTexture1295.Rgba8, textures.NullImage.Format);
        Assert.Equal(VmVkTexture1295.Sampled, textures.NullImage.Usage);
        Assert.Equal("VkDescriptorSetManager.NullTexture", textures.NullImage.Name);
        Assert.Equal(256, textures.ShadowImage!.Width);
        Assert.Equal(1024, textures.ShadowImage.Height);
        Assert.Equal(VmVkTexture1295.R32f, textures.ShadowImage.Format);
        Assert.Equal(VmVkTexture1295.ColorAttachment | VmVkTexture1295.Sampled, textures.ShadowImage.Usage);
        Assert.Equal(new ushort[] { 0, 0, 0, VmVkTexture1295.HalfOne }, textures.LightTexels);
        Assert.Equal(3, textures.LightSourceRead);
        Assert.Equal(8, textures.StagingBytes);
        Assert.Equal(1, textures.LightResets);

        textures.SetLightmap(1, 1, new ushort[] { 1, 2, 3, 9 });
        Assert.Equal(new ushort[] { 1, 2, 3, VmVkTexture1295.HalfOne }, textures.LightTexels);
        Assert.Equal(3, textures.LightSourceRead);
        Assert.Equal(VmVkTexture1295.View2DArray, textures.LightView!.ViewType);
        Assert.Equal(VmVkTexture1295.LayoutTransferDst, textures.Transitions[^2].Layout);
        Assert.True(textures.Transitions[^2].Undefined);
        Assert.Equal(1, textures.Transitions[^2].LevelCount);
        Assert.Equal(VmVkTexture1295.LayoutShaderRead, textures.Transitions[^1].Layout);
        Assert.False(textures.Transitions[^1].Undefined);
        Assert.Equal(1, textures.CopyLayers);
        Assert.Equal(1, textures.CopyDepth);
        Assert.Equal("VkHardwareTexture.mStagingBuffer", textures.DeletedStaging[^1]);

        int resets = textures.ShadowResets;
        textures.BeginFrame();
        Assert.Equal(resets, textures.ShadowResets);
        textures.ShadowQuality = 128;
        textures.BeginFrame();
        Assert.Equal(resets + 1, textures.ShadowResets);
        Assert.Equal(128, textures.ShadowImage.Width);
        Assert.Equal(1024, textures.ShadowImage.Height);
    }

    [Fact]
    public void Lookup_WrapsThePipelineAndDrainsFromTheBack()
    {
        var textures = new VmVkTexture1295(64);
        textures.PipelineIndex = 1;
        Assert.Equal("pipeline-1", textures.GetTexture(VmVkTexture1295.CurrentPipeline, null));
        Assert.Equal("pipeline-0", textures.GetTexture(VmVkTexture1295.NextPipeline, null));
        Assert.Null(textures.GetTexture(VmVkTexture1295.SwapChain, null));
        Assert.Equal("scene-depth", textures.GetTexture(VmVkTexture1295.SceneDepth, null));
        Assert.Equal("shadow", textures.GetTexture(VmVkTexture1295.ShadowMap, null));
        var missing = Assert.Throws<VmVkTexture1295.Failure>(() => textures.GetTexture(99, null));
        Assert.Equal("VkPPRenderState::GetTexture not implemented yet for this texture type", missing.Message);

        var post = new VmVkTexture1295.PpTexture { Name = "bloom", Format = 97 };
        Assert.False(post.HasBackend);
        Assert.Equal(97, textures.GetFormat(post));
        Assert.True(post.HasBackend);
        Assert.Equal("pp:bloom", textures.GetTexture(VmVkTexture1295.PostTexture, post));

        var first = new VmVkTexture1295.HardwareTexture { Name = "first" };
        var second = new VmVkTexture1295.HardwareTexture { Name = "second" };
        textures.AddTexture(first);
        textures.AddTexture(second);
        var early = new VmVkTexture1295.PpTexture { Name = "early" };
        var late = new VmVkTexture1295.PpTexture { Name = "late" };
        textures.AddPost(early);
        textures.AddPost(late);
        textures.Destroy();
        Assert.Equal(new[] { "second", "first", "late", "early" }, textures.Removed);
        Assert.Empty(textures.Textures);
        Assert.Empty(textures.PostTextures);
        Assert.True(second.Reset);
        Assert.True(second.Detached);
        Assert.True(late.Detached);
    }
}
