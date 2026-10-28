namespace HCDE.Playsim.Tests;

public class VmPSightInitTests
{
    [Fact]
    public void InitSightCheck_SetsTraceFromEyeHeight()
    {
        var ctx = new VmPSightInit.SightInitContext
        {
            SourceX = 0,
            SourceY = 0,
            SourceZ = 10,
            SourceHeight = 40,
            TargetX = 8,
            TargetY = 0,
        };
        VmPSightInit.InitSightCheck(ctx);
        Assert.Equal(40, ctx.SourceZ);
        Assert.Equal(8, ctx.TraceDx);
    }

    [Fact]
    public void ResetSightCounters_UpdatesMaxWhenFull()
    {
        var state = new VmPSightInit.SightCounterState { CycleMs = 25, MaxCycleMs = 20 };
        VmPSightInit.ResetSightCounters(state, full: false);
        Assert.Equal(25, state.MaxCycleMs);
    }
}
