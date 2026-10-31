namespace HCDE.Playsim.Tests;

public class VmPMobjFloorBounce1111SliceTests
{
    [Fact]
    public void ResolveBounceSound_UsesWallWhenAirborneAndValid()
    {
        var volume = 2.0;
        var choice = VmPMobjFloorBounce1111Slice.ResolveBounceSound(
            onFloor: false,
            VmPMobjFloorBounce1111Slice.BounceFloors,
            wallSoundValid: true,
            ref volume);
        Assert.Equal(VmPMobjFloorBounce1111Slice.BounceSoundChoice.Wall, choice);
        Assert.Equal(1.0, volume);
    }

    [Fact]
    public void SpecialBounceMissileOutcome_InvertsNativeCases()
    {
        Assert.False(VmPMobjFloorBounce1111Slice.SpecialBounceMissileOutcome(1));
        Assert.True(VmPMobjFloorBounce1111Slice.SpecialBounceMissileOutcome(0));
        Assert.Null(VmPMobjFloorBounce1111Slice.SpecialBounceMissileOutcome(2));
    }

    [Fact]
    public void RunFloorBounceMissile_DestroysOnSkyWhenFlagged()
    {
        var ctx = new VmPMobjFloorBounce1111Slice.FloorBounceContext
        {
            BounceFlags = VmPMobjFloorBounce1111Slice.BounceFloors | VmPMobjFloorBounce1111Slice.BounceNotOnSky,
            NormalZ = 1,
            FloorIsSky = true
        };
        Assert.True(VmPMobjFloorBounce1111Slice.RunFloorBounceMissile(ctx));
        Assert.True(ctx.Destroyed);
    }

    [Fact]
    public void RunFloorBounceMissile_HereticEntersDeathAndReturnsFalse()
    {
        var ctx = new VmPMobjFloorBounce1111Slice.FloorBounceContext
        {
            BounceFlags = VmPMobjFloorBounce1111Slice.BounceFloors | VmPMobjFloorBounce1111Slice.BounceHereticType,
            NormalZ = 1,
            BounceCount = 2
        };
        Assert.False(VmPMobjFloorBounce1111Slice.RunFloorBounceMissile(ctx));
        Assert.True(ctx.EnterDeathState);
        Assert.Equal(1, ctx.BounceCount);
    }
}
