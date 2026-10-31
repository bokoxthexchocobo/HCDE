namespace HCDE.Playsim;

/// <summary>
/// Legacy uniform blanking, sampler binding removal, and the shader count from
/// <c>hw_shaderpatcher.cpp</c> (1-316). Chunk 1286. The deprecation prints are
/// warning flags. A missing semicolon blanks through the last stored character.
/// </summary>
public sealed class VmDefaultShader1286
{
    public string? Name;
    public string? Texel;
    public string? Light;
    public string? Defines;

    public VmDefaultShader1286(string? name, string? texel, string? light, string? defines)
    {
        Name = name;
        Texel = texel;
        Light = light;
        Defines = defines;
    }
}

public sealed class VmEffectShader1286
{
    public string? Name;
    public string? Vertex;
    public string? Frag1;
    public string? Frag2;
    public string? Frag3;
    public string? Defines;

    public VmEffectShader1286(string? name, string? vertex, string? frag1, string? frag2, string? frag3, string? defines)
    {
        Name = name;
        Vertex = vertex;
        Frag1 = frag1;
        Frag2 = frag2;
        Frag3 = frag3;
        Defines = defines;
    }
}

public sealed class VmShaderLegacy1286
{
    public string Code = "";
    public bool UniformWarning;
    public bool TextureWarning;
}

public static class VmShaderPatch1286
{
    public const int MaxPassTypes = 2;
    public const int MaxEffects = 5;
    public const int ShaderNoTexture = 6;

    public static readonly VmDefaultShader1286[] DefaultShaders =
    {
        new("Default", "shaders/glsl/func_normal.fp", "shaders/glsl/material_normal.fp", ""),
        new("Warp 1", "shaders/glsl/func_warp1.fp", "shaders/glsl/material_normal.fp", ""),
        new("Warp 2", "shaders/glsl/func_warp2.fp", "shaders/glsl/material_normal.fp", ""),
        new("Specular", "shaders/glsl/func_spec.fp", "shaders/glsl/material_specular.fp", "#define SPECULAR\n#define NORMALMAP\n"),
        new("PBR", "shaders/glsl/func_pbr.fp", "shaders/glsl/material_pbr.fp", "#define PBR\n#define NORMALMAP\n"),
        new("Paletted", "shaders/glsl/func_paletted.fp", "shaders/glsl/material_nolight.fp", "#define PALETTE_EMULATION\n"),
        new("No Texture", "shaders/glsl/func_notexture.fp", "shaders/glsl/material_normal.fp", "#define NO_LAYERS\n"),
        new("Basic Fuzz", "shaders/glsl/fuzz_standard.fp", "shaders/glsl/material_normal.fp", ""),
        new("Smooth Fuzz", "shaders/glsl/fuzz_smooth.fp", "shaders/glsl/material_normal.fp", ""),
        new("Swirly Fuzz", "shaders/glsl/fuzz_swirly.fp", "shaders/glsl/material_normal.fp", ""),
        new("Translucent Fuzz", "shaders/glsl/fuzz_smoothtranslucent.fp", "shaders/glsl/material_normal.fp", ""),
        new("Jagged Fuzz", "shaders/glsl/fuzz_jagged.fp", "shaders/glsl/material_normal.fp", ""),
        new("Noise Fuzz", "shaders/glsl/fuzz_noise.fp", "shaders/glsl/material_normal.fp", ""),
        new("Smooth Noise Fuzz", "shaders/glsl/fuzz_smoothnoise.fp", "shaders/glsl/material_normal.fp", ""),
        new("Software Fuzz", "shaders/glsl/fuzz_software.fp", "shaders/glsl/material_normal.fp", ""),
        new(null, null, null, null),
    };

    public static readonly VmEffectShader1286[] EffectShaders =
    {
        new("fogboundary", "shaders/glsl/main.vp", "shaders/glsl/fogboundary.fp", null, null, "#define NO_ALPHATEST\n"),
        new("spheremap", "shaders/glsl/main.vp", "shaders/glsl/main.fp", "shaders/glsl/func_normal.fp", "shaders/glsl/material_normal.fp", "#define SPHEREMAP\n#define NO_ALPHATEST\n"),
        new("burn", "shaders/glsl/main.vp", "shaders/glsl/burn.fp", null, null, "#define SIMPLE\n#define NO_ALPHATEST\n"),
        new("stencil", "shaders/glsl/main.vp", "shaders/glsl/stencil.fp", null, null, "#define SIMPLE\n#define NO_ALPHATEST\n"),
        new("dithertrans", "shaders/glsl/main.vp", "shaders/glsl/main.fp", "shaders/glsl/func_normal.fp", "shaders/glsl/material_normal.fp", "#define NO_ALPHATEST\n#define DITHERTRANS\n"),
    };

    public static int LastDefaultWalk;

    public static VmShaderLegacy1286 RemoveLegacy(string code)
    {
        bool found = Substitute(ref code, "uniform sampler2D tex;", "                      ");
        found = Substitute(ref code, "uniform float timer;", "                    ") || found;
        char[] chars = code.ToCharArray();
        int length = chars.Length;
        int start = 0;
        while (true)
        {
            int match = IndexOf(chars, "uniform", start);
            if (match < 0)
                break;
            bool legacy = false;
            bool keywordStart = match == 0 || IsGlslWs(chars[match - 1]);
            bool keywordEnd = match + 7 == length || (match + 7 < length && IsGlslWs(chars[match + 7]));
            if (keywordStart && keywordEnd)
            {
                int pos = match + 7;
                string type = NextToken(chars, ref pos);
                string identifier = NextToken(chars, ref pos);
                legacy = type == "float" && identifier == "timer";
            }
            if (legacy)
            {
                int semi = IndexOf(chars, ";", match + 7);
                int statement = semi < 0 ? length : semi;
                int last = statement < length ? statement : length - 1;
                for (int i = match; i <= last; i++)
                {
                    if (!IsGlslWs(chars[i]))
                        chars[i] = ' ';
                }
                start = statement;
                found = true;
            }
            else
                start = match + 7;
        }
        bool textureWarning = false;
        while (true)
        {
            int match = IndexOf(chars, "texture2d", start);
            if (match < 0)
                break;
            textureWarning = true;
            bool keywordStart = match == 0 || !IsAlnum(chars[match - 1]);
            bool keywordEnd = match + 9 == length || (match + 9 < length && !IsAlnum(chars[match + 9]));
            if (keywordStart && keywordEnd)
            {
                chars[match + 7] = ' ';
                chars[match + 8] = ' ';
            }
            start = match + 9;
        }
        return new VmShaderLegacy1286
        {
            Code = new string(chars),
            UniformWarning = found,
            TextureWarning = textureWarning,
        };
    }

    public static string RemoveSamplers(string code, List<(string Name, int Binding)> bindings)
    {
        char[] chars = code.ToCharArray();
        int length = chars.Length;
        int start = 0;
        while (true)
        {
            int match = IndexOf(chars, "layout(binding", start);
            if (match < 0)
                break;
            bool sampler = false;
            int endPos = 0;
            bool keywordStart = match == 0 || IsGlslWs(chars[match - 1]);
            bool keywordEnd = match + 14 == length || (match + 14 < length && (IsGlslWs(chars[match + 14]) || chars[match + 14] == '='));
            if (keywordStart && keywordEnd)
            {
                int pos = match + 14;
                while (pos < length && IsGlslWs(chars[pos]))
                    pos++;
                if (pos < length && chars[pos] == '=')
                {
                    pos++;
                    if (ConvertInteger(chars, pos, out int parsed, out long raw))
                    {
                        pos = parsed;
                        while (pos < length && IsGlslWs(chars[pos]))
                            pos++;
                        if (pos < length && chars[pos] == ')')
                        {
                            endPos = pos + 1;
                            int tokenPos = endPos;
                            string uniform = NextToken(chars, ref tokenPos);
                            string type = NextToken(chars, ref tokenPos);
                            string identifier = NextToken(chars, ref tokenPos);
                            sampler = uniform == "uniform" && IsShaderType(type);
                            if (sampler)
                            {
                                bindings.Add((identifier, unchecked((int)raw)));
                                for (int i = match; i < endPos; i++)
                                {
                                    if (!IsGlslWs(chars[i]))
                                        chars[i] = ' ';
                                }
                            }
                        }
                    }
                }
            }
            start = sampler ? endPos : match + 7;
        }
        return new string(chars);
    }

    public static string RemoveLayout(string code, string keyword)
    {
        char[] chars = code.ToCharArray();
        int length = chars.Length;
        int start = 0;
        while (true)
        {
            int match = IndexOf(chars, "layout(location", start);
            if (match < 0)
                break;
            int end = match;
            while (end < length && chars[end] != ')')
                end++;
            if (end >= length)
                break;
            end++;
            while (end < length && IsGlslWs(chars[end]))
                end++;
            bool keywordFound = true;
            int i = 0;
            for (; i < keyword.Length; i++)
            {
                char current = end + i < length ? chars[end + i] : '\0';
                if (current != keyword[i])
                {
                    keywordFound = false;
                    break;
                }
            }
            char after = end + i < length ? chars[end + i] : '\0';
            if (keywordFound && IsGlslWs(after))
            {
                for (int ii = match; ii < end; ii++)
                    chars[ii] = ' ';
            }
            start = end;
        }
        return new string(chars);
    }

    public static int ShaderCount(int userShaders)
    {
        int walked = 0;
        for (; walked < DefaultShaders.Length && DefaultShaders[walked].Name != null; walked++)
        {
        }
        LastDefaultWalk = walked;
        return MaxPassTypes * (DefaultShaders.Length - 1 + userShaders + MaxEffects + ShaderNoTexture);
    }

    static bool Substitute(ref string code, string oldText, string newText)
    {
        if (oldText.Length == 0)
            return false;
        bool found = false;
        var builder = new System.Text.StringBuilder();
        int cursor = 0;
        while (cursor < code.Length)
        {
            int hit = code.IndexOf(oldText, cursor, StringComparison.Ordinal);
            if (hit < 0)
            {
                builder.Append(code, cursor, code.Length - cursor);
                break;
            }
            found = true;
            builder.Append(code, cursor, hit - cursor);
            builder.Append(newText);
            cursor = hit + oldText.Length;
        }
        if (found)
            code = builder.ToString();
        return found;
    }

    static int IndexOf(char[] chars, string needle, int start)
    {
        if (start > 0 && start >= chars.Length)
            return -1;
        if (start < 0)
            start = 0;
        int last = chars.Length - needle.Length;
        for (int i = start; i <= last; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (chars[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return i;
        }
        return -1;
    }

    static string NextToken(char[] chars, ref int pos)
    {
        int length = chars.Length;
        int start = pos;
        while (start < length && IsGlslWs(chars[start]))
            start++;
        int end = start;
        while (end < length && !IsGlslWs(chars[end]) && chars[end] != ';')
            end++;
        pos = end;
        return new string(chars, start, end - start);
    }

    static bool ConvertInteger(char[] chars, int pos, out int end, out long value)
    {
        end = pos;
        value = 0;
        int length = chars.Length;
        int i = pos;
        while (i < length && IsCSpace(chars[i]))
            i++;
        int sign = 1;
        if (i < length && (chars[i] == '+' || chars[i] == '-'))
        {
            if (chars[i] == '-')
                sign = -1;
            i++;
        }
        if (i >= length)
            return false;
        int baseN = 10;
        if (chars[i] == '0')
        {
            if (i + 2 < length && (chars[i + 1] == 'x' || chars[i + 1] == 'X') && HexValue(chars[i + 2]) >= 0)
            {
                baseN = 16;
                i += 2;
            }
            else
                baseN = 8;
        }
        bool any = false;
        long acc = 0;
        while (i < length)
        {
            int dig = baseN == 16 ? HexValue(chars[i]) : DecimalValue(chars[i]);
            if (dig < 0 || dig >= baseN)
                break;
            any = true;
            acc = acc * baseN + dig;
            i++;
        }
        if (!any)
            return false;
        end = i;
        value = sign < 0 ? -acc : acc;
        return true;
    }

    static bool IsShaderType(string name)
    {
        return name == "sampler1D" || name == "sampler2D" || name == "sampler3D" || name == "samplerCube" || name == "sampler2DMS";
    }

    static bool IsGlslWs(char c)
    {
        return c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '\f';
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    static bool IsAlnum(char c)
    {
        int b = c & 255;
        return (b >= '0' && b <= '9') || (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z');
    }

    static int DecimalValue(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        return -1;
    }

    static int HexValue(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'a' && c <= 'f')
            return c - 'a' + 10;
        if (c >= 'A' && c <= 'F')
            return c - 'A' + 10;
        return -1;
    }
}
