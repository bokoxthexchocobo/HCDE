namespace HCDE.Scripting;

/// <summary>Native <c>FxLocalVariable</c> / <c>FxStaticArrayVariable</c> address rules.</summary>
public static class ZccLocalVariableAccess
{
    public const int VarFlagOut = ZccMemberFunctionCallResolve.VarFlagOut;

    public static bool IsOutVariable(int varFlags) => (varFlags & VarFlagOut) != 0;

    public static bool ShouldLoadThroughOutPointer(bool addressRequested, bool isOut) =>
        isOut && !addressRequested;

    public static bool IsWritable(int varFlags, Func<int, bool> isWritableInContext) =>
        isWritableInContext(varFlags);

    public static bool StaticArrayIsReadOnlyAddress(bool addressRequested) => addressRequested;
}
