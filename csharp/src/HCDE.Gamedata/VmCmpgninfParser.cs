namespace HCDE.Gamedata;

/// <summary><c>HCDE_ParseCMPGNINF</c> block parsing from <c>g_mapinfo.cpp</c>.</summary>
public sealed class VmCmpgninfBlock
{
    public string MapName { get; set; } = "";
    public int WaveLimit { get; set; }
    public bool HasWaveLimit { get; set; }
    public int DuelLimit { get; set; }
    public bool HasDuelLimit { get; set; }

    public bool ShouldApply() =>
        MapName.Length > 0 && (HasWaveLimit || HasDuelLimit);
}

public static class VmCmpgninfParser
{
    public static int ClampWaveLimit(int value) => Math.Clamp(value, 0, 3000);
    public static int ClampDuelLimit(int value) => Math.Clamp(value, 0, 255);

    public static bool TryApplyProperty(VmCmpgninfBlock block, string property, string value, out string error)
    {
        error = "";
        if (property.Equals("mapname", StringComparison.OrdinalIgnoreCase))
        {
            block.MapName = value;
            return true;
        }

        if (property.Equals("wavelimit", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(value, out var n))
            {
                error = "wavelimit must be numeric";
                return false;
            }

            block.WaveLimit = ClampWaveLimit(n);
            block.HasWaveLimit = true;
            return true;
        }

        if (property.Equals("duellimit", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(value, out var n))
            {
                error = "duellimit must be numeric";
                return false;
            }

            block.DuelLimit = ClampDuelLimit(n);
            block.HasDuelLimit = true;
            return true;
        }

        return true;
    }
}
