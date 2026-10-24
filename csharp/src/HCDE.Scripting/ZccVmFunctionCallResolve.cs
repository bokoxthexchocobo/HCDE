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
}
