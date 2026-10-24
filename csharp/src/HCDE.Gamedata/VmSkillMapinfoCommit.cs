namespace HCDE.Gamedata;

/// <summary>ParseSkill finalize and default field init gaps from <c>g_skill.cpp</c>.</summary>
public static class VmSkillMapinfoCommit
{
    public const int TicRate = 35;

    public static void ApplyNativeDefaultFields(VmSkillInfo skill)
    {
        skill.NoMenu = false;
        skill.AmmoFactor = 1;
        skill.DoubleAmmoFactor = 2;
        skill.DropAmmoFactor = -1;
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
        skill.TextColor = "";
        skill.Replacements.Clear();
        skill.ReplacedBy.Clear();
        skill.MonsterHealth = 1;
        skill.FriendlyHealth = 1;
        skill.NoPain = false;
        skill.Infighting = 0;
        skill.PlayerRespawn = false;
    }

    public static bool TryUpsertSkill(
        IList<VmSkillInfo> allSkills,
        VmSkillInfo skill,
        bool isDefault,
        bool acsReturnSet,
        ref int defaultSkillIndex)
    {
        for (var i = 0; i < allSkills.Count; i++)
        {
            if (!allSkills[i].Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)) continue;
            if (!acsReturnSet)
                skill.AcsReturn = allSkills[i].AcsReturn;
            allSkills[i] = VmSkillInfoRuntime.Clone(skill);
            if (isDefault) defaultSkillIndex = i;
            return true;
        }

        if (!acsReturnSet)
            skill.AcsReturn = allSkills.Count;
        if (isDefault)
            defaultSkillIndex = allSkills.Count;
        allSkills.Add(VmSkillInfoRuntime.Clone(skill));
        return false;
    }
}
