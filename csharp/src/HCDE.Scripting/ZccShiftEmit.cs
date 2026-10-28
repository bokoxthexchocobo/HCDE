namespace HCDE.Scripting;

/// <summary><c>FxShift::Emit</c> from <c>codegen.cpp</c> (4637–4684).</summary>
public static class ZccShiftEmit
{
    public static readonly int[][] InstrMap =
    {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
    };

    public static int OperatorIndex(int tkLShift, int tkRShift, int tkUrShift, int operatorToken) =>
        operatorToken == tkLShift ? 0 : operatorToken == tkRShift ? 1 : operatorToken == tkUrShift ? 2 : -1;

    public static int SelectInstruction(int index, bool leftKonst, bool rightIsImmediate)
    {
        if (leftKonst)
            return InstrMap[index][1];
        return rightIsImmediate ? InstrMap[index][2] : InstrMap[index][0];
    }
}
