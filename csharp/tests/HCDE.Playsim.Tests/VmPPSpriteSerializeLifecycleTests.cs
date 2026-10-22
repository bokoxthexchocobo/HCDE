namespace HCDE.Playsim.Tests;

public class VmPPSpriteSerializeLifecycleTests
{
    [Fact]
    public void SerializePsprite_WritesFieldOrder()
    {
        var pspr = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel { Id = 9, Alpha = 0.5 };
        var keys = new List<string>();
        VmPPSpriteSerializeLifecycle.SerializePsprite(pspr, (k, _) => keys.Add(k));
        Assert.Equal(VmPPSpriteSerializeLifecycle.SerializeFieldOrder, keys);
    }

    [Fact]
    public void DestroyPsprites_UnlinksAll()
    {
        var a = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel();
        var b = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel { Next = a };
        var owner = new VmPPSpriteSerializeLifecycle.PlayerPspriteOwner { Psprites = b };
        var destroyed = 0;
        VmPPSpriteSerializeLifecycle.DestroyPsprites(owner, _ => destroyed++);
        Assert.Null(owner.Psprites);
        Assert.Equal(2, destroyed);
        Assert.True(a.Destroyed);
    }

    [Fact]
    public void SetSafeFlash_UsesOffsetWhenOwned()
    {
        var states = new[]
        {
            new VmPPSpriteSerializeLifecycle.StateSlot(),
            new VmPPSpriteSerializeLifecycle.StateSlot()
        };
        object? setState = null;
        VmPPSpriteSerializeLifecycle.SetSafeFlash(
            new VmPPSpriteSerializeLifecycle.SafeFlashContext
            {
                WeaponBaseClass = "WeaponBase",
                OwnsState = (cls, st) => cls as string == "Gun" && ReferenceEquals(st, states[0]),
                OwnsStateAtOffset = (_, index) => index == 1,
                SetPsprite = (_, _, st, _) => setState = st
            },
            "Gun",
            new object(),
            states[0],
            1);
        Assert.Same(states[1], setState);
    }

    [Fact]
    public void OnDestroy_RemovesFromOwnerList()
    {
        var tail = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel();
        var head = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel { Next = tail };
        var owner = new VmPPSpriteSerializeLifecycle.PlayerPspriteOwner { Psprites = head };
        head.Owner = owner;
        tail.Owner = owner;
        VmPPSpriteSerializeLifecycle.OnDestroy(head, null);
        Assert.Same(tail, owner.Psprites);
    }

    [Fact]
    public void GetYAdjust_UsesStatusBarWhenNotFullscreen()
    {
        var pspr = new VmPPSpriteSerializeLifecycle.PspriteLifecycleModel { Caller = "weapon" };
        var y = VmPPSpriteSerializeLifecycle.GetYAdjust(
            pspr,
            fullscreen: false,
            _ => true,
            (_, _) => 2,
            ad => (float)(ad * 0.5));
        Assert.Equal(1f, y);
    }
}
