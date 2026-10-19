namespace HCDE.Scripting.Tests;

public class ZccExpressionIntCastTests
{
    [Fact]
    public void IntCastConstant_RunsThroughVm()
    {
        Assert.True(ZccExpressionIntCast.TryFoldConstant(99, out var folded));
        var function = ZccExpressionIntCast.EmitIntegerLiteral(folded);
        Assert.Equal(99, VmExec.RunFunction(function, []).Values[0].Value);
    }
}
