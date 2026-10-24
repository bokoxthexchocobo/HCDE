namespace HCDE.Gamedata;

public sealed class VmXlatExprState
{
    public int Tag { get; set; }
    public int LineType { get; set; }
    public bool IsConstant { get; set; } = true;
}

/// <summary>Xlat expression evaluators from <c>p_xlat.cpp</c>.</summary>
public static class VmXlatExpressionEval
{
    public static int Eval(int opcode, int left, int right, VmXlatExprState state)
    {
        return opcode switch
        {
            0 => left,
            1 => state.Tag,
            2 => left + right,
            3 => left - right,
            4 => left * right,
            5 => right == 0 ? 0 : left / right,
            6 => right == 0 ? 0 : left % right,
            7 => left & right,
            8 => left | right,
            9 => left ^ right,
            10 => -left,
            _ => 0
        };
    }

    public static void Div0Check(ref int op1, ref int op2, VmXlatExprState state)
    {
        if (op2 != 0) return;
        op1 = 0;
        op2 = 1;
    }
}
