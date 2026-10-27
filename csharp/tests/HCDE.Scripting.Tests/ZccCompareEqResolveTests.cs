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

    [Fact]
    public void RunCompareEqResolvePipeline_FoldsIntConstant()
    {
        var input = new ZccCompareEqResolve.CompareEqResolveInput(
            ZccCompareEqResolve.TkEq, true, false, false, true, "int", 2, 2, 0, 0,
            ZccCompareEqResolve.RegKind.Int, 0, 0, IntPtr.Zero, false, false);
        var result = ZccCompareEqResolve.RunCompareEqResolvePipeline(input);
        Assert.True(result.FoldedConstant);
        Assert.True(result.FoldedValue);
    }

    [Fact]
    public void VectorPairComparable_MatchesNativeVectorFamilies()
    {
        Assert.True(ZccCompareEqResolve.VectorPairComparable(true, true, false, false, false, false, false, false));
        Assert.True(ZccCompareEqResolve.ApproxEqOperatorLegal(ZccCompareEqResolve.TkApproxEq, 1, 2, 1));
        Assert.False(ZccCompareEqResolve.ApproxEqOperatorLegal(ZccCompareEqResolve.TkApproxEq, 1, 2, 3));
    }
}
