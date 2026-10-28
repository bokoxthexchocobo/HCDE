namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawString</c> <c>ParseStringValue</c>, <c>Reset</c>, and dynamic <c>Tick</c> cache policy from <c>sbarinfo_commands.cpp</c> (**751–900**).
/// Chunk **1078**; follows **1077** parse tail seam (**751–755** excluded). Runtime string/ACS host lookups remain injected.
/// </summary>
public static class VmSbarinfoCommandDrawString1078Slice
{
    public const int NumGlobalVars = 64;
    public const int InvalidStringCache = -1;

    public enum DrawStringValueKind
    {
        LevelName,
        LevelLump,
        SkillName,
        PlayerClass,
        PlayerName,
        Ammo1Tag,
        Ammo2Tag,
        WeaponTag,
        InventoryTag,
        GlobalVar,
        GlobalArray,
        Time,
        LogText,
        Constant
    }

    public static (bool Recognized, DrawStringValueKind Kind, bool InvalidKeyword) TryClassifyDrawStringValueKeyword(
        string identifier)
    {
        if (identifier.Equals("levelname", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.LevelName, false);
        if (identifier.Equals("levellump", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.LevelLump, false);
        if (identifier.Equals("skillname", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.SkillName, false);
        if (identifier.Equals("playerclass", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.PlayerClass, false);
        if (identifier.Equals("playername", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.PlayerName, false);
        if (identifier.Equals("ammo1tag", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.Ammo1Tag, false);
        if (identifier.Equals("ammo2tag", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.Ammo2Tag, false);
        if (identifier.Equals("weapontag", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.WeaponTag, false);
        if (identifier.Equals("inventorytag", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.InventoryTag, false);
        if (identifier.Equals("time", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.Time, false);
        if (identifier.Equals("logtext", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.LogText, false);
        if (identifier.Equals("globalvar", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.GlobalVar, false);
        if (identifier.Equals("globalarray", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringValueKind.GlobalArray, false);
        return (false, DrawStringValueKind.Constant, true);
    }

    public static bool IsGlobalVarIndexInRange(int index) => index >= 0 && index < NumGlobalVars;

    public static DrawStringValueKind ResolveConstantStringValueKind() => DrawStringValueKind.Constant;

    public static bool ShouldInvalidateDrawStringCacheOnReset(DrawStringValueKind kind) =>
        kind is DrawStringValueKind.PlayerClass or DrawStringValueKind.SkillName;

    public static bool ShouldRefreshLevelBoundDrawString(int levelLumpNum, int cache) => levelLumpNum != cache;

    public static bool ShouldRefreshPlayerClassDrawString(int classNum, int cache) => classNum != cache;

    public static bool ShouldRefreshGlobalVarDrawString(int globalValue, int cache) => globalValue != cache;

    public static bool ShouldRefreshGlobalArrayDrawString(int arrayValue, int cache) => arrayValue != cache;

    public static (int Hours, int Minutes, int Seconds) SplitLevelTimeComponents(int totalSeconds) =>
        (totalSeconds / 3600, (totalSeconds % 3600) / 60, totalSeconds % 60);

    public static string FormatDrawStringLevelTime(int totalSeconds)
    {
        var (hours, minutes, seconds) = SplitLevelTimeComponents(totalSeconds);
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}
