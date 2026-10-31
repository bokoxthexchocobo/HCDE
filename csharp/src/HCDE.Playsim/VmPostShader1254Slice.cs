namespace HCDE.Playsim;

/// <summary>
/// Post-process shader enable and uniform writes from <c>hw_postprocessshader.cpp</c> (<b>1–165</b>).
/// Chunk <b>1254</b>. The shader list is host storage. Name compare is case-sensitive, and a missing
/// uniform is created with an undefined type.
/// </summary>
public sealed class VmPostShader1254
{
    public readonly List<Shader> Shaders = new();
    public readonly List<string> Log = new();

    public int SetEnabled(string name, bool value)
    {
        int n = 0;
        foreach (Shader shader in Shaders)
        {
            if (shader.Name == name)
            {
                shader.Enabled = value;
                n++;
            }
        }
        Log.Add($"enabled {name} {(value ? 1 : 0)} n={n}");
        return 0;
    }

    public int SetUniform1f(string name, string uniform, double value)
    {
        return Write(name, uniform, "1f", value, 0.0, 0.0, 1.0);
    }

    public int SetUniform2f(string name, string uniform, double x, double y)
    {
        return Write(name, uniform, "2f", x, y, 0.0, 1.0);
    }

    public int SetUniform3f(string name, string uniform, double x, double y, double z)
    {
        return Write(name, uniform, "3f", x, y, z, 1.0);
    }

    public int SetUniform4f(string name, string uniform, double x, double y, double z, double w)
    {
        return Write(name, uniform, "4f", x, y, z, w);
    }

    public int SetUniform1i(string name, string uniform, int value)
    {
        return Write(name, uniform, "1i", (double)value, 0.0, 0.0, 1.0);
    }

    int Write(string name, string uniform, string kind, double x, double y, double z, double w)
    {
        int n = 0;
        foreach (Shader shader in Shaders)
        {
            if (shader.Name != name)
                continue;
            Uniform slot = Slot(shader, uniform);
            slot.Values[0] = x;
            slot.Values[1] = y;
            slot.Values[2] = z;
            slot.Values[3] = w;
            n++;
        }
        Log.Add($"u{kind} {name} {uniform} n={n} {Fmt(x)} {Fmt(y)} {Fmt(z)} {Fmt(w)}");
        return 0;
    }

    static Uniform Slot(Shader shader, string uniform)
    {
        if (!shader.Uniforms.TryGetValue(uniform, out Uniform? slot))
        {
            slot = new Uniform();
            shader.Uniforms[uniform] = slot;
        }
        return slot;
    }

    public static string Fmt(double value)
    {
        return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
    }

    public sealed class Shader
    {
        public string Name = "";
        public bool Enabled;
        public Dictionary<string, Uniform> Uniforms = new();
    }

    public sealed class Uniform
    {
        public int Type;
        public double[] Values = { 0.0, 0.0, 0.0, 0.0 };
    }
}
