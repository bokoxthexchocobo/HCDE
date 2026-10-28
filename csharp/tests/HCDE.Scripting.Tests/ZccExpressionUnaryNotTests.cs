using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccExpressionUnaryNotTests
{
    [Fact]
    public void TryFoldBitwiseNot_MatchesResolve()
    {
        Assert.True(ZccExpressionUnaryNot.TryFoldBitwiseNot(0, out var folded));
        Assert.Equal(-1, folded);
    }

    [Fact]
    public void EmitBitwiseNot_RunsVmOpcode()
    {
        var function = ZccExpressionUnaryNot.EmitBitwiseNot();
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(0)]);
        Assert.Equal(-1, result.Values[0].Value);
    }
}
