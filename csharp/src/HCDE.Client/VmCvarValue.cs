namespace HCDE.Client;

public enum VmCvarValueType
{
    Bool,
    Int,
    Float,
    String,
    Color,
}

public readonly record struct VmCvarValue(bool Bool = false, int Int = 0, double Float = 0, string? String = null)
{
    public static VmCvarValue FromBool(bool value) => new(Bool: value);
    public static VmCvarValue FromInt(int value) => new(Int: value);
}
