namespace HCDE.Playsim.Tests;

public class VmDThinkerListDestroyTests
{
    [Fact]
    public void DoDestroyThinkers_ClearsSentinelAndMarksNodes()
    {
        var sentinel = new VmDThinkerListDestroy.ThinkerNode { ObjectFlags = VmDThinkerListDestroy.OfSentinel };
        var a = new VmDThinkerListDestroy.ThinkerNode();
        var b = new VmDThinkerListDestroy.ThinkerNode();
        sentinel.Next = a;
        sentinel.Prev = b;
        a.Prev = sentinel;
        a.Next = b;
        b.Prev = a;
        b.Next = sentinel;
        var list = new VmDThinkerListDestroy.ListModel { Sentinel = sentinel };
        Assert.False(VmDThinkerListDestroy.DoDestroyThinkers(list));
        Assert.Null(list.Sentinel);
        Assert.True(a.Destroyed);
        Assert.True(b.Destroyed);
    }

    [Fact]
    public void RemoveTravellers_StripsFlagWhenSaveGame()
    {
        var sentinel = new VmDThinkerListDestroy.ThinkerNode { ObjectFlags = VmDThinkerListDestroy.OfSentinel };
        var traveller = new VmDThinkerListDestroy.ThinkerNode
        {
            ObjectFlags = VmDThinkerListDestroy.OfTravelling,
            Next = sentinel,
            Prev = sentinel,
        };
        sentinel.Next = sentinel.Prev = traveller;
        var list = new VmDThinkerListDestroy.ListModel { Sentinel = sentinel };
        VmDThinkerListDestroy.RemoveTravellers(list, saveGame: true);
        Assert.Equal(0, traveller.ObjectFlags & VmDThinkerListDestroy.OfTravelling);
    }
}
