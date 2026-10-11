namespace HCDE.Scripting;

/// <summary>The flattened register types for a fixed native callback signature.</summary>
public sealed class VmNativePrototype
{
    private readonly VmValueType[] _types;
    private readonly VmValueType?[] _referenceTypes;

    public VmNativePrototype(IReadOnlyList<VmValueType> parameterTypes, IReadOnlyList<bool>? outParameters = null)
    {
        ArgumentNullException.ThrowIfNull(parameterTypes);
        if (parameterTypes.Any(type => (int)type is < 0 or > 3 && type is not (VmValueType.Vector2 or VmValueType.Vector3 or VmValueType.Vector4)))
            throw new ArgumentException("Unsupported native parameter type.", nameof(parameterTypes));
        if (outParameters is not null && outParameters.Count > parameterTypes.Count)
            throw new ArgumentException("Too many parameter flags.", nameof(outParameters));
        var slots = new List<VmValueType>();
        var references = new List<VmValueType?>();
        for (var i = 0; i < parameterTypes.Count; i++)
        {
            var type = parameterTypes[i];
            var byReference = outParameters is not null && i < outParameters.Count && outParameters[i];
            if (byReference && type is VmValueType.Vector2 or VmValueType.Vector3 or VmValueType.Vector4)
                throw new ArgumentException("Vector references require an unsupported native memory view.", nameof(outParameters));
            if (byReference && type != VmValueType.Object)
            {
                slots.Add(VmValueType.Object);
                references.Add(type);
                continue;
            }
            var count = type == VmValueType.Vector2 ? 2 : type == VmValueType.Vector3 ? 3 : type == VmValueType.Vector4 ? 4 : 1;
            for (var component = 0; component < count; component++)
            {
                slots.Add(count == 1 ? type : VmValueType.Float);
                references.Add(null);
            }
        }
        _types = slots.ToArray();
        _referenceTypes = references.ToArray();
        if (_types.Length > 256) throw new ArgumentException("Too many parameter slots.", nameof(parameterTypes));
    }

    internal VmFault Validate(IReadOnlyList<VmValue> arguments)
    {
        if (arguments.Count != _types.Length) return VmFault.ParameterRange;
        for (var i = 0; i < _types.Length; i++)
            if (!Matches(arguments[i], i)) return VmFault.InvalidCall;
        return VmFault.None;
    }

    internal bool AcceptsDefaults(IReadOnlyList<VmValue> arguments) => arguments.Count <= _types.Length
        && arguments.Select((value, index) => Matches(value, index)).All(matches => matches);

    private bool Matches(VmValue argument, int index) => argument.Type == _types[index]
        && (_referenceTypes[index] is not { } required || argument.Object is VmRegisterReference reference && reference.Type == required);
}
