namespace HCDE.Gamedata;

public enum VmMapInfoLightMode
{
    NotSet = -1,
    Default = 0,
    Doom = 1,
    DoomBright = 2,
    Heretic = 3,
    Hexen = 4
}

public enum VmMapInfoLightBlendMode
{
    Default,
    ClampColor,
    NoClamp
}

/// <summary>Scalar MAPINFO level options from <c>g_mapinfo.cpp</c> (post-line-1560 band).</summary>
public static class VmMapInfoLevelScalars
{
    public static void ApplyTeamDamage(VmMapInfoLevelInfo info, double value) => info.TeamDamage = value;

    public static int ParseFogDensity(int raw) => Math.Clamp(raw, 0, 512) >> 1;

    public static int ParseSkyFog(int raw) => raw;

    public static float ParseThickFogDistance(double raw) => (float)raw;

    public static float? ParseThickFogMultiplier(double raw) => raw > 0 ? (float)raw : null;

    public static float ParsePixelStretch(double raw) => (float)raw;

    public static VmMapInfoLightMode ParseLightMode(int raw)
    {
        if (raw is 8 or 16) return VmMapInfoLightMode.NotSet;
        if (raw is >= 0 and <= 4) return (VmMapInfoLightMode)raw;
        return VmMapInfoLightMode.NotSet;
    }

    public static VmMapInfoLightBlendMode ParseLightBlendMode(string token)
    {
        if (token.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("Clamp", StringComparison.OrdinalIgnoreCase))
            return VmMapInfoLightBlendMode.Default;
        if (token.Equals("ColoredClamp", StringComparison.OrdinalIgnoreCase))
            return VmMapInfoLightBlendMode.ClampColor;
        if (token.Equals("Unclamped", StringComparison.OrdinalIgnoreCase))
            return VmMapInfoLightBlendMode.NoClamp;
        return VmMapInfoLightBlendMode.Default;
    }
}

public sealed class VmMapInfoLevelInfo
{
    public string EdName { get; set; } = "";
    public string AcsName { get; set; } = "";
    public bool NoTextureFill { get; set; }
    public bool LightAdditiveSurfaces { get; set; }
    public bool FsNoCheckPosition { get; set; }
    public double TeamDamage { get; set; }
    public string MapBackground { get; set; } = "";
    public int DefaultEnvironment { get; set; }
    public int HazardColor { get; set; }
    public int HazardFlash { get; set; }
    public int FogDensity { get; set; }
    public int OutsideFogDensity { get; set; }
    public int SkyFog { get; set; }
    public float ThickFogDistance { get; set; }
    public float? ThickFogMultiplier { get; set; }
    public float PixelStretch { get; set; } = 1;
    public int BrightFog { get; set; }
    public VmMapInfoLightMode LightMode { get; set; } = VmMapInfoLightMode.NotSet;
    public VmMapInfoLightBlendMode LightBlendMode { get; set; } = VmMapInfoLightBlendMode.Default;
}
