using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Fog table, light level, light color, and the fog-boundary check from
/// <c>hw_lighting.cpp</c> (1-271). Chunk 1277. Sky flat numbers are host inputs.
/// A non-positive light level returns before the final clamp. Software lighting
/// with a blend rewrites the fog mode to Doom before the density cases.
/// </summary>
public sealed class VmLightSector1277
{
    public uint Fade;
    public int LightLevel;
    public int Ceiling;
}

public sealed class VmLighting1277
{
    public const int LinearStandard = 0;
    public const int DoomBright = 1;
    public const int Doom = 2;
    public const int DoomDark = 3;
    public const int DoomLegacy = 4;
    public const int Build = 5;
    public const int ZDoomSoftware = 8;
    public const int DoomSoftware = 16;
    public const int NoLightFade = 0x8;

    public int DistFog { get; private set; } = 70;
    public int WeaponLight = 8;
    public bool EnhancedNightVision;
    public int FogDensity;
    public int OutsideFogDensity;
    public uint OutsideFog;
    public int Flags3;
    public int SkyFlat;
    public int LastFogMode;
    public readonly float[] Table0 = new float[256];
    public readonly float[] Table1 = new float[256];

    public VmLighting1277()
    {
        Rebuild(DistFog);
    }

    public void SetDistFog(int value)
    {
        DistFog = value;
        Rebuild(value);
    }

    public void Rebuild(int distFog)
    {
        int half = distFog >> 1;
        for (int i = 0; i < 256; i++)
        {
            if (i < 164)
                Table0[i] = half + distFog * (164 - i) / 164;
            else if (i < 230)
                Table0[i] = half - half * (i - 164) / (230 - 164);
            else
                Table0[i] = 0f;
            if (i < 128)
                Table1[i] = 6f + (half + distFog * (128 - i) / 48);
            else if (i < 216)
                Table1[i] = (216f - i) / (216f - 128f) * distFog / 10f;
            else
                Table1[i] = 0f;
        }
    }

    public static bool IsSoftware(int mode)
    {
        return mode == ZDoomSoftware || mode == DoomSoftware || mode == Build;
    }

    public static bool IsDoomSoftware(int mode)
    {
        return mode == ZDoomSoftware || mode == DoomSoftware;
    }

    public static bool IsDark(int mode)
    {
        return mode == Doom || mode == DoomDark;
    }

    public int CalcLightLevel(int mode, int lightLevel, int relLight, bool weapon, int blend, bool weaponPure)
    {
        if (lightLevel <= 0)
            return 0;
        bool dark = IsDark(mode) || (IsSoftware(mode) && blend > 0);
        int light;
        if ((dark && lightLevel < 192 && !weapon) || (weapon && weaponPure))
        {
            if (lightLevel > 100)
            {
                float curve = 192f - (192 - lightLevel) * 1.87f;
                light = RoundHalfEven(curve);
                if (light + relLight < 20)
                    light = 20 + (light + relLight - 20) / 5;
                else
                    light += relLight;
            }
            else
            {
                light = (lightLevel + relLight) / 5;
            }
        }
        else
        {
            light = lightLevel + relLight;
        }
        return Clamp(light, 1, 255);
    }

    public uint CalcLightColor(int mode, int light, uint color, int blend)
    {
        if (blend == 0)
        {
            if (IsSoftware(mode))
                return color;
            int r = Red(color) * light / 255;
            int g = Green(color) * light / 255;
            int b = Blue(color) * light / 255;
            return Pack(255, r, g, b);
        }
        int mix = light * (255 - blend);
        int mr = (mix + Red(color) * blend) / 255;
        int mg = (mix + Green(color) * blend) / 255;
        int mb = (mix + Blue(color) * blend) / 255;
        return Pack(255, mr, mg, mb);
    }

    public float GetFogDensity(int mode, int lightLevel, uint fogColor, int sectorFog, int blend)
    {
        int use = mode;
        if (IsSoftware(mode) && blend > 0)
            use = Doom;
        LastFogMode = use;
        float density;
        if (use == DoomLegacy)
            density = FogDensity != 0 ? FogDensity : 18f;
        else if (sectorFog != 0)
            density = sectorFog;
        else if ((fogColor & 0xffffff) == 0)
        {
            bool fade = (Flags3 & NoLightFade) != 0;
            if ((!IsDoomSoftware(use) || blend > 0) && !fade)
                density = TableRow(use)[ClampLight(lightLevel)];
            else
                density = 0f;
        }
        else if (OutsideFogDensity != 0 && Alpha(OutsideFog) != 0xff && (fogColor & 0xffffff) == (OutsideFog & 0xffffff))
            density = OutsideFogDensity;
        else if (FogDensity != 0)
            density = FogDensity;
        else if (lightLevel < 248)
            density = Clamp(255 - lightLevel, 30, 255);
        else
            density = 0f;
        return density;
    }

    public bool CheckFog(VmLightSector1277 front, VmLightSector1277 back, int mode)
    {
        if (ReferenceEquals(front, back))
            return false;
        uint fog = front.Fade;
        if ((fog & 0xffffff) == 0)
            return false;
        else if (Alpha(fog) != 0)
        {
        }
        else if (OutsideMatches(fog))
        {
        }
        else if (FogDensity != 0 || mode == DoomLegacy)
        {
        }
        else if (front.LightLevel >= 248)
            return false;
        fog = back.Fade;
        if ((fog & 0xffffff) == 0)
        {
        }
        else if (OutsideMatches(fog))
            return false;
        else if (FogDensity != 0 || mode == DoomLegacy)
            return false;
        else if (back.LightLevel < 248)
            return false;
        return front.Ceiling != SkyFlat || back.Ceiling != SkyFlat;
    }

    bool OutsideMatches(uint fog)
    {
        return OutsideFogDensity != 0 && Alpha(OutsideFog) != 0xff && (fog & 0xffffff) == (OutsideFog & 0xffffff);
    }

    float[] TableRow(int mode)
    {
        return mode != LinearStandard ? Table1 : Table0;
    }

    public static int ClampLight(int lightLevel)
    {
        return Clamp(lightLevel, 0, 255);
    }

    public static int Clamp(int value, int low, int high)
    {
        if (value < low)
            return low;
        if (value > high)
            return high;
        return value;
    }

    public static int RoundHalfEven(float value)
    {
        return (int)Math.Round((double)value, MidpointRounding.ToEven);
    }

    public static int Alpha(uint color) => (int)((color >> 24) & 0xff);
    public static int Red(uint color) => (int)((color >> 16) & 0xff);
    public static int Green(uint color) => (int)((color >> 8) & 0xff);
    public static int Blue(uint color) => (int)(color & 0xff);

    public static uint Pack(int alpha, int red, int green, int blue)
    {
        return (uint)((byte)alpha << 24 | (byte)red << 16 | (byte)green << 8 | (byte)blue);
    }

    public static string Num(float value) => value.ToString("G9", CultureInfo.InvariantCulture);
}
