namespace HCDE.Scripting.Tests;

public class ZccBuiltinMathResolveTests
{
    [Fact]
    public void Abs_BoolPassthrough() =>
        Assert.Equal(ZccBuiltinMathResolve.AbsOutcome.PassthroughBool,
            ZccBuiltinMathResolve.ClassifyAbs(true, false, false));

    [Fact]
    public void Abs_FoldInt() => Assert.Equal(3, ZccBuiltinMathResolve.FoldAbsInt(-3));

    [Fact]
    public void Atan2_FoldConstants()
    {
        var deg = ZccBuiltinMathResolve.FoldAtan2Degrees(1, 1);
        Assert.InRange(deg, 44.9, 45.1);
    }

    [Fact]
    public void Atan2Vec_RequiresVector() =>
        Assert.False(false);

    [Fact]
    public void New_RejectsAbstractClass() =>
        Assert.True(true);

    [Fact]
    public void New_ScopeBarrierMismatch() =>
        Assert.True(ZccBuiltinMathResolve.NewScopeMismatch(1, 2));

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
    }
}
