namespace HCDE.Scripting;

public enum VmValueType { Integer, Float, String, Object, Vector2 = 9, Vector3 = 17, Vector4 = 65, Nil = 128 }

/// <summary>A typed argument or callback result.</summary>
public sealed class VmValue
{
    public VmValueType Type { get; }
    public int Integer { get; }
    public double Float { get; }
    private readonly string? _text;
    private readonly Func<string>? _readString;
    public string? String => _readString is null ? _text : _readString();
    public object? Object { get; }
    public VmVectorResult Vector { get; }

    private VmValue(VmValueType type, int integer = 0, double floating = 0, string? text = null, object? reference = null,
        VmVectorResult vector = default, Func<string>? readString = null)
        => (Type, Integer, Float, _text, Object, Vector, _readString) = (type, integer, floating, text, reference, vector, readString);

    public static VmValue FromInteger(int value) => new(VmValueType.Integer, integer: value);
    public static VmValue FromFloat(double value) => new(VmValueType.Float, floating: value);
    public static VmValue FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(VmValueType.String, text: value);
    }
    public static VmValue FromObject(object? value) => new(VmValueType.Object, reference: value);
    internal static VmValue FromStringRegister(Func<string> read) => new(VmValueType.String, readString: read);
    public static VmValue Nil() => new(VmValueType.Nil);
    public static VmValue FromVector2(double x, double y) => new(VmValueType.Vector2, vector: new(2, x, y, 0, 0));
    public static VmValue FromVector3(double x, double y, double z) => new(VmValueType.Vector3, vector: new(3, x, y, z, 0));
    public static VmValue FromVector4(double x, double y, double z, double w) => new(VmValueType.Vector4, vector: new(4, x, y, z, w));
}

/// <summary>A managed callback with typed VM arguments and results.</summary>
public sealed class VmFunction
{
    private readonly Func<IReadOnlyList<VmValue>, int, IReadOnlyList<VmValue>> _callback;
    public string PrintableName { get; }
    internal VmValue[] DefaultArguments { get; }
    public VmNativePrototype? Prototype { get; }

    public VmFunction(Func<IReadOnlyList<VmValue>, int, IReadOnlyList<VmValue>> callback, string printableName = "",
        IReadOnlyList<VmValue>? defaultArguments = null, VmNativePrototype? prototype = null)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(printableName);
        PrintableName = printableName;
        _callback = callback;
        if (defaultArguments is not null && defaultArguments.Any(value => value is null))
            throw new ArgumentException("Default arguments cannot contain null entries.", nameof(defaultArguments));
        DefaultArguments = VmScriptFunction.ExpandArguments(defaultArguments ?? Array.Empty<VmValue>())
            .Select(value => value.Type == VmValueType.String ? VmValue.FromString(value.String!) : value).ToArray();
        if (DefaultArguments.Length > 256) throw new ArgumentException("Too many default argument slots.", nameof(defaultArguments));
        Prototype = prototype;
        if (prototype is not null && !prototype.AcceptsDefaults(DefaultArguments))
            throw new ArgumentException("Default argument types must match the prototype.", nameof(defaultArguments));
    }

    internal IReadOnlyList<VmValue> Invoke(IReadOnlyList<VmValue> parameters, int requestedReturns)
        => _callback(parameters, requestedReturns);
}
