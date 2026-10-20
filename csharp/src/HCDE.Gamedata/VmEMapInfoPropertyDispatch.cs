namespace HCDE.Gamedata;

public sealed class VmEMapInfoLevelDraft
{
    public string MapName { get; set; } = "";
    public string LevelName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string NextMap { get; set; } = "";
    public string NextSecretMap { get; set; } = "";
    public string EdName { get; set; } = "";
    public string AcsName { get; set; } = "";
    public int Flags { get; set; }
    public double AirControl { get; set; }
    public double Gravity { get; set; } = double.MaxValue;
    public string CustomColorMap { get; set; } = "";
    public int Partime { get; set; }
}

/// <summary><c>ResolveSiblingLump</c> tail and <c>ApplyEMapInfoProperty</c> from <c>hcde_emapinfo.cpp</c> (539–663).</summary>
public static class VmEMapInfoPropertyDispatch
{
    public static bool ShouldTryShortLumpName(string name) =>
        name.Length <= 8 && !VmEMapInfoLineParser.HasPathOrExtension(name);

    public static bool TryApplyLevelName(string key, string value, VmEMapInfoLevelDraft draft)
    {
        if (key is not ("levelname" or "inter-levelname")) return false;
        draft.LevelName = value;
        return true;
    }

    public static bool TryApplyGravity(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        VmEMapInfoLevelDraft draft)
    {
        if (key != "gravity") return false;
        if (!VmEMapInfoLineParser.TryParseInt(value, out var raw))
        {
            VmEMapInfoApplyHelpers.RecordInvalid(stats, mapName, key, line);
            return true;
        }

        draft.Gravity = raw == 0 ? double.MaxValue : raw / 65536.0;
        return true;
    }

    public static bool TryApplyAirControl(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        VmEMapInfoLevelDraft draft)
    {
        if (key != "aircontrol") return false;
        if (!VmEMapInfoLineParser.TryParseInt(value, out var raw))
        {
            VmEMapInfoApplyHelpers.RecordInvalid(stats, mapName, key, line);
            return true;
        }

        draft.AirControl = raw == 0 ? double.Epsilon : raw / 65536.0;
        return true;
    }

    public static bool TryApplyDisableJump(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        ref int flags)
    {
        if (key != "disable-jump") return false;
        var enabled = false;
        if (!VmEMapInfoApplyHelpers.ApplyBoolProperty(stats, mapName, key, value, line, ref enabled))
            return true;
        VmEMapInfoParsePrimitives.SetFlag(ref flags, 0x80, enabled);
        return true;
    }
}
