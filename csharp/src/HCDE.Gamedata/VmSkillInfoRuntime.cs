namespace HCDE.Gamedata;

/// <summary><c>FSkillInfo</c> copy, text color, and actor replacement helpers from <c>g_skill.cpp</c>.</summary>
public static class VmSkillInfoRuntime
{
    public const int CrUntranslated = -1;

    public static VmSkillInfo Clone(VmSkillInfo other)
    {
        var copy = new VmSkillInfo
        {
            Name = other.Name,
            AmmoFactor = other.AmmoFactor,
            NoMenu = other.NoMenu,
            DoubleAmmoFactor = other.DoubleAmmoFactor,
            DropAmmoFactor = other.DropAmmoFactor,
            DamageFactor = other.DamageFactor,
            KickbackFactor = other.KickbackFactor,
            FastMonsters = other.FastMonsters,
            SlowMonsters = other.SlowMonsters,
            DisableCheats = other.DisableCheats,
            AutoUseHealth = other.AutoUseHealth,
            EasyBossBrain = other.EasyBossBrain,
            EasyKey = other.EasyKey,
            RespawnCounter = other.RespawnCounter,
            RespawnLimit = other.RespawnLimit,
            Aggressiveness = other.Aggressiveness,
            SpawnFilter = other.SpawnFilter,
            SpawnMulti = other.SpawnMulti,
            InstantReaction = other.InstantReaction,
            SpawnMultiCoopOnly = other.SpawnMultiCoopOnly,
            AcsReturn = other.AcsReturn,
            MenuName = other.MenuName,
            PicName = other.PicName,
            MustConfirm = other.MustConfirm,
            MustConfirmText = other.MustConfirmText,
            Shortcut = other.Shortcut,
            TextColor = other.TextColor,
            MonsterHealth = other.MonsterHealth,
            FriendlyHealth = other.FriendlyHealth,
            NoPain = other.NoPain,
            Infighting = other.Infighting,
            ArmorFactor = other.ArmorFactor,
            HealthFactor = other.HealthFactor,
            PlayerRespawn = other.PlayerRespawn
        };
        foreach (var (k, v) in other.MenuNamesForPlayerClass) copy.MenuNamesForPlayerClass[k] = v;
        foreach (var (k, v) in other.Replacements) copy.Replacements[k] = v;
        foreach (var (k, v) in other.ReplacedBy) copy.ReplacedBy[k] = v;
        return copy;
    }

    public static int GetTextColor(VmSkillInfo skill, Func<string, int?>? tryParseFontColor = null, Action<string>? logUndefined = null)
    {
        if (string.IsNullOrEmpty(skill.TextColor)) return CrUntranslated;
        var color = tryParseFontColor?.Invoke(skill.TextColor);
        if (color is null)
        {
            logUndefined?.Invoke($"Undefined color '{skill.TextColor}' in definition of skill {skill.Name}");
            return CrUntranslated;
        }

        return color.Value;
    }

    public static void SetReplacement(VmSkillInfo skill, string actor, string replacement) =>
        skill.Replacements[actor] = replacement;

    public static string? GetReplacement(VmSkillInfo skill, string actor) =>
        skill.Replacements.TryGetValue(actor, out var r) ? r : null;

    public static void SetReplacedBy(VmSkillInfo skill, string replacer, string replaced) =>
        skill.ReplacedBy[replacer] = replaced;

    public static string? GetReplacedBy(VmSkillInfo skill, string replacer) =>
        skill.ReplacedBy.TryGetValue(replacer, out var r) ? r : null;
}
