namespace HCDE.Gamedata;

/// <summary>
/// <c>FSkillInfo::operator=</c>, text color, replacement maps, VM exports, and native <c>GetTextColor</c> from <c>g_skill.cpp</c> (**541–690**).
/// Chunk **1047**. Uncredited <see cref="VmSkillInfoRuntime"/> parallels the same helpers — not re-credited.
/// </summary>
public static class VmGSkillInfoAssignExport1047Slice
{
    public const int CrUntranslated = -1;
    public const string AllSkillsGlobalExportName = "AllSkills";

    public static IReadOnlyList<string> DefineFieldExportNames { get; } =
    [
        "SkillName", "AmmoFactor", "DoubleAmmoFactor", "DropAmmoFactor", "DamageFactor", "ArmorFactor",
        "HealthFactor", "KickbackFactor", "FastMonsters", "SlowMonsters", "DisableCheats", "AutoUseHealth",
        "EasyBossBrain", "EasyKey", "NoMenu", "RespawnCounter", "RespawnLimit", "Aggressiveness", "SpawnFilter",
        "SpawnMulti", "InstantReaction", "SpawnMultiCoopOnly", "ACSReturn", "MenuName", "PicName",
        "MenuNamesForPlayerClass", "MustConfirm", "MustConfirmText", "Shortcut", "TextColor", "Replace",
        "Replaced", "MonsterHealth", "FriendlyHealth", "NoPain", "Infighting", "PlayerRespawn",
    ];

    public static bool IsExportedSkillInfoField(string fieldName) =>
        DefineFieldExportNames.Contains(fieldName, StringComparer.Ordinal);

    public static void ApplySkillInfoOperatorAssign(VmSkillInfo target, VmSkillInfo source)
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
        target.DisableCheats = source.DisableCheats;
        target.AutoUseHealth = source.AutoUseHealth;
        target.EasyBossBrain = source.EasyBossBrain;
        target.EasyKey = source.EasyKey;
        target.RespawnCounter = source.RespawnCounter;
        target.RespawnLimit = source.RespawnLimit;
        target.Aggressiveness = source.Aggressiveness;
        target.SpawnFilter = source.SpawnFilter;
        target.SpawnMulti = source.SpawnMulti;
        target.InstantReaction = source.InstantReaction;
        target.SpawnMultiCoopOnly = source.SpawnMultiCoopOnly;
        target.AcsReturn = source.AcsReturn;
        target.MenuName = source.MenuName;
        target.PicName = source.PicName;
        target.MustConfirm = source.MustConfirm;
        target.MustConfirmText = source.MustConfirmText;
        target.Shortcut = source.Shortcut;
        target.TextColor = source.TextColor;
        target.MonsterHealth = source.MonsterHealth;
        target.FriendlyHealth = source.FriendlyHealth;
        target.NoPain = source.NoPain;
        target.Infighting = source.Infighting;
        target.ArmorFactor = source.ArmorFactor;
        target.HealthFactor = source.HealthFactor;
        target.PlayerRespawn = source.PlayerRespawn;
        CopyStringDictionary(source.MenuNamesForPlayerClass, target.MenuNamesForPlayerClass);
        CopyStringDictionary(source.Replacements, target.Replacements);
        CopyStringDictionary(source.ReplacedBy, target.ReplacedBy);
    }

    public static int ResolveSkillInfoTextColor(
        VmSkillInfo skill,
        Func<string, int?>? tryParseFontColor,
        Action<string>? logMessage = null)
    {
        if (string.IsNullOrEmpty(skill.TextColor))
            return CrUntranslated;

        var parsed = tryParseFontColor?.Invoke(skill.TextColor);
        if (parsed is null)
        {
            logMessage?.Invoke(FormatUndefinedSkillTextColorMessage(skill.TextColor, skill.Name));
            return CrUntranslated;
        }

        return parsed.Value;
    }

    public static string FormatUndefinedSkillTextColorMessage(string textColor, string skillName) =>
        $"Undefined color '{textColor}' in definition of skill {skillName}";

    public static void SetSkillReplacement(VmSkillInfo skill, string actor, string replacement) =>
        skill.Replacements[actor] = replacement;

    public static string? GetSkillReplacement(VmSkillInfo skill, string actor) =>
        skill.Replacements.TryGetValue(actor, out var value) ? value : null;

    public static void SetSkillReplacedBy(VmSkillInfo skill, string replacer, string replaced) =>
        skill.ReplacedBy[replacer] = replaced;

    public static string? GetSkillReplacedBy(VmSkillInfo skill, string replacer) =>
        skill.ReplacedBy.TryGetValue(replacer, out var value) ? value : null;

    public static int InvokeNativeGetTextColorAction(VmSkillInfo self, Func<string, int?>? tryParseFontColor) =>
        ResolveSkillInfoTextColor(self, tryParseFontColor);

    private static void CopyStringDictionary(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> target)
    {
        target.Clear();
        foreach (var pair in source)
            target[pair.Key] = pair.Value;
    }
}
