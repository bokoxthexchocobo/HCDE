namespace HCDE.Scripting;

/// <summary>Native <c>FxAddSub::Resolve</c> integer constant folding from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionConstantFolder
{
    public static bool TryFoldAddSub(int op, int left, int right, out int result)
    {
        result = op switch
        {
            '+' => left + right,
            '-' => left - right,
            _ => 0,
        };
        return op is '+' or '-';
    }

    public static bool TryFoldMulDiv(int op, int left, int right, out int result)
    {
        result = op switch
        {
            '*' => left * right,
            '/' => right == 0 ? 0 : left / right,
            '%' => right == 0 ? 0 : left % right,
            _ => 0,
        };
        return op is '*' or '/' or '%';
    }

    public static bool TryFoldUnaryNegate(int value, out int result)
    {
        result = -value;
        return true;
    }

    public static VmScriptFunction EmitFoldedIntegerReturn(int value)
    {
        return new VmScriptFunction([VmCode.Reti((short)value)], [], []);
    }
}
