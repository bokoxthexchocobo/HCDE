namespace HCDE.Scripting;

/// <summary>
/// Numeric, string and managed-object slice of <c>src/common/scripting/vm/vmexec.cpp</c>.
/// Words match the little-endian <c>VMOP</c> layout. Raw native memory and the JIT
/// are not executed. Managed callbacks and nested bytecode calls are supported.
/// An unknown opcode stops the function.
/// </summary>
public static class VmExec
{
    public const int Budget = 256;

    /// <summary>Binds named arguments with the ordering rules for the requested script version.</summary>
    public static VmMultiResult RunNamedFunction(VmScriptFunction function, IReadOnlyList<VmCallArgument> arguments,
        int requestedReturns = 1, VmColorCatalog? colorCatalog = null, VmClassMetadata? classMetadata = null,
        Version? scriptVersion = null)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        var count = function.DeclaredParameterTypes.Length;
        var required = count - function.DefaultArguments.Length;
        var relaxed = scriptVersion is null || scriptVersion >= new Version(4, 13);
        if (arguments.Count > count) return new(false, VmFault.ParameterRange, Array.Empty<VmResult>());
        var ordered = new VmValue?[count];
        var index = 0;
        foreach (var argument in arguments)
        {
            if (argument.Value is null) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
            if (argument.Name is { } name)
            {
                if (function.ParameterNames is null) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
                var target = Array.FindIndex(function.ParameterNames, candidate => StringComparer.OrdinalIgnoreCase.Equals(candidate, name));
                if (target < 0) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
                if (!relaxed && (target < required || target < index)) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
                index = target;
            }
            if (index >= count) return new(false, VmFault.ParameterRange, Array.Empty<VmResult>());
            // Native OrderedArgs assigns the slot again when a name is repeated.
            ordered[index++] = argument.Value;
        }
        var complete = new VmValue[count];
        for (var i = 0; i < count; i++)
        {
            if (ordered[i] is { } supplied) complete[i] = supplied;
            else if (i >= required) complete[i] = function.DefaultArguments[i - required];
            else return new(false, VmFault.ParameterRange, Array.Empty<VmResult>());
        }
        return RunFunction(function, complete, requestedReturns, colorCatalog, classMetadata);
    }

    public static VmMultiResult RunNativeFunctionWithDefaults(VmFunction function, IReadOnlyList<VmValue> arguments, int requestedReturns = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        if (arguments.Any(value => value is null)) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
        var complete = VmScriptFunction.ExpandArguments(arguments).ToList();
        // Native DefaultArgs is indexed by the complete flattened parameter position.
        for (var i = complete.Count; i < function.DefaultArguments.Length; i++) complete.Add(function.DefaultArguments[i]);
        return RunNativeFunction(function, complete, requestedReturns);
    }

    public static VmMultiResult RunNativeFunctionWithDefaults(VmIntegerFunction function, IReadOnlyList<int> arguments, int requestedReturns = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        var complete = arguments.ToList();
        for (var i = complete.Count; i < function.DefaultArguments.Length; i++) complete.Add(function.DefaultArguments[i]);
        return RunNativeFunction(function, complete, requestedReturns);
    }

    public static VmMultiResult RunNativeFunction(VmFunction function, IReadOnlyList<VmValue> arguments, int requestedReturns = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        if (arguments.Any(value => value is null)) return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
        var expanded = VmScriptFunction.ExpandArguments(arguments).ToArray();
        if (expanded.Length > 256) return new(false, VmFault.ParameterRange, Array.Empty<VmResult>());
        var argumentFault = function.Prototype?.Validate(expanded) ?? VmFault.None;
        if (argumentFault != VmFault.None) return new(false, argumentFault, Array.Empty<VmResult>());
        return NativeResults(function.Invoke(Array.AsReadOnly(expanded), requestedReturns), requestedReturns);
    }

    public static VmMultiResult RunNativeFunction(VmIntegerFunction function, IReadOnlyList<int> arguments, int requestedReturns = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        if (arguments.Count > 256) return new(false, VmFault.ParameterRange, Array.Empty<VmResult>());
        var values = function.Invoke(Array.AsReadOnly(arguments.ToArray()), requestedReturns);
        return NativeResults(values?.Select(VmValue.FromInteger).ToArray(), requestedReturns);
    }

    private static VmMultiResult NativeResults(IReadOnlyList<VmValue>? values, int requestedReturns)
    {
        if (values is null || values.Count > requestedReturns || values.Any(value => value is null || value.Type == VmValueType.Nil))
            return new(false, VmFault.InvalidCall, Array.Empty<VmResult>());
        var results = values.Select(value => value.Type switch
        {
            VmValueType.Integer => new VmResult(true, value.Integer, VmFault.None),
            VmValueType.Float => new VmResult(true, 0, VmFault.None) { FloatValue = value.Float },
            VmValueType.String => new VmResult(true, 0, VmFault.None) { StringValue = value.String },
            VmValueType.Object => new VmResult(true, 0, VmFault.None) { HasObjectValue = true, ObjectValue = value.Object },
            _ => new VmResult(true, 0, VmFault.None) { VectorValue = value.Vector },
        }).ToArray();
        return new(true, VmFault.None, Array.AsReadOnly(results));
    }

    public static VmMultiResult RunFunction(VmScriptFunction function, IReadOnlyList<VmValue> arguments,
        int requestedReturns = 1, VmColorCatalog? colorCatalog = null, VmClassMetadata? classMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        var required = function.DeclaredParameterTypes.Length - function.DefaultArguments.Length;
        if (arguments.Count < required || arguments.Count > function.DeclaredParameterTypes.Length)
            return new VmMultiResult(false, VmFault.ParameterRange, Array.Empty<VmResult>());
        var complete = new VmValue[function.DeclaredParameterTypes.Length];
        for (var i = 0; i < complete.Length; i++)
        {
            var value = i < arguments.Count ? arguments[i] : function.DefaultArguments[i - required];
            if (value is null || value.Type != function.DeclaredParameterTypes[i])
                return new VmMultiResult(false, VmFault.InvalidCall, Array.Empty<VmResult>());
            complete[i] = value;
        }
        var slots = new ReturnSlots(requestedReturns);
        var result = RunCore(function.Code, function.Constants, function.FloatConstants, function.StringConstants,
            slots, colorCatalog, function.ObjectConstants, function.FrameExtraSpace, classMetadata,
            initialArguments: VmScriptFunction.ExpandArguments(complete).ToArray(), scriptFunction: function);
        return new VmMultiResult(result.Returned, result.Fault,
            Array.AsReadOnly(result.Returned ? slots.Values[..slots.Count] : Array.Empty<VmResult>())) { AbortCode = result.AbortCode, AbortFrames = result.AbortFrames, AbortContext = result.AbortContext };
    }
    private const int Nil = 128;
    private const double EqualEpsilon = 1.0 / 65536.0;
    private sealed class ExecutionState
    {
        public int Steps;
        public int Depth;
    }

    private static bool InvokeCallback<T>(Func<T> invoke, string name, out T result, out VmResult abort)
    {
        try
        {
            result = invoke();
            abort = default;
            return true;
        }
        catch (VmAbortException exception)
        {
            result = default!;
            abort = new VmResult(false, 0, VmFault.ScriptAbort)
            {
                AbortCode = exception.Reason,
                AbortContext = exception.Context,
                AbortFrames = Array.AsReadOnly(new[] { new VmAbortFrame(name, "", -1, -1) { IsNative = true } }),
            };
            return false;
        }
    }

    private static object? ResolveMemory(object? value, ref int offset, out VmFault fault)
    {
        fault = VmFault.None;
        if (value is not VmMemoryPointer pointer) return value;
        if (offset > 0 && pointer.ByteOffset > long.MaxValue - offset || offset < 0 && pointer.ByteOffset < long.MinValue - offset)
        {
            fault = VmFault.MemoryRange;
            return pointer.Memory;
        }
        var combined = pointer.ByteOffset + offset;
        if (combined < 0 || combined > int.MaxValue) fault = VmFault.MemoryRange;
        else offset = (int)combined;
        return pointer.Memory;
    }

    private static bool SameAddress(object? left, object? right)
    {
        var leftMemory = left is VmMemoryPointer leftPointer ? leftPointer.Memory : left;
        var rightMemory = right is VmMemoryPointer rightPointer ? rightPointer.Memory : right;
        var leftOffset = left is VmMemoryPointer leftPosition ? leftPosition.ByteOffset : 0;
        var rightOffset = right is VmMemoryPointer rightPosition ? rightPosition.ByteOffset : 0;
        return ReferenceEquals(leftMemory, rightMemory) && leftOffset == rightOffset;
    }

    private static VmValue ToCallbackValue(VmResult result)
    {
        if (result.HasObjectValue) return VmValue.FromObject(result.ObjectValue);
        if (result.StringValue is not null) return VmValue.FromString(result.StringValue);
        if (result.VectorValue is { } vector) return vector.Components switch
        {
            2 => VmValue.FromVector2(vector.X, vector.Y),
            3 => VmValue.FromVector3(vector.X, vector.Y, vector.Z),
            _ => VmValue.FromVector4(vector.X, vector.Y, vector.Z, vector.W),
        };
        if (result.FloatValue is { } floating) return VmValue.FromFloat(floating);
        return VmValue.FromInteger(result.Value);
    }

    public static VmResult Run(ReadOnlySpan<int> code, ReadOnlySpan<int> constants)
        => Run(code, constants, ReadOnlySpan<double>.Empty);

    public static VmResult Run(ReadOnlySpan<int> code, ReadOnlySpan<int> constants, ReadOnlySpan<double> floatConstants)
        => Run(code, constants, floatConstants, ReadOnlySpan<string>.Empty);

    public static VmResult Run(ReadOnlySpan<int> code, ReadOnlySpan<int> constants, ReadOnlySpan<double> floatConstants,
        ReadOnlySpan<string> stringConstants, VmColorCatalog? colorCatalog = null, IReadOnlyList<object?>? objectConstants = null,
        int frameExtraSpace = 0, VmClassMetadata? classMetadata = null)
        => RunCore(code, constants, floatConstants, stringConstants, new ReturnSlots(1), colorCatalog, objectConstants, frameExtraSpace, classMetadata);

    public static VmMultiResult RunWithReturns(ReadOnlySpan<int> code, ReadOnlySpan<int> constants,
        ReadOnlySpan<double> floatConstants, ReadOnlySpan<string> stringConstants, int requestedReturns, VmColorCatalog? colorCatalog = null,
        IReadOnlyList<object?>? objectConstants = null, int frameExtraSpace = 0, VmClassMetadata? classMetadata = null)
    {
        if (requestedReturns is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(requestedReturns));
        var slots = new ReturnSlots(requestedReturns);
        var result = RunCore(code, constants, floatConstants, stringConstants, slots, colorCatalog, objectConstants, frameExtraSpace, classMetadata);
        return new VmMultiResult(result.Returned, result.Fault,
            Array.AsReadOnly(result.Returned ? slots.Values[..slots.Count] : Array.Empty<VmResult>()))
        {
            AbortCode = result.AbortCode,
            AbortContext = result.AbortContext,
            AbortFrames = result.AbortFrames,
        };
    }

    private static VmResult RunCore(ReadOnlySpan<int> code, ReadOnlySpan<int> constants, ReadOnlySpan<double> floatConstants,
        ReadOnlySpan<string> stringConstants, ReturnSlots slots, VmColorCatalog? colorCatalog, IReadOnlyList<object?>? objectConstants, int frameExtraSpace,
        VmClassMetadata? classMetadata, ExecutionState? execution = null, IReadOnlyList<VmValue>? initialArguments = null,
        VmScriptFunction? scriptFunction = null)
    {
        var cursor = new InstructionCursor();
        var result = ExecuteCore(code, constants, floatConstants, stringConstants, slots, colorCatalog, objectConstants,
            frameExtraSpace, classMetadata, execution, initialArguments, cursor);
        if (result.Fault != VmFault.ScriptAbort || scriptFunction is null) return result;
        var frames = new List<VmAbortFrame>(result.AbortFrames ?? Array.Empty<VmAbortFrame>());
        frames.Add(new VmAbortFrame(scriptFunction.PrintableName, scriptFunction.SourceFileName,
            cursor.Index, scriptFunction.GetSourceLine(cursor.Index)));
        return result with { AbortFrames = frames.AsReadOnly() };
    }

    private sealed class InstructionCursor { public int Index; }

    private static VmResult ExecuteCore(ReadOnlySpan<int> code, ReadOnlySpan<int> constants, ReadOnlySpan<double> floatConstants,
        ReadOnlySpan<string> stringConstants, ReturnSlots slots, VmColorCatalog? colorCatalog, IReadOnlyList<object?>? objectConstants, int frameExtraSpace,
        VmClassMetadata? classMetadata, ExecutionState? execution, IReadOnlyList<VmValue>? initialArguments, InstructionCursor cursor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frameExtraSpace);
        var frameMemory = frameExtraSpace == 0 ? null : new byte[frameExtraSpace];
        foreach (var value in stringConstants) ArgumentNullException.ThrowIfNull(value);
        var reg = new int[256];
        var floats = new double[256];
        var objects = new object?[256];
        var parameters = new List<VmValue>();
        objectConstants ??= Array.Empty<object?>();
        var strings = new string[256];
        Array.Fill(strings, string.Empty);
        if (initialArguments is not null)
        {
            var indices = new int[4];
            foreach (var argument in initialArguments)
            {
                var index = indices[(int)argument.Type]++;
                switch (argument.Type)
                {
                    case VmValueType.Integer: reg[index] = argument.Integer; break;
                    case VmValueType.Float: floats[index] = argument.Float; break;
                    case VmValueType.String: strings[index] = argument.String!; break;
                    case VmValueType.Object: objects[index] = argument.Object; break;
                }
            }
        }
        execution ??= new ExecutionState();
        var pc = 0;
        while ((uint)pc < (uint)code.Length)
        {
            cursor.Index = pc;
            if (++execution.Steps > Budget)
                return new VmResult(false, 0, VmFault.Budget);

            var word = code[pc];
            var op = word & 0xFF;
            var a = (word >> 8) & 0xFF;
            var b = (word >> 16) & 0xFF;
            var c = (word >> 24) & 0xFF;
            var bc = (ushort)(word >> 16);
            switch (op)
            {
                case (int)VmOp.Scope:
                    if (c >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    var scopeSource = objects[a];
                    if (scopeSource is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    if (scopeSource is Type || !scopeSource.GetType().IsClass) return Stop(VmFault.InvalidClass);
                    if (objectConstants[c] is not VmFunction && objectConstants[c] is not VmIntegerFunction && objectConstants[c] is not VmScriptFunction) return Stop(VmFault.InvalidCall);
                    if (classMetadata is null || !classMetadata.TryGetScope(scopeSource.GetType(), out var innerScope)) return Stop(VmFault.InvalidClass);
                    if (innerScope != VmClassScope.PlainData && b - 1 != (int)innerScope)
                    {
                        var calledName = objectConstants[c] switch
                        {
                            VmScriptFunction script => script.PrintableName,
                            VmFunction callback => callback.PrintableName,
                            VmIntegerFunction integer => integer.PrintableName,
                            _ => throw new InvalidOperationException("Validated function type was lost."),
                        };
                        return new VmResult(false, 0, VmFault.ScriptAbort)
                        {
                            AbortCode = 0,
                            AbortContext = $"Cannot call {VmAbortDiagnostics.ScopeName((int)innerScope)} function {calledName} from {VmAbortDiagnostics.ScopeName(b - 1)} context",
                        };
                    }
                    pc++;
                    break;
                case (int)VmOp.Virtual:
                    var virtualSource = objects[b];
                    if (virtualSource is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    if (virtualSource is Type || !virtualSource.GetType().IsClass) return Stop(VmFault.InvalidClass);
                    if (classMetadata is null || !classMetadata.TryGetVirtual(virtualSource.GetType(), c, out var virtualMethod)) return Stop(VmFault.InvalidCall);
                    objects[a] = virtualMethod;
                    pc++;
                    break;
                case (int)VmOp.ParamImmediate:
                    if (parameters.Count >= 256) return Stop(VmFault.ParameterRange);
                    parameters.Add(VmValue.FromInteger(word >> 8));
                    pc++;
                    break;
                case (int)VmOp.Param:
                    if (parameters.Count >= 256) return Stop(VmFault.ParameterRange);
                    if (a <= 3)
                    {
                        if (bc >= reg.Length) return Stop(VmFault.RegisterRange);
                        var argumentIndex = (int)bc;
                        parameters.Add(a switch
                        {
                            0 => VmValue.FromInteger(reg[bc]), 1 => VmValue.FromFloat(floats[bc]),
                            2 => VmValue.FromStringRegister(() => strings[argumentIndex]), _ => VmValue.FromObject(objects[bc]),
                        });
                    }
                    else if (a == 4)
                    {
                        if (bc >= constants.Length) return Stop(VmFault.ConstantRange);
                        parameters.Add(VmValue.FromInteger(constants[bc]));
                    }
                    else if (a == 5)
                    {
                        if (bc >= floatConstants.Length) return Stop(VmFault.ConstantRange);
                        parameters.Add(VmValue.FromFloat(floatConstants[bc]));
                    }
                    else if (a == 6)
                    {
                        if (bc >= stringConstants.Length) return Stop(VmFault.ConstantRange);
                        parameters.Add(VmValue.FromString(stringConstants[bc]));
                    }
                    else if (a == 7)
                    {
                        if (bc >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                        parameters.Add(VmValue.FromObject(objectConstants[bc]));
                    }
                    else if (a is >= 32 and <= 35)
                    {
                        if (bc >= reg.Length) return Stop(VmFault.RegisterRange);
                        var referenceType = (VmValueType)(a - 32);
                        var referenceIndex = (int)bc;
                        var registerReference = new VmRegisterReference(referenceType,
                            () => referenceType switch
                            {
                                VmValueType.Integer => VmValue.FromInteger(reg[referenceIndex]),
                                VmValueType.Float => VmValue.FromFloat(floats[referenceIndex]),
                                VmValueType.String => VmValue.FromString(strings[referenceIndex]),
                                _ => VmValue.FromObject(objects[referenceIndex]),
                            },
                            value =>
                            {
                                switch (referenceType)
                                {
                                    case VmValueType.Integer: reg[referenceIndex] = value.Integer; break;
                                    case VmValueType.Float: floats[referenceIndex] = value.Float; break;
                                    case VmValueType.String: strings[referenceIndex] = value.String!; break;
                                    case VmValueType.Object: objects[referenceIndex] = value.Object; break;
                                }
                            });
                        parameters.Add(VmValue.FromObject(registerReference));
                    }
                    else if (a is 9 or 17 or 65)
                    {
                        var argumentComponents = a == 9 ? 2 : a == 17 ? 3 : 4;
                        if (bc + argumentComponents > floats.Length) return Stop(VmFault.RegisterRange);
                        if (parameters.Count + argumentComponents > 256) return Stop(VmFault.ParameterRange);
                        for (var component = 0; component < argumentComponents; component++)
                            parameters.Add(VmValue.FromFloat(floats[bc + component]));
                    }
                    else if (a == Nil) parameters.Add(VmValue.Nil());
                    else return Stop(VmFault.InvalidCall);
                    pc++;
                    break;
                case (int)VmOp.Call:
                case (int)VmOp.CallK:
                    if (op == (int)VmOp.CallK && a >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    var functionObject = op == (int)VmOp.CallK ? objectConstants[a] : objects[a];
                    if (functionObject is not VmIntegerFunction && functionObject is not VmFunction && functionObject is not VmScriptFunction) return Stop(VmFault.InvalidCall);
                    if (b > parameters.Count) return Stop(VmFault.ParameterRange);
                    if (c > 128 || (long)pc + c >= code.Length) return Stop(VmFault.InvalidCall);
                    var destinations = new int[c];
                    var destinationTypes = new int[c];
                    for (var resultIndex = 0; resultIndex < c; resultIndex++)
                    {
                        var descriptor = code[pc + resultIndex + 1];
                        var destinationType = (descriptor >> 16) & 255;
                        if ((descriptor & 255) != (int)VmOp.Result || (destinationType > 3 && destinationType is not (9 or 17 or 65))
                            || functionObject is VmIntegerFunction && destinationType != 0)
                            return Stop(VmFault.InvalidCall);
                        destinations[resultIndex] = (descriptor >> 24) & 255;
                        var destinationComponents = destinationType == 9 ? 2 : destinationType == 17 ? 3 : destinationType == 65 ? 4 : 1;
                        if (destinations[resultIndex] + destinationComponents > reg.Length) return Stop(VmFault.RegisterRange);
                        destinationTypes[resultIndex] = destinationType;
                    }
                    var arguments = Array.AsReadOnly(parameters.GetRange(parameters.Count - b, b).ToArray());
                    IReadOnlyList<VmValue> callResults;
                    if (functionObject is VmIntegerFunction integerFunction)
                    {
                        if (arguments.Any(value => value.Type != VmValueType.Integer)) return Stop(VmFault.InvalidCall);
                        if (!InvokeCallback(() => integerFunction.Invoke(Array.AsReadOnly(arguments.Select(value => value.Integer).ToArray()), c),
                            integerFunction.PrintableName, out var integerResults, out var integerAbort)) return integerAbort;
                        if (integerResults is null) return Stop(VmFault.InvalidCall);
                        callResults = integerResults.Select(VmValue.FromInteger).ToArray();
                    }
                    else if (functionObject is VmScriptFunction scriptFunction)
                    {
                        if (arguments.Count != scriptFunction.ParameterTypes.Length) return Stop(VmFault.ParameterRange);
                        for (var argument = 0; argument < arguments.Count; argument++)
                            if (arguments[argument].Type != scriptFunction.ParameterTypes[argument]) return Stop(VmFault.InvalidCall);
                        if (execution.Depth >= 64) return Stop(VmFault.InvalidCall);
                        var scriptSlots = new ReturnSlots(c);
                        VmResult scriptResult;
                        execution.Depth++;
                        try
                        {
                            scriptResult = RunCore(scriptFunction.Code, scriptFunction.Constants, scriptFunction.FloatConstants,
                                scriptFunction.StringConstants, scriptSlots, colorCatalog, scriptFunction.ObjectConstants,
                                scriptFunction.FrameExtraSpace, classMetadata, execution, arguments, scriptFunction);
                        }
                        finally { execution.Depth--; }
                        if (!scriptResult.Returned) return scriptResult;
                        if (scriptSlots.Count != c) return Stop(VmFault.InvalidCall);
                        callResults = scriptSlots.Values.Take(c).Select(ToCallbackValue).ToArray();
                    }
                    else
                    {
                        var callback = (VmFunction)functionObject;
                        var callbackArgumentFault = callback.Prototype?.Validate(arguments) ?? VmFault.None;
                        if (callbackArgumentFault != VmFault.None) return Stop(callbackArgumentFault);
                        if (!InvokeCallback(() => callback.Invoke(arguments, c), callback.PrintableName, out callResults, out var callbackAbort)) return callbackAbort;
                    }
                    if (callResults is null || callResults.Count != c) return Stop(VmFault.InvalidCall);
                    for (var resultIndex = 0; resultIndex < c; resultIndex++)
                        if (callResults[resultIndex] is null || (int)callResults[resultIndex].Type != destinationTypes[resultIndex]) return Stop(VmFault.InvalidCall);
                    for (var resultIndex = 0; resultIndex < c; resultIndex++)
                    {
                        var value = callResults[resultIndex];
                        var destination = destinations[resultIndex];
                        switch (value.Type)
                        {
                            case VmValueType.Integer: reg[destination] = value.Integer; break;
                            case VmValueType.Float: floats[destination] = value.Float; break;
                            case VmValueType.String: strings[destination] = value.String!; break;
                            case VmValueType.Object: objects[destination] = value.Object; break;
                            case VmValueType.Vector2:
                            case VmValueType.Vector3:
                            case VmValueType.Vector4:
                                floats[destination] = value.Vector.X;
                                floats[destination + 1] = value.Vector.Y;
                                if (value.Vector.Components >= 3) floats[destination + 2] = value.Vector.Z;
                                if (value.Vector.Components == 4) floats[destination + 3] = value.Vector.W;
                                break;
                        }
                    }
                    parameters.RemoveRange(parameters.Count - b, b);
                    pc += c + 1;
                    break;
                case (int)VmOp.Result:
                    return Stop(VmFault.InvalidCall);
                case (int)VmOp.Meta:
                    var metadataSource = objects[b];
                    if (metadataSource is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    if (metadataSource is Type || !metadataSource.GetType().IsClass) return Stop(VmFault.InvalidClass);
                    if (classMetadata is null || !classMetadata.TryGet(metadataSource.GetType(), out var metadataAllocation)) return Stop(VmFault.InvalidClass);
                    objects[a] = metadataAllocation;
                    pc++;
                    break;
                case (int)VmOp.LoadFramePointer:
                    if (frameMemory is null) return Stop(VmFault.InvalidFrame);
                    objects[a] = frameMemory;
                    pc++;
                    break;
                case (int)VmOp.AddPointerR:
                case (int)VmOp.AddPointerK:
                    if (op == (int)VmOp.AddPointerK && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var addressDelta = op == (int)VmOp.AddPointerR ? reg[c] : constants[c];
                    if (objects[b] is null) objects[a] = null;
                    else
                    {
                        var addressSource = objects[b];
                        var allocation = addressSource is VmMemoryPointer address ? address.Memory : addressSource;
                        var addressOffset = addressSource is VmMemoryPointer addressWithOffset ? addressWithOffset.ByteOffset : 0;
                        if (allocation is not byte[] && allocation is not VmStringMemory && allocation is not VmObjectMemory) return Stop(VmFault.InvalidMemory);
                        if (addressDelta > 0 && addressOffset > long.MaxValue - addressDelta || addressDelta < 0 && addressOffset < long.MinValue - addressDelta)
                            return Stop(VmFault.Overflow);
                        objects[a] = new VmMemoryPointer(allocation, addressOffset + addressDelta);
                    }
                    pc++;
                    break;
                case (int)VmOp.SubPointer:
                    var leftAddress = objects[b];
                    var rightAddress = objects[c];
                    var leftAllocation = leftAddress is VmMemoryPointer leftPointer ? leftPointer.Memory : leftAddress;
                    var rightAllocation = rightAddress is VmMemoryPointer rightPointer ? rightPointer.Memory : rightAddress;
                    if (!ReferenceEquals(leftAllocation, rightAllocation)) return Stop(VmFault.InvalidMemory);
                    if (leftAllocation is not null && leftAllocation is not byte[] && leftAllocation is not VmStringMemory && leftAllocation is not VmObjectMemory)
                        return Stop(VmFault.InvalidMemory);
                    var leftOffset = leftAddress is VmMemoryPointer leftPosition ? leftPosition.ByteOffset : 0;
                    var rightOffset = rightAddress is VmMemoryPointer rightPosition ? rightPosition.ByteOffset : 0;
                    reg[a] = unchecked((int)(leftOffset - rightOffset));
                    pc++;
                    break;
                case (int)VmOp.LoadCString:
                case (int)VmOp.LoadCStringR:
                    var cStringRegisterOffset = op == (int)VmOp.LoadCStringR;
                    if (!cStringRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var cStringOffset = cStringRegisterOffset ? reg[c] : constants[c];
                    if (objects[b] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    var cStringMemoryBase = ResolveMemory(objects[b], ref cStringOffset, out var cStringMemoryFault);
                    if (cStringMemoryFault != VmFault.None) return Stop(cStringMemoryFault);
                    if (cStringMemoryBase is not VmObjectMemory cStringMemory) return Stop(VmFault.InvalidMemory);
                    if (!cStringMemory.TryRead(cStringOffset, out var cStringValue)) return Stop(VmFault.MemoryRange);
                    if (cStringValue is not null && cStringValue is not string) return Stop(VmFault.InvalidMemory);
                    var cString = (string?)cStringValue ?? string.Empty;
                    var cStringEnd = cString.IndexOf('\0');
                    strings[a] = cStringEnd < 0 ? cString : cString[..cStringEnd];
                    pc++;
                    break;
                case (int)VmOp.LoadObject:
                case (int)VmOp.LoadObjectR:
                case (int)VmOp.LoadPointer:
                case (int)VmOp.LoadPointerR:
                case (int)VmOp.StorePointer:
                case (int)VmOp.StorePointerR:
                case (int)VmOp.StoreObject:
                case (int)VmOp.StoreObjectR:
                    var referenceStore = op >= (int)VmOp.StorePointer;
                    var referenceRegisterOffset = referenceStore ? (op & 1) != 0 : (op & 1) == 0;
                    if (!referenceRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var referenceOffset = referenceRegisterOffset ? reg[c] : constants[c];
                    var referenceBase = objects[referenceStore ? a : b];
                    if (referenceBase is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = (ushort)(referenceStore ? 2 : 1) };
                    var referenceMemoryBase = ResolveMemory(referenceBase, ref referenceOffset, out var referenceMemoryFault);
                    if (referenceMemoryFault != VmFault.None) return Stop(referenceMemoryFault);
                    if (referenceMemoryBase is not VmObjectMemory referenceMemory) return Stop(VmFault.InvalidMemory);
                    if (!referenceMemory.TryRead(referenceOffset, out var referenceValue)) return Stop(VmFault.MemoryRange);
                    if (!referenceStore) objects[a] = referenceValue;
                    // Native SO_R only barriers the existing field; it does not assign B.
                    else if (op != (int)VmOp.StoreObjectR && !referenceMemory.TryWrite(referenceOffset, objects[b])) return Stop(VmFault.MemoryRange);
                    pc++;
                    break;
                case (int)VmOp.LoadString:
                case (int)VmOp.LoadStringR:
                case (int)VmOp.StoreString:
                case (int)VmOp.StoreStringR:
                    var stringStore = op is (int)VmOp.StoreString or (int)VmOp.StoreStringR;
                    var stringRegisterOffset = op is (int)VmOp.LoadStringR or (int)VmOp.StoreStringR;
                    if (!stringRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var stringOffset = stringRegisterOffset ? reg[c] : constants[c];
                    var stringBase = objects[stringStore ? a : b];
                    if (stringBase is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = (ushort)(stringStore ? 2 : 1) };
                    var stringMemoryBase = ResolveMemory(stringBase, ref stringOffset, out var stringMemoryFault);
                    if (stringMemoryFault != VmFault.None) return Stop(stringMemoryFault);
                    if (stringMemoryBase is not VmStringMemory stringMemory) return Stop(VmFault.InvalidMemory);
                    if (stringStore)
                    {
                        if (!stringMemory.TryWrite(stringOffset, strings[b])) return Stop(VmFault.MemoryRange);
                    }
                    else
                    {
                        if (!stringMemory.TryRead(stringOffset, out var fieldValue)) return Stop(VmFault.MemoryRange);
                        strings[a] = fieldValue;
                    }
                    pc++;
                    break;
                case (int)VmOp.LoadBit:
                    if (objects[b] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    var bitReadOffset = 0;
                    var bitReadBase = ResolveMemory(objects[b], ref bitReadOffset, out var bitReadFault);
                    if (bitReadFault != VmFault.None) return Stop(bitReadFault);
                    if (bitReadBase is not byte[] bitReadMemory) return Stop(VmFault.InvalidMemory);
                    if ((uint)bitReadOffset >= (uint)bitReadMemory.Length) return Stop(VmFault.MemoryRange);
                    reg[a] = (bitReadMemory[bitReadOffset] & c) != 0 ? 1 : 0;
                    pc++;
                    break;
                case (int)VmOp.StoreBit:
                    if (objects[a] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 };
                    var bitWriteOffset = 0;
                    var bitWriteBase = ResolveMemory(objects[a], ref bitWriteOffset, out var bitWriteFault);
                    if (bitWriteFault != VmFault.None) return Stop(bitWriteFault);
                    if (bitWriteBase is not byte[] bitWriteMemory) return Stop(VmFault.InvalidMemory);
                    if ((uint)bitWriteOffset >= (uint)bitWriteMemory.Length) return Stop(VmFault.MemoryRange);
                    bitWriteMemory[bitWriteOffset] = unchecked((byte)(reg[b] != 0 ? bitWriteMemory[bitWriteOffset] | c : bitWriteMemory[bitWriteOffset] & ~c));
                    pc++;
                    break;
                case (int)VmOp.LoadFloat:
                case (int)VmOp.LoadFloatR:
                case (int)VmOp.LoadDouble:
                case (int)VmOp.LoadDoubleR:
                    var floatingLoadRegisterOffset = (op & 1) == 0;
                    if (!floatingLoadRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var floatingLoadOffset = floatingLoadRegisterOffset ? reg[c] : constants[c];
                    if (objects[b] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    var floatingLoadMemoryBase = ResolveMemory(objects[b], ref floatingLoadOffset, out var floatingLoadMemoryFault);
                    if (floatingLoadMemoryFault != VmFault.None) return Stop(floatingLoadMemoryFault);
                    if (floatingLoadMemoryBase is not byte[] floatingLoadMemory) return Stop(VmFault.InvalidMemory);
                    var floatingLoadWidth = op is (int)VmOp.LoadFloat or (int)VmOp.LoadFloatR ? 4 : 8;
                    if (floatingLoadOffset < 0 || (long)floatingLoadOffset + floatingLoadWidth > floatingLoadMemory.Length) return Stop(VmFault.MemoryRange);
                    var floatingLoadBytes = floatingLoadMemory.AsSpan(floatingLoadOffset, floatingLoadWidth);
                    floats[a] = floatingLoadWidth == 4
                        ? System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(floatingLoadBytes)
                        : System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(floatingLoadBytes);
                    pc++;
                    break;
                case (int)VmOp.LoadVector2:
                case (int)VmOp.LoadVector2R:
                case (int)VmOp.LoadVector3:
                case (int)VmOp.LoadVector3R:
                case (int)VmOp.LoadVector4:
                case (int)VmOp.LoadVector4R:
                case (int)VmOp.LoadFloatVector2:
                case (int)VmOp.LoadFloatVector2R:
                case (int)VmOp.LoadFloatVector3:
                case (int)VmOp.LoadFloatVector3R:
                case (int)VmOp.LoadFloatVector4:
                case (int)VmOp.LoadFloatVector4R:
                    var singleVectorLoad = op >= (int)VmOp.LoadFloatVector2;
                    var loadComponents = 2 + (op - (singleVectorLoad ? (int)VmOp.LoadFloatVector2 : (int)VmOp.LoadVector2)) / 2;
                    if (a + loadComponents > floats.Length) return Stop(VmFault.RegisterRange);
                    var vectorLoadRegisterOffset = (op & 1) == 0;
                    if (!vectorLoadRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var vectorLoadOffset = vectorLoadRegisterOffset ? reg[c] : constants[c];
                    if (objects[b] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    var vectorLoadMemoryBase = ResolveMemory(objects[b], ref vectorLoadOffset, out var vectorLoadMemoryFault);
                    if (vectorLoadMemoryFault != VmFault.None) return Stop(vectorLoadMemoryFault);
                    if (vectorLoadMemoryBase is not byte[] vectorLoadMemory) return Stop(VmFault.InvalidMemory);
                    var loadComponentWidth = singleVectorLoad ? 4 : 8;
                    if (vectorLoadOffset < 0 || (long)vectorLoadOffset + loadComponents * loadComponentWidth > vectorLoadMemory.Length) return Stop(VmFault.MemoryRange);
                    for (var component = 0; component < loadComponents; component++)
                    {
                        var componentBytes = vectorLoadMemory.AsSpan(vectorLoadOffset + component * loadComponentWidth, loadComponentWidth);
                        floats[a + component] = singleVectorLoad
                            ? System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(componentBytes)
                            : System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(componentBytes);
                    }
                    pc++;
                    break;
                case (int)VmOp.StoreVector2:
                case (int)VmOp.StoreVector2R:
                case (int)VmOp.StoreVector3:
                case (int)VmOp.StoreVector3R:
                case (int)VmOp.StoreVector4:
                case (int)VmOp.StoreVector4R:
                case (int)VmOp.StoreFloatVector2:
                case (int)VmOp.StoreFloatVector2R:
                case (int)VmOp.StoreFloatVector3:
                case (int)VmOp.StoreFloatVector3R:
                case (int)VmOp.StoreFloatVector4:
                case (int)VmOp.StoreFloatVector4R:
                    var singleVectorStore = op >= (int)VmOp.StoreFloatVector2;
                    var storeComponents = 2 + (op - (singleVectorStore ? (int)VmOp.StoreFloatVector2 : (int)VmOp.StoreVector2)) / 2;
                    if (b + storeComponents > floats.Length) return Stop(VmFault.RegisterRange);
                    var vectorRegisterOffset = (op & 1) != 0;
                    if (!vectorRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var vectorOffset = vectorRegisterOffset ? reg[c] : constants[c];
                    if (objects[a] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 };
                    var vectorMemoryBase = ResolveMemory(objects[a], ref vectorOffset, out var vectorMemoryFault);
                    if (vectorMemoryFault != VmFault.None) return Stop(vectorMemoryFault);
                    if (vectorMemoryBase is not byte[] vectorMemory) return Stop(VmFault.InvalidMemory);
                    var componentWidth = singleVectorStore ? 4 : 8;
                    if (vectorOffset < 0 || (long)vectorOffset + storeComponents * componentWidth > vectorMemory.Length) return Stop(VmFault.MemoryRange);
                    for (var component = 0; component < storeComponents; component++)
                    {
                        var componentBytes = vectorMemory.AsSpan(vectorOffset + component * componentWidth, componentWidth);
                        if (singleVectorStore) System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(componentBytes, (float)floats[b + component]);
                        else System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(componentBytes, floats[b + component]);
                    }
                    pc++;
                    break;
                case (int)VmOp.StoreFloat:
                case (int)VmOp.StoreFloatR:
                case (int)VmOp.StoreDouble:
                case (int)VmOp.StoreDoubleR:
                    var floatingRegisterOffset = (op & 1) != 0;
                    if (!floatingRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var floatingOffset = floatingRegisterOffset ? reg[c] : constants[c];
                    if (objects[a] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 };
                    var floatingMemoryBase = ResolveMemory(objects[a], ref floatingOffset, out var floatingMemoryFault);
                    if (floatingMemoryFault != VmFault.None) return Stop(floatingMemoryFault);
                    if (floatingMemoryBase is not byte[] floatingMemory) return Stop(VmFault.InvalidMemory);
                    var floatingWidth = op is (int)VmOp.StoreFloat or (int)VmOp.StoreFloatR ? 4 : 8;
                    if (floatingOffset < 0 || (long)floatingOffset + floatingWidth > floatingMemory.Length) return Stop(VmFault.MemoryRange);
                    var floatingBytes = floatingMemory.AsSpan(floatingOffset, floatingWidth);
                    if (floatingWidth == 4) System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(floatingBytes, (float)floats[b]);
                    else System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(floatingBytes, floats[b]);
                    pc++;
                    break;
                case (int)VmOp.StoreByte:
                case (int)VmOp.StoreByteR:
                case (int)VmOp.StoreHalf:
                case (int)VmOp.StoreHalfR:
                case (int)VmOp.StoreWord:
                case (int)VmOp.StoreWordR:
                    var storeRegisterOffset = (op & 1) != 0;
                    if (!storeRegisterOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var storeOffset = storeRegisterOffset ? reg[c] : constants[c];
                    if (objects[a] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 };
                    var storeMemoryBase = ResolveMemory(objects[a], ref storeOffset, out var storeMemoryFault);
                    if (storeMemoryFault != VmFault.None) return Stop(storeMemoryFault);
                    if (storeMemoryBase is not byte[] storeMemory) return Stop(VmFault.InvalidMemory);
                    var storeWidth = op is (int)VmOp.StoreWord or (int)VmOp.StoreWordR ? 4
                        : op is (int)VmOp.StoreHalf or (int)VmOp.StoreHalfR ? 2 : 1;
                    if (storeOffset < 0 || (long)storeOffset + storeWidth > storeMemory.Length) return Stop(VmFault.MemoryRange);
                    var storeBytes = storeMemory.AsSpan(storeOffset, storeWidth);
                    if (storeWidth == 1) storeBytes[0] = unchecked((byte)reg[b]);
                    else if (storeWidth == 2) System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(storeBytes, unchecked((short)reg[b]));
                    else System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(storeBytes, reg[b]);
                    pc++;
                    break;
                case (int)VmOp.LoadByte:
                case (int)VmOp.LoadByteR:
                case (int)VmOp.LoadHalf:
                case (int)VmOp.LoadHalfR:
                case (int)VmOp.LoadWord:
                case (int)VmOp.LoadWordR:
                case (int)VmOp.LoadUnsignedByte:
                case (int)VmOp.LoadUnsignedByteR:
                case (int)VmOp.LoadUnsignedHalf:
                case (int)VmOp.LoadUnsignedHalfR:
                    var registerOffset = (op & 1) == 0;
                    if (!registerOffset && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var offset = registerOffset ? reg[c] : constants[c];
                    if (objects[b] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    var memoryBase = ResolveMemory(objects[b], ref offset, out var memoryFault);
                    if (memoryFault != VmFault.None) return Stop(memoryFault);
                    if (memoryBase is not byte[] memory) return Stop(VmFault.InvalidMemory);
                    var loadWidth = op is (int)VmOp.LoadWord or (int)VmOp.LoadWordR ? 4
                        : op is (int)VmOp.LoadHalf or (int)VmOp.LoadHalfR or (int)VmOp.LoadUnsignedHalf or (int)VmOp.LoadUnsignedHalfR ? 2 : 1;
                    if (offset < 0 || (long)offset + loadWidth > memory.Length) return Stop(VmFault.MemoryRange);
                    var loadBytes = memory.AsSpan(offset, loadWidth);
                    reg[a] = op switch
                    {
                        (int)VmOp.LoadByte or (int)VmOp.LoadByteR => unchecked((sbyte)loadBytes[0]),
                        (int)VmOp.LoadUnsignedByte or (int)VmOp.LoadUnsignedByteR => loadBytes[0],
                        (int)VmOp.LoadHalf or (int)VmOp.LoadHalfR => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(loadBytes),
                        (int)VmOp.LoadUnsignedHalf or (int)VmOp.LoadUnsignedHalfR => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(loadBytes),
                        _ => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(loadBytes),
                    };
                    pc++;
                    break;
                case (int)VmOp.Class:
                    var classSource = objects[b];
                    if (classSource is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 1 };
                    if (classSource is Type || !classSource.GetType().IsClass) return Stop(VmFault.InvalidClass);
                    objects[a] = classSource.GetType();
                    pc++;
                    break;
                case (int)VmOp.DynCastR:
                case (int)VmOp.DynCastK:
                case (int)VmOp.DynCastClassR:
                case (int)VmOp.DynCastClassK:
                    var constantClass = op is (int)VmOp.DynCastK or (int)VmOp.DynCastClassK;
                    if (constantClass && c >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    var castSource = objects[b];
                    if (castSource is null) objects[a] = null;
                    else
                    {
                        var descriptor = constantClass ? objectConstants[c] : objects[c];
                        if (descriptor is not Type targetClass || !targetClass.IsClass) return Stop(VmFault.InvalidClass);
                        var classCast = op is (int)VmOp.DynCastClassR or (int)VmOp.DynCastClassK;
                        if (classCast && (castSource is not Type sourceClass || !sourceClass.IsClass))
                            return Stop(VmFault.InvalidClass);
                        var compatible = classCast ? targetClass.IsAssignableFrom((Type)castSource)
                            : targetClass.IsInstanceOfType(castSource);
                        objects[a] = compatible ? castSource : null;
                    }
                    pc++;
                    break;
                case (int)VmOp.Lkp:
                    if (bc >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    objects[a] = objectConstants[bc];
                    pc++;
                    break;
                case (int)VmOp.LkpR:
                    var objectIndex = (long)reg[b] + c;
                    if (objectIndex < 0 || objectIndex >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    objects[a] = objectConstants[(int)objectIndex];
                    pc++;
                    break;
                case (int)VmOp.MoveA:
                    objects[a] = objects[b];
                    pc++;
                    break;
                case (int)VmOp.EqaR:
                case (int)VmOp.EqaK:
                    if (op == (int)VmOp.EqaK && c >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                    var sameObject = SameAddress(objects[b], op == (int)VmOp.EqaK ? objectConstants[c] : objects[c]);
                    if (!Branch(code, ref pc, a, sameObject)) return Stop(VmFault.BadJump);
                    break;
                case (int)VmOp.NullCheck:
                    if (objects[a] is null) return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = 2 };
                    pc++;
                    break;
                case (int)VmOp.Throw:
                    // Native object-exception branches are disabled; every A uses the BC abort reason.
                    return new VmResult(false, 0, VmFault.ScriptAbort) { AbortCode = bc };
                case (int)VmOp.CmpS:
                    var stringMethod = a & 6;
                    if (stringMethod == 6) return Stop(VmFault.InvalidComparison);
                    var stringConstantLeft = (a & 8) != 0;
                    var stringConstantRight = (a & 16) != 0;
                    if (stringConstantLeft && b >= stringConstants.Length || stringConstantRight && c >= stringConstants.Length)
                        return Stop(VmFault.ConstantRange);
                    var stringOrder = CompareStrings(stringConstantLeft ? stringConstants[b] : strings[b],
                        stringConstantRight ? stringConstants[c] : strings[c], (a & VmCode.CompareApprox) != 0);
                    var stringComparison = stringMethod == 0 ? stringOrder == 0 : stringMethod == 2 ? stringOrder < 0 : stringOrder <= 0;
                    if (!Branch(code, ref pc, a, stringComparison)) return Stop(VmFault.BadJump);
                    break;
                case (int)VmOp.Lks:
                    if (bc >= stringConstants.Length) return Stop(VmFault.ConstantRange);
                    strings[a] = stringConstants[bc];
                    pc++;
                    break;
                case (int)VmOp.LksR:
                    var stringIndex = (long)reg[b] + c;
                    if (stringIndex < 0 || stringIndex >= stringConstants.Length) return Stop(VmFault.ConstantRange);
                    strings[a] = stringConstants[(int)stringIndex];
                    pc++;
                    break;
                case (int)VmOp.MoveS:
                    strings[a] = strings[b];
                    pc++;
                    break;
                case (int)VmOp.Concat:
                    strings[a] = strings[b] + strings[c];
                    pc++;
                    break;
                case (int)VmOp.LenS:
                    reg[a] = System.Text.Encoding.UTF8.GetByteCount(strings[b]);
                    pc++;
                    break;
                case (int)VmOp.MulQv3:
                case (int)VmOp.MulQq:
                case (int)VmOp.ConjQ:
                    var quaternionVector = op == (int)VmOp.MulQv3;
                    var conjugate = op == (int)VmOp.ConjQ;
                    if (a + (quaternionVector ? 3 : 4) > floats.Length || b + 4 > floats.Length
                        || (!conjugate && c + (quaternionVector ? 3 : 4) > floats.Length))
                        return Stop(VmFault.RegisterRange);
                    var quaternion = (X: floats[b], Y: floats[b + 1], Z: floats[b + 2], W: floats[b + 3]);
                    (double X, double Y, double Z, double W) quaternionResult;
                    if (conjugate) quaternionResult = (-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
                    else if (quaternionVector)
                    {
                        var vectorQuaternion = (floats[c], floats[c + 1], floats[c + 2], 0.0);
                        var conjugatedQuaternion = (-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
                        quaternionResult = MultiplyQuaternion(quaternion, MultiplyQuaternion(vectorQuaternion, conjugatedQuaternion));
                    }
                    else quaternionResult = MultiplyQuaternion(quaternion, (floats[c], floats[c + 1], floats[c + 2], floats[c + 3]));
                    floats[a] = quaternionResult.X;
                    floats[a + 1] = quaternionResult.Y;
                    floats[a + 2] = quaternionResult.Z;
                    if (!quaternionVector) floats[a + 3] = quaternionResult.W;
                    pc++;
                    break;
                case (int)VmOp.CrossV:
                    if (a > 253 || b > 253 || c > 253) return Stop(VmFault.RegisterRange);
                    var crossZ = floats[b] * floats[c + 1] - floats[b + 1] * floats[c];
                    var crossY = floats[b + 2] * floats[c] - floats[b] * floats[c + 2];
                    var crossX = floats[b + 1] * floats[c + 2] - floats[b + 2] * floats[c + 1];
                    floats[a] = crossX;
                    floats[a + 1] = crossY;
                    floats[a + 2] = crossZ;
                    pc++;
                    break;
                case (int)VmOp.EqV2R:
                case (int)VmOp.EqV2K:
                case (int)VmOp.EqV3R:
                case (int)VmOp.EqV3K:
                case (int)VmOp.EqV4R:
                case (int)VmOp.EqV4K:
                    var equalitySize = op <= (int)VmOp.EqV2K ? 2 : op <= (int)VmOp.EqV3K ? 3 : 4;
                    var equalityConstant = op is (int)VmOp.EqV2K or (int)VmOp.EqV3K or (int)VmOp.EqV4K;
                    if (b + equalitySize > floats.Length || (!equalityConstant && c + equalitySize > floats.Length))
                        return Stop(VmFault.RegisterRange);
                    if (equalityConstant && c + equalitySize > floatConstants.Length) return Stop(VmFault.ConstantRange);
                    var vectorEqual = true;
                    for (var component = 0; component < equalitySize; component++)
                    {
                        var vectorRight = equalityConstant ? floatConstants[c + component] : floats[c + component];
                        vectorEqual &= (a & VmCode.CompareApprox) != 0
                            ? Math.Abs(floats[b + component] - vectorRight) < EqualEpsilon
                            : floats[b + component] == vectorRight;
                    }
                    if (!Branch(code, ref pc, a, vectorEqual)) return Stop(VmFault.BadJump);
                    break;
                case (int)VmOp.MulVf2Rr:
                case (int)VmOp.MulVf2Rk:
                case (int)VmOp.DivVf2Rr:
                case (int)VmOp.DivVf2Rk:
                case (int)VmOp.LenV2:
                case (int)VmOp.MulVf3Rr:
                case (int)VmOp.MulVf3Rk:
                case (int)VmOp.DivVf3Rr:
                case (int)VmOp.DivVf3Rk:
                case (int)VmOp.LenV3:
                case (int)VmOp.MulVf4Rr:
                case (int)VmOp.MulVf4Rk:
                case (int)VmOp.DivVf4Rr:
                case (int)VmOp.DivVf4Rk:
                case (int)VmOp.LenV4:
                    var scaledSize = op <= (int)VmOp.LenV2 ? 2 : op <= (int)VmOp.LenV3 ? 3 : 4;
                    var lengthOperation = op is (int)VmOp.LenV2 or (int)VmOp.LenV3 or (int)VmOp.LenV4;
                    if (b + scaledSize > floats.Length || (!lengthOperation && a + scaledSize > floats.Length))
                        return Stop(VmFault.RegisterRange);
                    if (lengthOperation)
                    {
                        var squaredLength = floats[b] * floats[b];
                        for (var component = 1; component < scaledSize; component++)
                            squaredLength += floats[b + component] * floats[b + component];
                        floats[a] = Math.Sqrt(squaredLength);
                    }
                    else
                    {
                        var constantScale = op is (int)VmOp.MulVf2Rk or (int)VmOp.DivVf2Rk
                            or (int)VmOp.MulVf3Rk or (int)VmOp.DivVf3Rk or (int)VmOp.MulVf4Rk or (int)VmOp.DivVf4Rk;
                        if (constantScale && c >= floatConstants.Length) return Stop(VmFault.ConstantRange);
                        var scale = constantScale ? floatConstants[c] : floats[c];
                        var divideVector = op is (int)VmOp.DivVf2Rr or (int)VmOp.DivVf2Rk
                            or (int)VmOp.DivVf3Rr or (int)VmOp.DivVf3Rk or (int)VmOp.DivVf4Rr or (int)VmOp.DivVf4Rk;
                        // Native vector division permits IEEE zero division; capture the scalar before writes.
                        for (var component = 0; component < scaledSize; component++)
                            floats[a + component] = divideVector ? floats[b + component] / scale : floats[b + component] * scale;
                    }
                    pc++;
                    break;
                case (int)VmOp.MoveV2:
                case (int)VmOp.MoveV3:
                case (int)VmOp.MoveV4:
                case (int)VmOp.NegV2:
                case (int)VmOp.AddV2:
                case (int)VmOp.SubV2:
                case (int)VmOp.DotV2:
                case (int)VmOp.NegV3:
                case (int)VmOp.AddV3:
                case (int)VmOp.SubV3:
                case (int)VmOp.DotV3:
                case (int)VmOp.NegV4:
                case (int)VmOp.AddV4:
                case (int)VmOp.SubV4:
                case (int)VmOp.DotV4:
                    var vectorSize = op is (int)VmOp.MoveV2 or >= (int)VmOp.NegV2 and <= (int)VmOp.DotV2 ? 2
                        : op is (int)VmOp.MoveV3 or >= (int)VmOp.NegV3 and <= (int)VmOp.DotV3 ? 3 : 4;
                    var vectorMove = op is (int)VmOp.MoveV2 or (int)VmOp.MoveV3 or (int)VmOp.MoveV4;
                    var vectorNegate = op is (int)VmOp.NegV2 or (int)VmOp.NegV3 or (int)VmOp.NegV4;
                    var vectorDot = op is (int)VmOp.DotV2 or (int)VmOp.DotV3 or (int)VmOp.DotV4;
                    var vectorSubtract = op is (int)VmOp.SubV2 or (int)VmOp.SubV3 or (int)VmOp.SubV4;
                    if (b + vectorSize > floats.Length || (!vectorDot && a + vectorSize > floats.Length)
                        || (!vectorMove && !vectorNegate && c + vectorSize > floats.Length))
                        return Stop(VmFault.RegisterRange);
                    if (vectorDot)
                    {
                        var dot = floats[b] * floats[c];
                        for (var component = 1; component < vectorSize; component++)
                            dot += floats[b + component] * floats[c + component];
                        floats[a] = dot;
                    }
                    else
                    {
                        // Native vector operations write components in order, including overlapping ranges.
                        for (var component = 0; component < vectorSize; component++)
                            floats[a + component] = vectorMove ? floats[b + component]
                                : vectorNegate ? -floats[b + component]
                                : vectorSubtract ? floats[b + component] - floats[c + component]
                                : floats[b + component] + floats[c + component];
                    }
                    pc++;
                    break;
                case (int)VmOp.Cast:
                    switch ((VmCast)c)
                    {
                        case VmCast.StringToColor:
                            int color;
                            if (colorCatalog is null || !colorCatalog.TryGetColor(strings[b], out color))
                                if (!TryParseColor(strings[b], out color)) return Stop(VmFault.InvalidColor);
                            reg[a] = color;
                            break;
                        case VmCast.ColorToString:
                            var packedColor = unchecked((uint)reg[b]);
                            strings[a] = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                                $"{(packedColor >> 16) & 255:x2} {(packedColor >> 8) & 255:x2} {packedColor & 255:x2}");
                            break;
                        case VmCast.StringToFloat:
                            floats[a] = ParseStringFloat(strings[b]);
                            break;
                        case VmCast.StringToInt:
                            reg[a] = ParseStringInteger(strings[b]);
                            break;
                        case VmCast.FloatToString:
                            strings[a] = FormatFloat(floats[b]);
                            break;
                        case VmCast.Vector2ToString:
                        case VmCast.Vector3ToString:
                            var formattedSize = c == (int)VmCast.Vector2ToString ? 2 : 3;
                            if (b + formattedSize > floats.Length) return Stop(VmFault.RegisterRange);
                            var formattedComponents = new string[formattedSize];
                            for (var component = 0; component < formattedSize; component++)
                                formattedComponents[component] = FormatFloat(floats[b + component]);
                            strings[a] = "(" + string.Join(", ", formattedComponents) + ")";
                            break;
                        case VmCast.IntToString:
                            strings[a] = reg[b].ToString(System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case VmCast.UnsignedToString:
                            strings[a] = unchecked((uint)reg[b]).ToString(System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case VmCast.IntToFloat:
                            floats[a] = reg[b];
                            break;
                        case VmCast.UnsignedToFloat:
                            floats[a] = unchecked((uint)reg[b]);
                            break;
                        case VmCast.FloatToInt:
                        case VmCast.FloatToUnsigned:
                            var truncated = Math.Truncate(floats[b]);
                            var toUnsigned = c == (int)VmCast.FloatToUnsigned;
                            // Native out-of-range floating-to-integer casts are not portable.
                            if (!double.IsFinite(truncated) || truncated < (toUnsigned ? 0.0 : int.MinValue)
                                || truncated > (toUnsigned ? uint.MaxValue : int.MaxValue))
                                return Stop(VmFault.Overflow);
                            reg[a] = toUnsigned ? unchecked((int)(uint)truncated) : (int)truncated;
                            break;
                        default:
                            return Stop(VmFault.InvalidCast);
                    }
                    pc++;
                    break;
                case (int)VmOp.CastB:
                    if (c == (int)VmCast.IntToBool) reg[a] = reg[b] != 0 ? 1 : 0;
                    else if (c == (int)VmCast.FloatToBool) reg[a] = floats[b] != 0.0 ? 1 : 0;
                    else if (c == (int)VmCast.StringToBool) reg[a] = strings[b].Length > 0 ? 1 : 0;
                    else if (c == (int)VmCast.ObjectToBool) reg[a] = objects[b] is not null ? 1 : 0;
                    else return Stop(VmFault.InvalidCast);
                    pc++;
                    break;
                case (int)VmOp.Atan2:
                    floats[a] = Math.Atan2(floats[b], floats[c]) * (180.0 / Math.PI);
                    pc++;
                    break;
                case (int)VmOp.Flop:
                    if (c > (int)VmFloatFunction.Round) return Stop(VmFault.InvalidFloatFunction);
                    floats[a] = ApplyFloatFunction((VmFloatFunction)c, floats[b]);
                    pc++;
                    break;
                case (int)VmOp.EqfR:
                case (int)VmOp.EqfK:
                case (int)VmOp.LtfRr:
                case (int)VmOp.LtfRk:
                case (int)VmOp.LtfKr:
                case (int)VmOp.LefRr:
                case (int)VmOp.LefRk:
                case (int)VmOp.LefKr:
                    var comparisonConstantLeft = op is (int)VmOp.LtfKr or (int)VmOp.LefKr;
                    var comparisonConstantRight = op is (int)VmOp.EqfK or (int)VmOp.LtfRk or (int)VmOp.LefRk;
                    if (comparisonConstantLeft && b >= floatConstants.Length || comparisonConstantRight && c >= floatConstants.Length)
                        return Stop(VmFault.ConstantRange);
                    var comparisonLeft = comparisonConstantLeft ? floatConstants[b] : floats[b];
                    var comparisonRight = comparisonConstantRight ? floatConstants[c] : floats[c];
                    var approximate = (a & VmCode.CompareApprox) != 0;
                    var comparison = op switch
                    {
                        (int)VmOp.EqfR or (int)VmOp.EqfK => approximate
                            ? Math.Abs(comparisonRight - comparisonLeft) < EqualEpsilon : comparisonLeft == comparisonRight,
                        (int)VmOp.LtfRr or (int)VmOp.LtfRk or (int)VmOp.LtfKr => approximate
                            ? comparisonLeft - comparisonRight < -EqualEpsilon : comparisonLeft < comparisonRight,
                        // Native approximate <= uses the negative epsilon boundary too.
                        _ => approximate ? comparisonLeft - comparisonRight <= -EqualEpsilon : comparisonLeft <= comparisonRight,
                    };
                    if (!Branch(code, ref pc, a, comparison)) return Stop(VmFault.BadJump);
                    break;
                case (int)VmOp.Lkf:
                    if (bc >= floatConstants.Length) return Stop(VmFault.ConstantRange);
                    floats[a] = floatConstants[bc];
                    pc++;
                    break;
                case (int)VmOp.LkfR:
                    var floatIndex = (long)reg[b] + c;
                    if (floatIndex < 0 || floatIndex >= floatConstants.Length) return Stop(VmFault.ConstantRange);
                    floats[a] = floatConstants[(int)floatIndex];
                    pc++;
                    break;
                case (int)VmOp.MoveF:
                    floats[a] = floats[b];
                    pc++;
                    break;
                case (int)VmOp.AddfRr:
                case (int)VmOp.AddfRk:
                case (int)VmOp.SubfRr:
                case (int)VmOp.SubfRk:
                case (int)VmOp.SubfKr:
                case (int)VmOp.MulfRr:
                case (int)VmOp.MulfRk:
                case (int)VmOp.DivfRr:
                case (int)VmOp.DivfRk:
                case (int)VmOp.DivfKr:
                case (int)VmOp.ModfRr:
                case (int)VmOp.ModfRk:
                case (int)VmOp.ModfKr:
                case (int)VmOp.PowfRr:
                case (int)VmOp.PowfRk:
                case (int)VmOp.PowfKr:
                case (int)VmOp.MinfRr:
                case (int)VmOp.MinfRk:
                case (int)VmOp.MaxfRr:
                case (int)VmOp.MaxfRk:
                    var floatConstantLeft = op is (int)VmOp.SubfKr or (int)VmOp.DivfKr or (int)VmOp.ModfKr or (int)VmOp.PowfKr;
                    var floatConstantRight = op is (int)VmOp.AddfRk or (int)VmOp.SubfRk or (int)VmOp.MulfRk or (int)VmOp.DivfRk
                        or (int)VmOp.ModfRk or (int)VmOp.PowfRk or (int)VmOp.MinfRk or (int)VmOp.MaxfRk;
                    if (floatConstantLeft && b >= floatConstants.Length || floatConstantRight && c >= floatConstants.Length)
                        return Stop(VmFault.ConstantRange);
                    var floatLeft = floatConstantLeft ? floatConstants[b] : floats[b];
                    var floatRight = floatConstantRight ? floatConstants[c] : floats[c];
                    if (op is >= (int)VmOp.DivfRr and <= (int)VmOp.ModfKr && floatRight == 0.0)
                        return Stop(VmFault.DivisionByZero);
                    floats[a] = op switch
                    {
                        (int)VmOp.AddfRr or (int)VmOp.AddfRk => floatLeft + floatRight,
                        (int)VmOp.SubfRr or (int)VmOp.SubfRk or (int)VmOp.SubfKr => floatLeft - floatRight,
                        (int)VmOp.MulfRr or (int)VmOp.MulfRk => floatLeft * floatRight,
                        // Native Lua-style modulo uses floor, unlike C# remainder.
                        (int)VmOp.ModfRr or (int)VmOp.ModfRk or (int)VmOp.ModfKr => floatLeft - Math.Floor(floatLeft / floatRight) * floatRight,
                        (int)VmOp.PowfRr or (int)VmOp.PowfRk or (int)VmOp.PowfKr => Math.Pow(floatLeft, floatRight),
                        (int)VmOp.MinfRr or (int)VmOp.MinfRk => floatLeft < floatRight ? floatLeft : floatRight,
                        (int)VmOp.MaxfRr or (int)VmOp.MaxfRk => floatLeft > floatRight ? floatLeft : floatRight,
                        _ => floatLeft / floatRight,
                    };
                    pc++;
                    break;
                case (int)VmOp.Nop:
                    pc++;
                    break;
                case (int)VmOp.Li:
                    reg[a] = (short)bc;
                    pc++;
                    break;
                case (int)VmOp.Lk:
                    if (bc >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    reg[a] = constants[bc];
                    pc++;
                    break;
                case (int)VmOp.Move:
                    reg[a] = reg[b];
                    pc++;
                    break;
                case (int)VmOp.LkR:
                    var constantIndex = (long)reg[b] + c;
                    if (constantIndex < 0 || constantIndex >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    reg[a] = constants[(int)constantIndex];
                    pc++;
                    break;
                case (int)VmOp.TestN:
                    if (reg[a] != -(int)bc)
                        pc++;
                    pc++;
                    break;
                case (int)VmOp.IJmp:
                    // Native execution uses the register offset, ignoring BC.
                    var tableJump = (long)pc + reg[a] + 1;
                    if (tableJump < 0 || tableJump >= code.Length || (code[(int)tableJump] & 0xFF) != (int)VmOp.Jmp)
                        return Stop(VmFault.BadJump);
                    pc = (int)tableJump + I24(code[(int)tableJump]) + 1;
                    break;
                case (int)VmOp.Bound:
                case (int)VmOp.BoundK:
                case (int)VmOp.BoundR:
                    if (op == (int)VmOp.BoundK && bc >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    var bound = op == (int)VmOp.Bound ? bc
                        : op == (int)VmOp.BoundK ? constants[bc] : reg[b];
                    if (reg[a] < 0 || reg[a] >= bound)
                        return Stop(VmFault.ArrayBounds);
                    pc++;
                    break;
                case (int)VmOp.Test:
                    if (reg[a] != bc)
                        pc++;
                    pc++;
                    break;
                case (int)VmOp.Jmp:
                    pc += I24(word) + 1;
                    break;
                case (int)VmOp.Ret:
                    if (b == Nil)
                        return new VmResult(true, 0, VmFault.None);
                    slots.Index = a & ~VmCode.RetFinal;
                    if (slots.Index >= slots.Values.Length)
                    {
                        if ((a & VmCode.RetFinal) != 0) return slots.Finish();
                        pc++;
                        break;
                    }
                    var returnConstant = (b & 4) != 0;
                    var returnType = b & ~4;
                    if (returnType == 3)
                    {
                        if (returnConstant && c >= objectConstants.Count) return Stop(VmFault.ConstantRange);
                        slots.Values[slots.Index] = new VmResult(true, 0, VmFault.None)
                        {
                            HasObjectValue = true,
                            ObjectValue = returnConstant ? objectConstants[c] : objects[c],
                        };
                        if ((a & VmCode.RetFinal) != 0) return slots.Finish();
                        pc++;
                        break;
                    }
                    if (returnType == 2)
                    {
                        if (returnConstant && c >= stringConstants.Length) return Stop(VmFault.ConstantRange);
                        slots.Values[slots.Index] = new VmResult(true, 0, VmFault.None) { StringValue = returnConstant ? stringConstants[c] : strings[c] };
                        if ((a & VmCode.RetFinal) != 0) return slots.Finish();
                        pc++;
                        break;
                    }
                    if (returnType is not (0 or 1 or 9 or 17 or 65))
                        return Stop(VmFault.UnknownOpcode);
                    var returnSize = returnType == 9 ? 2 : returnType == 17 ? 3 : returnType == 65 ? 4 : 1;
                    if (returnConstant && c + returnSize > (returnType == 0 ? constants.Length : floatConstants.Length))
                        return Stop(VmFault.ConstantRange);
                    if (!returnConstant && c + returnSize > floats.Length) return Stop(VmFault.RegisterRange);
                    if (returnType == 0)
                        slots.Values[slots.Index] = new VmResult(true, returnConstant ? constants[c] : reg[c], VmFault.None);
                    else
                    {
                        var returnFloats = returnConstant ? floatConstants : floats.AsSpan();
                        if (returnSize == 1)
                            slots.Values[slots.Index] = new VmResult(true, 0, VmFault.None) { FloatValue = returnFloats[c] };
                        else slots.Values[slots.Index] = new VmResult(true, 0, VmFault.None)
                        {
                            VectorValue = new VmVectorResult(returnSize, returnFloats[c], returnFloats[c + 1],
                                returnSize >= 3 ? returnFloats[c + 2] : 0, returnSize == 4 ? returnFloats[c + 3] : 0),
                        };
                    }
                    if ((a & VmCode.RetFinal) != 0) return slots.Finish();
                    pc++;
                    break;
                case (int)VmOp.Reti:
                    slots.Index = a & ~VmCode.RetFinal;
                    if (slots.Index < slots.Values.Length)
                        slots.Values[slots.Index] = new VmResult(true, (short)bc, VmFault.None);
                    if ((a & VmCode.RetFinal) != 0)
                        return slots.Finish();
                    pc++;
                    break;
                case (int)VmOp.AddRr:
                    reg[a] = unchecked(reg[b] + reg[c]);
                    pc++;
                    break;
                case (int)VmOp.AddRk:
                    if (c >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    reg[a] = unchecked(reg[b] + constants[c]);
                    pc++;
                    break;
                case (int)VmOp.Addi:
                    reg[a] = unchecked(reg[b] + (sbyte)c);
                    pc++;
                    break;
                case (int)VmOp.SubRr:
                    reg[a] = unchecked(reg[b] - reg[c]);
                    pc++;
                    break;
                case (int)VmOp.MulRr:
                    reg[a] = unchecked(reg[b] * reg[c]);
                    pc++;
                    break;
                case (int)VmOp.SubRk:
                case (int)VmOp.MulRk:
                    if (c >= constants.Length) return Stop(VmFault.ConstantRange);
                    reg[a] = op == (int)VmOp.SubRk
                        ? unchecked(reg[b] - constants[c]) : unchecked(reg[b] * constants[c]);
                    pc++;
                    break;
                case (int)VmOp.SubKr:
                    if (b >= constants.Length) return Stop(VmFault.ConstantRange);
                    reg[a] = unchecked(constants[b] - reg[c]);
                    pc++;
                    break;
                case (int)VmOp.DivRr:
                case (int)VmOp.DivRk:
                case (int)VmOp.DivKr:
                case (int)VmOp.DivuRr:
                case (int)VmOp.DivuRk:
                case (int)VmOp.DivuKr:
                case (int)VmOp.ModRr:
                case (int)VmOp.ModRk:
                case (int)VmOp.ModKr:
                case (int)VmOp.ModuRr:
                case (int)VmOp.ModuRk:
                case (int)VmOp.ModuKr:
                    var constantLeft = op is (int)VmOp.DivKr or (int)VmOp.DivuKr or (int)VmOp.ModKr or (int)VmOp.ModuKr;
                    var constantRight = op is (int)VmOp.DivRk or (int)VmOp.DivuRk or (int)VmOp.ModRk or (int)VmOp.ModuRk;
                    if (constantLeft && b >= constants.Length || constantRight && c >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    var numerator = constantLeft ? constants[b] : reg[b];
                    var denominator = constantRight ? constants[c] : reg[c];
                    if (denominator == 0)
                        return Stop(VmFault.DivisionByZero);
                    var remainder = op >= (int)VmOp.ModRr;
                    var unsigned = op is (int)VmOp.DivuRr or (int)VmOp.DivuRk or (int)VmOp.DivuKr
                        or (int)VmOp.ModuRr or (int)VmOp.ModuRk or (int)VmOp.ModuKr;
                    try
                    {
                        reg[a] = unsigned
                            ? unchecked((int)(remainder ? (uint)numerator % (uint)denominator : (uint)numerator / (uint)denominator))
                            : remainder ? numerator % denominator : numerator / denominator;
                    }
                    catch (OverflowException)
                    {
                        return Stop(VmFault.Overflow);
                    }

                    pc++;
                    break;
                case (int)VmOp.AndRr:
                    reg[a] = reg[b] & reg[c];
                    pc++;
                    break;
                case (int)VmOp.OrRr:
                    reg[a] = reg[b] | reg[c];
                    pc++;
                    break;
                case (int)VmOp.XorRr:
                    reg[a] = reg[b] ^ reg[c];
                    pc++;
                    break;
                case (int)VmOp.AndRk:
                case (int)VmOp.OrRk:
                case (int)VmOp.XorRk:
                case (int)VmOp.MinRr:
                case (int)VmOp.MinRk:
                case (int)VmOp.MaxRr:
                case (int)VmOp.MaxRk:
                case (int)VmOp.MinuRr:
                case (int)VmOp.MinuRk:
                case (int)VmOp.MaxuRr:
                case (int)VmOp.MaxuRk:
                    var useConstant = op is (int)VmOp.AndRk or (int)VmOp.OrRk or (int)VmOp.XorRk
                        or (int)VmOp.MinRk or (int)VmOp.MaxRk or (int)VmOp.MinuRk or (int)VmOp.MaxuRk;
                    if (useConstant && c >= constants.Length) return Stop(VmFault.ConstantRange);
                    var left = reg[b];
                    var right = useConstant ? constants[c] : reg[c];
                    reg[a] = op switch
                    {
                        (int)VmOp.AndRk => left & right,
                        (int)VmOp.OrRk => left | right,
                        (int)VmOp.XorRk => left ^ right,
                        (int)VmOp.MinRr or (int)VmOp.MinRk => Math.Min(left, right),
                        (int)VmOp.MaxRr or (int)VmOp.MaxRk => Math.Max(left, right),
                        (int)VmOp.MinuRr or (int)VmOp.MinuRk => (uint)left < (uint)right ? left : right,
                        _ => (uint)left > (uint)right ? left : right,
                    };
                    pc++;
                    break;
                case (int)VmOp.SllRr:
                case (int)VmOp.SllRi:
                case (int)VmOp.SllKr:
                case (int)VmOp.SrlRr:
                case (int)VmOp.SrlRi:
                case (int)VmOp.SrlKr:
                case (int)VmOp.SraRr:
                case (int)VmOp.SraRi:
                case (int)VmOp.SraKr:
                    var constantSource = op is (int)VmOp.SllKr or (int)VmOp.SrlKr or (int)VmOp.SraKr;
                    if (constantSource && b >= constants.Length) return Stop(VmFault.ConstantRange);
                    var shiftSource = constantSource ? constants[b] : reg[b];
                    var immediateShift = op is (int)VmOp.SllRi or (int)VmOp.SrlRi or (int)VmOp.SraRi;
                    var shiftCount = immediateShift ? c : reg[c];
                    if ((uint)shiftCount > 31) return Stop(VmFault.ShiftRange);
                    reg[a] = op switch
                    {
                        (int)VmOp.SllRr or (int)VmOp.SllRi or (int)VmOp.SllKr => shiftSource << shiftCount,
                        (int)VmOp.SrlRr or (int)VmOp.SrlRi or (int)VmOp.SrlKr => (int)((uint)shiftSource >> shiftCount),
                        _ => shiftSource >> shiftCount,
                    };
                    pc++;
                    break;
                case (int)VmOp.Abs:
                    reg[a] = reg[b] < 0 ? unchecked(-reg[b]) : reg[b];
                    pc++;
                    break;
                case (int)VmOp.Neg:
                    reg[a] = unchecked(-reg[b]);
                    pc++;
                    break;
                case (int)VmOp.Not:
                    reg[a] = ~reg[b];
                    pc++;
                    break;
                case (int)VmOp.EqR:
                case (int)VmOp.EqK:
                case (int)VmOp.LtRr:
                case (int)VmOp.LtRk:
                case (int)VmOp.LtKr:
                case (int)VmOp.LeRr:
                case (int)VmOp.LeRk:
                case (int)VmOp.LeKr:
                case (int)VmOp.LtuRr:
                case (int)VmOp.LtuRk:
                case (int)VmOp.LtuKr:
                case (int)VmOp.LeuRr:
                case (int)VmOp.LeuRk:
                case (int)VmOp.LeuKr:
                    var compareConstantLeft = op is (int)VmOp.LtKr or (int)VmOp.LeKr or (int)VmOp.LtuKr or (int)VmOp.LeuKr;
                    var compareConstantRight = op is (int)VmOp.EqK or (int)VmOp.LtRk or (int)VmOp.LeRk or (int)VmOp.LtuRk or (int)VmOp.LeuRk;
                    if (compareConstantLeft && b >= constants.Length || compareConstantRight && c >= constants.Length)
                        return Stop(VmFault.ConstantRange);
                    var compareLeft = compareConstantLeft ? constants[b] : reg[b];
                    var compareRight = compareConstantRight ? constants[c] : reg[c];
                    var test = op switch
                    {
                        (int)VmOp.EqR or (int)VmOp.EqK => compareLeft == compareRight,
                        (int)VmOp.LtRr or (int)VmOp.LtRk or (int)VmOp.LtKr => compareLeft < compareRight,
                        (int)VmOp.LeRr or (int)VmOp.LeRk or (int)VmOp.LeKr => compareLeft <= compareRight,
                        (int)VmOp.LtuRr or (int)VmOp.LtuRk or (int)VmOp.LtuKr => (uint)compareLeft < (uint)compareRight,
                        _ => (uint)compareLeft <= (uint)compareRight,
                    };
                    if (!Branch(code, ref pc, a, test))
                        return Stop(VmFault.BadJump);
                    break;
                default:
                    return Stop(VmFault.UnknownOpcode);
            }
        }

        return Stop(VmFault.RanOffEnd);
    }

    private static bool Branch(ReadOnlySpan<int> code, ref int pc, int a, bool test)
    {
        var expect = (a & 1) != 0;
        if (test != expect)
        {
            pc += 2;
            return true;
        }

        if ((uint)(pc + 1) >= (uint)code.Length || (code[pc + 1] & 0xFF) != (int)VmOp.Jmp)
            return false;
        pc += 1 + I24(code[pc + 1]) + 1;
        return true;
    }

    private static int I24(int word)
    {
        var value = (word >> 8) & 0xFFFFFF;
        if ((value & 0x800000) != 0)
            value -= 0x1000000;
        return value;
    }

    private static VmResult Stop(VmFault fault) => new(false, 0, fault);

    private static bool TryParseColor(string value, out int color)
    {
        color = 0;
        var terminator = value.IndexOf('\0');
        if (terminator >= 0) value = value[..terminator];
        if (value.StartsWith('#'))
        {
            if (value.Length is not (4 or 7)) return true;
            for (var component = 0; component < 3; component++)
            {
                var first = IntegerDigit(value[1 + component * (value.Length == 4 ? 1 : 2)]);
                var second = value.Length == 4 ? first : IntegerDigit(value[2 + component * 2]);
                if (first < 0 || second < 0) return false;
                color = (color << 8) | (first << 4) | second;
            }
            return true;
        }
        if (value.Length == 6 && int.TryParse(value, System.Globalization.NumberStyles.AllowHexSpecifier,
            System.Globalization.CultureInfo.InvariantCulture, out color)) return true;
        color = 0;
        var position = 0;
        for (var component = 0; component < 3; component++)
        {
            while (position < value.Length && value[position] <= ' ') position++;
            var start = position;
            while (position < value.Length && value[position] > ' ') position++;
            var length = position - start;
            var part = 0;
            if (length > 0)
            {
                var first = IntegerDigit(value[start]);
                var second = length == 1 ? first : IntegerDigit(value[start + 1]);
                if (first < 0 || second < 0) return false;
                part = (first << 4) | second;
            }
            color = (color << 8) | part;
        }
        return true;
    }

    private sealed class ReturnSlots
    {
        public VmResult[] Values { get; }
        public int Index { get; set; }
        public int Count { get; private set; }

        public ReturnSlots(int requested)
        {
            Values = new VmResult[requested];
            Array.Fill(Values, new VmResult(true, 0, VmFault.None));
        }

        public VmResult Finish()
        {
            Count = Math.Min(Index + 1, Values.Length);
            return Values.Length > 0 ? Values[0] : new VmResult(true, 0, VmFault.None);
        }
    }

    private static double ParseStringFloat(string value)
    {
        var prefix = System.Text.RegularExpressions.Regex.Match(value,
            @"^[ \t\n\r\v\f]*([+-]?)(?:(inf(?:inity)?)|(nan)(?:\([a-z0-9_]*\))?|(0x(?:[0-9a-f]+(?:\.[0-9a-f]*)?|\.[0-9a-f]+)(?:p[+-]?[0-9]+)?)|((?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:e[+-]?[0-9]+)?))",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (!prefix.Success) return 0;
        var negative = prefix.Groups[1].Value == "-";
        if (prefix.Groups[2].Success) return negative ? double.NegativeInfinity : double.PositiveInfinity;
        if (prefix.Groups[3].Success) return double.NaN;
        if (!prefix.Groups[4].Success)
            return double.Parse(prefix.Groups[1].Value + prefix.Groups[5].Value,
                System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
        var hex = prefix.Groups[4].Value.AsSpan(2);
        var exponentPosition = hex.IndexOfAny('p', 'P');
        var power = 0;
        if (exponentPosition >= 0)
        {
            var exponentText = hex[(exponentPosition + 1)..];
            var exponentNegative = exponentText[0] == '-';
            if (exponentText[0] is '+' or '-') exponentText = exponentText[1..];
            foreach (var digit in exponentText) power = Math.Min(100000, power * 10 + digit - '0');
            if (exponentNegative) power = -power;
            hex = hex[..exponentPosition];
        }
        var significand = System.Numerics.BigInteger.Zero;
        var fractional = false;
        foreach (var digit in hex)
        {
            if (digit == '.') { fractional = true; continue; }
            significand = (significand << 4) + IntegerDigit(digit);
            if (fractional) power -= 4;
        }
        if (significand.IsZero) return negative ? -0.0 : 0.0;
        var bits = significand.GetBitLength();
        var highestPower = bits - 1 + power;
        if (highestPower > 1023) return negative ? double.NegativeInfinity : double.PositiveInfinity;
        if (highestPower < -1075) return negative ? -0.0 : 0.0;
        var shift = (int)Math.Max(bits - 53, -1074L - power);
        if (shift > 0)
        {
            var rounded = significand >> shift;
            var remainder = significand - (rounded << shift);
            var half = System.Numerics.BigInteger.One << (shift - 1);
            if (remainder > half || remainder == half && !rounded.IsEven) rounded++;
            significand = rounded;
        }
        else significand <<= -shift;
        var result = Math.ScaleB((double)significand, power + shift);
        return negative ? -result : result;
    }

    private static int ParseStringInteger(string value)
    {
        var position = 0;
        while (position < value.Length && value[position] is ' ' or '\t' or '\n' or '\r' or '\v' or '\f') position++;
        var negative = position < value.Length && value[position] == '-';
        if (position < value.Length && value[position] is '+' or '-') position++;
        var radix = 10;
        if (position < value.Length && value[position] == '0')
        {
            radix = 8;
            if (position + 2 < value.Length && value[position + 1] is 'x' or 'X'
                && IntegerDigit(value[position + 2]) is >= 0 and < 16)
            {
                radix = 16;
                position += 2;
            }
        }
        var limit = negative ? 1UL << 63 : (ulong)long.MaxValue;
        ulong magnitude = 0;
        while (position < value.Length)
        {
            var digit = IntegerDigit(value[position]);
            if (digit < 0 || digit >= radix) break;
            magnitude = magnitude > (limit - (uint)digit) / (uint)radix ? limit : magnitude * (uint)radix + (uint)digit;
            position++;
        }
        // Native strtoll saturates to int64 bounds, then VM_SWORD retains the low 32 bits.
        return unchecked((int)(negative ? 0UL - magnitude : magnitude));
    }

    private static int IntegerDigit(char value) => value is >= '0' and <= '9' ? value - '0'
        : value is >= 'a' and <= 'f' ? value - 'a' + 10 : value is >= 'A' and <= 'F' ? value - 'A' + 10 : -1;

    private static string FormatFloat(double value)
    {
        if (double.IsNaN(value)) return "nan";
        if (double.IsPositiveInfinity(value)) return "inf";
        if (double.IsNegativeInfinity(value)) return "-inf";
        var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
        var exponent = (int)((bits >> 52) & 2047);
        var significand = bits & 0xFFFFFFFFFFFFFUL;
        if (exponent != 0) significand |= 1UL << 52;
        var power = exponent == 0 ? -1074 : exponent - 1075;
        var numerator = new System.Numerics.BigInteger(significand) * 100000;
        var denominator = System.Numerics.BigInteger.One;
        if (power >= 0) numerator <<= power;
        else denominator <<= -power;
        var rounded = System.Numerics.BigInteger.DivRem(numerator, denominator, out var remainder);
        var halfway = (remainder * 2).CompareTo(denominator);
        if (halfway > 0 || halfway == 0 && !rounded.IsEven) rounded++;
        var digits = rounded.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(6, '0');
        // Native %.5f formats the binary value, including signed zero, without exponent notation.
        return ((bits >> 63) != 0 ? "-" : "") + digits[..^5] + "." + digits[^5..];
    }

    private static int CompareStrings(string left, string right, bool ignoreCase)
    {
        var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
        var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);
        for (var index = 0; ; index++)
        {
            var leftByte = index < leftBytes.Length ? leftBytes[index] : 0;
            var rightByte = index < rightBytes.Length ? rightBytes[index] : 0;
            if (ignoreCase)
            {
                if (leftByte is >= 'A' and <= 'Z') leftByte += 'a' - 'A';
                if (rightByte is >= 'A' and <= 'Z') rightByte += 'a' - 'A';
            }
            if (leftByte != rightByte) return leftByte.CompareTo(rightByte);
            if (leftByte == 0) return 0;
        }
    }

    private static (double X, double Y, double Z, double W) MultiplyQuaternion(
        (double X, double Y, double Z, double W) left, (double X, double Y, double Z, double W) right) =>
        (left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
         left.W * right.Y - left.X * right.Z + left.Y * right.W + left.Z * right.X,
         left.W * right.Z + left.X * right.Y - left.Y * right.X + left.Z * right.W,
         left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z);

    private static double ApplyFloatFunction(VmFloatFunction function, double value) => function switch
    {
        VmFloatFunction.Abs => Math.Abs(value),
        VmFloatFunction.Neg => -value,
        VmFloatFunction.Exp => Math.Exp(value),
        VmFloatFunction.Log => Math.Log(value),
        VmFloatFunction.Log10 => Math.Log10(value),
        VmFloatFunction.Sqrt => Math.Sqrt(value),
        VmFloatFunction.Ceil => Math.Ceiling(value),
        VmFloatFunction.Floor => Math.Floor(value),
        VmFloatFunction.Acos => Math.Acos(value),
        VmFloatFunction.Asin => Math.Asin(value),
        VmFloatFunction.Atan => Math.Atan(value),
        VmFloatFunction.Cos => Math.Cos(value),
        VmFloatFunction.Sin => Math.Sin(value),
        VmFloatFunction.Tan => Math.Tan(value),
        VmFloatFunction.AcosDegrees => Math.Acos(value) * (180.0 / Math.PI),
        VmFloatFunction.AsinDegrees => Math.Asin(value) * (180.0 / Math.PI),
        VmFloatFunction.AtanDegrees => Math.Atan(value) * (180.0 / Math.PI),
        VmFloatFunction.CosDegrees => Math.Cos(value * (Math.PI / 180.0)),
        VmFloatFunction.SinDegrees => Math.Sin(value * (Math.PI / 180.0)),
        VmFloatFunction.TanDegrees => Math.Tan(value * (Math.PI / 180.0)),
        VmFloatFunction.Cosh => Math.Cosh(value),
        VmFloatFunction.Sinh => Math.Sinh(value),
        VmFloatFunction.Tanh => Math.Tanh(value),
        VmFloatFunction.Round => Math.Round(value, MidpointRounding.AwayFromZero),
        _ => throw new ArgumentOutOfRangeException(nameof(function)),
    };
}
