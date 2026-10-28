namespace HCDE.Playsim.Tests;

public class VmPMobjTidMissileSlamTests
{
    [Fact]
    public void IsTidUsed_FindsMatchingTid()
    {
        var chain = new VmPMobjTidMissileSlam.TidNode { Tid = 5, Next = new() { Tid = 9 } };
        var ctx = new VmPMobjTidMissileSlam.TidLevelContext
        {
            GetHashHead = (_, _) => chain
        };
        Assert.True(VmPMobjTidMissileSlam.IsTidUsed(ctx, 9, false));
        Assert.False(VmPMobjTidMissileSlam.IsTidUsed(ctx, 2, false));
    }

    [Fact]
    public void FindUniqueTid_LinearSearch()
    {
        var used = new HashSet<int> { 3 };
        var ctx = new VmPMobjTidMissileSlam.TidLevelContext
        {
            GetHashHead = (_, _) => null
        };
        Assert.Equal(4, VmPMobjTidMissileSlam.FindUniqueTid(ctx, 4, 10, false));
    }

    [Fact]
    public void GetMissileDamage_ConstantDamage()
    {
        var ctx = new VmPMobjTidMissileSlam.MissileDamageContext { RollMissileDamage = () => 2 };
        var self = new VmPMobjTidMissileSlam.MissileDamageActor { DamageVal = 3 };
        Assert.Equal((2 + 1) * 3, VmPMobjTidMissileSlam.GetMissileDamage(ctx, self, 7, 1));
    }

    [Fact]
    public void Howl_SkipsWhenAlreadyPlaying()
    {
        var played = false;
        VmPMobjTidMissileSlam.Howl(new()
        {
            IsPlayingHowl = () => true,
            PlayHowl = () => played = true
        });
        Assert.False(played);
    }

    [Fact]
    public void Slam_OnlySlamSolidPass()
    {
        var idle = false;
        var pass = VmPMobjTidMissileSlam.Slam(
            new() { SetIdle = () => idle = true },
            new() { Flags8 = VmPMobjTidMissileSlam.Mf8Onlyslamsolid, Health = 1 },
            new() { Flags = 0 },
            new());
        Assert.True(pass);
        Assert.False(idle);
    }

    [Fact]
    public void CallSlam_UsesVirtualWhenPresent()
    {
        var native = false;
        Assert.True(VmPMobjTidMissileSlam.CallSlam(
            new()
            {
                VirtualSlam = (_, _) => true,
                Native = new() { SetIdle = () => native = true }
            },
            hasVirtual: true,
            new(),
            new(),
            new()));
        Assert.False(native);
    }
}
