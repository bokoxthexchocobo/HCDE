using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmASpecialSpotTests
{
    [Fact]
    public void AddSpot_Deduplicates()
    {
        var state = new VmASpecialSpot.SpotState();
        var cls = new object();
        var spot = new VmASpecialSpot.SpotActor { Class = cls };
        Assert.True(state.AddSpot(spot));
        Assert.False(state.AddSpot(spot));
    }

    [Fact]
    public void GetNextInList_RotatesAfterSkipCounter()
    {
        var list = new VmASpecialSpot.SpotList(new object());
        var a = new VmASpecialSpot.SpotActor();
        var b = new VmASpecialSpot.SpotActor();
        list.Add(a);
        list.Add(b);

        Assert.Same(a, list.GetNextInList(0));
        Assert.Same(b, list.GetNextInList(0));
    }

    [Fact]
    public void GetSpotWithMinMaxDistance_FindsInRing()
    {
        var list = new VmASpecialSpot.SpotList(new object());
        list.Add(new VmASpecialSpot.SpotActor { X = 0, Y = 0 });
        list.Add(new VmASpecialSpot.SpotActor { X = 12, Y = 0 });

        var hit = list.GetSpotWithMinMaxDistance(5, 0, 6, 12, _ => 0);
        Assert.NotNull(hit);
        Assert.Equal(12, hit!.X);
    }

    [Fact]
    public void GetRandomSpot_OnlyOnce()
    {
        var list = new VmASpecialSpot.SpotList(new object());
        list.Add(new VmASpecialSpot.SpotActor());
        Assert.NotNull(list.GetRandomSpot(true, _ => 0));
        Assert.Null(list.GetRandomSpot(true, _ => 0));
    }
}
