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
    public void P_Thing_CheckProximity_ExactCountMatch()
    {
        var candidates = new[]
        {
            new VmPThingsRaiseRemoveWarpProximity.ProximityCandidate
            {
                Actor = 1,
                ExactClassMatch = true,
                InRange = true,
                Distance2D = 10,
            },
            new VmPThingsRaiseRemoveWarpProximity.ProximityCandidate
            {
                Actor = 2,
                ExactClassMatch = true,
                InRange = true,
                Distance2D = 20,
            },
        };
        var result = VmPThingsRaiseRemoveWarpProximity.P_Thing_CheckProximity(
            new VmPThingsRaiseRemoveWarpProximity.CheckProximityContext
            {
                Self = new VmPThingsRaiseRemoveWarpProximity.ProximityPointerActor(),
                Ref = new VmPThingsRaiseRemoveWarpProximity.ProximityPointerActor(),
                ClassValid = true,
                Distance = 100,
                Count = 2,
                Flags = 0,
                Counting = false,
                Candidates = candidates,
            });
        Assert.Equal(1, result);
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
}
