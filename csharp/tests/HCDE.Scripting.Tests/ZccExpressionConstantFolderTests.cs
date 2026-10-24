namespace HCDE.Scripting.Tests;

public class ZccExpressionConstantFolderTests
{
    [Fact]
    public void FoldAddSub_MatchesCodegenIntegerRules()
    {
        Assert.True(ZccExpressionConstantFolder.TryFoldAddSub('+', 3, 5, out var sum));
        Assert.Equal(8, sum);
        Assert.True(ZccExpressionConstantFolder.TryFoldAddSub('-', 9, 4, out var diff));
        Assert.Equal(5, diff);
    }

    [Fact]
    public void FoldMulDiv_EmitsRunnableScript()
    {
        Assert.True(ZccExpressionConstantFolder.TryFoldMulDiv('*', 6, 7, out var product));
        var function = ZccExpressionConstantFolder.EmitFoldedIntegerReturn(product);
        var result = VmExec.RunFunction(function, []);
        Assert.Equal(VmFault.None, result.Fault);
        Assert.Equal(42, result.Values[0].Value);
    }

    [Fact]
    public void FoldUnaryNegate_RunsThroughVm()
    {
        Assert.True(ZccExpressionConstantFolder.TryFoldUnaryNegate(15, out var negated));
        var function = ZccExpressionConstantFolder.EmitFoldedIntegerReturn(negated);
        Assert.Equal(-15, VmExec.RunFunction(function, []).Values[0].Value);
    }
}
