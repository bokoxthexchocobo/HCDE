namespace HCDE.Scripting.Tests;

public class ZccCompareEqResolveTests
{
    [Fact]
    public void FoldStringEqual_ApproxIgnoresCase() =>
        Assert.True(ZccCompareEqResolve.FoldStringEqual(true, "AbC", "abc"));

    [Fact]
    public void FoldFloatEqual_Neq() =>
        Assert.True(ZccCompareEqResolve.FoldFloatEqual(ZccCompareEqResolve.TkNeq, 1, 2));

    [Fact]
    public void ShouldUnaryNotRight_EqAndZero() =>
        Assert.True(ZccCompareEqResolve.ShouldUnaryNotRight(ZccCompareEqResolve.TkEq, true));
}
