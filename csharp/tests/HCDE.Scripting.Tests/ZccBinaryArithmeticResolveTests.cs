namespace HCDE.Scripting.Tests;

public class ZccBinaryArithmeticResolveTests
{
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
    public void PowFoldConstants()
    {
        Assert.Equal(8, ZccBinaryArithmeticResolve.PowFoldConstants(true, 2, 3));
    }

    [Fact]
    public void PowRequiresNumericOperands()
    {
        Assert.True(ZccBinaryArithmeticResolve.PowRequiresNumericOperands(true, true));
        Assert.False(ZccBinaryArithmeticResolve.PowRequiresNumericOperands(true, false));
    }
}
