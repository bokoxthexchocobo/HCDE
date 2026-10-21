namespace HCDE.Scripting;

/// <summary><c>FxVMFunctionCall::Emit</c> and <c>CheckEmitCast</c> decision helpers (<c>codegen.cpp</c>).</summary>
public static class ZccVmFunctionCallEmit
{
    public static bool ShouldEarlyCastReturnNative(int count) => count == 1;

    public static bool IsStaticCallNative(bool fnPtrCall, bool isFinal, bool noVirtualIndex, bool noVirtualFlag) =>
        fnPtrCall || isFinal || noVirtualIndex || noVirtualFlag;

    public static bool ShouldEmitScopeBarrierNative(int innerSide, int selfSide, int outerSide) =>
        innerSide == 3 && selfSide == 0 && outerSide != 3;

    public static bool ShouldPassActionStatePointersNative(int numImplicits, int nap, int selfReg) =>
        numImplicits == nap && selfReg == 0;

    public static bool ShouldPassSelfAsStateOwnerNative(int numImplicits, int nap, int selfReg) =>
        !(numImplicits == nap && selfReg == 0);

    public static bool IsDecorateCastKludgeNative(string funcName) =>
        funcName is "__decorate_internal_int__" or "__decorate_internal_bool__" or "__decorate_internal_float__";

    public static bool ShouldEmitRetIntImmediateNative(bool isConstant, string funcName) =>
        isConstant && funcName is "__decorate_internal_int__" or "__decorate_internal_bool__";

    public static bool ShouldNullCheckFnPtrSelfNative(bool fnPtrCall) => fnPtrCall;

    public static bool ShouldSetVirtualRegNative(bool staticCall) => !staticCall;

    public static int MultiReturnAssignCountNative(int assignCount) => Math.Max(assignCount, 1);

    public static bool ShouldAddReturnSlotsNative(int resultCount) => resultCount > 0;

    public static int NapConstantNative() => 3;

    public static bool ActionFunctionUsesStatePointersNative(bool actionFlag, int numImplicits, int nap) =>
        actionFlag && numImplicits == nap;

    public static bool CheckEmitCastReturnsTrueNative(string funcName) =>
        IsDecorateCastKludgeNative(funcName);

    public static bool ShouldEmitArgDirectlyNative(bool returnIt) => !returnIt;

    public static bool ShouldEmitRetFromArgNative(bool returnIt, bool isConstant, bool intOrBool) =>
        returnIt && isConstant && intOrBool;

    public static bool ShouldEmitOpRetNative(bool returnIt) => returnIt;

    public static bool MatchesDefaultArgRegTypeNative(int regType, int expectedRegType) => regType == expectedRegType;
}
