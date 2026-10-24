namespace HCDE.Gamedata;

public sealed class VmDamageTypeDefinition
{
    public double DefaultFactor { get; set; } = 1;
    public bool ReplaceFactor { get; set; }
    public bool NoArmor { get; set; }
    public string Obituary { get; set; } = "";
}

/// <summary><c>DmgFactors</c> / <c>DamageTypeDefinition</c> from <c>info.cpp</c>.</summary>
public static class VmDamageTypeDefinitionRuntime
{
    private static readonly Dictionary<string, VmDamageTypeDefinition> Global = new(StringComparer.OrdinalIgnoreCase);

    public static void Apply(string damageType, VmDamageTypeDefinition def) => Global[damageType] = def;

    public static int ApplyActorFactors(IReadOnlyDictionary<string, double> factors, string damageType, int damage)
    {
        if (factors.TryGetValue(damageType, out var specific)) return (int)(damage * specific);
        if (factors.TryGetValue("None", out var fallback)) return (int)(damage * fallback);
        return damage;
    }

    public static double GetMobjDamageFactor(string damageType, IReadOnlyDictionary<string, double>? factors)
    {
        if (factors is not null)
        {
            foreach (var (type, factor) in factors)
            {
                if (type.Equals(damageType, StringComparison.OrdinalIgnoreCase)) return factor;
                if (type.Equals("None", StringComparison.OrdinalIgnoreCase)) return ResolveWithGlobal(damageType, factor);
            }

            if (damageType.Equals("None", StringComparison.OrdinalIgnoreCase)) return 1;
        }
        else if (damageType.Equals("None", StringComparison.OrdinalIgnoreCase))
            return 1;

        return Global.TryGetValue(damageType, out var g) ? g.DefaultFactor : 1;
    }

    private static double ResolveWithGlobal(string damageType, double untypedFactor)
    {
        if (!Global.TryGetValue(damageType, out var g)) return untypedFactor;
        if (g.ReplaceFactor) return g.DefaultFactor;
        return untypedFactor * g.DefaultFactor;
    }

    public static bool IgnoreArmor(string damageType) =>
        Global.TryGetValue(damageType, out var g) && g.NoArmor;

    public static bool TryParseDamageDefinition(IReadOnlyList<string> tokens, ref int i, VmDamageTypeDefinition def)
    {
        if (i >= tokens.Count) return false;
        var key = tokens[i++];
        if (key.Equals("FACTOR", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count && tokens[i] == "=")
        {
            def.DefaultFactor = double.Parse(tokens[i + 1], System.Globalization.CultureInfo.InvariantCulture);
            if (def.DefaultFactor == 0) def.ReplaceFactor = true;
            i += 2;
            return true;
        }

        if (key.Equals("NOARMOR", StringComparison.OrdinalIgnoreCase)) { def.NoArmor = true; return true; }
        if (key.Equals("REPLACEFACTOR", StringComparison.OrdinalIgnoreCase)) { def.ReplaceFactor = true; return true; }
        return false;
    }
}
