namespace HCDE.Playsim;

/// <summary>Test and mod hooks for native <c>DLevelScript::ScriptCall</c> static ZScript entry points.</summary>
public sealed class AcsScriptCallRegistry
{
    private readonly Dictionary<string, AcsScriptCallBinding> _bindings = new(StringComparer.Ordinal);

    public void Register(AcsScriptCallBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        _bindings[Key(binding.ClassName, binding.FunctionName)] = binding;
    }

    internal bool TryGet(string className, string functionName, out AcsScriptCallBinding binding) =>
        _bindings.TryGetValue(Key(className, functionName), out binding!);

    private static string Key(string className, string functionName) => $"{className}\0{functionName}";
}

public enum AcsScriptValueKind
{
    Void,
    Int32,
    Bool,
    Float64,
    Name,
    String,
    Sound,
}

public sealed class AcsScriptCallBinding
{
    public required string ClassName { get; init; }
    public required string FunctionName { get; init; }
    public bool LeadingActorParameter { get; init; }
    public AcsScriptValueKind[] ParameterTypes { get; init; } = [];
    public bool[] OptionalParameters { get; init; } = [];
    public AcsScriptValueKind ReturnType { get; init; } = AcsScriptValueKind.Void;
    public required Func<AcsScriptCallInvocation, AcsScriptCallOutcome> Invoke { get; init; }
}

public readonly record struct AcsScriptCallInvocation(
    Actor? Activator,
    IReadOnlyList<AcsScriptCallArgument> Arguments,
    string[] StringTable);

public readonly record struct AcsScriptCallArgument(AcsScriptValueKind Kind, int IntValue, string? Text);

public readonly record struct AcsScriptCallOutcome(int IntResult = 0, string? StringResult = null, double? DoubleResult = null);
