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

    public static bool TryResolveSelf(bool hasFunction, bool hasSelfClass) =>
        hasFunction && hasSelfClass;

    public static bool GlobalVariableWritable(bool addressWritable, bool ctxWritable) =>
        addressWritable && ctxWritable;

    public static bool GlobalVariableAddressRequested(bool requested, bool writable, out bool outWritable)
    {
        outWritable = writable;
        return requested;
    }

    public static string GlobalVariableLoadOp(bool useBitLoad, string memberLoadOp) =>
        useBitLoad ? "OP_LBIT" : memberLoadOp;

    public static string EmitLoadOp(CvarRealType type) => type switch
    {
        CvarRealType.String => "OP_LS",
        CvarRealType.Float => "OP_LSP",
        CvarRealType.Bool => "OP_LBU",
        CvarRealType.Flag => "OP_LW",
        CvarRealType.Mask => "OP_LW",
        CvarRealType.Color => "OP_LW",
        CvarRealType.Int => "OP_LW",
        _ => "OP_LW",
    };

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
        if (addressRequested)
            return new GlobalVariableEmitPlan(true, false, "REGT_POINTER", 0);
        if (ShouldUseBitLoad(bitValue))
            return new GlobalVariableEmitPlan(true, true, "OP_LBIT", 1 << bitValue);
        return new GlobalVariableEmitPlan(false, false, memberLoadOp, 0);
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

    public static int MaskCvarShift(int bitNum) => bitNum;

    public static int FlagCvarBitShift(int bitNum) => bitNum;

    public static string FlagCvarEmitSequence(int bitNum) =>
        $"OP_LW;OP_SRL_RI:{bitNum};OP_AND_RK:1";

    public static string MaskCvarEmitSequence(int bitVal, int bitNum) =>
        $"OP_LW;OP_AND_RK:{bitVal};OP_SRL_RI:{bitNum}";

    public static bool CvarEmitUsesPointerLookup(CvarRealType type) => type != CvarRealType.Unknown;

    public static bool CvarResolveMapsToBool(CvarRealType type) =>
        type is CvarRealType.Bool or CvarRealType.Flag;

    public static bool CvarResolveMapsToInt32(CvarRealType type) =>
        type is CvarRealType.Int or CvarRealType.Mask;

    public static bool SelfEmitIncludesCheck(bool check) => check;

    public static int SelfPointerRegister => 0;
}
