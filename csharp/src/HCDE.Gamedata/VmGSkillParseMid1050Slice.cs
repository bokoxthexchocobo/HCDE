namespace HCDE.Gamedata;

/// <summary>
/// <c>FMapInfoParser::ParseSkill</c> continuation from <c>g_skill.cpp</c> (**151–300**).
/// Chunk **1050**; follows **1049** (**1–150**). Uncredited <see cref="VmSkillMapinfoParser"/> — not re-credited.
/// </summary>
public static class VmGSkillParseMid1050Slice
{
    public const int TicRate = 35;
    public const int Level2NoInfighting = 1;
    public const int SpawnFilterBaby = 1;
    public const int SpawnFilterEasy = 2;
    public const int SpawnFilterNormal = 4;
    public const int SpawnFilterHard = 8;
    public const int SpawnFilterNightmare = 16;

    public enum SkillParseMidKeyword
    {
        Unknown,
        RespawnTime,
        RespawnLimit,
        Aggressiveness,
        SpawnFilter,
        SpawnMulti,
        SpawnMultiCoopOnly,
        InstantReaction,
        AcsReturn,
        ReplaceActor,
        MenuName,
        PlayerClassName,
        PicName,
        MustConfirm,
        ShortcutKey,
        TextColor,
        MonsterHealth,
        FriendlyHealth,
        NoPain,
        ArmorFactor,
        HealthFactor,
        NoInfighting,
    }

    public static SkillParseMidKeyword ClassifySkillParseMidKeyword(string key)
    {
        if (key.Equals("respawntime", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.RespawnTime;
        if (key.Equals("respawnlimit", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.RespawnLimit;
        if (key.Equals("aggressiveness", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.Aggressiveness;
        if (key.Equals("spawnfilter", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.SpawnFilter;
        if (key.Equals("spawnmulti", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.SpawnMulti;
        if (key.Equals("spawnmulticooponly", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.SpawnMultiCoopOnly;
        if (key.Equals("instantreaction", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.InstantReaction;
        if (key.Equals("acsreturn", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.AcsReturn;
        if (key.Equals("replaceactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.ReplaceActor;
        if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.MenuName;
        if (key.Equals("playerclassname", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.PlayerClassName;
        if (key.Equals("picname", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.PicName;
        if (key.Equals("mustconfirm", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.MustConfirm;
        if (key.Equals("key", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.ShortcutKey;
        if (key.Equals("textcolor", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.TextColor;
        if (key.Equals("monsterhealth", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.MonsterHealth;
        if (key.Equals("friendlyhealth", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.FriendlyHealth;
        if (key.Equals("nopain", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.NoPain;
        if (key.Equals("armorfactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.ArmorFactor;
        if (key.Equals("healthfactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.HealthFactor;
        if (key.Equals("noinfighting", StringComparison.OrdinalIgnoreCase))
            return SkillParseMidKeyword.NoInfighting;
        return SkillParseMidKeyword.Unknown;
    }

    public static int ConvertRespawnTimeSecondsToTics(double seconds) =>
        (int)(seconds * TicRate);

    public static double ConvertAggressivenessInput(double input) =>
        1 - Math.Clamp(input, 0, 1);

    public static int ApplySpawnFilterNumericBit(int currentFilter, int spawnNumber) =>
        spawnNumber > 0 ? currentFilter | (1 << (spawnNumber - 1)) : currentFilter;

    public static int ApplySpawnFilterWord(int currentFilter, string word)
    {
        if (word.Equals("baby", StringComparison.OrdinalIgnoreCase))
            return currentFilter | SpawnFilterBaby;
        if (word.Equals("easy", StringComparison.OrdinalIgnoreCase))
            return currentFilter | SpawnFilterEasy;
        if (word.Equals("normal", StringComparison.OrdinalIgnoreCase))
            return currentFilter | SpawnFilterNormal;
        if (word.Equals("hard", StringComparison.OrdinalIgnoreCase))
            return currentFilter | SpawnFilterHard;
        if (word.Equals("nightmare", StringComparison.OrdinalIgnoreCase))
            return currentFilter | SpawnFilterNightmare;
        return currentFilter;
    }

    public static void ApplyReplaceActorPair(VmSkillInfo skill, string replaced, string replacer)
    {
        VmGSkillInfoAssignExport1047Slice.SetSkillReplacement(skill, replaced, replacer);
        VmGSkillInfoAssignExport1047Slice.SetSkillReplacedBy(skill, replacer, replaced);
    }

    public static string FormatSkillTextColorToken(string inner) => $"[{inner}]";

    public static char ParseShortcutKeyChar(string token) =>
        token.Length > 0 ? char.ToLowerInvariant(token[0]) : '\0';

    public static string StripMenuNameQuotes(string token) =>
        token.Trim().Trim('"');

    public static bool TryApplySkillParseMidProperty(
        VmSkillInfo skill,
        SkillParseMidKeyword keyword,
        double floatValue,
        int intValue,
        string stringValue,
        string? secondString,
        out bool acsReturnWasSet)
    {
        acsReturnWasSet = false;
        switch (keyword)
        {
            case SkillParseMidKeyword.RespawnTime:
                skill.RespawnCounter = ConvertRespawnTimeSecondsToTics(floatValue);
                return true;
            case SkillParseMidKeyword.RespawnLimit:
                skill.RespawnLimit = intValue;
                return true;
            case SkillParseMidKeyword.Aggressiveness:
                skill.Aggressiveness = ConvertAggressivenessInput(floatValue);
                return true;
            case SkillParseMidKeyword.AcsReturn:
                skill.AcsReturn = intValue;
                acsReturnWasSet = true;
                return true;
            case SkillParseMidKeyword.MenuName:
                skill.MenuName = StripMenuNameQuotes(stringValue);
                return true;
            case SkillParseMidKeyword.PicName:
                skill.PicName = stringValue;
                return true;
            case SkillParseMidKeyword.MonsterHealth:
                skill.MonsterHealth = floatValue;
                return true;
            case SkillParseMidKeyword.FriendlyHealth:
                skill.FriendlyHealth = floatValue;
                return true;
            case SkillParseMidKeyword.ArmorFactor:
                skill.ArmorFactor = floatValue;
                return true;
            case SkillParseMidKeyword.HealthFactor:
                skill.HealthFactor = floatValue;
                return true;
            case SkillParseMidKeyword.TextColor:
                skill.TextColor = FormatSkillTextColorToken(stringValue);
                return true;
            case SkillParseMidKeyword.ShortcutKey:
                skill.Shortcut = ParseShortcutKeyChar(stringValue);
                return true;
            case SkillParseMidKeyword.PlayerClassName:
                if (secondString is null)
                    return false;
                skill.MenuNamesForPlayerClass[stringValue] = secondString;
                return true;
            case SkillParseMidKeyword.ReplaceActor:
                if (secondString is null)
                    return false;
                ApplyReplaceActorPair(skill, stringValue, secondString);
                return true;
            default:
                return false;
        }
    }

    public static bool TryApplySkillParseMidFlag(VmSkillInfo skill, SkillParseMidKeyword keyword)
    {
        switch (keyword)
        {
            case SkillParseMidKeyword.SpawnMulti:
                skill.SpawnMulti = true;
                return true;
            case SkillParseMidKeyword.SpawnMultiCoopOnly:
                skill.SpawnMultiCoopOnly = true;
                return true;
            case SkillParseMidKeyword.InstantReaction:
                skill.InstantReaction = true;
                return true;
            case SkillParseMidKeyword.MustConfirm:
                skill.MustConfirm = true;
                return true;
            case SkillParseMidKeyword.NoPain:
                skill.NoPain = true;
                return true;
            case SkillParseMidKeyword.NoInfighting:
                skill.Infighting = Level2NoInfighting;
                return true;
            default:
                return false;
        }
    }
}
