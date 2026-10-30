namespace HCDE.Playsim.Tests;

public class VmVkHwTexture1302SliceTests
{
    [Fact]
    public void Upload_WaitsAboveSixtyFourMegabytesAndSkipsANullBuffer()
    {
        var host = new VmVkHwTexture1302.Services();
        var tex = new VmVkHwTexture1302(host, 4);
        Assert.Equal(4, tex.TexelSize);
        Assert.Same(tex, Assert.Single(host.Textures));

        var ex = Assert.Throws<InvalidOperationException>(() => tex.GetImage(Image(0, 4, 1), 3, 0));
        Assert.Equal("Trying to create zero size texture", ex.Message);
        Assert.Throws<InvalidOperationException>(() => tex.GetImage(Image(4, -1, 1), 0, 0));
        Assert.Null(tex.Color.Image);
        Assert.Empty(host.Transfer);
        Assert.Equal(
            new[] { (3, VmVkHwTexture1302.CtfProcessData), (0, VmVkHwTexture1302.CtfProcessData) },
            tex.TexBuffers);
        Assert.Equal(1, VmVkHwTexture1302.GetMipLevels(1, 1));
        Assert.Equal(2, VmVkHwTexture1302.GetMipLevels(2, 1));
        Assert.Equal(3, VmVkHwTexture1302.GetMipLevels(4, 3));
        Assert.Equal(3, VmVkHwTexture1302.GetMipLevels(5, 1));

        VmVkHwTexture1302.Tex src = Image(4, 3, 4);
        VmVkHwTexture1302.TexImage image = tex.GetImage(src, 7, 0);
        Assert.Equal((7, VmVkHwTexture1302.CtfProcessData), tex.TexBuffers[^1]);
        Assert.Equal(VmVkHwTexture1302.FormatBgra, image.Image!.Format);
        Assert.Equal(3, image.Image.Levels);
        Assert.Equal(VmVkHwTexture1302.UsageTransferSrc | VmVkHwTexture1302.UsageTransferDst | VmVkHwTexture1302.UsageSampled, image.Image.Usage);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, image.Layout);
        Assert.Equal(1, tex.MipmapCalls);
        VmVkHwTexture1302.GpuBuffer staging = Assert.Single(host.Transfer);
        Assert.Equal(48, staging.Size);
        Assert.Equal(src.Pixels, staging.Data);
        Assert.Equal(VmVkHwTexture1302.MemoryCpuOnly, staging.Memory);
        Assert.Equal(VmVkHwTexture1302.UsageTransferSrc, staging.Usage);
        Assert.Equal(new VmVkHwTexture1302.Copy("VkHardwareTexture.mStagingBuffer", "VkHardwareTexture.mImage", 4, 3, 1, VmVkHwTexture1302.AspectColor, 1), host.Copies[0]);
        Assert.Equal(new VmVkHwTexture1302.Transition("transfer", "VkHardwareTexture.mImage", VmVkHwTexture1302.LayoutTransferDst, true), host.Transitions[0]);
        Assert.Same(image, tex.GetImage(Image(9, 9, 4), 9, 0));
        Assert.Equal(3, tex.TexBuffers.Count);
        Assert.Equal(0, host.Waits);

        var indexed = new VmVkHwTexture1302(host, 4);
        VmVkHwTexture1302.Tex indexedPixels = Image(2, 2, 1);
        VmVkHwTexture1302.TexImage indexedImage = indexed.GetImage(indexedPixels, 1, VmVkHwTexture1302.CtfIndexed);
        Assert.Equal((1, VmVkHwTexture1302.CtfIndexed | VmVkHwTexture1302.CtfProcessData), indexed.TexBuffers[0]);
        Assert.Equal(VmVkHwTexture1302.FormatR8, indexedImage.Image!.Format);
        Assert.Equal(1, indexedImage.Image.Levels);
        Assert.Equal(0, indexed.MipmapCalls);
        Assert.Equal(VmVkHwTexture1302.LayoutTransferDst, indexedImage.Layout);
        Assert.Equal(indexedPixels.Pixels, host.Transfer[^1].Data);
        Assert.Equal(4, host.Transfer[^1].Size);

        var canvas = new VmVkHwTexture1302(host, 4);
        VmVkHwTexture1302.TexImage hdr = canvas.GetImage(Image(8, 8, 1, canvas: true, hdr: true), 0, VmVkHwTexture1302.CtfIndexed);
        Assert.Empty(canvas.TexBuffers);
        Assert.Equal(VmVkHwTexture1302.FormatRgba32f, hdr.Image!.Format);
        Assert.Equal(VmVkHwTexture1302.UsageColor | VmVkHwTexture1302.UsageSampled, hdr.Image.Usage);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, hdr.Layout);
        Assert.Equal(new VmVkHwTexture1302.Transition("transfer", "VkHardwareTexture.mImage", VmVkHwTexture1302.LayoutShaderRead, true), host.Transitions[^1]);
        var flat = new VmVkHwTexture1302(host, 4);
        Assert.Equal(VmVkHwTexture1302.FormatRgba, flat.GetImage(Image(2, 2, 1, canvas: true), 0, 0).Image!.Format);

        host.SceneDepthFormat = 129;
        var depthTex = new VmVkHwTexture1302(host, 4);
        var depthSource = new VmVkHwTexture1302.Tex { Width = 16, Height = 8 };
        VmVkHwTexture1302.TexImage depth = depthTex.GetDepthStencil(depthSource);
        int transitions = host.Transitions.Count;
        Assert.Same(depth, depthTex.GetDepthStencil(depthSource));
        Assert.Equal(transitions, host.Transitions.Count);
        Assert.Equal(129, depth.Image!.Format);
        Assert.Equal(16, depth.Image.Width);
        Assert.Equal(8, depth.Image.Height);
        Assert.Equal(VmVkHwTexture1302.SampleCount1, depth.Image.Samples);
        Assert.Equal(VmVkHwTexture1302.UsageDepth, depth.Image.Usage);
        Assert.Equal(VmVkHwTexture1302.AspectDepth | VmVkHwTexture1302.AspectStencil, depth.Aspect);
        Assert.Equal(VmVkHwTexture1302.AspectDepth | VmVkHwTexture1302.AspectStencil, depth.View!.Aspect);
        Assert.Equal("VkHardwareTexture.DepthStencil", depth.Image.Name);
        Assert.Equal("VkHardwareTexture.DepthStencilView", depth.View.Name);
        Assert.Equal(VmVkHwTexture1302.LayoutDepth, depth.Layout);
        Assert.Equal(new VmVkHwTexture1302.Transition("transfer", "VkHardwareTexture.DepthStencil", VmVkHwTexture1302.LayoutDepth, true), host.Transitions[^1]);

        tex.Color.PostFramebuffer = "pp";
        tex.Color.Passes.Add((2, "rs-b"));
        tex.Color.Passes.Add((1, "rs-a"));
        tex.Color.DepthOnly = new VmVkHwTexture1302.View { Name = "depth-only" };
        tex.Mapped = new byte[4];
        tex.Color.Image!.MappedOpen = true;
        VmVkHwTexture1302.GpuImage retired = tex.Color.Image;
        int retiredBefore = host.DrawRetired.Count;
        tex.Reset();
        Assert.Null(tex.Mapped);
        Assert.False(retired.MappedOpen);
        Assert.True(retired.Queued);
        Assert.True(retired.Alive);
        Assert.Equal(VmVkHwTexture1302.AspectColor, tex.Color.Aspect);
        Assert.Equal(VmVkHwTexture1302.LayoutUndefined, tex.Color.Layout);
        Assert.Null(tex.Color.Image);
        Assert.Equal(
            new[] { "pp", "rs-a", "rs-b", "depth-only", "VkHardwareTexture.mImageView", "VkHardwareTexture.mImage" },
            host.DrawRetired.Skip(retiredBefore).ToArray());

        var detached = new VmVkHwTexture1302(host, 4);
        detached.GetImage(Image(1, 1, 4), 0, 0);
        byte[] mapped = detached.MapBuffer();
        Assert.Equal(4, mapped.Length);
        Assert.Same(mapped, detached.MapBuffer());
        detached.Live = false;
        int drawCount = host.DrawRetired.Count;
        detached.Reset();
        Assert.Same(mapped, detached.Mapped);
        Assert.Equal(drawCount, host.DrawRetired.Count);
        VmVkHwTexture1302.GpuImage detachedImage = detached.Color.Image!;
        Assert.True(detachedImage.Alive);
        detached.Destroy();
        Assert.False(detachedImage.Alive);
        Assert.Contains(detached, host.Textures);

        var live = new VmVkHwTexture1302(host, 4);
        live.GetImage(Image(1, 1, 4), 0, 0);
        VmVkHwTexture1302.GpuImage liveImage = live.Color.Image!;
        drawCount = host.DrawRetired.Count;
        live.Destroy();
        Assert.False(liveImage.Alive);
        Assert.Equal(drawCount, host.DrawRetired.Count);
        Assert.DoesNotContain(live, host.Textures);

        host.NextAllocatedBytes = 1000;
        var software = new VmVkHwTexture1302(host, 1);
        software.AllocateBuffer(8, 10, 4);
        Assert.Equal(VmVkHwTexture1302.FormatBgra, software.Color.Image!.Format);
        Assert.True(software.Color.Image.Linear);
        Assert.Equal(VmVkHwTexture1302.UsageSampled, software.Color.Image.Usage);
        Assert.Equal(VmVkHwTexture1302.MemoryUnknown, software.Color.Image.Memory);
        Assert.Equal(VmVkHwTexture1302.AllocDedicated | VmVkHwTexture1302.AllocMapped, software.Color.Image.AllocFlags);
        Assert.Equal(VmVkHwTexture1302.MemHostVisible | VmVkHwTexture1302.MemHostCoherent, software.Color.Image.RequiredMemory);
        Assert.Equal(VmVkHwTexture1302.MemHostVisible | VmVkHwTexture1302.MemHostCoherent | VmVkHwTexture1302.MemDeviceLocal, software.Color.Image.PreferredMemory);
        Assert.Equal(1000, software.Color.Image.AllocatedBytes);
        Assert.Equal(25, software.BufferPitch);
        Assert.Equal(4, software.TexelSize);
        Assert.Equal(VmVkHwTexture1302.LayoutGeneral, software.Color.Layout);
        Assert.True(host.Transitions[^1].Undefined);
        byte[] pixels = software.MapBuffer();
        Assert.Equal(320, pixels.Length);
        Assert.Same(pixels, software.MapBuffer());
        VmVkHwTexture1302.GpuImage same = software.Color.Image;
        software.AllocateBuffer(8, 10, 4);
        Assert.Same(same, software.Color.Image);
        Assert.Equal(25, software.BufferPitch);
        host.NextAllocatedBytes = 80;
        software.AllocateBuffer(8, 4, 1);
        Assert.True(same.Queued);
        Assert.True(same.Alive);
        Assert.Equal(VmVkHwTexture1302.FormatR8, software.Color.Image!.Format);
        Assert.Equal(1, software.TexelSize);
        Assert.Equal(20, software.BufferPitch);

        var screen = new VmVkHwTexture1302(host, 4);
        Assert.Equal(0, screen.CreateTexture(null, 8, 8, 3, true, "screen"));
        Assert.Null(screen.Color.Image);
        Assert.Equal(0, screen.CreateTexture(new byte[] { 9, 8, 7, 6 }, 1, 1, 3, false, "screen"));
        Assert.Equal("VkHardwareTexture.mImage", screen.Color.Image!.Name);
        Assert.Equal(VmVkHwTexture1302.LayoutTransferDst, screen.Color.Layout);
        Assert.Equal(0, screen.MipmapCalls);
        Assert.Equal(new byte[] { 9, 8, 7, 6 }, host.Transfer[^1].Data);
        VmVkHwTexture1302.GpuImage replaced = screen.Color.Image;
        drawCount = host.DrawRetired.Count;
        Assert.Equal(0, screen.CreateTexture(new byte[16], 2, 2, 1, true, "ignored"));
        Assert.False(replaced.Alive);
        Assert.Equal(drawCount, host.DrawRetired.Count);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, screen.Color.Layout);

        host.BufferWidth = 640;
        host.BufferHeight = 480;
        int clears = host.Clears.Count;
        var wipe = new VmVkHwTexture1302(host, 1);
        wipe.CreateWipeTexture(32, 16, "wipe");
        Assert.Equal(4, wipe.TexelSize);
        Assert.Equal("wipe", wipe.Color.Image!.Name);
        Assert.Equal("wipe", wipe.Color.View!.Name);
        Assert.Equal(VmVkHwTexture1302.FormatBgra, wipe.Color.Image.Format);
        Assert.Equal(VmVkHwTexture1302.UsageSampled | VmVkHwTexture1302.UsageTransferDst, wipe.Color.Image.Usage);
        Assert.Equal(VmVkHwTexture1302.MemoryGpuOnly, wipe.Color.Image.Memory);
        Assert.Equal(("wipe", VmVkHwTexture1302.LayoutShaderRead), host.Blits[^1]);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, wipe.Color.Layout);
        Assert.Equal(clears, host.Clears.Count);

        host.BufferWidth = 0;
        var empty = new VmVkHwTexture1302(host, 1);
        empty.CreateWipeTexture(4, 4, "empty");
        Assert.Equal(VmVkHwTexture1302.LayoutTransferDst, host.Clears[^1].Layout);
        Assert.Equal(0f, host.Clears[^1].R);
        Assert.Equal(0f, host.Clears[^1].G);
        Assert.Equal(0f, host.Clears[^1].B);
        Assert.Equal(1f, host.Clears[^1].A);
        Assert.Equal(VmVkHwTexture1302.AspectColor, host.Clears[^1].Aspect);
        Assert.Equal(1, host.Clears[^1].Layers);
        Assert.Equal(1, host.Clears[^1].Levels);
        Assert.Equal("empty", host.Clears[^1].Image);
        Assert.Equal(new VmVkHwTexture1302.Transition("transfer", "empty", VmVkHwTexture1302.LayoutShaderRead, false), host.Transitions[^1]);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, empty.Color.Layout);

        host.BufferWidth = 10;
        host.BufferHeight = 0;
        int blits = host.Blits.Count;
        var shortFrame = new VmVkHwTexture1302(host, 1);
        shortFrame.CreateWipeTexture(2, 2, "short");
        Assert.Equal(blits, host.Blits.Count);
        Assert.Equal("short", host.Clears[^1].Image);

        var cap = new VmVkHwTexture1302.Services { TransferBytes = VmVkHwTexture1302.UploadLimit - 16 };
        var under = new VmVkHwTexture1302(cap, 4);
        under.GetImage(Image(2, 2, 4), 0, 0);
        Assert.Equal(VmVkHwTexture1302.UploadLimit, cap.TransferBytes);
        Assert.Equal(0, cap.Waits);
        VmVkHwTexture1302.GpuBuffer held = Assert.Single(cap.Transfer);
        Assert.True(held.Alive);
        cap.TransferBytes = VmVkHwTexture1302.UploadLimit - 15;
        var over = new VmVkHwTexture1302(cap, 4);
        over.GetImage(Image(2, 2, 4), 0, 0);
        Assert.Equal(1, cap.Waits);
        Assert.False(cap.LastWaitFinish);
        Assert.True(cap.LastWaitUpload);
        Assert.Equal(0, cap.TransferBytes);
        Assert.Empty(cap.Transfer);
        Assert.False(held.Alive);
        Assert.Equal("wait:upload", cap.Trace[^1]);
    }

    [Fact]
    public void Descriptors_CacheTheClampAndLeaveTheTwelfthBindingEmpty()
    {
        var host = new VmVkHwTexture1302.Services();
        var mat = new VmVkMaterial1302(host);
        Assert.Same(mat, Assert.Single(host.Materials));
        mat.SwCanvas = true;
        mat.HardwareCanvas = true;
        Assert.Equal(VmVkHwTexture1302.ClampNoFilter, mat.ResolvedClamp(VmVkHwTexture1302.ClampXy));
        mat.SwCanvas = false;
        Assert.Equal(VmVkHwTexture1302.ClampCamtex, mat.ResolvedClamp(VmVkHwTexture1302.ClampXy));
        mat.HardwareCanvas = false;
        mat.Warped = true;
        Assert.Equal(VmVkHwTexture1302.ClampNone, mat.ResolvedClamp(VmVkHwTexture1302.ClampXy));
        Assert.Equal(4, mat.ResolvedClamp(4));
        mat.Warped = false;
        mat.ShaderIndex = VmVkHwTexture1302.FirstUserShader;
        Assert.Equal(VmVkHwTexture1302.ClampNone, mat.ResolvedClamp(1));
        Assert.Equal(4, mat.ResolvedClamp(4));
        mat.ShaderIndex = VmVkHwTexture1302.FirstUserShader - 1;
        Assert.Equal(VmVkHwTexture1302.ClampXy, mat.ResolvedClamp(VmVkHwTexture1302.ClampXy));

        mat.ShaderIndex = 0;
        var first = new VmVkHwTexture1302(host, 4);
        var second = new VmVkHwTexture1302(host, 4);
        mat.Layers.Add(new VmVkMaterial1302.Layer { Hw = first, Source = Image(1, 1, 4) });
        VmVkHwTexture1302.Descriptor set = mat.GetDescriptorSet(new VmVkMaterial1302.State(4, 0));
        Assert.Equal("sampler:4", set.Writes[0].Sampler);
        Assert.Equal(11, set.Slots);
        Assert.Equal(11, set.Writes.Count);
        Assert.Equal(Enumerable.Range(0, 11), set.Writes.Select(write => write.Binding));
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, set.Writes[0].Layout);
        Assert.Equal("VkHardwareTexture.mImageView", set.Writes[0].View);
        Assert.Equal("null-view", set.Writes[1].View);
        Assert.Equal("VkHardwareTexture.mDescriptorSets", set.Name);
        Assert.Equal(1, host.DeviceExecutes);
        Assert.Equal(50000, mat.Entries[0].Remap);
        Assert.Same(set, mat.GetDescriptorSet(new VmVkMaterial1302.State(4, 0)));
        Assert.Equal(1, host.DeviceExecutes);

        int lum = (1 << 30) | 0x11;
        int lookups = host.PaletteLookups;
        VmVkHwTexture1302.Descriptor lumSet = mat.GetDescriptorSet(new VmVkMaterial1302.State(4, lum));
        Assert.Equal(lookups, host.PaletteLookups);
        Assert.Equal(lum, mat.Entries[^1].Remap);
        Assert.NotSame(set, lumSet);
        Assert.True(VmVkMaterial1302.IsLuminosity(lum));
        Assert.False(VmVkMaterial1302.IsLuminosity(0));

        int trans = (2 << 16) | 5;
        mat.GetDescriptorSet(new VmVkMaterial1302.State(4, trans));
        Assert.Equal(52005, mat.Entries[^1].Remap);
        Assert.Equal(lookups + 1, host.PaletteLookups);
        Assert.Equal(2, VmVkMaterial1302.TranslationType(trans));
        Assert.Equal(5, VmVkMaterial1302.TranslationIndex(trans));

        mat.Layers.Add(new VmVkMaterial1302.Layer { Hw = second, Source = Image(1, 1, 4) });
        VmVkHwTexture1302.Descriptor two = mat.GetDescriptorSet(new VmVkMaterial1302.State(8, (1 << 16) | 4));
        Assert.Equal("sampler:8", two.Writes[0].Sampler);
        Assert.Equal((0, (1 << 16) | 4, (1 << 16) | 4), mat.LayerCalls[^2]);
        Assert.Equal((1, 0, 0), mat.LayerCalls[^1]);
        Assert.Equal(second.Color.View!.Name, two.Writes[1].View);
        Assert.Equal("null-view", two.Writes[2].View);
        mat.SwCanvas = true;
        mat.HardwareCanvas = true;
        VmVkHwTexture1302.Descriptor swatched = mat.GetDescriptorSet(new VmVkMaterial1302.State(VmVkHwTexture1302.ClampXy, 8));
        Assert.Equal("sampler:" + VmVkHwTexture1302.ClampNoFilter, swatched.Writes[0].Sampler);

        var indexedMat = new VmVkMaterial1302(host);
        int indexedTranslation = (3 << 16) | 9;
        var layer0 = new VmVkHwTexture1302(host, 4);
        var layer1 = new VmVkHwTexture1302(host, 4);
        var layer2 = new VmVkHwTexture1302(host, 4);
        var unused = new VmVkHwTexture1302(host, 4);
        indexedMat.Layers.Add(new VmVkMaterial1302.Layer { Hw = layer0, Source = Image(2, 2, 1), ScaleFlags = VmVkHwTexture1302.CtfIndexed });
        indexedMat.Layers.Add(new VmVkMaterial1302.Layer { Hw = layer1, Source = Image(2, 2, 4) });
        indexedMat.Layers.Add(new VmVkMaterial1302.Layer { Hw = layer2, Source = Image(2, 2, 4) });
        for (int i = 0; i < 9; i++)
            indexedMat.Layers.Add(new VmVkMaterial1302.Layer { Hw = unused, Source = new VmVkHwTexture1302.Tex() });
        VmVkHwTexture1302.Descriptor indexed = indexedMat.GetDescriptorSet(new VmVkMaterial1302.State(4, indexedTranslation));
        Assert.Equal(12, indexed.Slots);
        Assert.Equal(11, indexed.Writes.Count);
        Assert.Equal(Enumerable.Range(0, 11), indexed.Writes.Select(write => write.Binding));
        Assert.Equal(VmVkHwTexture1302.LayoutTransferDst, indexed.Writes[0].Layout);
        Assert.Equal(VmVkHwTexture1302.FormatR8, layer0.Color.Image!.Format);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, indexed.Writes[1].Layout);
        Assert.Equal(VmVkHwTexture1302.FormatBgra, layer1.Color.Image!.Format);
        Assert.Equal(host.NullView, indexed.Writes[3].View);
        Assert.Equal(VmVkHwTexture1302.LayoutShaderRead, indexed.Writes[10].Layout);
        Assert.Null(unused.Color.Image);
        Assert.Equal(
            new[]
            {
                (0, indexedTranslation, indexedTranslation),
                (1, indexedTranslation, 0),
                (2, indexedTranslation, 0),
            },
            indexedMat.LayerCalls);
        indexedMat.DeleteDescriptors();
        Assert.Empty(indexedMat.Entries);
        Assert.True(indexed.Queued);
        Assert.True(indexed.Alive);
        Assert.Same(indexed, Assert.Single(host.DrawDescriptors));
        int executes = host.DeviceExecutes;
        Assert.NotSame(indexed, indexedMat.GetDescriptorSet(new VmVkMaterial1302.State(4, indexedTranslation)));
        Assert.Equal(executes + 1, host.DeviceExecutes);

        var stuck = new VmVkMaterial1302(host);
        stuck.Layers.Add(new VmVkMaterial1302.Layer { Hw = new VmVkHwTexture1302(host, 4), Source = Image(1, 1, 4) });
        VmVkHwTexture1302.Descriptor kept = stuck.GetDescriptorSet(new VmVkMaterial1302.State(0, 1));
        stuck.Live = false;
        int queued = host.DrawDescriptors.Count;
        stuck.DeleteDescriptors();
        Assert.Equal(queued, host.DrawDescriptors.Count);
        Assert.Single(stuck.Entries);
        stuck.Destroy();
        Assert.Contains(stuck, host.Materials);
        Assert.False(kept.Alive);
        Assert.Empty(stuck.Entries);

        var mortal = new VmVkMaterial1302(host);
        mortal.Destroy();
        Assert.DoesNotContain(mortal, host.Materials);
    }

    static VmVkHwTexture1302.Tex Image(int width, int height, int bytesPerTexel, bool canvas = false, bool hdr = false)
    {
        int count = Math.Max(0, width) * Math.Max(0, height) * bytesPerTexel;
        var pixels = new byte[count];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i + 1);
        return new VmVkHwTexture1302.Tex
        {
            Width = width,
            Height = height,
            Pixels = pixels,
            HardwareCanvas = canvas,
            Hdr = hdr,
        };
    }
}
