namespace HCDE.Playsim.Tests;

public class AcsFadeRangeServiceTests
{
    [Fact]
    public void InstantFade_SetsTargetBlend()
    {
        var blend = new AcsFadeRangeService.BlendState();
        AcsFadeRangeService.Apply(blend, 0, 0, 0, -1, 255, 0, 0, 32768, 0);
        Assert.Equal(1f, blend.R);
        Assert.Equal(0f, blend.G);
        Assert.InRange(blend.A, 0.49f, 0.51f);
    }

    [Fact]
    public void TimedFade_QueuesFromCurrentWhenNotFadingFrom()
    {
        var blend = new AcsFadeRangeService.BlendState { R = 0.2f, G = 0.3f, B = 0.4f, A = 0.5f };
        AcsFadeRangeService.Apply(blend, 0, 0, 0, -1, 0, 255, 0, 65536, 65536);
        Assert.Equal(0.2f, blend.PendingFromR);
        Assert.Equal(0.3f, blend.PendingFromG);
        Assert.Equal(1f, blend.PendingToG);
        Assert.Equal(1f, blend.PendingSeconds);
    }
}
