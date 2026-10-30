namespace HCDE.Playsim;

/// <summary>
/// Shader compile, the program binary, and the present and shadow binds from
/// <c>gl_shaderprogram.cpp</c> (1-366). Chunk 1304.
/// </summary>
public sealed class VmGlShaderProgram1304
{
    public const int Vertex = 0, Fragment = 1, TypeCount = 2;
    public const int GlVertexShader = 0x8B31, GlFragmentShader = 0x8B30;
    public const int GlShader = 0x82E1, GlProgram = 0x82E2;
    public const int GlTrue = 1, LogCap = 9999, PostprocessBinding = 2;
    public const uint InvalidBlock = 0xFFFFFFFFu;
    public static readonly (string Type, string Name)[] PresentFields =
    {
        ("float", "InvGamma"), ("float", "Contrast"), ("float", "Saturation"),
        ("float", "BlackPoint"), ("float", "WhitePoint"), ("float", "ColorScale"),
        ("int", "GrayFormula"), ("int", "WindowPositionParity"),
        ("vec2", "UVScale"), ("vec2", "UVOffset"), ("int", "HdrMode"),
    };
    public static readonly (string Type, string Name)[] ShadowFields =
    {
        ("float", "ShadowmapQuality"), ("int", "NodesCount"),
        ("float", "Padding0"), ("float", "Padding1"),
    };

    public sealed class Host
    {
        public float GlslVersion = 3.3f;
        public float ScreenGlsl = 3.3f;
        public string Vendor = "NVIDIA";
        public bool DebugApi, ProgramBinary = true, CompileOk = true, LinkOk = true, BinaryLinkOk = true;
        public int DebugLevel, BinaryFormat = 1;
        public byte[] SavedBinary = new byte[] { 9, 8 };
        public string ShaderLog = "", ProgramLog = "";
        public int NextShader = 1, NextProgram = 1, DeletedPrograms, DeletedShaders, Buffers;
        public Dictionary<string, string> Lumps = new();
        public Dictionary<string, (int Format, byte[] Data)> Cache = new();
        public Dictionary<string, int> Uniforms = new();
        public Dictionary<string, uint> Blocks = new();
        public List<string> Steps = new();
        public List<(int Program, int Shader)> Attaches = new();
        public List<(int Index, int Value)> UniformWrites = new();
        public List<(uint Block, int Binding)> BlockBinds = new();
        bool _cacheReady, _cacheOn;
        public bool CacheActive()
        {
            if (!_cacheReady)
            {
                _cacheReady = true;
                _cacheOn = Vendor.Contains("Intel", StringComparison.Ordinal);
            }
            return _cacheOn;
        }
    }

    public readonly Host Device;
    public int ProgramId;
    public readonly int[] Shaders = new int[TypeCount];
    public readonly string[] Names = { "", "" };
    public readonly string[] Sources = { "", "" };
    public readonly List<(string Name, int Binding)> Samplers = new();
    public string? LastLinkName;

    public VmGlShaderProgram1304(Host device) => Device = device;

    public static string CacheKey(string vertex, string fragment) => vertex + "\0" + fragment;

    public static int BumpedMax(float glsl, int max) => max < 420 && glsl >= 4.2f ? 420 : max;

    public static int ShaderVersion(float glsl, int max)
    {
        max = BumpedMax(glsl, max);
        int tens = (int)Math.Round(glsl * 10f, MidpointRounding.AwayFromZero);
        return Math.Min(tens * 10, max);
    }

    public static string Declaration(string name, (string Type, string Name)[] fields, int binding, float screenGlsl)
    {
        string layout = binding == -1 ? "push_constant"
            : screenGlsl < 4.20f ? "std140"
            : "std140, binding = " + binding;
        string body = "";
        foreach ((string type, string field) in fields)
            body += "\t" + type + " " + field + ";\n";
        return "layout(" + layout + ") uniform " + name + "\n{\n" + body + "};\n";
    }

    public void CompileLump(int type, string lumpName, string? defines, int maxVersion)
    {
        if (!Device.Lumps.TryGetValue(lumpName, out string? code))
            throw new InvalidOperationException("Unable to load '" + lumpName + "'");
        Compile(type, lumpName, code, defines, maxVersion);
    }

    public void Compile(int type, string name, string code, string? defines, int maxVersion)
    {
        Names[type] = name;
        Sources[type] = Patch(type, code, defines, maxVersion);
    }

    public string Patch(int type, string code, string? defines, int maxVersion)
    {
        int max = BumpedMax(Device.GlslVersion, maxVersion);
        string text = "#version " + ShaderVersion(Device.GlslVersion, maxVersion) + "\n";
        if (defines != null) text += defines;
        text += "precision highp int;\nprecision highp float;\n#line 1\n";
        text += VmShaderPatch1286.RemoveLayout(code, type == Vertex ? "out" : "in");
        if (max < 420) text = VmShaderPatch1286.RemoveSamplers(text, Samplers);
        return text;
    }

    public void CreateShader(int type)
    {
        int glType = type == Fragment ? GlFragmentShader : GlVertexShader;
        Shaders[type] = Device.NextShader++;
        Device.Steps.Add("gen-shader:" + glType + ":" + Shaders[type]);
    }

    public void CompileShader(int type)
    {
        CreateShader(type);
        int handle = Shaders[type];
        Label(GlShader, handle, Names[type]);
        Device.Steps.Add("source:" + handle + ":" + Sources[type].Length);
        Device.Steps.Add("compile:" + handle);
        if (!Device.CompileOk)
            throw new InvalidOperationException("Compile Shader '" + Names[type] + "':\n" + Clip(Device.ShaderLog) + "\n");
        if (ProgramId == 0)
        {
            ProgramId = Device.NextProgram++;
            Device.Steps.Add("gen-prog:" + ProgramId);
        }
        Device.Attaches.Add((ProgramId, handle));
        Device.Steps.Add("attach:" + ProgramId + ":" + handle);
    }

    public void Link(string name)
    {
        LastLinkName = name;
        Label(GlProgram, ProgramId, name);
        byte[] binary = Array.Empty<byte>();
        int format = 0;
        if (Device.CacheActive())
        {
            Device.Steps.Add("cache-load");
            if (Device.Cache.TryGetValue(CacheKey(Sources[Vertex], Sources[Fragment]), out var hit))
            {
                format = hit.Format;
                binary = hit.Data;
            }
        }
        bool loaded = false;
        if (binary.Length > 0 && Device.ProgramBinary)
        {
            EnsureProgram();
            Device.Steps.Add("binary:" + ProgramId + ":" + format + ":" + binary.Length);
            loaded = Device.BinaryLinkOk;
            Device.Steps.Add(loaded ? "binary-link:ok" : "binary-link:fail");
        }
        if (!loaded)
        {
            CompileShader(Vertex);
            CompileShader(Fragment);
            Device.Steps.Add("link:" + ProgramId);
            if (!Device.LinkOk)
                throw new InvalidOperationException("Link Shader '" + name + "':\n" + Clip(Device.ProgramLog) + "\n");
            if (Device.ProgramBinary && Device.CacheActive())
            {
                Device.Steps.Add("binary-get:" + Device.SavedBinary.Length);
                Device.Cache[CacheKey(Sources[Vertex], Sources[Fragment])] = (Device.BinaryFormat, Device.SavedBinary);
                Device.Steps.Add("cache-save:" + Device.SavedBinary.Length);
            }
        }
        if (Device.ScreenGlsl < 4.20f)
        {
            Device.Steps.Add("use:" + ProgramId);
            foreach ((string sampler, int unit) in Samplers)
            {
                int index = Device.Uniforms.TryGetValue(sampler, out int found) ? found : -1;
                Device.Steps.Add("uniform-loc:" + sampler + ":" + index);
                if (index >= 0)
                {
                    Device.UniformWrites.Add((index, unit));
                    Device.Steps.Add("uniform:" + index + ":" + unit);
                }
            }
        }
        Samplers.Clear();
    }

    public void SetUniformBufferLocation(int index, string name)
    {
        if (Device.ScreenGlsl >= 4.20f) return;
        uint block = Device.Blocks.TryGetValue(name, out uint found) ? found : InvalidBlock;
        Device.Steps.Add("block:" + name + ":" + block);
        if (block != InvalidBlock)
        {
            Device.BlockBinds.Add((block, index));
            Device.Steps.Add("block-bind:" + block + ":" + index);
        }
    }

    public void Bind() => Device.Steps.Add("use:" + ProgramId);

    public void Destroy()
    {
        if (ProgramId != 0)
        {
            Device.DeletedPrograms++;
            Device.Steps.Add("del-prog:" + ProgramId);
            ProgramId = 0;
        }
        for (int i = 0; i < TypeCount; i++)
        {
            if (Shaders[i] == 0) continue;
            Device.DeletedShaders++;
            Device.Steps.Add("del-shader:" + Shaders[i]);
            Shaders[i] = 0;
        }
    }

    void EnsureProgram()
    {
        if (ProgramId != 0) return;
        ProgramId = Device.NextProgram++;
        Device.Steps.Add("gen-prog:" + ProgramId);
    }

    void Label(int type, int id, string? name)
    {
        if (!Device.DebugApi || Device.DebugLevel == 0) return;
        Device.Steps.Add("label:" + type + ":" + id + ":" + name);
    }

    static string Clip(string log) => log.Length > LogCap ? log[..LogCap] : log;
}

/// <summary>Present and shadow-map shader binds from the same file.</summary>
public sealed class VmGlPresent1304
{
    public const int Present = 0, Checker = 1, Column = 2, Row = 3, Shadow = 4;
    readonly VmGlShaderProgram1304.Host _host;
    readonly int _kind;
    public VmGlShaderProgram1304? Shader;
    public int BufferInits;

    public VmGlPresent1304(VmGlShaderProgram1304.Host host, int kind)
    {
        _host = host;
        _kind = kind;
    }

    public void Bind()
    {
        if (Shader == null)
        {
            bool shadow = _kind == Shadow;
            var fields = shadow ? VmGlShaderProgram1304.ShadowFields : VmGlShaderProgram1304.PresentFields;
            string prolog = VmGlShaderProgram1304.Declaration("Uniforms", fields, VmGlShaderProgram1304.PostprocessBinding, _host.ScreenGlsl);
            string fragment = _kind switch
            {
                Checker => "shaders/pp/present_checker3d.fp",
                Column => "shaders/pp/present_column3d.fp",
                Row => "shaders/pp/present_row3d.fp",
                Shadow => "shaders/pp/shadowmap.fp",
                _ => "shaders/pp/present.fp",
            };
            string program = _kind switch
            {
                Checker => "shaders/pp/presentChecker3d",
                Column => "shaders/pp/presentColumn3d",
                Row => "shaders/pp/presentRow3d",
                Shadow => "shaders/glsl/shadowmap",
                _ => "shaders/pp/present",
            };
            int max = shadow ? 430 : 330;
            Shader = new VmGlShaderProgram1304(_host);
            Shader.CompileLump(VmGlShaderProgram1304.Vertex, "shaders/pp/screenquad.vp", shadow ? "" : prolog, max);
            Shader.CompileLump(VmGlShaderProgram1304.Fragment, fragment, shadow ? prolog : prolog, max);
            Shader.Link(program);
            Shader.SetUniformBufferLocation(VmGlShaderProgram1304.PostprocessBinding, "Uniforms");
            if (BufferInits == 0)
            {
                BufferInits = 1;
                _host.Buffers++;
                _host.Steps.Add("ubo:" + VmGlShaderProgram1304.PostprocessBinding + ":false:false");
            }
        }
        Shader.Bind();
    }
}
