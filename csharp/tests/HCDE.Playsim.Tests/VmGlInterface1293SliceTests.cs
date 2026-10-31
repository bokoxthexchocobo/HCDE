namespace HCDE.Playsim.Tests;

public class VmGlInterface1293SliceTests
{
    [Fact]
    public void Load_CapsTheBlockAndPrintsTheLiveQueries()
    {
        var gl = new VmGlInterface1293
        {
            NumExtensions = 4,
            GlVersion = "4.6.0 NVIDIA",
            ShadingLanguage = "4.60",
            Vendor = "NVIDIA Corporation",
            Renderer = "GeForce",
            MaxVertexShaderStorageBlocks = 0,
            MaxFragmentUniformComponents = 1024,
            MaxUniformBlockSize = 100000,
            UniformBufferOffsetAlignment = 256,
            MaxTextureSize = 16384,
            ContextProfileMask = VmGlInterface1293.CoreProfileBit,
            MaxTextureImageUnits = 16,
            MaxVaryingFloats = 32,
        };
        gl.ReportedExtensions.Add("GL_ARB_texture_compression");
        gl.ReportedExtensions.Add("GL_EXT_texture_compression_s3tc");
        gl.ReportedExtensions.Add("GL_ARB_bindless_texture");
        gl.ReportedExtensions.Add("gl_khr_debug");

        gl.LoadExtensions();

        int flags = VmGlInterface1293.TextureCompression
            | VmGlInterface1293.TextureCompressionS3tc
            | VmGlInterface1293.BufferStorage
            | VmGlInterface1293.InvalidateBuffer
            | VmGlInterface1293.Debug;
        Assert.Equal(flags, gl.Flags);
        Assert.Equal(0, gl.Flags & VmGlInterface1293.ShaderStorageBuffer);
        Assert.Equal(0, gl.Flags & VmGlInterface1293.NoClipPlanes);
        Assert.True(gl.Bindless);
        Assert.Equal(65536, gl.MaxUniformBlock);
        Assert.Equal(1024, gl.MaxUniforms);
        Assert.Equal(256, gl.UniformBlockAlignment);
        Assert.Equal(16384, gl.MaxTextureSizeStored);
        Assert.Equal(1, gl.UnpackAlignment);
        Assert.True(gl.PixelStoreCalled);
        Assert.Equal("NVIDIA Corporation", gl.VendorStored);
        Assert.Equal("GeForce", gl.ModelStored);
        Assert.Empty(gl.Log);
        var info = gl.GetInfo();
        Assert.Equal(gl.RealVersion, info.Version);
        Assert.True(info.Bindless);
        gl.SetGlVersion(1.5);
        Assert.Equal(1.5, gl.GetInfo().Version);
        Assert.True(gl.GetInfo().Bindless);

        gl.Vendor = "NV";
        gl.MaxTextureSize = 99;
        gl.PrintStartupLog();
        Assert.Equal(new[]
        {
            "GL_VENDOR: NV\n",
            "GL_RENDERER: GeForce\n",
            "GL_VERSION: 4.6.0 NVIDIA (Core profile)\n",
            "GL_SHADING_LANGUAGE_VERSION: 4.60\n",
            "GL_EXTENSIONS:",
            " GL_ARB_texture_compression",
            " GL_EXT_texture_compression_s3tc",
            " GL_ARB_bindless_texture",
            " gl_khr_debug",
            "\nMax. texture size: 99\n",
            "Max. texture units: 16\n",
            "Max. varying: 32\n",
            "Max. uniform block size: 100000\n",
            "Uniform block alignment: 256\n",
        }, gl.Log);
        Assert.Equal("NVIDIA Corporation", gl.VendorStored);
        Assert.Equal(16384, gl.MaxTextureSizeStored);
    }

    [Fact]
    public void Reload_KeepsStaleExtensionsAndClearsFlagsOnFailure()
    {
        var gl = new VmGlInterface1293
        {
            Windows = true,
            NumExtensions = 1,
            GlVersion = "3.2.0",
            VersionOverride = "3.1",
            Vendor = "ATI Technologies",
            Renderer = "Radeon",
            ShadingLanguage = "1.50",
        };
        gl.ReportedExtensions.Add("GL_ARB_texture_compression");
        var fail = Assert.Throws<VmGlInterface1293.Failure>(() => gl.LoadExtensions());
        Assert.Equal("Unsupported OpenGL version.\nAt least OpenGL 3.3 is required to run HCDE.\n", fail.Message);
        Assert.Empty(gl.Log);
        Assert.Equal(0, gl.Flags);
        Assert.Equal(new[] { "GL_ARB_texture_compression" }, gl.Extensions);
        Assert.False(gl.PixelStoreCalled);

        gl.GlVersion = "3.3.0";
        gl.ReportedExtensions[0] = "GL_ARB_invalidate_subdata";
        gl.MaxUniformBlockSize = 100;
        gl.MaxFragmentUniformComponents = 2048;
        gl.UniformBufferOffsetAlignment = 32;
        gl.MaxTextureSize = 8192;
        gl.LoadExtensions();

        Assert.Equal(new[] { "Emulating OpenGL v 3.3\n" }, gl.Log);
        Assert.Equal(VmGlInterface1293.TextureCompression | VmGlInterface1293.InvalidateBuffer | VmGlInterface1293.NoClipPlanes, gl.Flags);
        Assert.Equal(0, gl.Flags & VmGlInterface1293.Debug);
        Assert.Equal(0, gl.Flags & VmGlInterface1293.BufferStorage);
        Assert.Equal((double)3.31f, gl.GlslVersion);
        Assert.Equal(100, gl.MaxUniformBlock);
        Assert.Equal(2048, gl.MaxUniforms);
        Assert.False(gl.Bindless);
        Assert.True(gl.PixelStoreCalled);
        Assert.Equal(new[] { "GL_ARB_texture_compression", "GL_ARB_invalidate_subdata" }, gl.Extensions);

        gl.VersionOverride = null;
        gl.GlVersion = "2.0";
        gl.ReportedExtensions[0] = "GL_KHR_debug";
        var again = Assert.Throws<VmGlInterface1293.Failure>(() => gl.LoadExtensions());
        Assert.Equal(fail.Message, again.Message);
        Assert.Equal(0, gl.Flags);
        Assert.False(gl.Bindless);
        Assert.Equal((double)3.31f, gl.GlslVersion);
        Assert.Equal("ATI Technologies", gl.VendorStored);
        Assert.Equal(3, gl.Extensions.Count);
        Assert.Equal("GL_KHR_debug", gl.Extensions[2]);
        Assert.Equal(new[] { "Emulating OpenGL v 3.3\n" }, gl.Log);
    }
}
