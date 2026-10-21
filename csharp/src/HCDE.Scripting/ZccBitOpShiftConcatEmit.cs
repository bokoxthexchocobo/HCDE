namespace HCDE.Scripting;

/// <summary><c>FxBitOp::Emit</c>, <c>FxShift::Emit</c>, and <c>FxConcat::Emit</c> from <c>codegen.cpp</c> (4546–4926).</summary>
public static class ZccBitOpShiftConcatEmit
{
    public static readonly int[][] ShiftInstrMap =
    {
        new[] { 1, 2, 3 },
        new[] { 4, 5, 6 },
        new[] { 7, 8, 9 },
    };

    public const int OpAndRr = 1;
    public const int OpOrRr = 2;
    public const int OpXorRr = 3;

    public static int BitOpInstruction(char op) =>
        op switch
        {
            '&' => OpAndRr,
            '|' => OpOrRr,
            '^' => OpXorRr,
            _ => -1,
        };

    public static int BitOpEmitOpcode(int instr, bool rightKonst) =>
        rightKonst ? instr + 1 : instr;

    public static int ShiftInstrIndex(int opLShift, int opRShift, int opUrShift, int operatorToken) =>
        operatorToken == opLShift ? 0 : operatorToken == opRShift ? 1 : operatorToken == opUrShift ? 2 : -1;

    public static int ShiftEmitOpcode(int[][] instrMap, int index, bool leftKonst, bool rightImmediate) =>
        leftKonst ? instrMap[index][1] : rightImmediate ? instrMap[index][2] : instrMap[index][0];

    public static int ConcatCastOpcode(bool isFloat, int regCount, bool isUInt, bool isName, bool isSound, bool isColor, bool isPointer, bool isInt)
    {
        if (isFloat) return regCount switch { 1 => 1, 2 => 2, 3 => 3, _ => 4 };
        if (isUInt) return 5;
        if (isName) return 6;
        if (isSound) return 7;
        if (isColor) return 8;
        if (isPointer) return 9;
        if (isInt) return 10;
        return -1;
    }

    public static bool UseStringKonstLoad(bool isString, bool isKonst) => isString && isKonst;

    public static int ShiftRightImmediateFromConstant(int value) => value;

    public static bool ShouldSwapKonstOperands(bool leftKonst) => leftKonst;

    public static int SelectShiftEmitOpcode(int operatorIndex, bool leftKonst, bool rightIsImmediate) =>
        ShiftEmitOpcode(ShiftInstrMap, operatorIndex, leftKonst, rightIsImmediate);

    public static int InvertCompareCheck(int check, bool invert) => invert ? check ^ 1 : check;
}
