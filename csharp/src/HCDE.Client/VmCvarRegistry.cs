namespace HCDE.Client;

/// <summary>Dictionary-backed <c>FindCVar</c> / <c>GetCVar</c> subset.</summary>
public sealed class VmCvarRegistry
{
    private readonly Dictionary<string, VmCvarEntry> map = new(StringComparer.OrdinalIgnoreCase);

    public void Register(VmCvarEntry entry) => map[entry.Name] = entry;

    public VmCvarEntry? Find(string? name) =>
        name is null ? null : map.GetValueOrDefault(name);

    public VmCvarEntry? FindSub(string name, int length) =>
        length <= 0 || length > name.Length ? null : Find(name[..length]);

    public VmCvarEntry? GetCVar(int playernum, string name, Func<int, string, VmCvarEntry?>? getUserCVar = null)
    {
        var cvar = Find(name);
        if (cvar is null || (cvar.Flags & VmCvarFlags.Ignore) != 0) return null;
        if ((cvar.Flags & VmCvarFlags.UserInfo) != 0 && getUserCVar is not null)
            return getUserCVar(playernum, name);
        return cvar;
    }

    public IEnumerable<VmCvarEntry> All => map.Values;

    public VmCvarEntry Create(string name, string value, uint flags)
    {
        if (Find(name) is not null) throw new InvalidOperationException($"cvar {name} already exists");
        var entry = new VmCvarEntry(name, flags | VmCvarFlags.Auto, value);
        Register(entry);
        return entry;
    }

    public void SetArchiveDefaults()
    {
        foreach (var cvar in map.Values)
        {
            if ((cvar.Flags & VmCvarFlags.Archive) == 0) continue;
            if (cvar.DefaultValue is not null) cvar.Value = cvar.DefaultValue;
        }
    }

    public void DestroyFlagged(uint flags)
    {
        foreach (var key in map.Keys.ToList())
        {
            if ((map[key].Flags & flags) != 0) map.Remove(key);
        }
    }
}
