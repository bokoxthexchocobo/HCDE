namespace HCDE.Gamedata.Tests;

public class VmDehackedNumericConversionTests
{
    [Fact]
    public void DehToDouble_ConvertsFixedPoint() =>
        Assert.Equal(1.0, VmDehackedNumericConversion.DehToDouble(65536));
}
