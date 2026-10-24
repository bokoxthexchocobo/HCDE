namespace HCDE.Playsim;

/// <summary>Native <c>GlobalACSStrings</c> pool for CallFunc results such as <c>GetActorClass</c>.</summary>
internal sealed class AcsGlobalStrings
{
    private readonly List<string> _entries = new();
    private readonly Dictionary<string, int> _indices = new(StringComparer.Ordinal);

    public int Add(string value)
    {
        value ??= string.Empty;
        if (_indices.TryGetValue(value, out var existing))
            return AcsStringIds.FromGlobalIndex(existing);
        var index = _entries.Count;
        _entries.Add(value);
        _indices.Add(value, index);
        return AcsStringIds.FromGlobalIndex(index);
    }

    public string Get(int stringId) =>
        AcsStringIds.IsGlobalPool(stringId) ? GetByIndex(AcsStringIds.GlobalIndex(stringId)) : string.Empty;

    public string GetByIndex(int index) =>
        index >= 0 && index < _entries.Count ? _entries[index] : string.Empty;

    internal IReadOnlyList<string> Snapshot() => _entries.ToArray();

    internal void ReplaceFrom(IReadOnlyList<string> entries)
    {
        _entries.Clear();
        _indices.Clear();
        foreach (var entry in entries)
            Add(entry ?? string.Empty);
    }
}
