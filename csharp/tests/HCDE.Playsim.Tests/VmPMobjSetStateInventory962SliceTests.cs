namespace HCDE.Playsim.Tests;

public class VmPMobjSetStateInventory962SliceTests
{
    [Fact]
    public void StateLoopExhausted_MatchesNativeGuard()
    {
        Assert.True(VmPMobjSetStateInventory962Slice.StateLoopExhausted(0));
    }

    [Fact]
    public void ResolveSpriteAfterStateChange_UsesSkinWhenEligible()
    {
        var sprite = VmPMobjSetStateInventory962Slice.ResolveSpriteAfterStateChange(
            flags4: 0,
            newSprite: 5,
            spawnStateSprite: 5,
            previousSprite: 4,
            hasPlayer: true,
            skinCount: 2,
            currentSprite: 1,
            pickSkinSprite: () => 99);
        Assert.Equal(99, sprite);
    }

    [Fact]
    public void ResolveSpriteAfterStateChange_KeepsSpriteWhenNoSkinAndSameBodySprite()
    {
        var sprite = VmPMobjSetStateInventory962Slice.ResolveSpriteAfterStateChange(
            flags4: 0,
            newSprite: 5,
            spawnStateSprite: 5,
            previousSprite: 5,
            hasPlayer: false,
            skinCount: 0,
            currentSprite: 3,
            pickSkinSprite: null);
        Assert.Equal(3, sprite);
    }

    [Fact]
    public void ShouldMarkRecreateLights_WhenAnyLightAssociationOrStateLight()
    {
        Assert.True(VmPMobjSetStateInventory962Slice.ShouldMarkRecreateLights(false, true, false));
    }

    [Fact]
    public void ApplyRecreateLightsFlags_SetsMf8Bit()
    {
        var flags = VmPMobjSetStateInventory962Slice.ApplyRecreateLightsFlags(0);
        Assert.Equal(VmPMobjSetStateInventory962Slice.Mf8RecreateLights, flags);
    }

    [Fact]
    public void CollectInventoryForDestruction_UnlinksChain()
    {
        var a = new InvNode { Next = null };
        var b = new InvNode { Next = a };
        object? head = b;
        var pending = new List<object>();
        VmPMobjSetStateInventory962Slice.CollectInventoryForDestruction(
            head,
            n => ((InvNode)n).Next,
            _ => { },
            n => ((InvNode)n).Next = null,
            pending,
            ref head);
        Assert.Null(head);
        Assert.Equal(2, pending.Count);
    }

    [Fact]
    public void DestroyPendingInventory_SkipsEuthanizedItems()
    {
        var destroyed = new List<object>();
        var live = new FlagNode { Flags = 0 };
        var dead = new FlagNode { Flags = VmPMobjSetStateInventory962Slice.OfEuthanizeMe };
        VmPMobjSetStateInventory962Slice.DestroyPendingInventory(
            [live, dead],
            n => ((FlagNode)n).Flags,
            destroyed.Add);
        Assert.Single(destroyed);
        Assert.Same(live, destroyed[0]);
    }

    [Fact]
    public void ShouldAbortSetStateAfterAction_WhenEuthanizing()
    {
        Assert.True(VmPMobjSetStateInventory962Slice.ShouldAbortSetStateAfterAction(
            VmPMobjSetStateInventory962Slice.OfEuthanizeMe));
    }

    private sealed class InvNode
    {
        public InvNode? Next { get; set; }
    }

    private sealed class FlagNode
    {
        public int Flags { get; set; }
    }
}
