namespace HCDE.Client.Tests;

public class VmGameClockTests
{
    [Fact]
    public void FrameTimeAndTics_AdvancesWithInjectedClock()
    {
        ulong now = 0;
        var clock = new VmGameClock { GetTimePoint = () => now };
        clock.InitTime();
        now = 1_000_000_000;
        clock.SetFrameTime();
        now = 2_000_000_000;
        clock.SetFrameTime();
        Assert.Equal(35, clock.GetTime(35));
        Assert.InRange(clock.GetTimeFrac(35), 0, 1);
    }

    [Fact]
    public void DeltaTime_Clamped()
    {
        ulong now = 0;
        var clock = new VmGameClock { GetTimePoint = () => now };
        clock.InitTime();
        now = 500_000_000;
        clock.SetDeltaTime();
        now = 2_000_000_000;
        clock.SetDeltaTime();
        Assert.InRange(clock.GetDeltaTime(false), 0.001, 0.2);
    }
}
