namespace HCDE.Gamedata;

/// <summary>
/// Global damage types, <c>GetMobjDamageFactor</c>, and <c>ParseDamageDefinition</c> from <c>info.cpp</c> (**894–1043**).
/// Chunk **1039**; pivot from **983–1132** (EOF **1043**, **61** lines sub-150). Follows **1038** (**650–799**).
/// </summary>
public static class VmInfoDamageType1039Slice
{
    public readonly record struct GlobalDamageTypeDraft(
        double DefaultFactor,
        bool ReplaceFactor,
        bool NoArmor,
        string Obituary);

    public enum ParseDamageProperty
    {
        Factor,
        Obituary,
        ReplaceFactor,
        NoArmor,
        Unexpected
    }

    public static void RegisterGlobalDamageType(
        IDictionary<string, GlobalDamageTypeDraft> table,
        string damageType,
        GlobalDamageTypeDraft draft) =>
        table[damageType] = draft;

    public static bool GlobalIgnoresArmor(IReadOnlyDictionary<string, GlobalDamageTypeDraft> table, string damageType) =>
        table.TryGetValue(damageType, out var d) && d.NoArmor;

    public static string ResolveGlobalObituary(IReadOnlyDictionary<string, GlobalDamageTypeDraft> table, string damageType) =>
        table.TryGetValue(damageType, out var d) ? d.Obituary : "";

    public static double ResolveMobjDamageFactor(
        string damageType,
        IReadOnlyList<(string Type, double Factor)>? actorFactors,
        IReadOnlyDictionary<string, GlobalDamageTypeDraft> global)
    {
        var defaultUntyped = -1.0;
        if (actorFactors != null)
        {
            foreach (var (type, factor) in actorFactors)
            {
                if (type.Equals(damageType, StringComparison.OrdinalIgnoreCase))
                    return factor;
                if (type.Equals("None", StringComparison.OrdinalIgnoreCase))
                    defaultUntyped = factor;
            }

            if (damageType.Equals("None", StringComparison.OrdinalIgnoreCase))
                return 1.0;
        }
        else if (damageType.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }
        else if (global.TryGetValue(damageType, out var loneGlobal))
        {
            return loneGlobal.DefaultFactor;
        }
        else
        {
            return 1.0;
        }

        global.TryGetValue(damageType, out var dtd);
        if (defaultUntyped >= 0.0)
        {
            if (global.ContainsKey(damageType))
            {
                if (dtd.ReplaceFactor)
                    return dtd.DefaultFactor;
                return defaultUntyped * dtd.DefaultFactor;
            }

            return defaultUntyped;
        }

        if (global.ContainsKey(damageType))
            return dtd.DefaultFactor;
        return 1.0;
    }

    public static int ApplyMobjDamageFactor(
        int damage,
        string damageType,
        IReadOnlyList<(string Type, double Factor)>? actorFactors,
        IReadOnlyDictionary<string, GlobalDamageTypeDraft> global)
    {
        var factor = ResolveMobjDamageFactor(damageType, actorFactors, global);
        return (int)(damage * factor);
    }

    public static ParseDamageProperty ClassifyDamageDefinitionProperty(string token)
    {
        if (token.Equals("FACTOR", StringComparison.OrdinalIgnoreCase))
            return ParseDamageProperty.Factor;
        if (token.Equals("OBITUARY", StringComparison.OrdinalIgnoreCase))
            return ParseDamageProperty.Obituary;
        if (token.Equals("REPLACEFACTOR", StringComparison.OrdinalIgnoreCase))
            return ParseDamageProperty.ReplaceFactor;
        if (token.Equals("NOARMOR", StringComparison.OrdinalIgnoreCase))
            return ParseDamageProperty.NoArmor;
        return ParseDamageProperty.Unexpected;
    }

    public static GlobalDamageTypeDraft ApplyFactorProperty(GlobalDamageTypeDraft draft, double value)
    {
        draft = draft with { DefaultFactor = value };
        return value == 0 ? draft with { ReplaceFactor = true } : draft;
    }

    public static GlobalDamageTypeDraft ApplyReplaceFactorFlag(GlobalDamageTypeDraft draft) =>
        draft with { ReplaceFactor = true };

    public static GlobalDamageTypeDraft ApplyNoArmorFlag(GlobalDamageTypeDraft draft) =>
        draft with { NoArmor = true };

    public static GlobalDamageTypeDraft ApplyObituaryProperty(GlobalDamageTypeDraft draft, string obituary) =>
        draft with { Obituary = obituary };
}
