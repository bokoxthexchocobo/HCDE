namespace HCDE.Scripting;

/// <summary>Managed reference fields keyed by their VM byte offsets.</summary>
public sealed class VmObjectMemory
{
    private readonly Dictionary<int, object?> _fields;

    public VmObjectMemory(IReadOnlyDictionary<int, object?> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        _fields = new Dictionary<int, object?>();
        foreach (var field in fields)
        {
            if (field.Key < 0) throw new ArgumentOutOfRangeException(nameof(fields));
            _fields.Add(field.Key, field.Value);
        }
    }

    public object? this[int byteOffset] => _fields[byteOffset];

    internal bool TryRead(int byteOffset, out object? value) => _fields.TryGetValue(byteOffset, out value);

    internal bool TryWrite(int byteOffset, object? value)
    {
        if (!_fields.ContainsKey(byteOffset)) return false;
        _fields[byteOffset] = value;
        return true;
    }
}
