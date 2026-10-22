namespace HCDE.Playsim.Tests;

public class VmPMobjFakeFloorLandTests
{
    [Fact]
    public void CheckFakeFloorTriggers_HitFakeFloor()
    {
        var fired = 0;
        var ctx = new VmPMobjFakeFloorLand.FakeFloorContext { TriggerSectorAction = _ => fired++ };
        VmPMobjFakeFloorLand.CheckFakeFloorTriggers(ctx,
            new() { Z = 15 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterZ = 20 },
            oldZ: 25,
            oldZHasViewheight: false);
        Assert.Equal(1, fired);
    }

    [Fact]
    public void CallFallAndSink_NativeFallback()
    {
        var called = false;
        VmPMobjFakeFloorLand.CallFallAndSink(false, null, (_, _) => called = true, 1, 2);
        Assert.True(called);
    }

    [Fact]
    public void AddToHash_ZeroTidClearsLinks()
    {
        var node = new VmPMobjFakeFloorLand.HashNode { Next = new() };
        VmPMobjFakeFloorLand.AddToHash(node, 0, _ => null, (_, _) => { });
        Assert.Null(node.Next);
    }
}
