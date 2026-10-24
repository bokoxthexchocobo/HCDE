namespace HCDE.Scripting;

/// <summary>Native <c>FxAddSub::Emit</c> and <c>FxMulDiv::Emit</c> integer paths from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionIntegerBinary
{
    public static VmScriptFunction EmitAddSub(int op, bool rightOperandIsConstant = false)
    {
        if (op is not ('+' or '-'))
            throw new ArgumentOutOfRangeException(nameof(op));
        var vmOp = op switch
        {
            '+' => rightOperandIsConstant ? VmOp.AddRk : VmOp.AddRr,
            '-' => rightOperandIsConstant ? VmOp.SubRk : VmOp.SubRr,
            _ => VmOp.AddRr,
        };
        return new VmScriptFunction(
            [VmCode.Rrr(vmOp, 2, 0, 1), VmCode.RetInt(2)],
            [],
            [VmValueType.Integer, VmValueType.Integer]);
    }

    public static VmScriptFunction EmitMulDiv(int op, bool leftConstant = false, bool rightConstant = false)
    {
        var vmOp = op switch
        {
            '*' => rightConstant ? VmOp.MulRk : VmOp.MulRr,
            '/' => leftConstant ? VmOp.DivKr : rightConstant ? VmOp.DivRk : VmOp.DivRr,
            '%' => leftConstant ? VmOp.ModKr : rightConstant ? VmOp.ModRk : VmOp.ModRr,
            _ => throw new ArgumentOutOfRangeException(nameof(op)),
        };
        return new VmScriptFunction(
            [VmCode.Rrr(vmOp, 2, 0, 1), VmCode.RetInt(2)],
            [],
            [VmValueType.Integer, VmValueType.Integer]);
    }

    public static bool TryFoldMulDiv(int op, int left, int right, out int result) =>
        ZccExpressionConstantFolder.TryFoldMulDiv(op, left, right, out result);
}
