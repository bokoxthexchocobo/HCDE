namespace HCDE.Playsim.Tests;

public class VmPThingsRaiseRemoveWarpProximityTests
{
    [Fact]
    public void P_RemoveThing_SkipsLivePlayerMo()
    {
        var actor = new VmPThingsRaiseRemoveWarpProximity.RemoveThingActor
        {
            HasPlayer = true,
            PlayerIsMo = true,
        };
        VmPThingsRaiseRemoveWarpProximity.P_RemoveThing(actor, () => throw new InvalidOperationException(), () => throw new InvalidOperationException());
        Assert.False(actor.Removed);
    }

    [Fact]
    public void P_Thing_CanRaise_RestoresDimensions()
    {
        var thing = new VmPThingsRaiseRemoveWarpProximity.RaiseActor
        {
            Height = 16,
            Radius = 20,
            DefaultInfo = new object(),
        };
        var ctx = new VmPThingsRaiseRemoveWarpProximity.RaiseContext
        {
            Thing = thing,
            GetRaiseState = _ => new object(),
            DefaultDimensions = _ => (56, 32),
            CheckPosition = _ => true,
        };
        Assert.True(VmPThingsRaiseRemoveWarpProximity.P_Thing_CanRaise(ctx));
        Assert.Equal(16, thing.Height);
        Assert.Equal(20, thing.Radius);
    }

    [Fact]
    public void P_Thing_SetVelocity_ClearsWhenNotAdd()
    {
        var actor = new VmPThingsRaiseRemoveWarpProximity.VelocityActor
        {
            VelX = 1,
            VelY = 2,
            VelZ = 3,
            HasPlayer = true,
            PlayerVelX = 4,
            PlayerVelY = 5,
        };
        VmPThingsRaiseRemoveWarpProximity.P_Thing_SetVelocity(
            actor, new VmPThingsSpawnMoveIntercept.Vec3(10, 0, 0), add: false, setbob: true);
        Assert.Equal(10, actor.VelX);
        Assert.Equal(10, actor.PlayerVelX);
        Assert.Equal(0, actor.PlayerVelY);
    }

    [Fact]
    public void ProximityInRange_MatchesNativeZGate()
    {
        Assert.True(VmPThingsRaiseRemoveWarpProximity.ProximityInRange(
            0, 0, 32, 56, 10, 0, 0, 32, 100, noZ: false));
        Assert.False(VmPThingsRaiseRemoveWarpProximity.ProximityInRange(
            0, 0, 0, 32, 200, 0, 0, 32, 50, noZ: false));
    }

    [Fact]
    public void P_Thing_Warp_RestoresOnFailedFit()
    {
        var caller = new VmPThingsRaiseRemoveWarpProximity.WarpActor { PosX = 1, PosY = 2, PosZ = 3, Radius = 16, Height = 56 };
        var reference = new VmPThingsRaiseRemoveWarpProximity.WarpActor { PosX = 10, PosY = 10, PosZ = 0, YawDegrees = 90, Radius = 16, Height = 56 };
        var ok = VmPThingsRaiseRemoveWarpProximity.P_Thing_Warp(new VmPThingsRaiseRemoveWarpProximity.WarpContext
        {
            Caller = caller,
            Reference = reference,
            TestMobjLocation = _ => false,
        });
        Assert.False(ok);
        Assert.Equal(1, caller.PosX);
        Assert.Equal(2, caller.PosY);
        Assert.Equal(3, caller.PosZ);
    }

    [Fact]
    public void P_Thing_Warp_AppliesYawOnSuccess()
    {
        var caller = new VmPThingsRaiseRemoveWarpProximity.WarpActor { PosX = 0, PosY = 0, PosZ = 0, Radius = 16, Height = 56 };
        var reference = new VmPThingsRaiseRemoveWarpProximity.WarpActor { PosX = 10, PosY = 0, PosZ = 0, YawDegrees = 0, Radius = 16, Height = 56 };
        var ok = VmPThingsRaiseRemoveWarpProximity.P_Thing_Warp(new VmPThingsRaiseRemoveWarpProximity.WarpContext
        {
            Caller = caller,
            Reference = reference,
            AngleDegrees = 90,
            Flags = VmPThingsRaiseRemoveWarpProximity.WarpfAbsoluteposition,
            TestMobjLocation = _ => true,
        });
        Assert.True(ok);
        Assert.Equal(90, caller.YawDegrees);
    }
}
