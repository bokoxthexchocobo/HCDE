namespace HCDE.Gamedata;

public enum VmSkillPropertyInt
{
    FastMonsters,
    SlowMonsters,
    Respawn,
    RespawnLimit,
    DisableCheats,
    AutoUseHealth,
    EasyBossBrain,
    EasyKey,
    SpawnFilter,
    AcsReturn,
    NoPain,
    Infight,
    PlayerRespawn,
    SpawnMulti,
    InstantReaction,
    SpawnMultiCoopOnly
}

public enum VmSkillPropertyFloat
{
    AmmoFactor,
    DropAmmoFactor,
    ArmorFactor,
    HealthFactor,
    DamageFactor,
    Aggressiveness,
    MonsterHealth,
    FriendlyHealth,
    KickbackFactor
}

public sealed class VmGSkillQueryContext
{
    public int GameSkill { get; set; }
    public uint DmFlags { get; set; }
    public uint DmFlags2 { get; set; }
    public int DefaultRespawnTimeSeconds { get; set; } = 12;
    public int LevelInfighting { get; set; }
    public const uint DfFastMonsters = 1u << 2;
    public const uint DfMonstersRespawn = 1u << 3;
    public const uint Df2YesDoubleAmmo = 1u << 14;
    public const int TicRate = 35;
    public const int Level2NoInfighting = 1;
    public const int Level2TotalInfighting = 2;
}

/// <summary><c>G_SkillProperty</c>, <c>G_SkillName</c>, <c>G_VerifySkill</c> from <c>g_skill.cpp</c>.</summary>
public static class VmGSkillPropertyQuery
{
    public static int QueryInt(
        VmSkillPropertyInt prop,
        IReadOnlyList<VmSkillInfo> skills,
        VmGSkillQueryContext ctx)
    {
        if (skills.Count == 0) return 0;
        var skill = skills[ctx.GameSkill];
        return prop switch
        {
            VmSkillPropertyInt.FastMonsters => (skill.FastMonsters ? 1 : 0) |
                                               ((ctx.DmFlags & VmGSkillQueryContext.DfFastMonsters) != 0 ? 1 : 0),
            VmSkillPropertyInt.SlowMonsters => skill.SlowMonsters ? 1 : 0,
            VmSkillPropertyInt.Respawn => (ctx.DmFlags & VmGSkillQueryContext.DfMonstersRespawn) != 0 &&
                                          skill.RespawnCounter == 0
                ? VmGSkillQueryContext.TicRate * ctx.DefaultRespawnTimeSeconds
                : skill.RespawnCounter,
            VmSkillPropertyInt.RespawnLimit => skill.RespawnLimit,
            VmSkillPropertyInt.DisableCheats => skill.DisableCheats ? 1 : 0,
            VmSkillPropertyInt.AutoUseHealth => skill.AutoUseHealth ? 1 : 0,
            VmSkillPropertyInt.EasyBossBrain => skill.EasyBossBrain ? 1 : 0,
            VmSkillPropertyInt.EasyKey => skill.EasyKey ? 1 : 0,
            VmSkillPropertyInt.SpawnFilter => skill.SpawnFilter,
            VmSkillPropertyInt.AcsReturn => skill.AcsReturn,
            VmSkillPropertyInt.NoPain => skill.NoPain ? 1 : 0,
            VmSkillPropertyInt.Infight => skill.Infighting switch
            {
                VmGSkillQueryContext.Level2TotalInfighting => 1,
                VmGSkillQueryContext.Level2NoInfighting => -1,
                _ => ctx.LevelInfighting
            },
            VmSkillPropertyInt.PlayerRespawn => skill.PlayerRespawn ? 1 : 0,
            VmSkillPropertyInt.SpawnMulti => skill.SpawnMulti ? 1 : 0,
            VmSkillPropertyInt.InstantReaction => skill.InstantReaction ? 1 : 0,
            VmSkillPropertyInt.SpawnMultiCoopOnly => skill.SpawnMultiCoopOnly ? 1 : 0,
            _ => 0
        };
    }

    public static double QueryFloat(
        VmSkillPropertyFloat prop,
        IReadOnlyList<VmSkillInfo> skills,
        VmGSkillQueryContext ctx)
    {
        if (skills.Count == 0) return 0;
        var skill = skills[ctx.GameSkill];
        return prop switch
        {
            VmSkillPropertyFloat.AmmoFactor => (ctx.DmFlags2 & VmGSkillQueryContext.Df2YesDoubleAmmo) != 0
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
            _ => 0
        };
    }

    public static string ResolveSkillMenuName(
        VmSkillInfo skill,
        string? playerClassDisplayName,
        Func<string, string?>? resolveDollarString = null)
    {
        var name = string.IsNullOrEmpty(skill.MenuName) ? skill.Name : skill.MenuName;
        if (!string.IsNullOrEmpty(playerClassDisplayName) &&
            skill.MenuNamesForPlayerClass.TryGetValue(playerClassDisplayName, out var perClass))
            name = perClass;
        if (name.StartsWith('$') && resolveDollarString is not null)
            name = resolveDollarString(name[1..]) ?? name;
        return name;
    }

    public static int VerifyGameSkill(int gameSkill, int skillCount)
    {
        if (skillCount <= 0) return 0;
        if (gameSkill >= skillCount) return skillCount - 1;
        if (gameSkill < 0) return 0;
        return gameSkill;
    }
}
