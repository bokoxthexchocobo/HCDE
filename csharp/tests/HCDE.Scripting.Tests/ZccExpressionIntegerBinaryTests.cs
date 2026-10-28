using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccExpressionIntegerBinaryTests
{
    [Theory]
    [InlineData('+', 11, 31, 42)]
    [InlineData('-', 50, 8, 42)]
    public void EmitAddSub_RunsIntegerVmOp(char op, int left, int right, int expected)
    {
        var function = ZccExpressionIntegerBinary.EmitAddSub(op);
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(left), VmValue.FromInteger(right)]);
        Assert.Equal(expected, result.Values[0].Value);
    }

    [Theory]
    [InlineData('*', 6, 7, 42)]
    [InlineData('/', 84, 2, 42)]
    [InlineData('%', 47, 5, 2)]
    public void EmitMulDiv_RunsIntegerVmOp(char op, int left, int right, int expected)
    {
        var function = ZccExpressionIntegerBinary.EmitMulDiv(op);
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(left), VmValue.FromInteger(right)]);
        Assert.Equal(expected, result.Values[0].Value);
    }

    [Fact]
    public void TryFoldMulDiv_MatchesResolveConstantPath()
    {
        Assert.True(ZccExpressionIntegerBinary.TryFoldMulDiv('*', 3, 14, out var product));
        Assert.Equal(42, product);
    }
}
