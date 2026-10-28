namespace HCDE.Gamedata;

/// <summary>
/// <c>G_SkillProperty</c> tail, exports, <c>G_SkillName</c>, <c>G_VerifySkill</c>, and <c>FSkillInfo::operator=</c> head from <c>g_skill.cpp</c> (**391–540**).
/// Chunk **1048**; follows **1047** (**541–690**). Uncredited <see cref="VmGSkillPropertyQuery"/> parallels query/name/clamp — not re-credited.
/// </summary>
public static class VmGSkillPropertyExport1048Slice
{
    public const int Level2NoInfighting = 1;
    public const int Level2TotalInfighting = 2;
    public const uint Df2YesDoubleAmmo = 1u << 14;

    public const string SkillPropertyIntExportName = "G_SkillPropertyInt";
    public const string SkillPropertyFloatExportName = "G_SkillPropertyFloat";
    public const string SkillNameExportName = "G_SkillName";

    public static int ResolveNoPainProperty(VmSkillInfo skill) => skill.NoPain ? 1 : 0;

    public static int ResolveInfightingProperty(VmSkillInfo skill, int levelInfighting)
    {
        if (skill.Infighting == Level2TotalInfighting)
            return 1;
        if (skill.Infighting == Level2NoInfighting)
            return -1;
        return levelInfighting;
    }

    public static int ResolveBoolSkillFlag(bool value) => value ? 1 : 0;

    public static double ResolveFloatSkillProperty(
        VmSkillPropertyFloat property,
        VmSkillInfo skill,
        uint dmFlags2)
    {
        return property switch
        {
            VmSkillPropertyFloat.AmmoFactor => (dmFlags2 & Df2YesDoubleAmmo) != 0
                ? skill.DoubleAmmoFactor
                : skill.AmmoFactor,
            VmSkillPropertyFloat.DropAmmoFactor => skill.DropAmmoFactor,
            VmSkillPropertyFloat.ArmorFactor => skill.ArmorFactor,
            VmSkillPropertyFloat.HealthFactor => skill.HealthFactor,
            VmSkillPropertyFloat.DamageFactor => skill.DamageFactor,
            VmSkillPropertyFloat.Aggressiveness => skill.Aggressiveness,
            VmSkillPropertyFloat.MonsterHealth => skill.MonsterHealth,
            VmSkillPropertyFloat.FriendlyHealth => skill.FriendlyHealth,
            VmSkillPropertyFloat.KickbackFactor => skill.KickbackFactor,
            _ => 0,
        };
    }

    public static string ResolveDisplayedSkillName(
        VmSkillInfo skill,
        string? playerClassDisplayName,
        Func<string, string?>? localizedLookup)
    {
        var name = string.IsNullOrEmpty(skill.MenuName) ? skill.Name : skill.MenuName;
        if (!string.IsNullOrEmpty(playerClassDisplayName)
            && skill.MenuNamesForPlayerClass.TryGetValue(playerClassDisplayName, out var perClass))
        {
            name = perClass;
        }

        if (name.StartsWith('$'))
            return localizedLookup?.Invoke(name[1..]) ?? name[1..];
        return name;
    }

    public static int ClampActiveGameSkill(int gameSkill, int skillCount)
    {
        if (skillCount <= 0)
            return 0;
        if (gameSkill >= skillCount)
            return skillCount - 1;
        if (gameSkill < 0)
            return 0;
        return gameSkill;
    }

    public static void ApplySkillInfoOperatorAssignHead(VmSkillInfo target, VmSkillInfo source)
    {
        target.Name = source.Name;
        target.AmmoFactor = source.AmmoFactor;
        target.NoMenu = source.NoMenu;
        target.DoubleAmmoFactor = source.DoubleAmmoFactor;
        target.DropAmmoFactor = source.DropAmmoFactor;
        target.DamageFactor = source.DamageFactor;
        target.KickbackFactor = source.KickbackFactor;
        target.FastMonsters = source.FastMonsters;
        target.SlowMonsters = source.SlowMonsters;
    }

    public static int ResolveSkillPropertyIntTail(
        VmSkillPropertyInt property,
        VmSkillInfo skill,
        int levelInfighting)
    {
        return property switch
        {
            VmSkillPropertyInt.NoPain => ResolveNoPainProperty(skill),
            VmSkillPropertyInt.Infight => ResolveInfightingProperty(skill, levelInfighting),
            VmSkillPropertyInt.PlayerRespawn => ResolveBoolSkillFlag(skill.PlayerRespawn),
            VmSkillPropertyInt.SpawnMulti => ResolveBoolSkillFlag(skill.SpawnMulti),
            VmSkillPropertyInt.InstantReaction => ResolveBoolSkillFlag(skill.InstantReaction),
            VmSkillPropertyInt.SpawnMultiCoopOnly => ResolveBoolSkillFlag(skill.SpawnMultiCoopOnly),
            VmSkillPropertyInt.AcsReturn => skill.AcsReturn,
            _ => 0,
        };
    }
}
