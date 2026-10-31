namespace HCDE.Playsim.Tests;

public class VmShaderPatch1286SliceTests
{
    [Fact]
    public void Legacy_SkipsAnEarlyTexture2dAndBlanksTheTabTimer()
    {
        var early = VmShaderPatch1286.RemoveLegacy(
            "EARLY texture2d(keep);\nuniform sampler2D tex;\nuniform float timer;\nxuniform float timer;\nuniform\tfloat\ttimer;\nAFTER texture2d(uv);\nmytexture2d(no);\n");
        Assert.True(early.UniformWarning);
        Assert.True(early.TextureWarning);
        Assert.Equal(
            "EARLY texture2d(keep);\n                      \n                    \nx                    \n       \t     \t      \nAFTER texture  (uv);\nmytexture2d(no);\n",
            early.Code);

        var later = VmShaderPatch1286.RemoveLegacy("uniform float timer; texture2d(mid); uniform float other;");
        Assert.True(later.UniformWarning);
        Assert.False(later.TextureWarning);
        Assert.Equal("                     texture2d(mid); uniform float other;", later.Code);

        var token = VmShaderPatch1286.RemoveLegacy("texture2d(x);_texture2d(z);texture2dfoo");
        Assert.False(token.UniformWarning);
        Assert.True(token.TextureWarning);
        Assert.Equal("texture  (x);_texture  (z);texture2dfoo", token.Code);

        var vertical = VmShaderPatch1286.RemoveLegacy("\vuniform\tfloat\ttimer;");
        Assert.False(vertical.UniformWarning);
        Assert.False(vertical.TextureWarning);
        Assert.Equal("\vuniform\tfloat\ttimer;", vertical.Code);

        var open = VmShaderPatch1286.RemoveLegacy("uniform float timer");
        Assert.True(open.UniformWarning);
        Assert.False(open.TextureWarning);
        Assert.Equal("                   ", open.Code);

        var longer = VmShaderPatch1286.RemoveLegacy("uniform float timerX;");
        Assert.False(longer.UniformWarning);
        Assert.Equal("uniform float timerX;", longer.Code);
    }

    [Fact]
    public void Bindings_ParseTheBaseAndStopAtAnOpenLayout()
    {
        var bindings = new List<(string Name, int Binding)>();
        string samplers = VmShaderPatch1286.RemoveSamplers(
            "layout(binding=2) uniform sampler2D diffuse;\n" +
            "layout(binding = 0x10) uniform samplerCube cube;\n" +
            "layout(binding=010) uniform sampler3D vol;\n" +
            "layout(binding=1) uniform float nope;\n" +
            "layout(binding=4) uniform sampler2DArray skip;\n" +
            "layout(binding=2) uniform Sampler2D casefold;\n" +
            "xlayout(binding=9) uniform sampler2D hidden;\n" +
            "layout(binding=-3) uniform sampler2DMS ms;",
            bindings);
        Assert.Equal(
            "                  uniform sampler2D diffuse;\n" +
            "                       uniform samplerCube cube;\n" +
            "                    uniform sampler3D vol;\n" +
            "layout(binding=1) uniform float nope;\n" +
            "layout(binding=4) uniform sampler2DArray skip;\n" +
            "layout(binding=2) uniform Sampler2D casefold;\n" +
            "xlayout(binding=9) uniform sampler2D hidden;\n" +
            "                   uniform sampler2DMS ms;",
            samplers);
        Assert.Equal(new[] { ("diffuse", 2), ("cube", 16), ("vol", 8), ("ms", -3) }, bindings);

        Assert.Equal(
            "                     in vec2 uv;\nlayout(location=9",
            VmShaderPatch1286.RemoveLayout("layout(location\n= 0) in vec2 uv;\nlayout(location=9", "in"));
        Assert.Equal(
            "                   out vec4 color;",
            VmShaderPatch1286.RemoveLayout("layout(location=1) out vec4 color;", "out"));
        Assert.Equal(
            "layout(location=2) inout vec4 v;",
            VmShaderPatch1286.RemoveLayout("layout(location=2) inout vec4 v;", "in"));
        Assert.Equal(
            "                   inout vec4 v;",
            VmShaderPatch1286.RemoveLayout("layout(location=2) inout vec4 v;", "inout"));

        Assert.Equal(52, VmShaderPatch1286.ShaderCount(0));
        Assert.Equal(15, VmShaderPatch1286.LastDefaultWalk);
        Assert.Equal(58, VmShaderPatch1286.ShaderCount(3));
        Assert.Equal(16, VmShaderPatch1286.DefaultShaders.Length);
        Assert.Null(VmShaderPatch1286.DefaultShaders[15].Name);
        Assert.Equal("Specular", VmShaderPatch1286.DefaultShaders[3].Name);
        Assert.Equal("#define SPECULAR\n#define NORMALMAP\n", VmShaderPatch1286.DefaultShaders[3].Defines);
        Assert.Equal("No Texture", VmShaderPatch1286.DefaultShaders[6].Name);
        Assert.Equal("fogboundary", VmShaderPatch1286.EffectShaders[0].Name);
        Assert.Null(VmShaderPatch1286.EffectShaders[0].Frag2);
        Assert.Equal("#define NO_ALPHATEST\n#define DITHERTRANS\n", VmShaderPatch1286.EffectShaders[4].Defines);
        Assert.Equal(5, VmShaderPatch1286.EffectShaders.Length);
    }
}
