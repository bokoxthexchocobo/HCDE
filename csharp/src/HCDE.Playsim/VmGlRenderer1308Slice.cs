namespace HCDE.Playsim;

/// <summary>
/// Renderer setup, the offscreen framebuffer, and the save-picture buffers from
/// <c>gl_renderer.cpp</c> (1-172). Chunk 1308.
/// </summary>
public sealed class VmGlRenderer1308
{
    public const int GlVertexArray = 0x8074;
    public const int GlFramebuffer = 0x8D40;
    public const int GlFramebufferBinding = 0x8CA6;
    public const int SavePicWidth = 216;
    public const int SavePicHeight = 162;

    readonly Host _host;

    public int FbId { get; private set; }
    public int VaoId { get; private set; }
    public int OldFbId { get; private set; }
    public Buf? Buffers { get; private set; }
    public Buf? ScreenBuffers { get; private set; }
    public Buf? SaveBuffers { get; private set; }
    public bool Present { get; private set; }
    public bool Checker { get; private set; }
    public bool Column { get; private set; }
    public bool Row { get; private set; }
    public bool Shadow { get; private set; }
    public bool ShaderManager { get; private set; }
    public bool SamplerManager { get; private set; }

    public VmGlRenderer1308(Host host)
    {
        _host = host;
    }

    public void Initialize(int width, int height)
    {
        ScreenBuffers = new Buf();
        SaveBuffers = new Buf();
        Buffers = ScreenBuffers;
        Present = true;
        Checker = true;
        Column = true;
        Row = true;
        Shadow = true;
        VaoId = _host.GenVertexArrays();
        _host.BindVertexArray(VaoId);
        _host.Label(GlVertexArray, VaoId, "FGLRenderer.mVAOID");
        FbId = 0;
        OldFbId = 0;
        ShaderManager = true;
        SamplerManager = true;
        _ = (width, height);
    }

    public void Destroy()
    {
        _host.FlushModels();
        _host.FlushTextures();
        if (ShaderManager)
            _host.Delete("shader-manager");
        if (SamplerManager)
            _host.Delete("sampler-manager");
        if (FbId != 0)
            _host.DeleteFramebuffers(FbId);
        if (VaoId != 0)
        {
            _host.BindVertexArray(0);
            _host.DeleteVertexArrays(VaoId);
        }
        if (Buffers != null)
            _host.Delete(Buffers == ScreenBuffers ? "screen-buffers" : "buffers");
        if (SaveBuffers != null)
            _host.Delete("save-buffers");
        if (Present)
            _host.Delete("present");
        if (Checker)
            _host.Delete("checker");
        if (Column)
            _host.Delete("column");
        if (Row)
            _host.Delete("row");
        if (Shadow)
            _host.Delete("shadow");
    }

    public bool StartOffscreen()
    {
        bool firstBind = FbId == 0;
        if (FbId == 0)
            FbId = _host.GenFramebuffers();
        OldFbId = _host.GetInteger(GlFramebufferBinding);
        _host.BindFramebuffer(GlFramebuffer, FbId);
        if (firstBind)
            _host.Label(GlFramebuffer, FbId, "OffscreenFB");
        return true;
    }

    public void EndOffscreen()
    {
        _host.BindFramebuffer(GlFramebuffer, OldFbId);
    }

    public void BindToFrameBuffer(Tex tex)
    {
        Layer layer = tex.Hardware ?? throw new InvalidOperationException("hardware texture");
        layer.BindOrCreate(tex, 0, 0, 0, 0);
        Layer.Unbind(_host, 0);
        _host.ClearLastMaterial();
        layer.BindToFrameBuffer(tex.Width, tex.Height);
    }

    public void BeginFrame()
    {
        Buf screen = ScreenBuffers ?? throw new InvalidOperationException("screen buffers");
        Buf save = SaveBuffers ?? throw new InvalidOperationException("save buffers");
        screen.Setup(_host.ScreenWidth, _host.ScreenHeight, _host.SceneWidth, _host.SceneHeight);
        save.Setup(SavePicWidth, SavePicHeight, SavePicWidth, SavePicHeight);
    }

    public sealed class Buf
    {
        public List<string> Setups { get; } = new();

        public void Setup(int screenWidth, int screenHeight, int sceneWidth, int sceneHeight)
        {
            Setups.Add($"{screenWidth}x{screenHeight}:{sceneWidth}x{sceneHeight}");
        }
    }

    public sealed class Tex
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public Layer? Hardware { get; init; }
    }

    public sealed class Layer
    {
        public List<string> Calls { get; } = new();

        public void BindOrCreate(Tex tex, int clamp, int translation, int flags, int material)
        {
            Calls.Add($"bind-or-create:{tex.Width}x{tex.Height}:{clamp}:{translation}:{flags}:{material}");
        }

        public void BindToFrameBuffer(int width, int height)
        {
            Calls.Add($"bind-fb:{width}x{height}");
        }

        public static void Unbind(Host host, int unit)
        {
            host.Steps.Add($"unbind:{unit}");
        }
    }

    public sealed class Host
    {
        public int NextVao { get; set; } = 1;
        public int NextFramebuffer { get; set; } = 1;
        public int FramebufferBinding { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int SceneWidth { get; set; }
        public int SceneHeight { get; set; }
        public List<string> Steps { get; } = new();

        public int GenVertexArrays()
        {
            Steps.Add("gen-vao");
            return NextVao;
        }

        public void BindVertexArray(int id)
        {
            Steps.Add($"bind-vao:{id}");
        }

        public void Label(int kind, int id, string name)
        {
            Steps.Add($"label:{kind}:{id}:{name}");
        }

        public int GenFramebuffers()
        {
            Steps.Add("gen-fb");
            return NextFramebuffer;
        }

        public int GetInteger(int pname)
        {
            Steps.Add($"get:{pname}");
            return FramebufferBinding;
        }

        public void BindFramebuffer(int target, int id)
        {
            Steps.Add($"bind-fb:{target}:{id}");
        }

        public void DeleteFramebuffers(int id)
        {
            Steps.Add($"del-fb:{id}");
        }

        public void DeleteVertexArrays(int id)
        {
            Steps.Add($"del-vao:{id}");
        }

        public void FlushModels()
        {
            Steps.Add("flush-models");
        }

        public void FlushTextures()
        {
            Steps.Add("flush-textures");
        }

        public void Delete(string kind)
        {
            Steps.Add($"del:{kind}");
        }

        public void ClearLastMaterial()
        {
            Steps.Add("clear-material");
        }
    }
}
