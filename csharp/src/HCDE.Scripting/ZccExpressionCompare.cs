namespace HCDE.Scripting;

/// <summary>Native <c>FxCompareRel</c> / <c>FxCompareEq</c> integer paths from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionCompare
{
    public static bool TryFoldRelational(int op, int left, int right, out bool result)
    {
        result = op switch
        {
            '<' => left < right,
            '>' => left > right,
            _ => false,
        };
        return op is '<' or '>';
    }

    public static bool TryFoldEquality(int op, int left, int right, out bool result)
    {
        result = op switch
        {
            '=' or 'e' => left == right,
            '!' => left != right,
            _ => false,
        };
        return op is '=' or 'e' or '!';
    }

    public static VmScriptFunction EmitRelational(int op)
    {
        if (op == '<')
            return EmitBooleanCompare(VmOp.LtRr, 0, 1);
        if (op == '>')
            return EmitBooleanCompare(VmOp.LtRr, 1, 0);
        throw new ArgumentOutOfRangeException(nameof(op));
    }

    public static VmScriptFunction EmitEquality(bool equal)
    {
        return equal
            ? EmitBooleanCompare(VmOp.EqR, 0, 1)
            : new VmScriptFunction(
            [
                VmCode.Li(2, 1),
                VmCode.Cmp(VmOp.EqR, 2, 0, 1),
                VmCode.Jmp(1),
                VmCode.Li(2, 0),
                VmCode.RetInt(2),
            ], [], [VmValueType.Integer, VmValueType.Integer]);
    }

    private static VmScriptFunction EmitBooleanCompare(VmOp compareOp, int leftReg, int rightReg)
    {
        return new VmScriptFunction(
        [
            VmCode.Li(2, 0),
            VmCode.Cmp(compareOp, 2, leftReg, rightReg),
            VmCode.Jmp(1),
            VmCode.Li(2, 1),
            VmCode.RetInt(2),
        ], [], [VmValueType.Integer, VmValueType.Integer]);
    }
}
