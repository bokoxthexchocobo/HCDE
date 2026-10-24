namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::ScriptCall</c> (p_acs.cpp).</summary>
internal static class AcsScriptCallService
{
    public static bool TryInvoke(
        AuthoritySimulation sim,
        List<int> stack,
        Actor? activator,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        int argCount,
        out int result)
    {
        result = 0;
        if (argCount < 2 || stack.Count < argCount)
            return false;

        var start = stack.Count - argCount;
        var rawArgs = stack.GetRange(start, argCount);
        var className = AcsStringIds.Lookup(rawArgs[0], stringTable, globalStrings);
        var functionName = AcsStringIds.Lookup(rawArgs[1], stringTable, globalStrings);
        stack.RemoveRange(start, argCount);

        if (!sim.ScriptCalls.TryGet(className, functionName, out var binding))
            return true;

        var invocationArgs = new List<AcsScriptCallArgument>();
        var typeIndex = binding.LeadingActorParameter ? 1 : 0;
        if (binding.LeadingActorParameter)
            invocationArgs.Add(new(AcsScriptValueKind.Int32, 0, null));

        for (var acsIndex = 2; acsIndex < rawArgs.Count; acsIndex++)
        {
            if (typeIndex >= binding.ParameterTypes.Length)
                return true;

            var kind = binding.ParameterTypes[typeIndex++];
            var raw = rawArgs[acsIndex];
            invocationArgs.Add(ConvertArgument(kind, raw, stringTable, globalStrings));
        }

        if (typeIndex < binding.ParameterTypes.Length)
        {
            var optionalIndex = typeIndex;
            if (optionalIndex >= binding.OptionalParameters.Length || !binding.OptionalParameters[optionalIndex])
                return true;
        }

        var outcome = binding.Invoke(new(activator, invocationArgs, stringTable));
        result = binding.ReturnType switch
        {
            AcsScriptValueKind.Void => 0,
            AcsScriptValueKind.Float64 => AcsFixedPoint.FromDouble(outcome.DoubleResult ?? 0),
            AcsScriptValueKind.String => globalStrings.Add(outcome.StringResult ?? string.Empty),
            AcsScriptValueKind.Name => globalStrings.Add(outcome.StringResult ?? string.Empty),
            AcsScriptValueKind.Sound => globalStrings.Add(outcome.StringResult ?? string.Empty),
            _ => outcome.IntResult,
        };
        return true;
    }

    private static AcsScriptCallArgument ConvertArgument(
        AcsScriptValueKind kind,
        int raw,
        string[] stringTable,
        AcsGlobalStrings globalStrings) =>
        kind switch
        {
            AcsScriptValueKind.Bool => new(kind, raw != 0 ? 1 : 0, null),
            AcsScriptValueKind.Float64 => new(kind, raw, null),
            AcsScriptValueKind.Name => new(kind, 0, AcsStringIds.Lookup(raw, stringTable, globalStrings)),
            AcsScriptValueKind.String => new(kind, 0, AcsStringIds.Lookup(raw, stringTable, globalStrings)),
            AcsScriptValueKind.Sound => new(kind, 0, AcsStringIds.Lookup(raw, stringTable, globalStrings)),
            _ => new(AcsScriptValueKind.Int32, raw, null),
        };
}
