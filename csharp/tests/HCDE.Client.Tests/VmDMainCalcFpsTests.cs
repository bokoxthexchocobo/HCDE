namespace HCDE.Client.Tests;

public class VmDMainCalcFpsTests
{
    [Fact]
    public void Tick_AdvancesFrameCountOncePerFrame()
    {
        var state = new VmDMainCalcFps.FpsState(0, 0, 0);
        var (next, howLong, _, updated) = VmDMainCalcFps.Tick(state, 16, 1.0);
        Assert.True(updated);
        Assert.Equal(16ul, howLong);
        Assert.Equal(1ul, next.FrameCount);
    }
}
