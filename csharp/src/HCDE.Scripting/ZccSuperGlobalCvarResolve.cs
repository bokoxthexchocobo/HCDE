namespace HCDE.Scripting;

/// <summary><c>FxSuper</c>, <c>FxGlobalVariable</c>, and <c>FxCVar</c> bands from <c>codegen.cpp</c> (7318–7508).</summary>
public static class ZccSuperGlobalCvarResolve
{
    public enum CvarRealType
    {
        Bool,
        Flag,
        Int,
        Mask,
        Color,
        Float,
        String,
        Unknown,
    }

    public readonly record struct GlobalVariableEmitPlan(bool LookupPointer, bool UseBitLoad, string LoadOp, int BitMask);

    public readonly record struct CvarEmitPlan(string LookupOp, string LoadOp, bool UsesStringRegister, int ExtraShift, int AndMask);

    public static bool TryResolveSuper(bool hasFunction, bool hasSelfClass, out bool invalidType)
    {
        invalidType = !(hasFunction && hasSelfClass);
        return !invalidType;
    }

    public static bool GlobalVariableWritable(bool addressWritable, bool ctxWritable) =>
        addressWritable && ctxWritable;

    public static CvarRealType MapCvarRealType(int nativeType) => nativeType switch
    {
        0 or 1 => CvarRealType.Bool,
        2 or 6 => CvarRealType.Int,
        3 => CvarRealType.Color,
        4 => CvarRealType.Float,
        5 => CvarRealType.String,
        _ => CvarRealType.Unknown,
    };

    public static bool ShouldUseBitLoad(int bitValue) => bitValue != -1;

    public static GlobalVariableEmitPlan PlanGlobalVariableEmit(bool addressRequested, int bitValue, string memberLoadOp)
    {
        if (ShouldUseBitLoad(bitValue))
            return new GlobalVariableEmitPlan(true, true, "OP_LBIT", 1 << bitValue);
        return new GlobalVariableEmitPlan(addressRequested, false, memberLoadOp, 0);
    }

    public static CvarEmitPlan PlanCvarEmit(CvarRealType type, int flagBitNum = 0, int maskBitVal = 0, int maskBitNum = 0) => type switch
    {
        CvarRealType.String => new CvarEmitPlan("OP_LKP", "OP_LS", true, 0, 0),
        CvarRealType.Float => new CvarEmitPlan("OP_LKP", "OP_LSP", false, 0, 0),
        CvarRealType.Bool => new CvarEmitPlan("OP_LKP", "OP_LBU", false, 0, 0),
        CvarRealType.Color => new CvarEmitPlan("OP_LKP", "OP_LW", false, 0, 0),
        CvarRealType.Int => new CvarEmitPlan("OP_LKP", "OP_LW", false, 0, 0),
        CvarRealType.Flag => new CvarEmitPlan("OP_LKP", "OP_LW", false, flagBitNum, 1),
        CvarRealType.Mask => new CvarEmitPlan("OP_LKP", "OP_LW", false, maskBitNum, maskBitVal),
        _ => new CvarEmitPlan("OP_LKP", "OP_LW", false, 0, 0),
    };

    public static bool CvarResolveDeletesOnUnknown(CvarRealType type) => type == CvarRealType.Unknown;

    public static bool CvarEmitUsesStringRegister(CvarRealType type) => type == CvarRealType.String;

    public static int ApplyFlagCvarShift(int loadedWord, int bitNum) => loadedWord >> bitNum;

    public static int ApplyFlagCvarMask(int shifted, int andMask) => shifted & andMask;

    public static int ApplyMaskCvar(int loadedWord, int bitVal, int bitNum) => (loadedWord & bitVal) >> bitNum;

    public static bool GlobalVariableReturnsPointerWhenRequested(bool addressRequested, int regOffset) =>
        addressRequested && regOffset == 0;
}
