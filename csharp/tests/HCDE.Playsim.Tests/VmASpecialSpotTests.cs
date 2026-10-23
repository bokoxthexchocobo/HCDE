using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmASpecialSpotTests
{
    static Actor Spot(double x, double y) => new()
    {
        X = Fixed.FromDouble(x),
        Y = Fixed.FromDouble(y),
    };

    [Fact]
    public void AddSpot_RejectsDuplicate()
    {
        var state = new VmASpecialSpot.SpotState();
        var type = new object();
        var a = Spot(0, 0);
        Assert.True(state.AddSpot(a, type));
        Assert.False(state.AddSpot(a, type));
        Assert.Single(state.SpotLists[0].Spots);
    }

    [Fact]
    public void RemoveSpot_DropsEntryAndAdjustsIndex()
    {
        var state = new VmASpecialSpot.SpotState();
        var type = new object();
        var a = Spot(0, 0);
        var b = Spot(8, 0);
        state.AddSpot(a, type);
        state.AddSpot(b, type);
        state.SpotLists[0].Index = 1;
        Assert.True(state.RemoveSpot(a, type));
        Assert.Single(state.SpotLists[0].Spots);
        Assert.Equal(0u, state.SpotLists[0].Index);
    }

    [Fact]
    public void GetNextInList_RotatesAfterSkipThreshold()
    {
        var state = new VmASpecialSpot.SpotState();
        var type = new object();
        var a = Spot(0, 0);
        var b = Spot(4, 0);
        state.AddSpot(a, type);
        state.AddSpot(b, type);
        Assert.Null(state.GetNextInList(type, 1));
        var first = state.GetNextInList(type, 0);
        Assert.NotNull(first);
        var second = state.GetNextInList(type, 0);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void GetSpotWithMinMaxDistance_FindsActorInRing()
    {
        var state = new VmASpecialSpot.SpotState();
        var type = new object();
        state.AddSpot(Spot(10, 0), type);
        state.AddSpot(Spot(100, 0), type);
        var hit = state.GetSpotWithMinMaxDistance(type, 0, 0, 5, 20, n => 0);
        Assert.NotNull(hit);
        Assert.Equal(10, hit!.X.ToDouble(), 3);
    }

    [Fact]
    public void GetRandomSpot_OnlyOnceReturnsNullOnSecondCall()
    {
        var state = new VmASpecialSpot.SpotState();
        var type = new object();
        state.AddSpot(Spot(0, 0), type);
        Assert.NotNull(state.GetRandomSpot(type, true, _ => 0));
        Assert.Null(state.GetRandomSpot(type, true, _ => 0));
    }

    [Fact]
    public void SerializeSpotList_RoundTripsFields()
    {
        var list = new VmASpecialSpot.SpotList { Type = new object(), Index = 2, SkipCount = 1, NumCalls = 3 };
        list.Spots.Add(Spot(1, 2));
        var archive = VmASpecialSpot.SerializeSpotList(list);
        var copy = new VmASpecialSpot.SpotList { Type = list.Type };
        VmASpecialSpot.DeserializeSpotList(copy, archive);
        Assert.Equal(list.Index, copy.Index);
        Assert.Equal(list.SkipCount, copy.SkipCount);
        Assert.Equal(list.NumCalls, copy.NumCalls);
        Assert.Single(copy.Spots);
        Assert.Equal(list.Type, copy.Type);
    }

    [Fact]
    public void OnDestroy_ClearsLists()
    {
        var state = new VmASpecialSpot.SpotState();
        state.AddSpot(Spot(0, 0), new object());
        state.OnDestroy();
        Assert.Empty(state.SpotLists);
    }
}
