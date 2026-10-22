namespace HCDE.Playsim.Tests;

public class VmPMobjFakeFloorLandTests
{
    [Fact]
    public void CheckFakeFloorTriggers_SkipsWhenPredicting()
    {
        var fired = false;
        VmPMobjFakeFloorLand.CheckFakeFloorTriggers(
            new() { TriggerSectorAction = _ => fired = true },
            new() { HasPlayer = true, PlayerCheats = VmPMobjFakeFloorLand.CfPredicting, Z = 0 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterZ = 10 },
            oldZ: 20,
            oldZHasViewheight: false);
        Assert.False(fired);
    }

    [Fact]
    public void CheckFakeFloorTriggers_FeetHitFakeFloor()
    {
        int? action = null;
        VmPMobjFakeFloorLand.CheckFakeFloorTriggers(
            new() { TriggerSectorAction = a => action = a },
            new() { Z = 15, ViewHeight = 41 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterZ = 20 },
            oldZ: 25,
            oldZHasViewheight: false);
        Assert.Equal(VmPMobjFakeFloorLand.SecspacHitFakeFloor, action);
    }

    [Fact]
    public void CheckFakeFloorTriggers_EyesCrossSurface()
    {
        int? action = null;
        VmPMobjFakeFloorLand.CheckFakeFloorTriggers(
            new() { TriggerSectorAction = a => action = a },
            new() { Z = 25, ViewHeight = 10 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterZ = 20 },
            oldZ: 5,
            oldZHasViewheight: true);
        Assert.Equal(VmPMobjFakeFloorLand.SecspacEyesSurface, action);
    }

    [Fact]
    public void CheckFakeFloorTriggers_EyesDiveBelowSurface()
    {
        int? action = null;
        VmPMobjFakeFloorLand.CheckFakeFloorTriggers(
            new() { TriggerSectorAction = a => action = a },
            new() { Z = 10, ViewHeight = 5 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterZ = 20 },
            oldZ: 30,
            oldZHasViewheight: true);
        Assert.Equal(VmPMobjFakeFloorLand.SecspacEyesDive, action);
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
