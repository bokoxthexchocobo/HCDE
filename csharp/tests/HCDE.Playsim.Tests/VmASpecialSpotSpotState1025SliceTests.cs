namespace HCDE.Playsim.Tests;

public class VmASpecialSpotSpotState1025SliceTests
{
    private static VmASpecialSpotSpotList1024Slice.SpotActor Spot(string name, double x = 0, double y = 0) =>
        new() { ClassName = name, X = x, Y = y };

    [Fact]
    public void FindSpotList_CreatesListOnFirstUse()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        var list = VmASpecialSpotSpotState1025Slice.FindSpotList(state, "BrainTarget");
        Assert.NotNull(list);
        Assert.Single(state.SpotLists);
        Assert.Same(list, VmASpecialSpotSpotState1025Slice.FindSpotList(state, "BrainTarget"));
    }

    [Fact]
    public void FindSpotList_NullTypeReturnsNullWithoutInsert()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        Assert.Null(VmASpecialSpotSpotState1025Slice.FindSpotList(state, null));
        Assert.Empty(state.SpotLists);
    }

    [Fact]
    public void AddSpotNative_RegistersOnMatchingClassList()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        var spot = Spot("PickupSpot");
        Assert.True(VmASpecialSpotSpotState1025Slice.AddSpotNative(state, spot));
        Assert.Single(state.SpotLists[0].Spots);
    }

    [Fact]
    public void RemoveSpotNative_DelegatesToList()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        var spot = Spot("PickupSpot");
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, spot);
        Assert.True(VmASpecialSpotSpotState1025Slice.RemoveSpotNative(state, spot));
        Assert.Empty(state.SpotLists[0].Spots);
    }

    [Fact]
    public void GetNextInListNative_ForwardsToListPolicy()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, Spot("T", 0, 0));
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, Spot("T", 1, 0));
        Assert.Equal("T", VmASpecialSpotSpotState1025Slice.GetNextInListNative(state, "T", 0)?.ClassName);
    }

    [Fact]
    public void GetSpotWithMinMaxDistanceNative_ForwardsToList()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, Spot("T", 0, 0));
        var pick = VmASpecialSpotSpotState1025Slice.GetSpotWithMinMaxDistanceNative(
            state, "T", 0, 0, minDist: 0, maxDist: 0, randomMod: _ => 0);
        Assert.NotNull(pick);
    }

    [Fact]
    public void GetRandomSpotNative_IgnoresOnlyOnceLikeNativeList()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, Spot("T", 0, 0));
        Assert.NotNull(VmASpecialSpotSpotState1025Slice.GetRandomSpotNative(state, "T", onlyOnce: false, randomMod: _ => 0));
    }

    [Fact]
    public void OnDestroyNative_ClearsSpotListsBeforeSuper()
    {
        var state = new VmASpecialSpotSpotState1025Slice.SpotState();
        VmASpecialSpotSpotState1025Slice.AddSpotNative(state, Spot("T"));
        var order = new List<string>();
        VmASpecialSpotSpotState1025Slice.OnDestroyNative(
            state,
            () => order.Add("reset"),
            () => order.Add("super"));
        Assert.Empty(state.SpotLists);
        Assert.Equal(["reset", "super"], order);
    }

    [Fact]
    public void ShouldWriteSpotListsArchiveKey_MatchesNativeKey()
    {
        Assert.True(VmASpecialSpotSpotState1025Slice.ShouldWriteSpotListsArchiveKey("spots"));
        Assert.False(VmASpecialSpotSpotState1025Slice.ShouldWriteSpotListsArchiveKey("spot"));
    }
}
