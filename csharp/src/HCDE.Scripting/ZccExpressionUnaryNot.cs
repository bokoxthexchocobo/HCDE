namespace HCDE.Scripting;

/// <summary>Native <c>FxUnaryNotBitwise::Resolve</c> / <c>Emit</c> from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionUnaryNot
{
    public static bool TryFoldBitwiseNot(int value, out int result)
    {
        result = ~value;
        return true;
    }

    public static VmScriptFunction EmitBitwiseNot()
    {
        return new VmScriptFunction(
            [VmCode.Rrr(VmOp.Not, 1, 0, 0), VmCode.RetInt(1)],
            [],
            [VmValueType.Integer]);
    }
}
