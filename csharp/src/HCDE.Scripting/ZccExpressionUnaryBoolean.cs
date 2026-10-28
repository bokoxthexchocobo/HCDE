namespace HCDE.Scripting;

/// <summary>Native <c>FxUnaryNotBoolean::Resolve</c> / <c>Emit</c> from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionUnaryBoolean
{
    public static bool TryFoldNot(bool value, out bool result)
    {
        result = !value;
        return true;
    }

    public static VmScriptFunction EmitLogicalNot()
    {
        return new VmScriptFunction(
        [
            VmCode.Li(2, 1),
            VmCode.Rrr(VmOp.XorRr, 1, 0, 2),
            VmCode.RetInt(1),
        ], [], [VmValueType.Integer]);
    }
}
