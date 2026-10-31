namespace HCDE.Playsim.Tests;

public class VmGlHwTexture1303SliceTests
{
    [Fact]
    public void Upload_ScalesPastTheCapAndUploadsThroughTheUnpackBuffer()
    {
        Array.Clear(VmGlHwTexture1303.LastBound);
        var host = new VmGlHwTexture1303.Host
        {
            MaxTextureSize = 2,
            TextureFilter = 6,
            BoundTexture2D = 9,
            DebugApi = true,
            DebugLevel = 1,
        };
        var tex = new VmGlHwTexture1303(host);
        Assert.Equal(4, tex.TextureBytes);
        Assert.False(VmGlHwTexture1303.Filters[0].Mip);
        Assert.True(VmGlHwTexture1303.Filters[6].Mip);
        Assert.Equal(VmGlHwTexture1303.GlNearest, VmGlHwTexture1303.Filters[6].Mag);
        Assert.Equal(VmGlHwTexture1303.GlLinearMipLinear, VmGlHwTexture1303.Filters[6].Min);

        byte[] src =
        {
            100, 0, 0, 0,
            10, 20, 30, 255,
            1, 0, 0, 255,
            2, 0, 0, 255,
        };
        uint id = tex.CreateTexture(src, 4, 1, -1, true, "wall");
        Assert.Equal(1u, id);
        Assert.Equal("wall", tex.LastName);
        Assert.Equal(0, VmGlHwTexture1303.LastBound[0]);
        var scaled = Assert.Single(host.Uploads);
        Assert.False(scaled.Sub);
        Assert.True(scaled.Freed);
        Assert.False(scaled.FromPbo);
        Assert.Equal(2, scaled.Width);
        Assert.Equal(1, scaled.Height);
        Assert.Equal(VmGlHwTexture1303.GlRgba8, scaled.InternalFormat);
        Assert.Equal(VmGlHwTexture1303.GlBgra, scaled.Source);
        Assert.Equal(16, scaled.Pixels!.Length);
        Assert.Equal(new byte[] { 10, 20, 30, 128, 2, 0, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0 }, scaled.Pixels);
        Assert.Equal((VmGlHwTexture1303.GlTexture, 1, "wall"), Assert.Single(host.Labels));
        Assert.Equal(1, host.Mipmaps);
        Assert.True(tex.Mipmapped);
        Assert.Equal(new[]
        {
            "gen-tex:1", "get-binding:9", "bind:1", "label:5890:1:wall",
            "resize:4x1->2x1", "tex-image", "free", "mipmap", "bind:9", "restore:9",
        }, host.Steps);

        host.TextureFilter = 2;
        host.DebugApi = false;
        host.Steps.Clear();
        uint again = tex.CreateTexture(null, 2, 2, 2, true, null);
        Assert.Equal(id, again);
        Assert.Equal(1, tex.TexId);
        Assert.Equal(1, host.Mipmaps);
        Assert.False(tex.Mipmapped);
        var blank = host.Uploads[^1];
        Assert.Equal(24, blank.Pixels!.Length);
        Assert.All(blank.Pixels, b => Assert.Equal(0, b));
        Assert.Equal(VmGlHwTexture1303.GlBgra, blank.Source);
        Assert.True(blank.Freed);
        Assert.Equal(1, VmGlHwTexture1303.LastBound[2]);
        Assert.Equal(new[]
        {
            "active:" + (VmGlHwTexture1303.GlTexture0 + 2), "bind:1", "calloc:24",
            "tex-image", "free", "active:" + VmGlHwTexture1303.GlTexture0,
        }, host.Steps);
        Assert.Single(host.Labels);

        var packed = new VmGlHwTexture1303(host);
        packed.AllocateBuffer(8, 8, 0);
        Assert.Equal(0, packed.BufferId);
        Assert.Equal(4, packed.TextureBytes);
        Assert.Equal(8, packed.BufferPitch);
        Assert.Null(packed.MapBuffer());
        packed.AllocateBuffer(3, 2, 1);
        Assert.Equal(1, packed.BufferId);
        Assert.Equal(1, packed.TextureBytes);
        Assert.Equal(3, packed.BufferPitch);
        byte[] mapped = packed.MapBuffer()!;
        Assert.Equal(6, mapped.Length);
        mapped[0] = 7;
        mapped[5] = 9;
        host.Uploads.Clear();
        host.Stores.Clear();
        host.Steps.Clear();
        Assert.Equal(2u, packed.CreateTexture(new byte[] { 1, 2, 3 }, 3, 2, 0, false, "pbo"));
        var pbo = Assert.Single(host.Uploads);
        Assert.False(pbo.Sub);
        Assert.True(pbo.FromPbo);
        Assert.False(pbo.Freed);
        Assert.Equal(2, pbo.Width);
        Assert.Equal(2, pbo.Height);
        Assert.Equal(new byte[] { 7, 0, 0, 0, 0, 9 }, pbo.Pixels!);
        Assert.Equal(VmGlHwTexture1303.GlR8, pbo.InternalFormat);
        Assert.Equal(VmGlHwTexture1303.GlRed, pbo.Source);
        Assert.Equal(new[] { 1, 4 }, host.Stores);
        Assert.Equal(2, packed.TexId);
        host.Uploads.Clear();
        Assert.Equal(2u, packed.CreateTexture(new byte[] { 4 }, 3, 2, 0, false, "pbo"));
        Assert.True(Assert.Single(host.Uploads).Sub);
        int deletedTex = host.DeletedTextures, deletedBuf = host.DeletedBuffers;
        packed.Destroy();
        packed.Destroy();
        Assert.Equal(deletedTex + 1, host.DeletedTextures);
        Assert.Equal(deletedBuf + 1, host.DeletedBuffers);
        Assert.Equal(0, packed.TexId);
        Assert.Equal(0, packed.BufferId);
    }

    [Fact]
    public void Bind_CachesTheUnitAndOffsetsTheIndexedClamp()
    {
        Array.Clear(VmGlHwTexture1303.LastBound);
        var host = new VmGlHwTexture1303.Host { MaxTextureSize = 4, TextureFilter = 6 };
        var tex = new VmGlHwTexture1303(host);
        var image = new VmGlHwTexture1303.Tex
        {
            Width = 2,
            Height = 2,
            Pixels = new byte[] { 1, 2, 3, 4 },
        };
        Assert.True(tex.BindOrCreate(image, 0, VmGlHwTexture1303.ClampXy, 7, VmGlHwTexture1303.CtfIndexed));
        Assert.Equal((7, VmGlHwTexture1303.CtfIndexed | VmGlHwTexture1303.CtfProcessData), Assert.Single(tex.Requests));
        Assert.Equal(1, tex.TextureBytes);
        Assert.True(tex.ForceNoFilter);
        Assert.False(tex.Mipmapped);
        var upload = Assert.Single(host.Uploads);
        Assert.Equal(VmGlHwTexture1303.GlR8, upload.InternalFormat);
        Assert.Equal(VmGlHwTexture1303.GlRed, upload.Source);
        Assert.Equal(new[] { 1 }, host.Stores);
        Assert.Equal((0, VmGlHwTexture1303.ClampXy + VmGlHwTexture1303.ClampNoFilter, 255), Assert.Single(host.Samplers));
        Assert.Equal("FHardwareTexture.BindOrCreate", tex.LastName);
        Assert.Contains("sampler:0:8:255", host.Steps);
        Assert.DoesNotContain("mipmap", host.Steps);

        int uploads = host.Uploads.Count;
        Assert.True(tex.BindOrCreate(image, 0, 0, 99, 0));
        Assert.Equal(uploads, host.Uploads.Count);
        Assert.Single(tex.Requests);
        Assert.Equal((0, VmGlHwTexture1303.ClampNoFilter, 255), host.Samplers[^1]);
        Assert.Equal(1, tex.TexId);

        var canvasTex = new VmGlHwTexture1303(host);
        var canvas = new VmGlHwTexture1303.Tex { Width = 8, Height = 2, HardwareCanvas = true, Pixels = new byte[] { 9 } };
        Assert.True(canvasTex.BindOrCreate(canvas, 3, 4, 5, VmGlHwTexture1303.CtfIndexed));
        Assert.Empty(canvasTex.Requests);
        var canvasUpload = host.Uploads[^1];
        Assert.Equal(4, canvasUpload.Width);
        Assert.Equal(2, canvasUpload.Height);
        Assert.Equal(VmGlHwTexture1303.GlR8, canvasUpload.InternalFormat);
        Assert.Equal(4 * 4 * 3, canvasUpload.Pixels!.Length);
        Assert.Equal((3, 4, 255), host.Samplers[^1]);
        Assert.Equal(canvasTex.TexId, VmGlHwTexture1303.LastBound[3]);

        var plain = new VmGlHwTexture1303(host, 4, disableFilter: true);
        var rgba = new VmGlHwTexture1303.Tex { Width = 2, Height = 1, Pixels = new byte[] { 1, 2, 3, 4 } };
        Assert.True(plain.BindOrCreate(rgba, 1, 1, 0, 0));
        Assert.False(plain.Mipmapped);
        Assert.Equal(VmGlHwTexture1303.GlRgba8, host.Uploads[^1].InternalFormat);
        Assert.Equal((1, 1 + VmGlHwTexture1303.ClampNoFilter, 255), host.Samplers[^1]);
        Assert.Contains("active:" + (VmGlHwTexture1303.GlTexture0 + 1), host.Steps);

        host.Steps.Clear();
        VmGlHwTexture1303.Unbind(host, 1);
        Assert.Equal(0, VmGlHwTexture1303.LastBound[1]);
        Assert.Equal(new[]
        {
            "active:" + (VmGlHwTexture1303.GlTexture0 + 1), "bind:0", "active:" + VmGlHwTexture1303.GlTexture0,
        }, host.Steps);
        int before = host.Steps.Count;
        VmGlHwTexture1303.Unbind(host, 1);
        Assert.Equal(before, host.Steps.Count);
        VmGlHwTexture1303.UnbindAll(host);
        Assert.All(VmGlHwTexture1303.LastBound, unit => Assert.Equal(0, unit));

        host.Mipmaps = 0;
        host.Steps.Clear();
        Array.Clear(VmGlHwTexture1303.LastBound);
        var rebound = new VmGlHwTexture1303(host);
        rebound.CreateTexture(new byte[] { 1, 2, 3, 4 }, 1, 1, 0, false, null);
        Assert.Equal(0, host.Mipmaps);
        Assert.Equal((uint)rebound.TexId, rebound.Bind(0, true));
        Assert.Equal(0, host.Mipmaps);
        VmGlHwTexture1303.Unbind(host, 0);
        Assert.Equal((uint)rebound.TexId, rebound.Bind(0, true));
        Assert.Equal(1, host.Mipmaps);
        Assert.True(rebound.Mipmapped);

        host.Steps.Clear();
        var depth = new VmGlHwTexture1303(host) { TexId = 12 };
        depth.BindToFrameBuffer(8, 2);
        int storageSteps = host.Steps.Count(s => s.StartsWith("storage:", StringComparison.Ordinal));
        depth.BindToFrameBuffer(8, 2);
        Assert.Equal(storageSteps, host.Steps.Count(s => s.StartsWith("storage:", StringComparison.Ordinal)));
        Assert.Equal(4, depth.DepthWidth);
        Assert.Equal(2, depth.DepthHeight);
        Assert.Equal(VmGlHwTexture1303.GlDepth24Stencil8, 0x88F0);
        Assert.Contains($"storage:{VmGlHwTexture1303.GlDepth24Stencil8}:4x2", host.Steps);
        Assert.Equal(depth.DepthId, host.Framebuffers.First(f => f.Target == VmGlHwTexture1303.GlDepthAttach).Id);
        Assert.Equal(depth.DepthId, host.Framebuffers.First(f => f.Target == VmGlHwTexture1303.GlStencilAttach).Id);
        Assert.Equal(12, host.Framebuffers[0].Id);
    }
}
