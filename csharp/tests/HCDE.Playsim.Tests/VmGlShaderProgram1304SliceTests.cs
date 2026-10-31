namespace HCDE.Playsim.Tests;

public class VmGlShaderProgram1304SliceTests
{
    [Fact]
    public void Patch_BumpsPast330AndAFailedCompileDoesNotAttach()
    {
        Assert.Equal(420, VmGlShaderProgram1304.BumpedMax(4.5f, 330));
        Assert.Equal(420, VmGlShaderProgram1304.ShaderVersion(4.5f, 330));
        Assert.Equal(330, VmGlShaderProgram1304.ShaderVersion(3.3f, 330));
        Assert.Equal(330, VmGlShaderProgram1304.ShaderVersion(3.3f, 450));
        Assert.Equal(450, VmGlShaderProgram1304.BumpedMax(3.3f, 450));
        Assert.Equal(430, VmGlShaderProgram1304.ShaderVersion(4.5f, 430));

        var host = new VmGlShaderProgram1304.Host { GlslVersion = 4.5f, ScreenGlsl = 4.5f, Vendor = "NVIDIA" };
        var prog = new VmGlShaderProgram1304(host);
        string kept = prog.Patch(VmGlShaderProgram1304.Vertex, "layout(binding=2) uniform sampler2D diffuse;\n", "#define X\n", 330);
        Assert.StartsWith("#version 420\n#define X\nprecision highp int;\n", kept);
        Assert.Contains("layout(binding=2) uniform sampler2D diffuse;", kept);
        Assert.Empty(prog.Samplers);

        host.GlslVersion = 3.3f;
        var low = new VmGlShaderProgram1304(host);
        string stripped = low.Patch(
            VmGlShaderProgram1304.Vertex,
            "layout(location=1) out vec4 color;\nlayout(binding=2) uniform sampler2D diffuse;\n",
            null,
            330);
        Assert.StartsWith("#version 330\nprecision highp int;\nprecision highp float;\n#line 1\n", stripped);
        Assert.Contains("out vec4 color;", stripped);
        Assert.DoesNotContain("layout(location=1)", stripped);
        Assert.DoesNotContain("layout(binding=2)", stripped);
        Assert.Equal(("diffuse", 2), Assert.Single(low.Samplers));
        string fragment = low.Patch(VmGlShaderProgram1304.Fragment, "layout(location=1) out vec4 color;\n", null, 450);
        Assert.Contains("layout(location=1) out vec4 color;", fragment);
        Assert.StartsWith("#version 330\n", fragment);

        var missing = Assert.Throws<InvalidOperationException>(() => low.CompileLump(0, "missing.fp", null, 330));
        Assert.Equal("Unable to load 'missing.fp'", missing.Message);

        host.CompileOk = false;
        host.ShaderLog = "boom";
        host.DebugApi = true;
        host.DebugLevel = 2;
        var bad = new VmGlShaderProgram1304(host);
        bad.Names[0] = "screen.vp";
        bad.Sources[0] = "void main(){}";
        var compile = Assert.Throws<InvalidOperationException>(() => bad.CompileShader(VmGlShaderProgram1304.Vertex));
        Assert.Equal("Compile Shader 'screen.vp':\nboom\n", compile.Message);
        Assert.Empty(host.Attaches);
        Assert.Equal(0, bad.ProgramId);
        Assert.Contains("gen-shader:" + VmGlShaderProgram1304.GlVertexShader + ":1", host.Steps);
        Assert.Contains("label:" + VmGlShaderProgram1304.GlShader + ":1:screen.vp", host.Steps);
        Assert.Equal(VmGlShaderProgram1304.GlFragmentShader, 0x8B30);

        host.CompileOk = true;
        host.LinkOk = false;
        host.ProgramLog = "unlinked";
        host.DebugLevel = 0;
        var link = new VmGlShaderProgram1304(host);
        link.Sources[0] = "v";
        link.Sources[1] = "f";
        link.Names[0] = "v";
        link.Names[1] = "f";
        var linked = Assert.Throws<InvalidOperationException>(() => link.Link("present"));
        Assert.Equal("Link Shader 'present':\nunlinked\n", linked.Message);
        Assert.Equal(2, host.Attaches.Count);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("cache-", StringComparison.Ordinal));
        link.Destroy();
        link.Destroy();
        Assert.Equal(1, host.DeletedPrograms);
        Assert.Equal(2, host.DeletedShaders);
    }

    [Fact]
    public void Bind_LoadsAnIntelBinaryAndInitsPresentOnce()
    {
        var host = new VmGlShaderProgram1304.Host
        {
            GlslVersion = 4.5f,
            ScreenGlsl = 4.5f,
            Vendor = "Intel OpenGL",
            ProgramBinary = true,
            BinaryLinkOk = true,
        };
        var prog = new VmGlShaderProgram1304(host);
        prog.Compile(VmGlShaderProgram1304.Vertex, "v", "void main(){}", null, 430);
        prog.Compile(VmGlShaderProgram1304.Fragment, "f", "void main(){}", null, 430);
        Assert.StartsWith("#version 430\nprecision highp int;\n", prog.Sources[0]);
        host.Cache[VmGlShaderProgram1304.CacheKey(prog.Sources[0], prog.Sources[1])] = (7, new byte[] { 1, 2, 3 });
        prog.Link("cached");
        Assert.Equal("cached", prog.LastLinkName);
        Assert.Contains("cache-load", host.Steps);
        Assert.Contains("binary:" + prog.ProgramId + ":7:3", host.Steps);
        Assert.Contains("binary-link:ok", host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("compile:", StringComparison.Ordinal));
        Assert.Empty(host.Attaches);
        Assert.Empty(prog.Samplers);

        host.BinaryLinkOk = false;
        host.SavedBinary = new byte[] { 4, 5 };
        host.BinaryFormat = 3;
        host.Steps.Clear();
        var again = new VmGlShaderProgram1304(host);
        again.Compile(0, "v", "void main(){}", null, 430);
        again.Compile(1, "f", "void other(){}", null, 430);
        host.Cache[VmGlShaderProgram1304.CacheKey(again.Sources[0], again.Sources[1])] = (7, new byte[] { 1, 2, 3 });
        again.Link("fresh");
        Assert.Contains("binary-link:fail", host.Steps);
        Assert.Equal(2, host.Attaches.Count);
        var saved = host.Cache[VmGlShaderProgram1304.CacheKey(again.Sources[0], again.Sources[1])];
        Assert.Equal(3, saved.Format);
        Assert.Equal(new byte[] { 4, 5 }, saved.Data);
        Assert.Contains("cache-save:2", host.Steps);

        host.GlslVersion = 3.3f;
        host.ScreenGlsl = 3.3f;
        host.Uniforms["diffuse"] = 4;
        host.Uniforms["missing"] = -1;
        host.Blocks["Uniforms"] = 8;
        var bound = new VmGlShaderProgram1304(host);
        bound.Compile(VmGlShaderProgram1304.Fragment, "f", "layout(binding=2) uniform sampler2D diffuse;\nlayout(binding=5) uniform sampler2D missing;\n", null, 330);
        bound.Compile(VmGlShaderProgram1304.Vertex, "v", "void main(){}", "", 330);
        host.Steps.Clear();
        bound.Link("samplers");
        Assert.Contains("use:" + bound.ProgramId, host.Steps);
        Assert.Contains("uniform:4:2", host.Steps);
        Assert.Contains("uniform-loc:missing:-1", host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("uniform:-1:", StringComparison.Ordinal) || step.Contains(":5", StringComparison.Ordinal) && step.StartsWith("uniform:", StringComparison.Ordinal));
        Assert.Empty(bound.Samplers);
        bound.SetUniformBufferLocation(2, "Uniforms");
        Assert.Equal((8u, 2), Assert.Single(host.BlockBinds));
        host.Blocks.Clear();
        bound.SetUniformBufferLocation(2, "Uniforms");
        Assert.Contains("block:Uniforms:" + VmGlShaderProgram1304.InvalidBlock, host.Steps);
        Assert.Single(host.BlockBinds);
        host.ScreenGlsl = 4.5f;
        int steps = host.Steps.Count;
        bound.SetUniformBufferLocation(2, "Uniforms");
        Assert.Equal(steps, host.Steps.Count);

        host.ScreenGlsl = 4.5f;
        host.GlslVersion = 4.5f;
        host.Lumps["shaders/pp/screenquad.vp"] = "void main(){}";
        host.Lumps["shaders/pp/present.fp"] = "void present(){}";
        host.Lumps["shaders/pp/shadowmap.fp"] = "void shadow(){}";
        host.Steps.Clear();
        var present = new VmGlPresent1304(host, VmGlPresent1304.Present);
        present.Bind();
        present.Bind();
        Assert.Equal(1, present.BufferInits);
        Assert.Equal(1, host.Buffers);
        Assert.Equal("shaders/pp/present", present.Shader!.LastLinkName);
        Assert.Contains("float InvGamma;", present.Shader.Sources[0]);
        Assert.Contains("layout(std140, binding = 2) uniform Uniforms\n", present.Shader.Sources[0]);
        Assert.Contains("int HdrMode;", present.Shader.Sources[1]);
        Assert.Equal(2, host.Steps.FindAll(step => step.StartsWith("use:", StringComparison.Ordinal)).Count);
        Assert.Contains("ubo:2:false:false", host.Steps);

        host.Steps.Clear();
        var shadow = new VmGlPresent1304(host, VmGlPresent1304.Shadow);
        shadow.Bind();
        Assert.Equal("shaders/glsl/shadowmap", shadow.Shader!.LastLinkName);
        Assert.StartsWith("#version 430\nprecision highp int;\n", shadow.Shader.Sources[0]);
        Assert.DoesNotContain("ShadowmapQuality", shadow.Shader.Sources[0]);
        Assert.Contains("\tfloat ShadowmapQuality;\n", shadow.Shader.Sources[1]);
        Assert.Equal("shaders/pp/shadowmap.fp", shadow.Shader.Names[1]);
        var checker = new VmGlPresent1304(host, VmGlPresent1304.Checker);
        Assert.Throws<InvalidOperationException>(() => checker.Bind());
        Assert.Equal(2, host.Buffers);
    }
}
