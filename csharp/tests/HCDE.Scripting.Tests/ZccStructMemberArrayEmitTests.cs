namespace HCDE.Scripting.Tests;

public class ZccStructMemberArrayEmitTests
{
    [Fact]
    public void ClassMemberCtor_UsesClassExprType() =>
        Assert.True(ZccStructMemberArrayEmit.ClassMemberCtorSetsClassExprType(true));

    [Fact]
    public void StructMemberUsesDeprecatedFlagsPath_AtThreshold() =>
        Assert.True(ZccStructMemberArrayEmit.StructMemberUsesDeprecatedFlagsPath(64));

    [Fact]
    public void DeprecatedFlagsArgument_SubtractsThreshold() =>
        Assert.Equal(6, ZccStructMemberArrayEmit.DeprecatedFlagsArgument(70));

    [Fact]
    public void StructMemberAddressReturnsObjectDirectly_WhenZeroOffset() =>
        Assert.True(ZccStructMemberArrayEmit.StructMemberAddressReturnsObjectDirectly(true, 0));

    [Fact]
    public void StructMemberEmitUsesBitLoad_WhenBitField() =>
        Assert.Equal(4, ZccStructMemberArrayEmit.StructMemberBitMask(2));

    [Fact]
    public void ArrayElementRequestAddress_PassthroughWritable()
    {
        Assert.True(ZccStructMemberArrayEmit.ArrayElementRequestAddress(true, out var w, true));
        Assert.True(w);
    }

    [Fact]
    public void ArrayIndexNeedsIntCast_WhenFloatReg() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayIndexNeedsIntCast(true));

    [Fact]
    public void ShouldRewriteArrayToOutDereference_WhenDynArrayPointer() =>
        Assert.True(ZccStructMemberArrayEmit.ShouldRewriteArrayToOutDereference(true, true));

    [Fact]
    public void StaticArrayConstantIndexInBounds_RejectsEqualCount() =>
        Assert.False(ZccStructMemberArrayEmit.StaticArrayConstantIndexInBounds(4, 4));

    [Fact]
    public void FoldedStaticArrayFieldOffset_MultipliesElementSize() =>
        Assert.Equal(20U, ZccStructMemberArrayEmit.FoldedStaticArrayFieldOffset(2, 8, 4));

    [Fact]
    public void InvalidResizableArrayExpr_RejectsPlain() =>
        Assert.True(ZccStructMemberArrayEmit.InvalidResizableArrayExpr(false, false, false));

    [Fact]
    public void ArrayResolveFailsNonArray_WhenNeither() =>
        Assert.False(ZccStructMemberArrayEmit.ArrayResolvePointerToArray(false, false, false));

    [Fact]
    public void ArrayElementOutOfBounds_AtElementCount() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayElementOutOfBounds(3, 3));
}
