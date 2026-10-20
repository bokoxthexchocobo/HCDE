namespace HCDE.Client;

public sealed class VmCvarEntry(string name, uint flags, string value, string? defaultValue = null)
{
    public string Name { get; } = name;
    public uint Flags { get; } = flags;
    public string Value { get; set; } = value;
    public string? DefaultValue { get; } = defaultValue;

    public bool IsDefault => (Flags & VmCvarFlags.IsDefault) != 0;

    public string ArchiveValue => IsDefault && DefaultValue is not null ? DefaultValue : Value;
}
