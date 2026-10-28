namespace HCDE.Client;

/// <summary>Dictionary-backed <c>FindCVar</c> / <c>GetCVar</c> subset.</summary>
public sealed class VmCvarRegistry
{
    private readonly Dictionary<string, VmCvarEntry> map = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, VmTypedCvar> typed = new(StringComparer.OrdinalIgnoreCase);

    public void Register(VmCvarEntry entry) => map[entry.Name] = entry;

    public void RegisterTyped(VmTypedCvar cvar)
    {
        typed[cvar.Name] = cvar;
        Register(new VmCvarEntry(cvar.Name, cvar.Flags, FormatTyped(cvar), FormatTypedDefault(cvar)));
    }

    public VmTypedCvar? FindTyped(string? name) =>
        name is null ? null : typed.GetValueOrDefault(name);

    public VmTypedCvar CreateTyped(VmCvarValueType type, string name, VmCvarValue defaultValue, uint flags)
    {
        if (FindTyped(name) is not null) throw new InvalidOperationException($"cvar {name} already exists");
        VmTypedCvar cvar = type switch
        {
            VmCvarValueType.Bool => new VmBoolCvar(name, defaultValue.Bool, flags | VmCvarFlags.Auto | VmCvarFlags.IsDefault),
            VmCvarValueType.Int => new VmIntCvar(name, defaultValue.Int, flags | VmCvarFlags.Auto | VmCvarFlags.IsDefault),
            VmCvarValueType.Float => new VmFloatCvar(name, defaultValue.Float, flags | VmCvarFlags.Auto | VmCvarFlags.IsDefault),
            VmCvarValueType.String => new VmStringCvar(name, defaultValue.String, flags | VmCvarFlags.Auto | VmCvarFlags.IsDefault),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
        RegisterTyped(cvar);
        return cvar;
    }

    private static string FormatTyped(VmTypedCvar cvar) =>
        cvar.RealType switch
        {
            VmCvarValueType.Bool => ((VmBoolCvar)cvar).Value ? "1" : "0",
            VmCvarValueType.Int => ((VmIntCvar)cvar).Value.ToString(),
            VmCvarValueType.Float => ((VmFloatCvar)cvar).Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            VmCvarValueType.String => ((VmStringCvar)cvar).Value,
            _ => "",
        };

    private static string FormatTypedDefault(VmTypedCvar cvar) =>
        cvar.RealType switch
        {
            VmCvarValueType.Bool => ((VmBoolCvar)cvar).DefaultValue ? "1" : "0",
            VmCvarValueType.Int => ((VmIntCvar)cvar).DefaultValue.ToString(),
            VmCvarValueType.Float => ((VmFloatCvar)cvar).DefaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            VmCvarValueType.String => ((VmStringCvar)cvar).DefaultValue,
            _ => "",
        };

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

        foreach (var key in typed.Keys.ToList())
        {
            if ((typed[key].Flags & flags) != 0) typed.Remove(key);
        }
    }

    public void SyncTypedEntry(VmTypedCvar cvar)
    {
        if (map.TryGetValue(cvar.Name, out var entry))
            entry.Value = FormatTyped(cvar);
    }
}
