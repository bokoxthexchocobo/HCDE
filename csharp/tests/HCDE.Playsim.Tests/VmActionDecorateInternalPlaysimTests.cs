namespace HCDE.Playsim.Tests;

public class VmActionDecorateInternalPlaysimTests
{
    [Fact]
    public void FloatIntrinsicWritesRet_WhenNumretPositive()
    {
        var wrote = VmActionDecorateInternalPlaysim.FloatIntrinsicWritesRet(true, 2.5, out var value);
        Assert.True(wrote);
        Assert.Equal(2.5, value);
    }

    [Fact]
    public void FloatIntrinsicReturnCount_MatchesNativeArity() =>
        Assert.Equal(0, VmActionDecorateInternalPlaysim.FloatIntrinsicReturnCount(false));
}
