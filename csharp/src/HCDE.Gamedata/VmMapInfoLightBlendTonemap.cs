namespace HCDE.Gamedata;

public enum VmMapInfoTonemapMode
{
    None,
    Linear,
    Uncharted2,
    HejlDawson,
    Reinhard
}

/// <summary><c>lightblendmode</c> and tonemap tail from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoLightBlendTonemap
{
    public static bool TryParseLightBlendMode(string token, out VmMapInfoLightBlendMode mode, out VmMapInfoTonemapMode? tonemap)
    {
        tonemap = null;
        if (token.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("Clamp", StringComparison.OrdinalIgnoreCase))
        {
            mode = VmMapInfoLightBlendMode.Default;
            return true;
        }

        if (token.Equals("ColoredClamp", StringComparison.OrdinalIgnoreCase))
        {
            mode = VmMapInfoLightBlendMode.ClampColor;
            return true;
        }

        if (token.Equals("Unclamped", StringComparison.OrdinalIgnoreCase))
        {
            mode = VmMapInfoLightBlendMode.NoClamp;
            return true;
        }

        mode = VmMapInfoLightBlendMode.Default;
        return false;
    }

    public static bool TryParseTonemap(string token, out VmMapInfoTonemapMode mode)
    {
        if (token.Equals("None", StringComparison.OrdinalIgnoreCase)) { mode = VmMapInfoTonemapMode.None; return true; }
        if (token.Equals("Linear", StringComparison.OrdinalIgnoreCase)) { mode = VmMapInfoTonemapMode.Linear; return true; }
        if (token.Equals("Uncharted2", StringComparison.OrdinalIgnoreCase)) { mode = VmMapInfoTonemapMode.Uncharted2; return true; }
        if (token.Equals("HejlDawson", StringComparison.OrdinalIgnoreCase)) { mode = VmMapInfoTonemapMode.HejlDawson; return true; }
        if (token.Equals("Reinhard", StringComparison.OrdinalIgnoreCase)) { mode = VmMapInfoTonemapMode.Reinhard; return true; }
        mode = VmMapInfoTonemapMode.None;
        return false;
    }
}
