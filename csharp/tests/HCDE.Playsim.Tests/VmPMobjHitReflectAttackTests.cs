namespace HCDE.Playsim.Tests;

public class VmPMobjHitReflectAttackTests
{
    [Fact]
    public void SpecialMissileHit_DefaultWhenNoVirtual() =>
        Assert.Equal(-1, VmPMobjHitReflectAttack.SpecialMissileHit(false, null));

    [Fact]
    public void AdjustReflectionAngle_DontReflectExplodes()
    {
        var angle = 10.0;
        var explode = VmPMobjHitReflectAttack.AdjustReflectionAngle(
            new() { Flags2 = VmPMobjHitReflectAttack.Mf2Dontreflect },
            new(),
            ref angle,
            64,
            null);
        Assert.True(explode);
    }

    [Fact]
    public void AdjustReflectionAngle_ShieldAdds45()
    {
        var angle = 0.0;
        VmPMobjHitReflectAttack.AdjustReflectionAngle(
            new() { Flags7 = 0 },
            new()
            {
                Flags4 = VmPMobjHitReflectAttack.Mf4Shieldreflect,
                YawDeg = 0
            },
            ref angle,
            64,
            (_, _) => 10);
        Assert.Equal(45, angle);
    }

    [Fact]
    public void AbsorbDamage_WalksInventoryChain()
    {
        var hits = 0;
        var tail = new VmPMobjHitReflectAttack.InventoryItem { HasAbsorbVirtual = true };
        var head = new VmPMobjHitReflectAttack.InventoryItem { Inventory = tail, HasAbsorbVirtual = true };
        VmPMobjHitReflectAttack.AbsorbDamage(head, 5, (_, _) => hits++);
        Assert.Equal(2, hits);
    }

    [Fact]
    public void PlayActiveSound_SkipsBusyVoice()
    {
        var played = false;
        VmPMobjHitReflectAttack.PlayActiveSound(
            new() { ActiveSoundValid = true, VoiceChannelBusy = true },
            _ => played = true);
        Assert.False(played);
    }

    [Fact]
    public void IsOkayToAttack_RejectsSelf()
    {
        var link = new VmPMobjHitReflectAttack.AttackLink { IsSelf = true, Player = true, Flags = 4 };
        Assert.False(VmPMobjHitReflectAttack.IsOkayToAttack(
            new() { DefaultFriend = link },
            link,
            false,
            false,
            () => true,
            null,
            0,
            0));
    }

    [Fact]
    public void IsOkayToAttack_ScreenSeekerFov()
    {
        var friend = new VmPMobjHitReflectAttack.AttackLink { Player = true, LinkPlayer = true };
        var link = new VmPMobjHitReflectAttack.AttackLink { Player = true, Flags = 4, Flags3 = 0 };
        Assert.True(VmPMobjHitReflectAttack.IsOkayToAttack(
            new() { Flags3 = VmPMobjHitReflectAttack.Mf3Screenseeker, DefaultFriend = friend },
            link,
            false,
            false,
            () => true,
            (_, _) => 5,
            0,
            0));
    }
}
