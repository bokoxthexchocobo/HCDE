namespace HCDE.Gamedata;

public sealed class VmSkillInfo
{
    public string Name { get; set; } = "";
    public double AmmoFactor { get; set; } = 1;
    public double DamageFactor { get; set; } = 1;
    public bool FastMonsters { get; set; }
    public bool SlowMonsters { get; set; }
    public bool DisableCheats { get; set; }
    public int AcsReturn { get; set; }
}

/// <summary><c>FMapInfoParser::ParseSkill</c> subset from <c>g_skill.cpp</c>.</summary>
public static class VmSkillMapinfoParser
{
    public static VmSkillInfo CreateDefaultSkill(string name) => new()
    {
        Name = name,
        AmmoFactor = 1,
        DamageFactor = 1
    };

    public static bool TryApplyOption(VmSkillInfo skill, string key, IReadOnlyList<string> tokens, ref int i)
    {
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

        if (key.Equals("fastmonsters", StringComparison.OrdinalIgnoreCase))
        {
            skill.FastMonsters = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        if (key.Equals("slowmonsters", StringComparison.OrdinalIgnoreCase))
        {
            skill.SlowMonsters = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        if (key.Equals("disablecheats", StringComparison.OrdinalIgnoreCase))
        {
            skill.DisableCheats = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        if (key.Equals("acsreturn", StringComparison.OrdinalIgnoreCase))
        {
            skill.AcsReturn = int.Parse(tokens[i++]);
            return true;
        }

        return false;
    }
}
