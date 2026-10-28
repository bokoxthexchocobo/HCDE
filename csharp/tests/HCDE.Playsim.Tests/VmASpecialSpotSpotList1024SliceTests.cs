namespace HCDE.Playsim.Tests;

public class VmASpecialSpotSpotList1024SliceTests
{
    private static VmASpecialSpotSpotList1024Slice.SpotActor Spot(string name, double x, double y) =>
        new() { ClassName = name, X = x, Y = y };

    [Fact]
    public void TryAddSpot_RejectsDuplicateReference()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "BrainTarget" };
        var a = Spot("a", 0, 0);
        Assert.True(VmASpecialSpotSpotList1024Slice.TryAddSpot(list, a));
        Assert.False(VmASpecialSpotSpotList1024Slice.TryAddSpot(list, a));
        Assert.Single(list.Spots);
    }

    [Fact]
    public void TryRemoveSpot_AdjustsIndexWhenRemovingEarlierEntry()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "Spot", Index = 2 };
        var s0 = Spot("0", 0, 0);
        var s1 = Spot("1", 1, 0);
        var s2 = Spot("2", 2, 0);
        list.Spots.AddRange([s0, s1, s2]);
        Assert.True(VmASpecialSpotSpotList1024Slice.TryRemoveSpot(list, s0));
        Assert.Equal(1u, list.Index);
        Assert.True(VmASpecialSpotSpotList1024Slice.TryRemoveSpot(list, s1));
        Assert.Equal(0u, list.Index);
    }

    [Fact]
    public void GetNextInList_RoundsRobinAfterSkipCounter()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "Spot" };
        list.Spots.AddRange([Spot("a", 0, 0), Spot("b", 1, 0)]);
        Assert.Null(VmASpecialSpotSpotList1024Slice.GetNextInList(list, skipCounter: 1));
        Assert.Equal("a", VmASpecialSpotSpotList1024Slice.GetNextInList(list, skipCounter: 0)?.ClassName);
        Assert.Equal("b", VmASpecialSpotSpotList1024Slice.GetNextInList(list, skipCounter: 0)?.ClassName);
        Assert.Equal("a", VmASpecialSpotSpotList1024Slice.GetNextInList(list, skipCounter: 0)?.ClassName);
    }

    [Fact]
    public void GetSpotWithMinMaxDistance_SearchesCircularOrder()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "Spot" };
        list.Spots.AddRange([Spot("near", 0, 0), Spot("far", 100, 0)]);
        var pick = VmASpecialSpotSpotList1024Slice.GetSpotWithMinMaxDistance(
            list, x: 50, y: 0, minDist: 49, maxDist: 51, randomMod: _ => 0);
        Assert.Equal("near", pick?.ClassName);
    }

    [Fact]
    public void GetSpotWithMinMaxDistance_ReturnsNullWhenNoMatch()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "Spot" };
        list.Spots.Add(Spot("only", 0, 0));
        Assert.Null(VmASpecialSpotSpotList1024Slice.GetSpotWithMinMaxDistance(
            list, 0, 0, minDist: 10, maxDist: 20, randomMod: _ => 0));
    }

    [Fact]
    public void GetRandomSpot_OnlyOnceBeforeNumCalls()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList { TypeName = "Spot" };
        list.Spots.AddRange([Spot("a", 0, 0), Spot("b", 2, 0)]);
        var first = VmASpecialSpotSpotList1024Slice.GetRandomSpot(list, randomMod: _ => 1);
        Assert.Equal("b", first?.ClassName);
        Assert.Null(VmASpecialSpotSpotList1024Slice.GetRandomSpot(list, randomMod: _ => 0));
    }

    [Fact]
    public void SerializeBundle_RoundTripsIndexFields()
    {
        var list = new VmASpecialSpotSpotList1024Slice.SpotList
        {
            TypeName = "MaceSpawner",
            Index = 3,
            SkipCount = 2,
            NumCalls = 5,
        };
        list.Spots.Add(Spot("x", 0, 0));
        var cap = VmASpecialSpotSpotList1024Slice.CaptureSerializeFields(list);
        list.Index = 0;
        list.SkipCount = 0;
        list.NumCalls = 0;
        VmASpecialSpotSpotList1024Slice.ApplySerializeFields(list, cap);
        Assert.Equal(3u, list.Index);
        Assert.Equal(2, list.SkipCount);
        Assert.Equal(5, list.NumCalls);
    }
}
