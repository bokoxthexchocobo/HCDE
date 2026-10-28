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

    public static bool GlobalVariableRequestAddress(
        bool addressWritable,
        bool ctxWritable,
        bool ctxMemberWritable,
        out bool writable)
    {
        writable = addressWritable && ctxWritable && ctxMemberWritable;
        return true;
    }

    public static bool GlobalVariableWritable(bool addressWritable, bool ctxWritable) =>
        addressWritable && ctxWritable;

    public static bool ShouldUseBitLoad(int bitValue) => bitValue != -1;

    public static int GlobalVariableBitMask(int bitValue) => 1 << bitValue;

    public static string GlobalVariableLoadOp(bool useBitLoad, string memberLoadOp) =>
        useBitLoad ? "OP_LBIT" : memberLoadOp;

    public static CvarRealType MapCvarRealType(int nativeType) => nativeType switch
    {
        0 => CvarRealType.Bool,
        5 => CvarRealType.Flag,
        1 => CvarRealType.Int,
        6 => CvarRealType.Mask,
        4 => CvarRealType.Color,
        2 => CvarRealType.Float,
        3 => CvarRealType.String,
        _ => CvarRealType.Unknown,
    };

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

    public static bool CvarResolveDeletesOnUnknown(CvarRealType type) => type == CvarRealType.Unknown;

    public static bool CvarEmitUsesStringRegister(CvarRealType type) => type == CvarRealType.String;

    public static int ApplyFlagCvarShift(int loadedWord, int bitNum) => loadedWord >> bitNum;

    public static int ApplyFlagCvarMask(int shifted, int andMask) => shifted & andMask;

    public static int ApplyMaskCvar(int loadedWord, int bitVal, int bitNum) => (loadedWord & bitVal) >> bitNum;
}
