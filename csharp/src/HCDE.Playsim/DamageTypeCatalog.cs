namespace HCDE.Playsim;

public readonly record struct DamageTypeFactor(double Factor, bool ReplaceFactor, bool NoArmor = false);

/// <summary>Simulation-local subset of native global DamageTypeDefinition factors.</summary>
public sealed class DamageTypeCatalog
{
    public bool TryLoadMapInfo(string text, out string? error)
    {
        if (!HCDE.Gamedata.MapInfoParser.TryParse(text, out var info, out error)) return false;
        foreach (var definition in info.DamageTypes)
            Define(definition.Name, definition.Factor, definition.ReplaceFactor, definition.NoArmor);
        return true;
    }
    private readonly Dictionary<string, DamageTypeFactor> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public DamageTypeCatalog()
    {
        // Built-in common MAPINFO damage definition.
        Define("Drowning", 1, noArmor: true);
    }

    public void Define(string damageType, double factor = 1, bool replaceFactor = false, bool noArmor = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(damageType);
        if (!double.IsFinite(factor)) throw new ArgumentOutOfRangeException(nameof(factor));
        _definitions[damageType] = new DamageTypeFactor(factor, replaceFactor, noArmor);
    }

    internal Dictionary<string, DamageTypeFactor> Capture() => new(_definitions, StringComparer.OrdinalIgnoreCase);

    internal void Restore(IReadOnlyDictionary<string, DamageTypeFactor> definitions)
    {
        _definitions.Clear();
        foreach (var entry in definitions) Define(entry.Key, entry.Value.Factor, entry.Value.ReplaceFactor, entry.Value.NoArmor);
    }

    internal DamageTypeFactor? Find(string damageType) =>
        _definitions.TryGetValue(damageType, out var definition) ? definition : null;

    internal uint MixChecksum(uint hash)
    {
        var entries = _definitions.Where(pair => !(pair.Key.Equals("Drowning", StringComparison.OrdinalIgnoreCase)
            && pair.Value == new DamageTypeFactor(1, false, true)))
            .OrderBy(pair => pair.Key.ToUpperInvariant(), StringComparer.Ordinal).ToArray();
        if (entries.Length == 0) return hash;
        hash = DamageRuleChecksum.Mix(hash, 0x44544341u);
        hash = DamageRuleChecksum.Mix(hash, (uint)entries.Length);
        foreach (var entry in entries)
        {
            hash = DamageRuleChecksum.Entry(hash, entry.Key, entry.Value.Factor);
            hash = DamageRuleChecksum.Mix(hash, entry.Value.ReplaceFactor ? 1u : 0u);
            hash = DamageRuleChecksum.Mix(hash, entry.Value.NoArmor ? 1u : 0u);
        }
        return hash;
    }
}
