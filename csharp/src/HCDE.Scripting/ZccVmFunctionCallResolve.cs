namespace HCDE.Scripting;

/// <summary><c>FxVMFunctionCall::Resolve</c> guards and named-argument placement (<c>codegen.cpp</c>).</summary>
public static class ZccVmFunctionCallResolve
{
    public static bool RequiresSelfPointerNative(bool isMethod, object? self) =>
        isMethod && self is null;

    public static bool RejectsAbstractDirectCallNative(bool noVirtual, bool isAbstract) =>
        noVirtual && isAbstract;

    public static bool TooManyArgumentsNative(int argCount, int implicitArgs, int protoArgCount, bool isVarArg) =>
        protoArgCount == 0 || (protoArgCount > 0 && !isVarArg && argCount + implicitArgs > protoArgCount);

    public static bool NamedArgsForbiddenInFnPtrNative(bool fnPtrCall, bool namedNode) =>
        fnPtrCall && namedNode;

    public static bool RelaxedNamedArgsNative(int versionMajor, int versionMinor) =>
        versionMajor > 4 || (versionMajor == 4 && versionMinor >= 13);

    public static bool NamedArgNotFoundNative(int count, int index) => index >= count;

    public static bool CannotNameVarArgNative(bool isVarArg, int index, int count) =>
        index >= count && isVarArg;

    public static bool NamedArgOutOfOrderNative(bool relaxed, bool optional, int foundIndex, int currentIndex) =>
        !relaxed && !optional && foundIndex < currentIndex;

    public static bool NamedArgRequiresPriorOptionalNative(bool relaxed, bool optional) =>
        !relaxed && !optional;

    public static int FindArgIndexByNameNative(IReadOnlyList<string> argNames, string name, int implicitOffset)
    {
        for (var j = 0; j < argNames.Count; j++)
        {
            if (argNames[j] == name)
                return j + implicitOffset;
        }

        return -1;
    }

    public static bool ShouldSetReturnTypeFromProtoNative(bool failed, int returnTypeCount) =>
        !failed && returnTypeCount > 0;

    public static bool ShouldAssignVoidReturnTypeNative(int returnTypeCount) => returnTypeCount == 0;

    public enum VmFunctionCallGuardFailure
    {
        None,
        MissingSelf,
        AbstractCall,
        TooManyArgs,
        NamedArgsInFnPtr,
        NamedArgNotFound,
        NamedArgOutOfOrder,
    }

    public readonly record struct VmFunctionCallGuardInput(
        bool IsMethod,
        object? Self,
        bool NoVirtual,
        bool IsAbstract,
        int ArgCount,
        int ImplicitArgs,
        int ProtoArgCount,
        bool IsVarArg,
        bool FnPtrCall,
        bool NamedNode,
        int VersionMajor,
        int VersionMinor,
        IReadOnlyList<string> ArgNames,
        string NamedArg,
        int NamedArgIndex,
        int CurrentArgIndex,
        bool NamedArgOptional);

    public static VmFunctionCallGuardFailure RunResolveGuardPipeline(VmFunctionCallGuardInput input)
    {
        if (RequiresSelfPointerNative(input.IsMethod, input.Self))
            return VmFunctionCallGuardFailure.MissingSelf;
        if (RejectsAbstractDirectCallNative(input.NoVirtual, input.IsAbstract))
            return VmFunctionCallGuardFailure.AbstractCall;
        if (TooManyArgumentsNative(input.ArgCount, input.ImplicitArgs, input.ProtoArgCount, input.IsVarArg))
            return VmFunctionCallGuardFailure.TooManyArgs;
        if (NamedArgsForbiddenInFnPtrNative(input.FnPtrCall, input.NamedNode))
            return VmFunctionCallGuardFailure.NamedArgsInFnPtr;
        if (input.NamedNode)
        {
            var relaxed = RelaxedNamedArgsNative(input.VersionMajor, input.VersionMinor);
            var index = FindArgIndexByNameNative(input.ArgNames, input.NamedArg, input.ImplicitArgs);
            if (NamedArgNotFoundNative(input.ArgNames.Count, index))
                return VmFunctionCallGuardFailure.NamedArgNotFound;
            if (NamedArgOutOfOrderNative(relaxed, input.NamedArgOptional, index, input.CurrentArgIndex))
                return VmFunctionCallGuardFailure.NamedArgOutOfOrder;
        }

        return VmFunctionCallGuardFailure.None;
    }
}
