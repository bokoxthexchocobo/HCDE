namespace HCDE.Scripting;

public readonly record struct VmStatementInfo(int InstructionIndex, int LineNumber);
public readonly record struct VmCallArgument(VmValue Value, string? Name = null);

/// <summary>An immutable bytecode function with explicitly declared parameters.</summary>
public sealed class VmScriptFunction
{
    internal int[] Code { get; }
    internal int[] Constants { get; }
    internal double[] FloatConstants { get; }
    internal string[] StringConstants { get; }
    internal object?[] ObjectConstants { get; }
    internal VmValueType[] ParameterTypes { get; }
    internal VmValueType[] DeclaredParameterTypes { get; }
    internal int FrameExtraSpace { get; }
    internal VmValue[] DefaultArguments { get; }
    internal string[]? ParameterNames { get; }
    private readonly VmStatementInfo[] _lineInfo;
    public string PrintableName { get; }
    public string SourceFileName { get; }

    public VmScriptFunction(int[] code, int[] constants, VmValueType[] parameterTypes,
        double[]? floatConstants = null, string[]? stringConstants = null,
        IReadOnlyList<object?>? objectConstants = null, int frameExtraSpace = 0,
        IReadOnlyList<VmValue>? trailingDefaults = null,
        IReadOnlyList<VmStatementInfo>? lineInfo = null,
        string printableName = "", string sourceFileName = "", IReadOnlyList<string>? parameterNames = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(parameterTypes);
        ArgumentNullException.ThrowIfNull(printableName);
        ArgumentNullException.ThrowIfNull(sourceFileName);
        PrintableName = printableName;
        SourceFileName = sourceFileName;
        ArgumentOutOfRangeException.ThrowIfNegative(frameExtraSpace);
        if (parameterTypes.Any(type => ((int)type < 0 || (int)type > 3) && type is not (VmValueType.Vector2 or VmValueType.Vector3 or VmValueType.Vector4)))
            throw new ArgumentException("Unsupported script parameter type.", nameof(parameterTypes));
        DeclaredParameterTypes = parameterTypes.ToArray();
        ParameterNames = parameterNames?.ToArray();
        if (ParameterNames is not null && (ParameterNames.Length != parameterTypes.Length
            || ParameterNames.Any(string.IsNullOrEmpty) || ParameterNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ParameterNames.Length))
            throw new ArgumentException("Parameter names must be nonempty, unique, and match the signature length.", nameof(parameterNames));
        ParameterTypes = parameterTypes.SelectMany(type => type switch
        {
            VmValueType.Vector2 => Enumerable.Repeat(VmValueType.Float, 2),
            VmValueType.Vector3 => Enumerable.Repeat(VmValueType.Float, 3),
            VmValueType.Vector4 => Enumerable.Repeat(VmValueType.Float, 4),
            _ => new[] { type },
        }).ToArray();
        if (ParameterTypes.Length > 256) throw new ArgumentException("Too many parameter slots.", nameof(parameterTypes));
        Code = code.ToArray(); Constants = constants.ToArray();
        _lineInfo = lineInfo?.ToArray() ?? Array.Empty<VmStatementInfo>();
        for (var i = 0; i < _lineInfo.Length; i++)
        {
            if (_lineInfo[i].InstructionIndex < 0 || (i > 0 && _lineInfo[i].InstructionIndex < _lineInfo[i - 1].InstructionIndex))
                throw new ArgumentException("Statement instruction indices must be nonnegative and ordered.", nameof(lineInfo));
        }
        FloatConstants = floatConstants?.ToArray() ?? Array.Empty<double>();
        StringConstants = stringConstants?.ToArray() ?? Array.Empty<string>();
        foreach (var value in StringConstants) ArgumentNullException.ThrowIfNull(value);
        ObjectConstants = objectConstants?.ToArray() ?? Array.Empty<object?>();
        FrameExtraSpace = frameExtraSpace;
        DefaultArguments = trailingDefaults?.ToArray() ?? Array.Empty<VmValue>();
        if (DefaultArguments.Length > DeclaredParameterTypes.Length) throw new ArgumentException("Too many default arguments.", nameof(trailingDefaults));
        for (var i = 0; i < DefaultArguments.Length; i++)
        {
            var value = DefaultArguments[i];
            if (value is null || value.Type != DeclaredParameterTypes[DeclaredParameterTypes.Length - DefaultArguments.Length + i])
                throw new ArgumentException("Default argument type must match its parameter.", nameof(trailingDefaults));
            if (value.Type == VmValueType.String) DefaultArguments[i] = VmValue.FromString(value.String!);
        }
    }

    /// <summary>Maps a bytecode position using the native statement-table lookup rules.</summary>
    public int GetSourceLine(int instructionIndex)
    {
        if (_lineInfo.Length == 1) return _lineInfo[0].LineNumber;
        for (var i = 1; i < _lineInfo.Length; i++)
            if (_lineInfo[i].InstructionIndex > instructionIndex) return _lineInfo[i - 1].LineNumber;
        // Native tables need a following boundary to resolve their final entry.
        return -1;
    }

    internal static IEnumerable<VmValue> ExpandArguments(IEnumerable<VmValue> arguments)
    {
        foreach (var value in arguments)
        {
            if (value.Type is not (VmValueType.Vector2 or VmValueType.Vector3 or VmValueType.Vector4)) { yield return value; continue; }
            yield return VmValue.FromFloat(value.Vector.X);
            yield return VmValue.FromFloat(value.Vector.Y);
            if (value.Vector.Components >= 3) yield return VmValue.FromFloat(value.Vector.Z);
            if (value.Vector.Components == 4) yield return VmValue.FromFloat(value.Vector.W);
        }
    }
}
