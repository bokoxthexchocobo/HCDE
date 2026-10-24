namespace HCDE.Playsim.Tests;

public class VmPMobjExplodeBounceTests
{
    [Fact]
    public void ExplodeMissile_SkyWithoutSkyExplode_Destroyed()
    {
        var mo = new VmPMobjExplodeBounce.MissileActor { Flags3 = 0 };
        var ctx = new VmPMobjExplodeBounce.ExplodeContext
        {
            Missile = mo,
            OnSky = true,
            Destroy = _ => mo.Destroyed = true,
        };
        Assert.True(VmPMobjExplodeBounce.ExplodeMissile(ctx));
        Assert.True(mo.Destroyed);
    }

    [Fact]
    public void ExplodeMissile_ClearsMissileFlag()
    {
        var mo = new VmPMobjExplodeBounce.MissileActor { Flags = VmPMobjExplodeBounce.MfMissile | VmPMobjExplodeBounce.MfShootable };
        var ctx = new VmPMobjExplodeBounce.ExplodeContext
        {
            Missile = mo,
            FindState = (_, _) => "Death",
            SetState = (_, _) => { },
        };
        VmPMobjExplodeBounce.ExplodeMissile(ctx);
        Assert.Equal(0, mo.Flags & VmPMobjExplodeBounce.MfMissile);
    }

    [Fact]
    public void FloorBounceMissile_ReflectsVelocity()
    {
        var mo = new VmPMobjExplodeBounce.MissileActor
        {
            BounceFlags = VmPMobjExplodeBounce.BounceFloors,
            VelX = 1,
            VelZ = -2,
            BounceFactor = 0.7,
        };
        var ctx = new VmPMobjExplodeBounce.FloorBounceContext
        {
            Missile = mo,
            NormZ = 1,
            PlayBounceSound = (_, _, _) => { },
        };
        Assert.False(VmPMobjExplodeBounce.FloorBounceMissile(ctx));
        Assert.True(mo.VelZ > 0);
    }

    [Fact]
    public void ExplodeMissile_ExpCount_ReturnsEarly()
    {
        var mo = new VmPMobjExplodeBounce.MissileActor
        {
            Flags3 = VmPMobjExplodeBounce.Mf3Explocount,
            Threshold = 0,
            DefThreshold = 2,
        };
        var ctx = new VmPMobjExplodeBounce.ExplodeContext { Missile = mo };
        Assert.False(VmPMobjExplodeBounce.ExplodeMissile(ctx));
    }

    [Fact]
    public void FloorBounceMissile_BounceCountZero_Explodes()
    {
        var exploded = false;
        var mo = new VmPMobjExplodeBounce.MissileActor
        {
            Flags = VmPMobjExplodeBounce.MfMissile,
            BounceCount = 1,
            BounceFlags = VmPMobjExplodeBounce.BounceFloors,
        };
        var ctx = new VmPMobjExplodeBounce.FloorBounceContext
        {
            Missile = mo,
            NormZ = 1,
            ExplodeMissile = _ => exploded = true,
            PlayBounceSound = (_, _, _) => { },
        };
        Assert.True(VmPMobjExplodeBounce.FloorBounceMissile(ctx));
        Assert.True(exploded);
    }
}
