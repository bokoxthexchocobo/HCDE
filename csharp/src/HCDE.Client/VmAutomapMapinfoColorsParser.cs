namespace HCDE.Client;

/// <summary><c>FMapInfoParser::ParseAMColors</c> from <c>am_map.cpp</c> (767–838).</summary>
public enum VmAutomapColorBasePreset
{
    Doom,
    Raven,
    Strife,
}

public sealed class VmAutomapMapinfoColorsDraft
{
    public VmAutomapColorBasePreset? BasePreset { get; set; }
    public bool ColorsAssigned { get; set; }
    public bool? DisplayLocks { get; set; }
    public Dictionary<int, VmAutomapColorArgb> SlotColors { get; } = new();
}

public static class VmAutomapMapinfoColorsParser
{
    public static bool TryParseBasePreset(string token, out VmAutomapColorBasePreset preset, out string? error)
    {
        if (token.Equals("doom", StringComparison.OrdinalIgnoreCase))
        {
            preset = VmAutomapColorBasePreset.Doom;
            error = null;
            return true;
        }

        if (token.Equals("raven", StringComparison.OrdinalIgnoreCase))
        {
            preset = VmAutomapColorBasePreset.Raven;
            error = null;
            return true;
        }

        if (token.Equals("strife", StringComparison.OrdinalIgnoreCase))
        {
            preset = VmAutomapColorBasePreset.Strife;
            error = null;
            return true;
        }

        preset = default;
        error = "Unknown value for 'base'. Must be 'Doom', 'Strife' or 'Raven'.";
        return false;
    }

    public static bool RequireBaseBeforeFirstColor(bool colorsAlreadyAssigned, string key)
    {
        if (!colorsAlreadyAssigned || key.Equals("base", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static string? ValidateBaseKeyOrder(string key, bool colorsAlreadyAssigned)
    {
        if (key.Equals("base", StringComparison.OrdinalIgnoreCase) && colorsAlreadyAssigned)
            return "'base' must be specified before the first color";
        return null;
    }

    public static bool TryParseShowLocks(bool falseLiteral, out bool displayLocks)
    {
        displayLocks = !falseLiteral;
        return true;
    }

    public static bool TryResolveColorNameIndex(string key, out int index)
    {
        for (var i = 0; i < VmAutomapColorsetIndex.ColorNames.Length; i++)
        {
            if (key.Equals(VmAutomapColorsetIndex.ColorNames[i], StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public static void ApplyRgbToSlot(VmAutomapMapinfoColorsDraft draft, int index, byte r, byte g, byte b)
    {
        draft.SlotColors[index] = VmAutomapColorArgb.FromRgb(r, g, b);
        draft.ColorsAssigned = true;
    }

    public static bool UsesRavenGridForBase(VmAutomapColorBasePreset preset) =>
        preset == VmAutomapColorBasePreset.Raven;

    public static void ApplyBasePreset(VmAutomapMapinfoColorsDraft draft, VmAutomapColorBasePreset preset)
    {
        draft.BasePreset = preset;
        draft.ColorsAssigned = false;
        draft.SlotColors.Clear();
    }
}
