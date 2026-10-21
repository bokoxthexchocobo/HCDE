namespace HCDE.Scripting.Tests;

public class ZccBinaryArithmeticResolveTests
{
    [Fact]
    public void FoldFloatBinary_AddSubMulDiv()
    {
        Assert.Equal(5, ZccBinaryArithmeticResolve.FoldFloatBinary('+', 2, 3));
        Assert.Equal(6, ZccBinaryArithmeticResolve.FoldFloatBinary('*', 2, 3));
    }

    [Fact]
    public void VectorBinaryCompatible_AllowsV3PlusV2()
    {
        Assert.True(ZccBinaryArithmeticResolve.VectorBinaryCompatible(true, true, false, true, false, false));
    }

    [Fact]
    public void ClassifyMulDivVector_RejectsModOnVector()
    {
        Assert.Equal(
            ZccBinaryArithmeticResolve.ResolveOutcome.Error,
            ZccBinaryArithmeticResolve.ClassifyMulDivVector('%', true, false, false, false, false, false, false, false));
    }

    [Fact]
    public void FoldFloatBinary_DivisionByZero_ReturnsNaN()
    {
        Assert.True(double.IsNaN(ZccBinaryArithmeticResolve.FoldFloatBinary('/', 1, 0)));
    }
}
