namespace HCDE.Scripting.Tests;

public class ZccConditionalAndTypeResolveTests
{
    [Fact]
    public void DotCross_ValidatesVectorOperands() =>
        Assert.False(ZccConditionalAndTypeResolve.DotCrossOperandsCompatible(
            false, true, ZccConditionalAndTypeResolve.TkDot, ZccConditionalAndTypeResolve.TypeVector3));

    [Fact]
    public void DotCross_ResultType()
    {
        Assert.Equal(ZccConditionalAndTypeResolve.TypeFloat64,
            ZccConditionalAndTypeResolve.DotCrossResultType(ZccConditionalAndTypeResolve.TkDot));
        Assert.Equal(ZccConditionalAndTypeResolve.TypeVector3,
            ZccConditionalAndTypeResolve.DotCrossResultType(ZccConditionalAndTypeResolve.TkCross));
    }

    [Fact]
    public void TypeCheck_UsesClassPointerPath() =>
        Assert.True(ZccConditionalAndTypeResolve.TypeCheckUsesClassPointerPath(true));

    [Fact]
    public void DynamicCast_PreservesConstPointer() =>
        Assert.True(ZccConditionalAndTypeResolve.DynamicCastNeedsObjectPointer(true, true));

    [Fact]
    public void Conditional_MergesIntegerBranches()
    {
        var merged = ZccConditionalAndTypeResolve.MergeConditionalValueType(
            false, false, true, false, false, false, "uint32", "sint32");
        Assert.Equal("sint32", merged);
    }

    [Fact]
    public void Conditional_FoldConstantCondition()
    {
        Assert.True(ZccConditionalAndTypeResolve.ShouldFoldConditional(true));
        Assert.True(ZccConditionalAndTypeResolve.FoldConditionalTakeTrue(true));
    }
}
