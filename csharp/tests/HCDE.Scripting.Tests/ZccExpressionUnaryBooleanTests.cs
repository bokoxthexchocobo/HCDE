using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccExpressionUnaryBooleanTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void TryFoldNot_MatchesResolve(bool value, bool expected)
    {
        Assert.True(ZccExpressionUnaryBoolean.TryFoldNot(value, out var folded));
        Assert.Equal(expected, folded);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public void EmitLogicalNot_RunsVmXorWithOne(int input, int expected)
    {
        var function = ZccExpressionUnaryBoolean.EmitLogicalNot();
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(input)]);
        Assert.Equal(expected, result.Values[0].Value);
    }
}
