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


    public static int MultiReturnAssignCountNative(int assignCount) => Math.Max(assignCount, 1);

    public static bool ShouldAddReturnSlotsNative(int resultCount) => resultCount > 0;

    public static int NapConstantNative() => 3;

    public static bool ActionFunctionUsesStatePointersNative(bool actionFlag, int numImplicits, int nap) =>
        actionFlag && numImplicits == nap;

    public static bool CheckEmitCastReturnsTrueNative(string funcName) =>
        IsDecorateCastKludgeNative(funcName);


    public static bool ShouldEmitRetFromArgNative(bool returnIt, bool isConstant, bool intOrBool) =>
        returnIt && isConstant && intOrBool;

    public static bool ShouldEmitOpRetNative(bool returnIt) => returnIt;

    public static bool MatchesDefaultArgRegTypeNative(int regType, int expectedRegType) => regType == expectedRegType;

    public readonly record struct VmFunctionCallEmitInput(
        int ReturnTypeCount,
        bool FnPtrCall,
        bool IsFinal,
        bool NoVirtualIndex,
        bool NoVirtualFlag,
        int InnerSide,
        int SelfSide,
        int OuterSide,
        int NumImplicits,
        int SelfReg,
        string FuncName,
        bool IsConstant);

    public static bool RunEmitProloguePipeline(VmFunctionCallEmitInput input)
    {
        if (ShouldEarlyCastReturnNative(input.ReturnTypeCount))
            return true;
        if (IsStaticCallNative(input.FnPtrCall, input.IsFinal, input.NoVirtualIndex, input.NoVirtualFlag))
            return true;
        if (ShouldEmitScopeBarrierNative(input.InnerSide, input.SelfSide, input.OuterSide))
            return true;
        return ShouldPassActionStatePointersNative(input.NumImplicits, NapConstantNative(), input.SelfReg);
    }

    public static bool RunCheckEmitCastPipeline(string funcName, bool isConstant) =>
        CheckEmitCastReturnsTrueNative(funcName)
        || ShouldEmitRetIntImmediateNative(isConstant, funcName);
}
