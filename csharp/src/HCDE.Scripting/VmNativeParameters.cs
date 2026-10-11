namespace HCDE.Scripting;

/// <summary>Parameter checks used by converted native callbacks.</summary>
public static class VmNativeParameters
{
    public static IReadOnlyList<VmValue> PackVariadicArguments(IReadOnlyList<VmValue> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Any(value => value is null)) throw new ArgumentException("Arguments cannot contain null entries.", nameof(arguments));
        var expanded = VmScriptFunction.ExpandArguments(arguments).ToList();
        if (expanded.Count >= 256) throw new ArgumentException("Variadic arguments require one hidden parameter slot.", nameof(arguments));
        if (expanded.Any(value => (int)value.Type is < 0 or > 3))
            throw new ArgumentException("Variadic metadata requires scalar register types.", nameof(arguments));
        var types = expanded.Select(value => (byte)value.Type).ToArray();
        expanded.Add(VmValue.FromObject(types));
        return expanded.AsReadOnly();
    }

    public static IReadOnlyList<VmValueType> ReadVariadicTypes(IReadOnlyList<VmValue> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0 || arguments[^1] is not { Type: VmValueType.Object, Object: byte[] metadata }
            || metadata.Length != arguments.Count - 1)
            throw new ArgumentException("Missing or invalid variadic type metadata.", nameof(arguments));
        var types = new VmValueType[metadata.Length];
        for (var i = 0; i < types.Length; i++)
        {
            if (metadata[i] > 3 || arguments[i] is null || (byte)arguments[i].Type != metadata[i])
                throw new ArgumentException("Variadic metadata does not match its argument bank.", nameof(arguments));
            types[i] = (VmValueType)metadata[i];
        }
        return Array.AsReadOnly(types);
    }

    private static object? ReadPointer(VmValue argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        if (argument.Type != VmValueType.Object)
            throw new ArgumentException("A pointer parameter requires an object register value.", nameof(argument));
        return argument.Object;
    }

    public static T? ReadObject<T>(VmValue argument) where T : class
    {
        if (!typeof(T).IsClass) throw new ArgumentException("An object parameter requires a class type.", nameof(T));
        var pointer = ReadPointer(argument);
        if (pointer is null) return null;
        if (pointer is Type || !pointer.GetType().IsClass || pointer is not T value)
            throw new ArgumentException("Object parameter does not derive from the required class.", nameof(argument));
        return value;
    }

    public static T RequireObject<T>(VmValue argument, string parameterName) where T : class
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        return ReadObject<T>(argument) ?? throw new VmAbortException(1, "In function parameter " + parameterName);
    }

    public static Type? ReadClass(VmValue argument, Type baseClass)
    {
        ArgumentNullException.ThrowIfNull(baseClass);
        if (!baseClass.IsClass) throw new ArgumentException("A class parameter requires a base class descriptor.", nameof(baseClass));
        var pointer = ReadPointer(argument);
        if (pointer is null) return null;
        if (pointer is not Type descriptor || !descriptor.IsClass || !baseClass.IsAssignableFrom(descriptor))
            throw new ArgumentException("Class parameter does not descend from the required base class.", nameof(argument));
        return descriptor;
    }

    public static Type RequireClass(VmValue argument, Type baseClass, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        return ReadClass(argument, baseClass) ?? throw new VmAbortException(1, "In function parameter " + parameterName);
    }

    /// <summary>Implements native PARAM_POINTER_NOT_NULL and its NullParam abort.</summary>
    public static object RequirePointer(VmValue argument, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(parameterName);
        return ReadPointer(argument) ?? throw new VmAbortException(1, "In function parameter " + parameterName);
    }
}
