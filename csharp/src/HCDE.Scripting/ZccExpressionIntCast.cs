namespace HCDE.Scripting;

/// <summary>Native <c>FxIntCast::Resolve</c> folding integer constants from <c>codegen.cpp</c>.</summary>
public static class ZccExpressionIntCast
{
    public static bool TryFoldConstant(int value, out int result)
    {
        result = value;
        return true;
    }

    public static VmScriptFunction EmitIntegerLiteral(int value)
        => ZccExpressionConstantFolder.EmitFoldedIntegerReturn(value);
}
