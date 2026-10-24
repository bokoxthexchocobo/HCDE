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

    [Fact]
    public void TryGetNullConstant_IntZero() =>
        Assert.True(ZccCompareEqResolve.TryGetNullConstant(ZccCompareEqResolve.RegKind.Int, 0, 1, IntPtr.Zero, out var n) && n);

    [Fact]
    public void TryFoldConstantCompare_IntEquality() =>
        Assert.True(ZccCompareEqResolve.TryFoldConstantCompare(ZccCompareEqResolve.TkEq, "int", 3, 3, 0, 0, out var folded) && folded);

    [Fact]
    public void ShouldRewriteEqToUnaryNot_WhenRightNull() =>
        Assert.True(ZccCompareEqResolve.ShouldRewriteEqToUnaryNot(ZccCompareEqResolve.TkEq, true));
}
