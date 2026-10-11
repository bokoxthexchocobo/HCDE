namespace HCDE.Scripting;

/// <summary>Initialized managed string fields keyed by their VM byte offsets.</summary>
public sealed class VmStringMemory
{
    private readonly Dictionary<int, string> _fields;

    public VmStringMemory(IReadOnlyDictionary<int, string> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        _fields = new Dictionary<int, string>();
        foreach (var field in fields)
        {
            if (field.Key < 0) throw new ArgumentOutOfRangeException(nameof(fields));
            ArgumentNullException.ThrowIfNull(field.Value);
            _fields.Add(field.Key, field.Value);
        }
    }

    public string this[int byteOffset] => _fields[byteOffset];

    internal bool TryRead(int byteOffset, out string value) => _fields.TryGetValue(byteOffset, out value!);

    internal bool TryWrite(int byteOffset, string value)
    {
        if (!_fields.ContainsKey(byteOffset)) return false;
        _fields[byteOffset] = value;
        return true;
    }
}
