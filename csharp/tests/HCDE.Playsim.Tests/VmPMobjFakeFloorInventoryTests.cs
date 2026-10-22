namespace HCDE.Playsim.Tests;

public class VmPMobjFakeFloorInventoryTests
{
    [Fact]
    public void CheckFakeFloorTriggers_SkipsWhenPredicting()
    {
        var fired = false;
        var ctx = new VmPMobjFakeFloorInventory.FakeFloorContext
        {
            TriggerSectorAction = (_, _) => fired = true,
        };
        VmPMobjFakeFloorInventory.CheckFakeFloorTriggers(ctx,
            new() { HasPlayer = true, PlayerCheats = VmPMobjFakeFloorInventory.CfPredicting },
            new() { HasHeightSec = true, HasSecActTarget = true },
            0, false);
        Assert.False(fired);
    }

    [Fact]
    public void CheckFakeFloorTriggers_HitFakeFloor()
    {
        var actions = new List<int>();
        var ctx = new VmPMobjFakeFloorInventory.FakeFloorContext
        {
            TriggerSectorAction = (_, a) => actions.Add(a),
        };
        VmPMobjFakeFloorInventory.CheckFakeFloorTriggers(ctx,
            new() { Z = 10 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterFloorZ = 12 },
            oldZ: 15,
            oldZHasViewheight: false);
        Assert.Contains(VmPMobjFakeFloorInventory.SecspacHitFakeFloor, actions);
    }

    [Fact]
    public void CheckFakeFloorTriggersAction_Delegates()
    {
        var fired = false;
        var ctx = new VmPMobjFakeFloorInventory.FakeFloorContext
        {
            TriggerSectorAction = (_, _) => fired = true,
        };
        VmPMobjFakeFloorInventory.CheckFakeFloorTriggersAction(ctx,
            new() { Z = 0 },
            new() { HasHeightSec = true, HasSecActTarget = true, WaterFloorZ = 5 },
            10, false);
        Assert.True(fired);
    }

    [Fact]
    public void PlayerLandedOnThing_RunsHooks()
    {
        var steps = new List<string>();
        var ctx = new VmPMobjFakeFloorInventory.LandContext
        {
            SquatView = (_, _) => steps.Add("squat"),
            FallingDamage = (_, _) => steps.Add("fall"),
            MakeRumble = (_, _) => steps.Add("rumble"),
            MakeGruntSound = (_, _) => steps.Add("grunt"),
        };
        VmPMobjFakeFloorInventory.PlayerLandedOnThing(ctx, new(), new(), playerCheats: 0);
        Assert.Equal(new[] { "squat", "fall", "rumble", "grunt" }, steps);
    }

    [Fact]
    public void CallFallAndSink_VirtualPath()
    {
        var vm = false;
        var ctx = new VmPMobjFakeFloorInventory.FallSinkContext
        {
            VmFallAndSink = (_, _, _) => { vm = true; return true; },
            NativeFallAndSink = (_, _, _) => Assert.Fail("native should not run"),
        };
        VmPMobjFakeFloorInventory.CallFallAndSink(ctx, new(), 1, 2);
        Assert.True(vm);
    }

    [Fact]
    public void CallFallAndSink_NativeFallback()
    {
        var native = false;
        var ctx = new VmPMobjFakeFloorInventory.FallSinkContext
        {
            NativeFallAndSink = (_, _, _) => native = true,
        };
        VmPMobjFakeFloorInventory.CallFallAndSink(ctx, new(), 1, 2);
        Assert.True(native);
    }

    [Fact]
    public void FindInventory_ExactClassMatch()
    {
        var chain = new VmPMobjFakeFloorInventory.InventoryItem
        {
            ClassName = "Clip",
            Next = new() { ClassName = "Shell" },
        };
        var found = VmPMobjFakeFloorInventory.FindInventory(chain, "Shell", subclass: false);
        Assert.Equal("Shell", found?.ClassName);
    }

    [Fact]
    public void FindInventory_SubclassMatch()
    {
        var chain = new VmPMobjFakeFloorInventory.InventoryItem { ClassName = "Base", IsKindOf = true };
        Assert.Same(chain, VmPMobjFakeFloorInventory.FindInventory(chain, "Derived", subclass: true));
    }

    [Fact]
    public void FindInventoryAction_Delegates()
    {
        var chain = new VmPMobjFakeFloorInventory.InventoryItem { ClassName = "A" };
        Assert.Same(chain, VmPMobjFakeFloorInventory.FindInventoryAction(chain, "A", false));
    }

    [Fact]
    public void GiveInventoryType_PickupFailsDestroys()
    {
        var destroyed = false;
        var ctx = new VmPMobjFakeFloorInventory.GiveInventoryContext
        {
            SpawnItem = _ => new(),
            TryPickup = (_, _) => false,
            DestroyItem = _ => destroyed = true,
        };
        Assert.Null(VmPMobjFakeFloorInventory.GiveInventoryType(ctx, new(), "Clip"));
        Assert.True(destroyed);
    }

    [Fact]
    public void GiveInventoryTypeAction_Delegates()
    {
        var ctx = new VmPMobjFakeFloorInventory.GiveInventoryContext
        {
            SpawnItem = _ => new object(),
            TryPickup = (_, _) => true,
        };
        Assert.NotNull(VmPMobjFakeFloorInventory.GiveInventoryTypeAction(ctx, new(), "X"));
    }
}
