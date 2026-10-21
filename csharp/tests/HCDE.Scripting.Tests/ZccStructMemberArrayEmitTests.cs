namespace HCDE.Scripting.Tests;

public class ZccStructMemberArrayEmitTests
{
    [Fact]
    public void PlanStructMemberEmit_DeprecatedFlagsPath()
    {
        var plan = ZccStructMemberArrayEmit.PlanStructMemberEmit(false, false, false, 0, 64, "OP_LW");
        Assert.Equal("CheckDeprecatedFlags", plan.LoadOp);
    }

    [Fact]
    public void PlanDeprecatedFlagsEmit_SubtractsThreshold()
    {
        var plan = ZccStructMemberArrayEmit.PlanDeprecatedFlagsEmit(70);
        Assert.Equal(6, plan.BitArgument);
    }

    [Fact]
    public void StructMemberAddressReturnsObjectDirectly_WhenZeroOffset() =>
        Assert.True(ZccStructMemberArrayEmit.StructMemberAddressReturnsObjectDirectly(true, 0));

    [Fact]
    public void ClassMemberSetsExprType_MatchesClassMember() =>
        Assert.True(ZccStructMemberArrayEmit.ClassMemberSetsExprType(ZccStructMemberArrayEmit.EfxClassMember));

    [Fact]
    public void ArrayElementRequestAddress_PassthroughWritable()
    {
        Assert.True(ZccStructMemberArrayEmit.ArrayElementRequestAddress(true, out var w, true));
        Assert.True(w);
    }

    [Fact]
    public void ArrayIndexNeedsIntCast_WhenFloatReg() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayIndexNeedsIntCast(1));

    [Fact]
    public void ArrayResolveAcceptsDynamicArray_PointerDynArray() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayResolveAcceptsDynamicArray(true, true));

    [Fact]
    public void StaticArrayConstantIndexInBounds_RejectsEqualCount() =>
        Assert.False(ZccStructMemberArrayEmit.StaticArrayConstantIndexInBounds(4, 4));

    [Fact]
    public void FoldedStaticArrayFieldOffset_MultipliesElementSize() =>
        Assert.Equal(20U, ZccStructMemberArrayEmit.FoldedStaticArrayFieldOffset(2, 8, 4));

    [Fact]
    public void InvalidResizableArrayExpr_RejectsUnknown() =>
        Assert.True(ZccStructMemberArrayEmit.InvalidResizableArrayExpr(99));

    [Fact]
    public void ArrayResolveFailsNonArray_WhenNeither() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayResolveFailsNonArray(false, false));

    [Fact]
    public void ArrayElementOutOfBounds_AtElementCount() =>
        Assert.True(ZccStructMemberArrayEmit.ArrayElementOutOfBounds(3, 3));

    [Fact]
    public void ResizableStaticArrayMember_AllowsStackVariable() =>
        Assert.True(ZccStructMemberArrayEmit.ResizableStaticArrayMember(ZccStructMemberArrayEmit.EfxStackVariable));

    [Fact]
    public void ShouldRewriteArrayToOutDereference_WhenDynArrayPointer() =>
        Assert.True(ZccStructMemberArrayEmit.ShouldRewriteArrayToOutDereference(true, true));
}
