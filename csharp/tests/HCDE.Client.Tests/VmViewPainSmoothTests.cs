using HCDE.Client;

namespace HCDE.Client.Tests;

public class VmViewPainSmoothTests
{
    [Fact]
    public void Scale_AtZeroStrengthMatchesVanillaLinear()
    {
        Assert.Equal(0.5f, VmViewPainSmooth.Scale(50, 0f));
        Assert.Equal(1f, VmViewPainSmooth.Scale(100, 0f));
    }

    [Fact]
    public void Scale_FullStrengthUsesSmoothstepPeak()
        => Assert.Equal(1f, VmViewPainSmooth.Scale(100, 1f));

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(150, 1)]
    public void Scale_ClampsDamageCount(int input, float expectedT)
        => Assert.Equal(expectedT, VmViewPainSmooth.Scale(input, 0f));
}
