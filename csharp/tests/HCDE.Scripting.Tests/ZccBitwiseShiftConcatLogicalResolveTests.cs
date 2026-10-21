namespace HCDE.Scripting.Tests;

public class ZccBitwiseShiftConcatLogicalResolveTests
{
    [Fact]
    public void BitOpFold_AndOrXor()
    {
        Assert.Equal(1, ZccBitwiseShiftConcatLogicalResolve.FoldBitOp('&', 5, 3));
        Assert.Equal(7, ZccBitwiseShiftConcatLogicalResolve.FoldBitOp('|', 5, 2));
        Assert.Equal(6, ZccBitwiseShiftConcatLogicalResolve.FoldBitOp('^', 5, 3));
    }

    [Fact]
    public void BitOpBoolPath_SetsBoolType() =>
        Assert.True(ZccBitwiseShiftConcatLogicalResolve.BitOpBoolOperands(true, true));

    [Fact]
    public void ShiftFold_LeftRightUnsigned()
    {
        Assert.Equal(8, ZccBitwiseShiftConcatLogicalResolve.FoldShift(ZccBitwiseShiftConcatLogicalResolve.TkLShift, 2, 2));
        Assert.Equal(-1, ZccBitwiseShiftConcatLogicalResolve.FoldShift(ZccBitwiseShiftConcatLogicalResolve.TkRShift, -2, 1));
    }

    [Fact]
    public void ShiftRewritesRShiftToUnsigned()
    {
        var op = ZccBitwiseShiftConcatLogicalResolve.RewriteRShiftForUnsignedRight(
            true,
            ZccTypeCastResolve.MakeVersion(3, 7, 0),
            ZccBitwiseShiftConcatLogicalResolve.TkRShift,
            ZccTypeCastResolve.MakeVersion(3, 7, 0));
        Assert.Equal(ZccBitwiseShiftConcatLogicalResolve.TkUrShift, op);
    }

    [Fact]
    public void ConcatFold_StringConstants()
    {
        Assert.True(ZccBitwiseShiftConcatLogicalResolve.ConcatConstantFoldEligible(true, true, true, true));
        Assert.Equal("ab", ZccBitwiseShiftConcatLogicalResolve.FoldStringConcat("a", "b"));
    }

    [Fact]
    public void ConcatRejectsNilRegisterOperands() =>
        Assert.False(ZccBitwiseShiftConcatLogicalResolve.ConcatOperandValid(ZccBitwiseShiftConcatLogicalResolve.RegtNil));

    [Fact]
    public void LogicalAnd_FoldFalse() =>
        Assert.Equal(0, ZccBitwiseShiftConcatLogicalResolve.LogicalAndFold(0, null));

    [Fact]
    public void LogicalOr_PrunesKnownSide() =>
        Assert.Equal(
            ZccBitwiseShiftConcatLogicalResolve.LogicalPrune.TakeRight,
            ZccBitwiseShiftConcatLogicalResolve.LogicalOrPrune(0, null));

    [Fact]
    public void Flatten_MergesSameOperatorLists()
    {
        var merged = ZccBitwiseShiftConcatLogicalResolve.FlattenBinaryLogical(
            ZccBitwiseShiftConcatLogicalResolve.TkAndAnd,
            binaryLogicalExprType: 1,
            leftExprType: 1,
            rightExprType: 1,
            leftOp: ZccBitwiseShiftConcatLogicalResolve.TkAndAnd,
            rightOp: ZccBitwiseShiftConcatLogicalResolve.TkAndAnd,
            left: "a",
            right: "b",
            leftList: new object?[] { "x", "y" },
            rightList: new object?[] { "z" });
        Assert.Equal(3, merged.Count);
    }
}
