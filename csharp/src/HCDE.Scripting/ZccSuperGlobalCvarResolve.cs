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

    public static string GlobalVariableLoadOp(bool useBitLoad, string memberLoadOp) =>
        useBitLoad ? "OP_LBIT" : memberLoadOp;

    public static string EmitLoadOp(CvarRealType type) => type switch
    {
        CvarRealType.String => "OP_LS",
        CvarRealType.Float => "OP_LSP",
        CvarRealType.Bool => "OP_LBU",
        CvarRealType.Flag => "OP_LW",
        CvarRealType.Mask => "OP_LW",
        _ => "OP_LW",
    };

    public static bool CvarEmitUsesStringRegister(CvarRealType type) => type == CvarRealType.String;

    public static bool CvarResolveDeletesOnUnknown(CvarRealType type) => type == CvarRealType.Unknown;

    public static int MaskCvarShift(int bitNum) => bitNum;

    public static int FlagCvarBitShift(int bitNum) => bitNum;

    public static string FlagCvarEmitSequence(int bitNum) =>
        $"OP_LW;OP_SRL_RI:{bitNum};OP_AND_RK:1";

    public static string MaskCvarEmitSequence(int bitVal, int bitNum) =>
        $"OP_LW;OP_AND_RK:{bitVal};OP_SRL_RI:{bitNum}";

    public static bool SelfEmitIncludesCheck(bool check) => check;

    public static int SelfPointerRegister => 0;
}
