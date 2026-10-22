namespace HCDE.Playsim.Tests;

public class VmPEffectJitterSplashTests
{
    [Fact]
    public void JitterDriftVelocity_ScalesRandom()
    {
        Assert.Equal(0, VmPEffectJitterSplash.JitterDriftVelocity(128, 1));
    }

    [Fact]
    public void FountainInnerJet_WhenCloseToCenter()
    {
        Assert.True(VmPEffectJitterSplash.FountainInnerJet(1, 16));
    }

    [Fact]
    public void SplashKindSupported_OnlySpark()
    {
        Assert.True(VmPEffectJitterSplash.SplashKindSupported(1));
        Assert.False(VmPEffectJitterSplash.SplashKindSupported(2));
    }
}
