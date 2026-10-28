namespace HCDE.Playsim.Tests;

public class VmAcsPlaneWatcherTests
{
    [Fact]
    public void Construct_RecordsWatchPlane()
    {
        var w = VmAcsPlaneWatcher.Construct(ceiling: true, planeD: 100, heightDelta: -10, special: 5, 1, 2, 3, 4, 5);
        Assert.Equal(100, w.LastD);
        Assert.Equal(90, w.WatchD);
        Assert.Equal(5, w.Special);
    }

    [Fact]
    public void Tick_FiresWhenPlaneCrossesWatchD()
    {
        var fired = false;
        var w = VmAcsPlaneWatcher.Construct(false, 50, 10, 7, 0, 0, 0, 0, 0);
        Assert.True(w.Tick(60, (_, _, _, _, _, _) => { fired = true; return true; }));
        Assert.True(fired);
        Assert.True(w.Destroyed);
    }
}
