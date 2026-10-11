namespace HCDE.Scripting;

/// <summary>A type-checked live view of one VM scalar register.</summary>
public sealed class VmRegisterReference
{
    private readonly Func<VmValue> _read;
    private readonly Action<VmValue> _write;
    public VmValueType Type { get; }

    internal VmRegisterReference(VmValueType type, Func<VmValue> read, Action<VmValue> write)
        => (Type, _read, _write) = (type, read, write);

    public VmValue Value
    {
        get => _read();
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Type != Type) throw new ArgumentException("Reference assignment must preserve the register type.", nameof(value));
            _write(value);
        }
    }
}
