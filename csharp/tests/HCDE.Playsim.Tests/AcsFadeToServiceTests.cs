namespace HCDE.Playsim.Tests;

public class AcsFadeToServiceTests
{
    [Fact]
    public void Apply_ClampsAndDelegatesToFadeRange()
    {
        var blend = new AcsFadeRangeService.BlendState();
        AcsFadeToService.Apply(blend, 300, -5, 128, 70000, 0);
        Assert.Equal(1f, blend.A, 3);
        Assert.Equal(1f, blend.R, 3);
        Assert.Equal(0f, blend.G, 3);
        Assert.Equal(128 / 255f, blend.B, 3);
    }

    [Fact]
    public void Apply_WithDurationSetsPendingFade()
    {
        var blend = new AcsFadeRangeService.BlendState();
        AcsFadeToService.Apply(blend, 0, 255, 0, 32768, 65536);
        Assert.Equal(32768 / 65536f, blend.PendingToA);
        Assert.Equal(1f, blend.PendingSeconds);
    }
}
