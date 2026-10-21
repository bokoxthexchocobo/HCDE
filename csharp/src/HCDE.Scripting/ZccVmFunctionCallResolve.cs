namespace HCDE.Scripting;

/// <summary><c>FxVMFunctionCall::Resolve</c> from <c>codegen.cpp</c> (9814–10156).</summary>
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

    public static bool NamedArgNotFoundNative(int count, int index) => index >= count;

    public static bool RelaxedNamedArgsNative(int versionMajor, int versionMinor) =>
        versionMajor > 4 || (versionMajor == 4 && versionMinor >= 13);

    public static bool ShouldSetReturnTypeFromProtoNative(bool failed, int returnTypeCount) =>
        !failed && returnTypeCount > 0;

    public static bool ShouldBlockJitOnFnPtrNative(bool fnPtrCall) => fnPtrCall;

    public static int ImplicitArgCountNative(int functionImplicit) => functionImplicit;

    public static bool NamedArgOutOfOrderNative(bool relaxed, bool optional, int index, int currentIndex) =>
        !relaxed && !optional && index < currentIndex;

    public static bool CannotNameVarArgNative(bool isVarArg, int index, int count) =>
        index >= count && isVarArg;

    public static int OrderedArgSlotNative(int index) => index;

    public static bool ResolveFailedDeleteExpressionNative(bool failed) => failed;

    public static bool ReturnVoidWhenNoProtoReturnsNative(int returnTypeCount) => returnTypeCount == 0;

    public static bool AccessibilityCheckRequiredNative(int version) => version > 0;

    public static bool SpecialFunctionHookMayReplaceNative(bool hasHook) => hasHook;

    public static int VarArgCountAdjustmentNative(bool isVarArg) => isVarArg ? 1 : 0;

    public static bool ShouldResizeOrderedArgsNative(int count) => count > 0;

    public static int NamedArgSearchIndexNative(int start) => start;

    public static bool ArgNameMatchesNative(string expected, string actual) => expected == actual;

    public static bool OptionalArgAllowsNamedNative(bool optional) => optional;

    public static bool StrictNamedArgRequiresOptionalNative(bool relaxed, bool optional) => !relaxed && !optional;

    public static int ProtoArgCountWithoutImplicitNative(int protoCount, int implicitCount, bool isVarArg) =>
        (protoCount - implicitCount) - (isVarArg ? 1 : 0);

    public static bool ShouldDeleteOnTooManyArgsNative(bool tooMany) => tooMany;

    public static bool ShouldDeleteOnMissingSelfNative(bool missingSelf) => missingSelf;

    public static bool ShouldDeleteOnAbstractCallNative(bool abstractCall) => abstractCall;

    public static bool ShouldDeleteOnNamedArgErrorNative(bool error) => error;

    public static bool ShouldDeleteOnAccessibilityFailureNative(bool failed) => failed;

    public static bool ShouldDeleteOnSpecialResolveFailureNative(bool failed) => failed;

    public static int ReturnTypeIndexNative() => 0;

    public static bool ValueTypeVoidNative(int returnCount) => returnCount == 0;

    public static bool ValueTypeFromProtoNative(int returnCount) => returnCount > 0;

    // Named-argument ordering mirrors the native `OrderedArgs` loop (9868–9935).
    public static int CountExplicitArgsNative(int argListSize) => argListSize;

    public static bool IsNamedNodeNative(int exprType, int namedNodeType) => exprType == namedNodeType;

    public static int FindArgIndexByNameNative(IReadOnlyList<string> argNames, string name, int implicitOffset)
    {
        for (var j = 0; j < argNames.Count; j++)
        {
            if (argNames[j] == name)
                return j + implicitOffset;
        }

        return -1;
    }

    public static bool ShouldAdvanceOrderedIndexNative(int foundIndex, int currentIndex, bool relaxed, bool optional) =>
        foundIndex >= 0 && (relaxed || optional || foundIndex >= currentIndex);

    public static bool ShouldFailNamedArgNotFoundNative(int foundIndex, int count) => foundIndex < 0 || foundIndex >= count;

    public static bool ShouldFailNamedArgBeforeRequiredNative(bool relaxed, bool optional, int foundIndex, int currentIndex) =>
        !relaxed && !optional && foundIndex < currentIndex;

    public static void PlaceOrderedArgNative(object?[] slots, int index, object? expression) =>
        slots[index] = expression;

    public static bool ShouldFillRemainingOrderedArgsNative(int index, int count) => index < count;

    public static int NextOrderedArgIndexNative(int index) => index + 1;

    public static bool ShouldResolveArgExpressionNative(bool failed) => !failed;

    public static bool ShouldApplyDefaultArgsNative(bool fnPtrCall) => !fnPtrCall;

    public static int DefaultArgRegTypeIntNative() => 0;

    public static int DefaultArgRegTypeFloatNative() => 1;

    public static int DefaultArgRegTypePointerNative() => 2;

    public static int DefaultArgRegTypeStringNative() => 3;

    public static bool ShouldContinueDefaultArgLoopNative(int emitterCount, int defaultCount) =>
        emitterCount < defaultCount;

    public static bool ResolveReturnsExpressionNative(bool failed) => !failed;

    public static bool ShouldAssignReturnTypeFromFirstProtoNative(int returnTypes) => returnTypes > 0;

    public static bool ShouldAssignVoidReturnTypeNative(int returnTypes) => returnTypes == 0;

    public static bool CheckResolvedNative(bool failed, object? result) => !failed && result is not null;

    public static int Version413Native() => 413;

    public static bool AtLeastVersionNative(int major, int minor, int reqMajor, int reqMinor) =>
        major > reqMajor || (major == reqMajor && minor >= reqMinor);

    public static bool ShouldValidateArgListNonEmptyNative(int size) => size > 0;

    public static bool ProtoHasArgumentsNative(int protoArgCount) => protoArgCount > 0;

    public static bool LastProtoArgIsVarArgNative(bool isVarArg) => isVarArg;

    public static int VarArgSliceStartNative(int count, bool isVarArg) => isVarArg ? count - 1 : count;

    public static bool ShouldDeleteOnResolveFailureNative(bool failed) => failed;

    public static bool ShouldKeepExpressionOnResolveNative(bool failed) => !failed;

    public static int EfxNamedNodeNative() => 1;

    public static bool ShouldCompareArgNameNative(string left, string right) => left == right;

    public static bool ShouldIncrementNamedSearchNative(bool found) => found;

    public static bool ShouldResetFailedFlagNative() => false;

    public static bool ShouldMarkFailedOnNullResolveNative(object? resolved) => resolved is null;

    public static bool ShouldUseVoidReturnNative(int returnCount) => returnCount == 0;

    public static bool ShouldUseFirstReturnNative(int returnCount) => returnCount > 0;

    public static bool ShouldBlockJitWhenFnPtrNative(bool fnPtrCall) => fnPtrCall;

    public static bool ShouldResolveSelfBeforeArgsNative(bool hasSelf) => hasSelf;

    public static bool ShouldAbortWhenInaccessibleNative(bool accessible) => !accessible;

    public static bool ShouldAbortWhenSpecialResolveFailsNative(bool ok) => !ok;

    public static bool ShouldAbortWhenTooManyArgsNative(bool tooMany) => tooMany;

    public static bool ShouldSelectVoidValueTypeNative(int returns) => returns == 0;

    public static bool ShouldSelectProtoValueTypeNative(int returns) => returns > 0;
}
