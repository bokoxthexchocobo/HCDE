namespace HCDE.Gamedata;

public sealed class VmSkillInfo
{
    public string Name { get; set; } = "";
    public double AmmoFactor { get; set; } = 1;
    public double DoubleAmmoFactor { get; set; } = 2;
    public double DamageFactor { get; set; } = 1;
    public double KickbackFactor { get; set; } = 1;
    public bool FastMonsters { get; set; }
    public bool SlowMonsters { get; set; }
    public bool DisableCheats { get; set; }
    public bool EasyBossBrain { get; set; }
    public bool EasyKey { get; set; }
    public bool AutoUseHealth { get; set; }
    public bool NoMenu { get; set; }
    public bool PlayerRespawn { get; set; }
    public int RespawnCounter { get; set; }
    public int RespawnLimit { get; set; }
    public double Aggressiveness { get; set; } = 1;
    public int SpawnFilter { get; set; }
    public bool SpawnMulti { get; set; }
    public bool SpawnMultiCoopOnly { get; set; }
    public bool InstantReaction { get; set; }
    public int AcsReturn { get; set; }
    public string MenuName { get; set; } = "";
    public string PicName { get; set; } = "";
    public bool MustConfirm { get; set; }
    public string MustConfirmText { get; set; } = "";
    public char Shortcut { get; set; }
    public string TextColor { get; set; } = "";
    public double MonsterHealth { get; set; } = 1;
    public double FriendlyHealth { get; set; } = 1;
    public bool NoPain { get; set; }
    public double ArmorFactor { get; set; } = 1;
    public double HealthFactor { get; set; } = 1;
    public int Infighting { get; set; }
    public Dictionary<string, string> Replacements { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> MenuNamesForPlayerClass { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary><c>FMapInfoParser::ParseSkill</c> from <c>g_skill.cpp</c>.</summary>
public static class VmSkillMapinfoParser
{
    public static VmSkillInfo CreateDefaultSkill(string name) => new() { Name = name };

    public static bool TryApplyFlag(VmSkillInfo skill, string key)
    {
        if (key.Equals("fastmonsters", StringComparison.OrdinalIgnoreCase)) { skill.FastMonsters = true; return true; }
        if (key.Equals("slowmonsters", StringComparison.OrdinalIgnoreCase)) { skill.SlowMonsters = true; return true; }
        if (key.Equals("disablecheats", StringComparison.OrdinalIgnoreCase)) { skill.DisableCheats = true; return true; }
        if (key.Equals("easybossbrain", StringComparison.OrdinalIgnoreCase)) { skill.EasyBossBrain = true; return true; }
        if (key.Equals("easykey", StringComparison.OrdinalIgnoreCase)) { skill.EasyKey = true; return true; }
        if (key.Equals("autousehealth", StringComparison.OrdinalIgnoreCase)) { skill.AutoUseHealth = true; return true; }
        if (key.Equals("nomenu", StringComparison.OrdinalIgnoreCase)) { skill.NoMenu = true; return true; }
        if (key.Equals("playerrespawn", StringComparison.OrdinalIgnoreCase)) { skill.PlayerRespawn = true; return true; }
        if (key.Equals("spawnmulti", StringComparison.OrdinalIgnoreCase)) { skill.SpawnMulti = true; return true; }
        if (key.Equals("spawnmulticooponly", StringComparison.OrdinalIgnoreCase)) { skill.SpawnMultiCoopOnly = true; return true; }
        if (key.Equals("instantreaction", StringComparison.OrdinalIgnoreCase)) { skill.InstantReaction = true; return true; }
        return false;
    }

    public static bool TryApplyOption(VmSkillInfo skill, string key, IReadOnlyList<string> tokens, ref int i)
    {
        if (TryApplyFlag(skill, key)) return true;
        if (i >= tokens.Count || tokens[i] != "=") return false;
        i++;
        if (key.Equals("ammofactor", StringComparison.OrdinalIgnoreCase))
        {
            skill.AmmoFactor = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("damagefactor", StringComparison.OrdinalIgnoreCase))
        {
            skill.DamageFactor = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("kickbackfactor", StringComparison.OrdinalIgnoreCase))
        {
            skill.KickbackFactor = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("respawntime", StringComparison.OrdinalIgnoreCase))
        {
            skill.RespawnCounter = (int)(double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture) * 35);
            return true;
        }

        if (key.Equals("respawnlimit", StringComparison.OrdinalIgnoreCase))
        {
            skill.RespawnLimit = int.Parse(tokens[i++]);
            return true;
        }

        if (key.Equals("aggressiveness", StringComparison.OrdinalIgnoreCase))
        {
            var v = Math.Clamp(double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture), 0, 1);
            skill.Aggressiveness = 1 - v;
            return true;
        }

        if (key.Equals("spawnfilter", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(tokens[i], out var num) && num > 0) skill.SpawnFilter |= 1 << (num - 1);
            else
            {
                var word = tokens[i++];
                if (word.Equals("baby", StringComparison.OrdinalIgnoreCase)) skill.SpawnFilter |= 1;
                else if (word.Equals("easy", StringComparison.OrdinalIgnoreCase)) skill.SpawnFilter |= 2;
                else if (word.Equals("normal", StringComparison.OrdinalIgnoreCase)) skill.SpawnFilter |= 4;
                else if (word.Equals("hard", StringComparison.OrdinalIgnoreCase)) skill.SpawnFilter |= 8;
                else if (word.Equals("nightmare", StringComparison.OrdinalIgnoreCase)) skill.SpawnFilter |= 16;
            }

            return true;
        }

        if (key.Equals("acsreturn", StringComparison.OrdinalIgnoreCase))
        {
            skill.AcsReturn = int.Parse(tokens[i++]);
            return true;
        }

        if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
        {
            skill.MenuName = tokens[i++].Trim('"');
            return true;
        }

        if (key.Equals("picname", StringComparison.OrdinalIgnoreCase))
        {
            skill.PicName = tokens[i++];
            return true;
        }

        if (key.Equals("key", StringComparison.OrdinalIgnoreCase))
        {
            var s = tokens[i++];
            skill.Shortcut = s.Length > 0 ? char.ToLowerInvariant(s[0]) : '\0';
            return true;
        }

        if (key.Equals("textcolor", StringComparison.OrdinalIgnoreCase))
        {
            skill.TextColor = $"[{tokens[i++]}]";
            return true;
        }

        if (key.Equals("monsterhealth", StringComparison.OrdinalIgnoreCase))
        {
            skill.MonsterHealth = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("friendlyhealth", StringComparison.OrdinalIgnoreCase))
        {
            skill.FriendlyHealth = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("armorfactor", StringComparison.OrdinalIgnoreCase))
        {
            skill.ArmorFactor = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("healthfactor", StringComparison.OrdinalIgnoreCase))
        {
            skill.HealthFactor = double.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (key.Equals("replaceactor", StringComparison.OrdinalIgnoreCase))
        {
            var replaced = tokens[i++];
            if (i >= tokens.Count || tokens[i] != ",") return false;
            i++;
            var replacer = tokens[i++];
            skill.Replacements[replaced] = replacer;
            return true;
        }

        return false;
    }

    public static bool TryApplyBareFlag(VmSkillInfo skill, string key)
    {
        if (key.Equals("mustconfirm", StringComparison.OrdinalIgnoreCase)) { skill.MustConfirm = true; return true; }
        if (key.Equals("nopain", StringComparison.OrdinalIgnoreCase)) { skill.NoPain = true; return true; }
        if (key.Equals("noinfighting", StringComparison.OrdinalIgnoreCase)) { skill.Infighting = 1; return true; }
        if (key.Equals("totalinfighting", StringComparison.OrdinalIgnoreCase)) { skill.Infighting = 2; return true; }
        if (key.Equals("defaultskill", StringComparison.OrdinalIgnoreCase)) return true;
        return TryApplyFlag(skill, key);
    }
}
