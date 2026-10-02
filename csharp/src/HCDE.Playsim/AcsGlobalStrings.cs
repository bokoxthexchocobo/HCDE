namespace HCDE.Playsim;

/// <summary>Native <c>GlobalACSStrings</c> pool for CallFunc results such as <c>GetActorClass</c>.</summary>
internal sealed class AcsGlobalStrings
{
    private readonly List<string> _entries = new();

    public int Add(string value)
    {
        var index = _entries.Count;
        _entries.Add(value ?? string.Empty);
        return AcsStringIds.FromGlobalIndex(index);
    }

    public string Get(int stringId) =>
        AcsStringIds.IsGlobalPool(stringId) ? GetByIndex(AcsStringIds.GlobalIndex(stringId)) : string.Empty;

    public string GetByIndex(int index) =>
        index >= 0 && index < _entries.Count ? _entries[index] : string.Empty;
}
