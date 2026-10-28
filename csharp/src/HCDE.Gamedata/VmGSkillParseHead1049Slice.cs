namespace HCDE.Gamedata;

/// <summary>
/// Global skill table and <c>FMapInfoParser::ParseSkill</c> head from <c>g_skill.cpp</c> (**1–150**).
/// Chunk **1049**; <c>g_skill.cpp</c> tail **391–690** credited **1048–1047**. Uncredited <see cref="VmSkillMapinfoParser"/> / <see cref="VmGSkillGlobals"/> — not re-credited.
/// </summary>
public static class VmGSkillParseHead1049Slice
{
    public const int DefaultSkillIndexUnset = -1;
    public const int TicRate = 35;
    public const double DropAmmoFactorUnset = -1;

    public enum SkillParseKeyword
    {
        Unknown,
        AmmoFactor,
        DoubleAmmoFactor,
        DropAmmoFactor,
        DamageFactor,
        KickbackFactor,
        FastMonsters,
        SlowMonsters,
        DisableCheats,
        EasyBossBrain,
        EasyKey,
        AutoUseHealth,
        NoMenu,
        PlayerRespawn,
    }

    public static void ApplyParseSkillFactoryDefaults(VmSkillInfo skill)
    {
        skill.NoMenu = false;
        skill.AmmoFactor = 1;
        skill.DoubleAmmoFactor = 2;
        skill.DropAmmoFactor = DropAmmoFactorUnset;
        skill.DamageFactor = 1;
        skill.ArmorFactor = 1;
        skill.HealthFactor = 1;
        skill.KickbackFactor = 1;
        skill.FastMonsters = false;
        skill.SlowMonsters = false;
        skill.DisableCheats = false;
        skill.EasyBossBrain = false;
        skill.EasyKey = false;
        skill.AutoUseHealth = false;
        skill.RespawnCounter = 0;
        skill.RespawnLimit = 0;
        skill.Aggressiveness = 1;
        skill.SpawnFilter = 0;
        skill.SpawnMulti = false;
        skill.SpawnMultiCoopOnly = false;
        skill.InstantReaction = false;
        skill.AcsReturn = 0;
        skill.MustConfirm = false;
        skill.Shortcut = '\0';
        skill.TextColor = string.Empty;
        skill.MonsterHealth = 1;
        skill.FriendlyHealth = 1;
        skill.NoPain = false;
        skill.Infighting = 0;
        skill.PlayerRespawn = false;
        skill.Replacements.Clear();
        skill.ReplacedBy.Clear();
        skill.MenuNamesForPlayerClass.Clear();
    }

    public static SkillParseKeyword ClassifySkillParseKeyword(string key)
    {
        if (key.Equals("ammofactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.AmmoFactor;
        if (key.Equals("doubleammofactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.DoubleAmmoFactor;
        if (key.Equals("dropammofactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.DropAmmoFactor;
        if (key.Equals("damagefactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.DamageFactor;
        if (key.Equals("kickbackfactor", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.KickbackFactor;
        if (key.Equals("fastmonsters", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.FastMonsters;
        if (key.Equals("slowmonsters", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.SlowMonsters;
        if (key.Equals("disablecheats", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.DisableCheats;
        if (key.Equals("easybossbrain", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.EasyBossBrain;
        if (key.Equals("easykey", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.EasyKey;
        if (key.Equals("autousehealth", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.AutoUseHealth;
        if (key.Equals("nomenu", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.NoMenu;
        if (key.Equals("playerrespawn", StringComparison.OrdinalIgnoreCase))
            return SkillParseKeyword.PlayerRespawn;
        return SkillParseKeyword.Unknown;
    }

    public static bool TryApplySkillParseFloatProperty(VmSkillInfo skill, SkillParseKeyword keyword, double value)
    {
        switch (keyword)
        {
            case SkillParseKeyword.AmmoFactor:
                skill.AmmoFactor = value;
                return true;
            case SkillParseKeyword.DoubleAmmoFactor:
                skill.DoubleAmmoFactor = value;
                return true;
            case SkillParseKeyword.DropAmmoFactor:
                skill.DropAmmoFactor = value;
                return true;
            case SkillParseKeyword.DamageFactor:
                skill.DamageFactor = value;
                return true;
            case SkillParseKeyword.KickbackFactor:
                skill.KickbackFactor = value;
                return true;
            default:
                return false;
        }
    }

    public static bool TryApplySkillParseFlagProperty(VmSkillInfo skill, SkillParseKeyword keyword)
    {
        switch (keyword)
        {
            case SkillParseKeyword.FastMonsters:
                skill.FastMonsters = true;
                return true;
            case SkillParseKeyword.SlowMonsters:
                skill.SlowMonsters = true;
                return true;
            case SkillParseKeyword.DisableCheats:
                skill.DisableCheats = true;
                return true;
            case SkillParseKeyword.EasyBossBrain:
                skill.EasyBossBrain = true;
                return true;
            case SkillParseKeyword.EasyKey:
                skill.EasyKey = true;
                return true;
            case SkillParseKeyword.AutoUseHealth:
                skill.AutoUseHealth = true;
                return true;
            case SkillParseKeyword.NoMenu:
                skill.NoMenu = true;
                return true;
            case SkillParseKeyword.PlayerRespawn:
                skill.PlayerRespawn = true;
                return true;
            default:
                return false;
        }
    }

    public static void AssignSkillBlockNameFromScanner(VmSkillInfo skill, string token) =>
        skill.Name = token;
}
