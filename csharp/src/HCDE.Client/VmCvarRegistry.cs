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
}
