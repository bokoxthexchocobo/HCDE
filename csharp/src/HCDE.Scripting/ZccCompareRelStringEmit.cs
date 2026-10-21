namespace HCDE.Scripting;

/// <summary>String branch of <c>FxCompareRel::EmitCommon</c> and emit wrappers from <c>codegen.cpp</c> (4055–4156).</summary>
public static class ZccCompareRelStringEmit
{
    public const int CmpLt = 1;
    public const int CmpLe = 2;
    public const int CmpCheck = 4;
    public const int CmpBk = 8;
    public const int CmpCk = 16;

    public static int StringCompareOpcode(char op, bool invert) =>
        op switch
        {
            '<' => invert ? CmpLe | CmpCheck : CmpLt,
            '>' => invert ? CmpLt | CmpCheck : CmpLe | CmpCheck,
            _ => CmpLe,
        };

    public static int ApplyKonstFlags(int opcode, bool leftKonst, bool rightKonst)
    {
        if (leftKonst) opcode |= CmpBk;
        if (rightKonst) opcode |= CmpCk;
        return opcode;
    }

    public static bool ShouldMaterializeBoolResult(bool forCompare) => !forCompare;
}
