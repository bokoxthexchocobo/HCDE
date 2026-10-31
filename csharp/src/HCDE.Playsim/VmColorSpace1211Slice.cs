namespace HCDE.Playsim;

/// <summary>
/// sRGB, OKLab, and OKLCH conversion from <c>colorspace.cpp</c> (<b>1–218</b>).
/// Chunk <b>1211</b>. Channels are <c>float</c>. Intermediates follow the native promotions to <c>double</c>.
/// </summary>
public sealed class VmColorSpace1211Slice
{
    public const int SpaceSrgb = 0;
    public const int SpaceOklab = 1;
    public const int SpaceOklch = 2;
    public const float Pi = (float)3.14159265358979323846;

    public static readonly float QuietNan = BitConverter.Int32BitsToSingle(0x7FC00000);

    public float V1;
    public float V2;
    public float V3;

    public int Type { get; private set; }

    VmColorSpace1211Slice(int type, float v1, float v2, float v3)
    {
        Type = type;
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public VmColorSpace1211Slice Clone()
    {
        return new VmColorSpace1211Slice(Type, V1, V2, V3);
    }

    public static VmColorSpace1211Slice Rgb(float r, float g, float b)
    {
        return new VmColorSpace1211Slice(SpaceSrgb, r, g, b);
    }

    public static VmColorSpace1211Slice Rgb(uint packed)
    {
        float r = (float)(((packed >> 16) & 0xff) / 255.0);
        float g = (float)(((packed >> 8) & 0xff) / 255.0);
        float b = (float)((packed & 0xff) / 255.0);
        return Rgb(r, g, b);
    }

    public static VmColorSpace1211Slice Rgb(VmColorSpace1211Slice color)
    {
        var copy = color.Clone();
        copy.ToRgb();
        return copy;
    }

    public uint Rgb24()
    {
        var copy = Clone();
        copy.ToRgb();
        int r = ByteChannel(copy.V1 * 255);
        int g = ByteChannel(copy.V2 * 255);
        int b = ByteChannel(copy.V3 * 255);
        return (uint)((r << 16) | (g << 8) | b);
    }

    public static VmColorSpace1211Slice Oklab(float l, float a, float b)
    {
        return new VmColorSpace1211Slice(SpaceOklab, l, a, b);
    }

    public static VmColorSpace1211Slice Oklab(VmColorSpace1211Slice color)
    {
        var copy = color.Clone();
        copy.ToOklab();
        return copy;
    }

    public static VmColorSpace1211Slice Oklch(float l, float c, float h)
    {
        return new VmColorSpace1211Slice(SpaceOklch, l, c, h);
    }

    public static VmColorSpace1211Slice Oklch(VmColorSpace1211Slice color)
    {
        var copy = color.Clone();
        copy.ToOklch();
        return copy;
    }

    public void ToRgb()
    {
        switch (Type)
        {
            case SpaceOklab:
                OklabToRgb();
                break;
            case SpaceOklch:
                OklchToRgb();
                break;
        }
    }

    public void ToOklch()
    {
        switch (Type)
        {
            case SpaceOklab:
                OklabToOklch();
                break;
            case SpaceSrgb:
                RgbToOklch();
                break;
        }
    }

    public void ToOklab()
    {
        switch (Type)
        {
            case SpaceOklch:
                OklchToOklab();
                break;
            case SpaceSrgb:
                RgbToOklab();
                break;
        }
    }

    public void RgbToOklab()
    {
        Require(SpaceSrgb, SpaceOklab);
        float r = Linear(V1);
        float g = Linear(V2);
        float b = Linear(V3);
        double l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        double m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        double s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
        V1 = (float)(l * 0.2104542553 + m * 0.7936177850 + s * -0.0040720468);
        V2 = (float)(l * 1.9779984951 + m * -2.4285922050 + s * 0.4505937099);
        V3 = (float)(l * 0.0259040371 + m * 0.7827717662 + s * -0.8086757660);
    }

    public void RgbToOklch()
    {
        RgbToOklab();
        OklabToOklch();
    }

    public void OklchToOklab()
    {
        Require(SpaceOklch, SpaceOklab);
        float c = V2;
        float h = V3;
        V2 = float.IsNaN(h) ? 0f : (float)(c * Math.Cos((double)(h * Pi / 180f)));
        V3 = float.IsNaN(h) ? 0f : (float)(c * Math.Sin((double)(h * Pi / 180f)));
    }

    public void OklchToRgb()
    {
        OklchToOklab();
        OklabToRgb();
    }

    public void OklabToRgb()
    {
        Require(SpaceOklab, SpaceSrgb);
        float l = V1;
        float a = V2;
        float b = V3;
        double ll = Math.Pow(l + a * 0.3963377774 + b * 0.2158037573, 3);
        double m = Math.Pow(l + a * -0.1055613458 + b * -0.0638541728, 3);
        double s = Math.Pow(l + a * -0.0894841775 + b * -1.2914855480, 3);
        V1 = Gamma((float)(ll * 4.0767416621 + m * -3.3077115913 + s * 0.2309699292));
        V2 = Gamma((float)(ll * -1.2684380046 + m * 2.6097574011 + s * -0.3413193965));
        V3 = Gamma((float)(ll * -0.0041960863 + m * -0.7034186147 + s * 1.7076147010));
    }

    public void OklabToOklch()
    {
        Require(SpaceOklab, SpaceOklch);
        float a = V2;
        float b = V3;
        V2 = (float)Math.Sqrt((double)(a * a + b * b));
        if (V2 < 0.0001f)
        {
            V3 = QuietNan;
        }
        else
        {
            double raw = Math.Atan2(b, a);
            V3 = (float)(raw * 180.0 / (double)Pi);
            V3 = V3 >= 0f ? V3 : V3 + 360f;
        }
    }

    public static VmColorSpace1211Slice Mix(VmColorSpace1211Slice a, VmColorSpace1211Slice b, float t)
    {
        var left = a.Clone();
        var right = b.Clone();
        if (t <= 0f)
            return left;
        if (t >= 1f)
            return right;
        int type = left.Type;
        left.ToOklch();
        right.ToOklch();
        left.V1 = Lerp(left.V1, right.V1, t);
        left.V2 = Lerp(left.V2, right.V2, t);
        left.V3 = Alerp(left.V3, right.V3, t);
        switch (type)
        {
            case SpaceSrgb:
                left.OklchToRgb();
                break;
            case SpaceOklab:
                left.OklchToOklab();
                break;
        }

        return left;
    }

    void Require(int from, int to)
    {
        if (Type != from)
            throw new InvalidOperationException("assert(color.type == from)");
        Type = to;
    }

    static float Linear(float c)
    {
        double cd = c;
        double v = cd >= 0.04045 ? Math.Pow((cd + 0.055) / 1.055, 2.4) : cd / 12.92;
        return (float)v;
    }

    static float Gamma(float c)
    {
        double cd = c;
        double v = cd >= 0.0031308 ? 1.055 * Math.Pow(cd, 1.0 / 2.4) - 0.055 : 12.92 * cd;
        if (v < 0.0)
            v = 0.0;
        else if (1.0 < v)
            v = 1.0;
        return (float)v;
    }

    static float Alerp(float a, float b, float t)
    {
        if (float.IsNaN(a))
            return float.IsNaN(b) ? 0f : b;
        if (float.IsNaN(b))
            return float.IsNaN(a) ? 0f : a;
        float delta = Fmod(b - a, 360f);
        if (delta > 180f)
            delta -= 360f;
        if (delta < -180f)
            delta += 360f;
        return Fmod(a + delta * t + 360f, 360f);
    }

    static float Lerp(float a, float b, float t)
    {
        if ((a <= 0f && b >= 0f) || (a >= 0f && b <= 0f))
            return t * b + (1f - t) * a;
        if (t == 1f)
            return b;
        float x = a + t * (b - a);
        return (t > 1f) == (b > a) ? (b < x ? x : b) : (b > x ? x : b);
    }

    static float Fmod(float x, float y)
    {
        double xd = x;
        double yd = y;
        return (float)(xd - yd * Math.Truncate(xd / yd));
    }

    static int ByteChannel(float x)
    {
        return (byte)x;
    }
}
