namespace HCDE.Playsim;

/// <summary>
/// Dynamic light rejection and the 16-float light record from <c>hw_dynlightdata.cpp</c> (<b>1–152</b>).
/// Chunk <b>1252</b>. Portal-relative position is a host offset. Degree cosine is the CRT cosine.
/// </summary>
public sealed class VmDynLight1252
{
    public readonly List<string> Log = new();
    public readonly List<float>[] Arrays = { new(), new(), new() };
    public bool Sprites = true;
    public bool Particles = true;

    public bool GetLight(Plane plane, Light light, int group, bool checkSide)
    {
        (double x, double y, double z) = light.Relative(group);
        float radius = light.Radius;
        float dist = MathF.Abs(plane.Dist(x, z, y));
        Log.Add($"light group={group} r={Fmt(radius)} dist={Fmt(dist)} side={(checkSide ? 1 : 0)}");
        if (radius <= 0f)
        {
            Log.Add("light reject radius");
            return false;
        }
        if (dist > radius)
        {
            Log.Add("light reject dist");
            return false;
        }
        if (checkSide && plane.OnSide(x, z, y))
        {
            Log.Add("light reject side");
            return false;
        }
        Add(light, group, false);
        return true;
    }

    public void Add(Light light, int group, bool forceAttenuate)
    {
        (double x, double y, double z) = light.Relative(group);
        float radius = light.Radius;
        float cs;
        int slot;
        if (light.Additive)
        {
            cs = 0.2f;
            slot = 2;
        }
        else
        {
            cs = 1f;
            slot = 0;
        }
        if (light.HasTarget && light.LightMultAlpha)
            cs *= light.Alpha;
        cs *= light.Intensity;
        float r = light.Red / 255f * cs;
        float g = light.Green / 255f * cs;
        float b = light.Blue / 255f * cs;
        if (light.Subtractive)
        {
            double len = Math.Sqrt((double)r * r + (double)g * g + (double)b * b);
            float length = (float)len;
            r = length - r;
            g = length - g;
            b = length - b;
            slot = 1;
        }
        float shadow = light.ShadowMap ? light.ShadowIndex + 1f : 1025f;
        if (light.Attenuated || forceAttenuate)
            shadow = -shadow;
        float lightType = 0f;
        float inner = 0f;
        float outer = 0f;
        float dx = 0f;
        float dy = 0f;
        float dz = 0f;
        if (light.Spot)
        {
            lightType = 1f;
            inner = (float)DegCos(light.InnerDeg);
            outer = (float)DegCos(light.OuterDeg);
            double xz = DegCos(-light.PitchDeg);
            dx = (float)(-DegCos(light.YawDeg) * xz);
            dy = (float)(-DegSin(-light.PitchDeg));
            dz = (float)(-DegSin(light.YawDeg) * xz);
        }
        float[] data =
        {
            (float)x, (float)z, (float)y, radius,
            r, g, b, shadow,
            dx, dy, dz, lightType,
            inner, outer, light.Linearity, light.Soft,
        };
        Arrays[slot].AddRange(data);
        Log.Add($"add slot={slot} {string.Join(',', data.Select(Fmt))}");
    }

    public static double DegCos(double degrees)
    {
        return Math.Cos(degrees * (Math.PI / 180.0));
    }

    public static double DegSin(double degrees)
    {
        return Math.Sin(degrees * (Math.PI / 180.0));
    }

    public static string Fmt(float value)
    {
        if (value == 0f)
            value = 0f;
        return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
    }

    public sealed class Plane
    {
        public float Nx;
        public float Ny;
        public float Nz;
        public float D;

        public float Dist(double x, double y, double z)
        {
            return Nx * (float)x + Ny * (float)y + Nz * (float)z + D;
        }

        public bool OnSide(double x, double y, double z)
        {
            return Dist(x, y, z) < 0f;
        }
    }

    public sealed class Light
    {
        public double X;
        public double Y;
        public double Z;
        public double OffX;
        public double OffY;
        public double OffZ;
        public float Radius = 1f;
        public bool Additive;
        public bool Subtractive;
        public bool Attenuated;
        public bool Spot;
        public bool HasTarget;
        public bool LightMultAlpha;
        public float Alpha = 1f;
        public float Intensity = 1f;
        public float Red = 255f;
        public float Green = 255f;
        public float Blue = 255f;
        public bool ShadowMap;
        public float ShadowIndex;
        public double InnerDeg;
        public double OuterDeg;
        public double PitchDeg;
        public double YawDeg;
        public float Linearity = -1f;
        public float Soft = -1f;

        public (double X, double Y, double Z) Relative(int group)
        {
            if (group == 0)
                return (X, Y, Z);
            return (X + OffX, Y + OffY, Z + OffZ);
        }
    }
}
