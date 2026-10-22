namespace HCDE.Playsim.Tests;

public class VmPThingsSpawnMoveInterceptTests
{
    [Fact]
    public void P_MoveThing_RestoresOriginWhenBlocked()
    {
        var actor = new VmPThingsSpawnMoveIntercept.MoveThingActor { PosX = 1, PosY = 2, PosZ = 3 };
        var ctx = new VmPThingsSpawnMoveIntercept.MoveThingContext
        {
            Source = actor,
            TestMobjLocation = () => false,
        };
        Assert.False(VmPThingsSpawnMoveIntercept.P_MoveThing(ctx, 9, 9, 9));
        Assert.Equal(1, actor.PosX);
        Assert.Equal(2, actor.PosY);
        Assert.Equal(3, actor.PosZ);
    }

    [Fact]
    public void P_MoveThing_SetsRenderFlagOnSuccess()
    {
        var actor = new VmPThingsSpawnMoveIntercept.MoveThingActor();
        var ctx = new VmPThingsSpawnMoveIntercept.MoveThingContext
        {
            Source = actor,
            TestMobjLocation = () => true,
            ClearInterpolation = () => { },
        };
        Assert.True(VmPThingsSpawnMoveIntercept.P_MoveThing(ctx, 4, 5, 6));
        Assert.NotEqual(0, actor.RenderFlags & VmPThingsSpawnMoveIntercept.RfNointerpolateview);
    }

    [Fact]
    public void EvThingSpawn_CountsSuccessfulSpawns()
    {
        var spots = new Queue<object>(new object[] { "a", "b" });
        var ctx = new VmPThingsSpawnMoveIntercept.EvThingSpawnContext
        {
            Tid = 1,
            Type = 1,
            AngleDegrees = VmPThingsSpawnMoveIntercept.AngleSentinelDegrees,
            NewTid = 5,
            GetSpawnableKind = _ => "kind",
            CreateSpotIterator = _ => () => spots.Count > 0 ? spots.Dequeue() : null,
            AsSpot = _ => new VmPThingsSpawnMoveIntercept.SpawnSpotActor { YawDegrees = 45 },
            Spawn = (_, _) => new VmPThingsSpawnMoveIntercept.SpawnedMobj(),
            TestMobjLocation = mobj =>
            {
                mobj.Flags2 = 0;
                return true;
            },
        };
        Assert.True(VmPThingsSpawnMoveIntercept.EvThingSpawn(ctx));
    }

    [Fact]
    public void VelIntercept_LeadTargetRotatesAimVector()
    {
        var mobj = new VmPThingsSpawnMoveIntercept.InterceptActor
        {
            PosX = 0,
            PosY = 0,
            PosZ = 0,
        };
        var targ = new VmPThingsSpawnMoveIntercept.InterceptTarget
        {
            PosX = 100,
            PosY = 0,
            PosZ = 0,
            Height = 32,
            VelX = 10,
            VelY = 0,
            VelZ = 0,
        };
        VmPThingsSpawnMoveIntercept.VelIntercept(
            targ, mobj, speed: 20, leadtarget: true, leadTargetRandom2: () => 128);
        Assert.True(mobj.VelX > 15);
        Assert.True(Math.Abs(mobj.VelY) < 1);
    }

    [Fact]
    public void VelInterceptAction_UsesSelfSpeedWhenNegative()
    {
        var self = new VmPThingsSpawnMoveIntercept.InterceptActor { Speed = 33, PosX = 0, PosY = 0, PosZ = 0 };
        var targ = new VmPThingsSpawnMoveIntercept.InterceptTarget { PosX = 10, PosY = 0, PosZ = 0, Height = 16 };
        VmPThingsSpawnMoveIntercept.VelInterceptAction(new VmPThingsSpawnMoveIntercept.VelInterceptVmArgs
        {
            Self = self,
            Target = targ,
            Speed = -1,
        });
        Assert.True(Math.Abs(self.VelX) + Math.Abs(self.VelY) > 0);
    }
}
