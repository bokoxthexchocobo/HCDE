namespace HCDE.Playsim.Tests;

public class VmPPSprWeaponReadyTests
{
    [Fact]
    public void LowerSpeed_MatchesNative() => Assert.Equal(6.0, VmPPSprWeaponReady.LowerSpeed);

    [Fact]
    public void ButtonChecks_HasZoomReloadUserSlots()
    {
        Assert.Equal(6, VmPPSprWeaponReady.ButtonChecks.Length);
        Assert.Contains(VmPPSprWeaponReady.ButtonChecks, b => b.StateName == "Zoom");
    }

    [Fact]
    public void PspfWeaponBobFlags_MatchNativeBits()
    {
        Assert.Equal(
            VmPPSprWeaponReady.PspfAddWeapon | VmPPSprWeaponReady.PspfAddBob,
            VmPPSprWeaponReady.DefaultWeaponPspriteFlags & (VmPPSprWeaponReady.PspfAddWeapon | VmPPSprWeaponReady.PspfAddBob));
    }

    [Fact]
    public void InsertPspriteLayer_SortedAndReplacesDuplicateId()
    {
        var owner = new VmPPSprWeaponReady.PlayerPsprites();
        var weapon = new object();
        var dup = new VmPPSprWeaponReady.PspriteNode { Id = 2 };
        owner.Head = dup;
        var inserted = VmPPSprWeaponReady.InsertPspriteLayer(owner, weapon, 2, _ => true, _ => false);
        Assert.True(dup.Destroyed);
        Assert.Same(inserted, owner.Head);
        Assert.NotEqual(0, inserted.Flags);
    }

    [Fact]
    public void FindPsprite_WalksChain()
    {
        var owner = new VmPPSprWeaponReady.PlayerPsprites();
        VmPPSprWeaponReady.InsertPspriteLayer(owner, new(), 1, _ => false, _ => false);
        var node = VmPPSprWeaponReady.InsertPspriteLayer(owner, new(), 3, _ => false, _ => false);
        Assert.Same(node, VmPPSprWeaponReady.FindPsprite(owner, 3));
    }
}
