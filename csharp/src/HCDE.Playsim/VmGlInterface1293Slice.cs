using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// OpenGL version gate, extension flags, and the startup log from <c>gl_interface.cpp</c>
/// (1-243). Chunk 1293. The GL strings and integer queries are host inputs.
/// </summary>
public sealed class VmGlInterface1293
{
    public const int TextureCompression = 1;
    public const int TextureCompressionS3tc = 2;
    public const int ShaderStorageBuffer = 4;
    public const int BufferStorage = 8;
    public const int NoClipPlanes = 32;
    public const int InvalidateBuffer = 64;
    public const int Debug = 128;
    public const int CoreProfileBit = 1;

    public sealed class Failure : Exception
    {
        public readonly string Detail;

        public Failure(string message) : base(message)
        {
            Detail = message;
        }
    }

    public bool Windows;
    public int NumExtensions;
    public List<string?> ReportedExtensions = new();
    public string? GlVersion;
    public string? ShadingLanguage;
    public string? Vendor;
    public string? Renderer;
    public string? VersionOverride;
    public int MaxVertexShaderStorageBlocks;
    public int MaxFragmentUniformComponents;
    public int MaxUniformBlockSize;
    public int UniformBufferOffsetAlignment;
    public int MaxTextureSize;
    public int ContextProfileMask;
    public int MaxTextureImageUnits;
    public int MaxVaryingFloats;
    public int MaxCombinedShaderStorageBlocks;

    public List<string> Extensions = new();
    public List<string> Log = new();
    public int Flags;
    public double RealVersion;
    public double GlslVersion;
    public string? VendorStored;
    public string? ModelStored;
    public int MaxUniforms;
    public int MaxUniformBlock;
    public int UniformBlockAlignment;
    public int MaxTextureSizeStored;
    public bool Bindless;
    public int UnpackAlignment;
    public bool PixelStoreCalled;

    public void LoadExtensions()
    {
        Flags = 0;
        for (int i = 0; i < NumExtensions; i++)
        {
            string? name = i < ReportedExtensions.Count ? ReportedExtensions[i] : null;
            Extensions.Add(name ?? "");
        }

        string? version = VersionOverride;
        RealVersion = Strtod(GlVersion);
        if (version == null)
        {
            version = GlVersion;
        }
        else
        {
            double requested = Strtod(version);
            if (requested >= 3.0 && requested < 3.3)
            {
                requested = 3.3;
                version = "3.3";
            }

            if (RealVersion < requested)
                version = GlVersion;
            else
                Log.Add("Emulating OpenGL v " + version + "\n");
        }

        float glVersion = (float)Strtod(version) + 0.01f;
        if (glVersion < 3.3f)
        {
            throw new Failure("Unsupported OpenGL version.\nAt least OpenGL 3.3 is required to run HCDE.\n");
        }

        GlslVersion = Strtod(ShadingLanguage) + 0.01;
        VendorStored = Vendor;
        ModelStored = Renderer;
        if (Has("GL_ARB_texture_compression"))
            Flags |= TextureCompression;
        if (Has("GL_EXT_texture_compression_s3tc"))
            Flags |= TextureCompressionS3tc;

        if (glVersion < 4f)
        {
            if (Windows && (VendorStored ?? "").IndexOf("ATI Tech", StringComparison.Ordinal) >= 0)
                Flags |= NoClipPlanes;
            GlslVersion = 3.31f;
        }
        else if (glVersion >= 4.5f)
        {
            Flags |= ShaderStorageBuffer | BufferStorage;
            if (MaxVertexShaderStorageBlocks == 0)
                Flags &= ~ShaderStorageBuffer;
        }

        if (glVersion >= 4.3f || Has("GL_ARB_invalidate_subdata"))
            Flags |= InvalidateBuffer;
        if (glVersion >= 4.3f || Has("GL_KHR_debug"))
            Flags |= Debug;

        MaxUniforms = MaxFragmentUniformComponents;
        MaxUniformBlock = Math.Min(65536, MaxUniformBlockSize);
        UniformBlockAlignment = UniformBufferOffsetAlignment;
        MaxTextureSizeStored = MaxTextureSize;
        UnpackAlignment = 1;
        PixelStoreCalled = true;
        Bindless = Has("GL_ARB_bindless_texture");
    }

    public void PrintStartupLog()
    {
        string profile = (ContextProfileMask & CoreProfileBit) != 0 ? "Core" : "Compatibility";
        Log.Add("GL_VENDOR: " + (Vendor ?? "") + "\n");
        Log.Add("GL_RENDERER: " + (Renderer ?? "") + "\n");
        Log.Add("GL_VERSION: " + (GlVersion ?? "") + " (" + profile + " profile)\n");
        Log.Add("GL_SHADING_LANGUAGE_VERSION: " + (ShadingLanguage ?? "") + "\n");
        Log.Add("GL_EXTENSIONS:");
        foreach (var name in Extensions)
            Log.Add(" " + name);
        Log.Add("\nMax. texture size: " + MaxTextureSize + "\n");
        Log.Add("Max. texture units: " + MaxTextureImageUnits + "\n");
        Log.Add("Max. varying: " + MaxVaryingFloats + "\n");
        if ((Flags & ShaderStorageBuffer) != 0)
        {
            Log.Add("Max. combined shader storage blocks: " + MaxCombinedShaderStorageBlocks + "\n");
            Log.Add("Max. vertex shader storage blocks: " + MaxVertexShaderStorageBlocks + "\n");
        }
        else
        {
            Log.Add("Max. uniform block size: " + MaxUniformBlockSize + "\n");
            Log.Add("Uniform block alignment: " + UniformBufferOffsetAlignment + "\n");
        }
    }

    public void SetGlVersion(double version)
    {
        RealVersion = version;
    }

    public (double Version, bool Bindless) GetInfo()
    {
        return (RealVersion, Bindless);
    }

    bool Has(string extension)
    {
        foreach (var name in Extensions)
        {
            if (SameAscii(name, extension))
                return true;
        }

        return false;
    }

    static bool SameAscii(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            int a = left[i];
            int b = right[i];
            if (a >= 'A' && a <= 'Z')
                a += 32;
            if (b >= 'A' && b <= 'Z')
                b += 32;
            if (a != b)
                return false;
        }

        return true;
    }

    static double Strtod(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        int i = 0;
        while (i < text.Length && text[i] <= ' ')
            i++;
        int start = i;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            i++;
        bool digits = false;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            digits = true;
            i++;
        }

        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            {
                digits = true;
                i++;
            }
        }

        if (digits && i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            int mark = i;
            int e = i + 1;
            if (e < text.Length && (text[e] == '+' || text[e] == '-'))
                e++;
            int exp = e;
            while (e < text.Length && text[e] >= '0' && text[e] <= '9')
                e++;
            if (e > exp)
                i = e;
            else
                i = mark;
        }

        if (!digits)
            return 0;
        return double.Parse(text.Substring(start, i - start), CultureInfo.InvariantCulture);
    }
}
