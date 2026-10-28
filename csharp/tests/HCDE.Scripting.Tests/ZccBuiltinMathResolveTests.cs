namespace HCDE.Scripting.Tests;

public class ZccBuiltinMathResolveTests
{
    [Fact]
    public void Abs_BoolPassthrough() =>
        Assert.Equal(ZccBuiltinMathResolve.AbsOutcome.PassthroughBool,
            ZccBuiltinMathResolve.ClassifyAbs(true, false, false));

    [Fact]
    public void Abs_FoldInt()
    {
        Assert.Equal(3, ZccBuiltinMathResolve.FoldAbsInt(-3));
        Assert.Equal(ZccBuiltinMathResolve.AbsResolveStep.FoldedInt,
            ZccBuiltinMathResolve.RunAbsResolve(false, true, true, ZccBuiltinMathResolve.RegtInt));
    }

    [Fact]
    public void Atan2_FoldConstants()
    {
        var deg = ZccBuiltinMathResolve.FoldAtan2Degrees(1, 1);
        Assert.InRange(deg, 44.9, 45.1);
        Assert.Equal(ZccBuiltinMathResolve.Atan2ResolveStep.FoldedConstant,
            ZccBuiltinMathResolve.RunAtan2Resolve(true, true, true, true, ZccBuiltinMathResolve.RegtInt, ZccBuiltinMathResolve.RegtInt));
    }

    [Fact]
    public void Atan2Vec_RequiresVector()
    {
        Assert.Equal(ZccBuiltinMathResolve.Atan2ResolveStep.ErrorNonNumeric,
            ZccBuiltinMathResolve.RunAtan2VecResolve(false));
        Assert.Equal(ZccBuiltinMathResolve.Atan2ResolveStep.Ready,
            ZccBuiltinMathResolve.RunAtan2VecResolve(true));
    }

    [Fact]
    public void New_RejectsAbstractClass() =>
        Assert.Equal(ZccBuiltinMathResolve.NewResolveStep.ErrorAbstract,
            ZccBuiltinMathResolve.RunNewResolve(true, true, true, true, 0, 0, 0));

    [Fact]
    public void New_ScopeBarrierMismatch()
    {
        Assert.True(ZccBuiltinMathResolve.NewScopeMismatch(1, 2));
        Assert.Equal(ZccBuiltinMathResolve.NewResolveStep.ErrorScopeBarrier,
            ZccBuiltinMathResolve.RunNewResolve(true, true, false, true, 1, 1, 2));
    }

    [Fact]
    public void MinMax_PicksFloatDomain()
    {
        var counts = ZccBuiltinMathResolve.ClassifyMinMaxOperands([(true, false, false, false)]);
        Assert.Equal("float64", ZccBuiltinMathResolve.MinMaxValueType(counts));
    }

    [Fact]
    public void MinMax_FoldConstants()
    {
        Assert.Equal(2, ZccBuiltinMathResolve.PickMinMaxInt(ZccBuiltinMathResolve.NameMin, 5, 2));
        Assert.Equal(7, ZccBuiltinMathResolve.PickMinMaxFloat(ZccBuiltinMathResolve.NameMax, 5, 7));
        var step = ZccBuiltinMathResolve.RunMinMaxResolve(
            ZccBuiltinMathResolve.NameMin,
            [true, true],
            [false, false],
            [true, true],
            [ZccBuiltinMathResolve.RegtInt, ZccBuiltinMathResolve.RegtInt],
            [ZccBuiltinMathResolve.TypeSInt32, ZccBuiltinMathResolve.TypeSInt32],
            [5, 2],
            [0, 0],
            out var valueType,
            out var swap);
        Assert.Equal(ZccBuiltinMathResolve.MinMaxResolveStep.FoldedAllConstants, step);
        Assert.Equal(ZccBuiltinMathResolve.TypeUInt32, valueType);
        Assert.False(swap);
    }
}
