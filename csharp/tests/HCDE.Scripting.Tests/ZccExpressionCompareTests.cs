using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccExpressionCompareTests
{
    [Theory]
    [InlineData('<', 2, 9, 1)]
    [InlineData('>', 9, 2, 1)]
    public void EmitRelational_RunsVmCompare(int op, int left, int right, int expected)
    {
        var function = ZccExpressionCompare.EmitRelational(op);
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(left), VmValue.FromInteger(right)]);
        Assert.Equal(expected, result.Values[0].Value);
    }

    [Fact]
    public void TryFoldEquality_MatchesNativeConstantFold()
    {
        Assert.True(ZccExpressionCompare.TryFoldEquality('=', 6, 6, out var same));
        Assert.True(same);
        Assert.True(ZccExpressionCompare.TryFoldEquality('!', 6, 7, out var diff));
        Assert.True(diff);
    }
}
