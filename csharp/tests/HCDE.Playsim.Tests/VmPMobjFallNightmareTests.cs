namespace HCDE.Playsim.Tests;

public class VmPMobjFallNightmareTests
{
    [Fact]
    public void FallAndSink_AppliesGravityWhenDry()
    {
        var actor = new VmPMobjFallNightmare.FallActor { WaterLevel = 0, VelZ = 0 };
        VmPMobjFallNightmare.FallAndSink(actor, grav: -1, oldfloorz: 0);
        Assert.Equal(-1, actor.VelZ);
    }

    [Fact]
    public void FallAndSink_WaterLevelOne_AdjustsVelocity()
    {
        var actor = new VmPMobjFallNightmare.FallActor
        {
            WaterLevel = 1,
            VelZ = -10,
            StartVelZ = 0,
            Mass = 100,
        };
        VmPMobjFallNightmare.FallAndSink(actor, grav: -1, oldfloorz: 0);
        Assert.True(actor.VelZ > -10);
    }

    [Fact]
    public void NightmareRespawn_FailsWhenPositionBlocked()
    {
        var mobj = new VmPMobjFallNightmare.NightmareActor();
        var ctx = new VmPMobjFallNightmare.NightmareContext
        {
            StaticSpawn = _ => new object(),
            CheckPosition = _ => false,
            ClearCounters = _ => { },
        };
        VmPMobjFallNightmare.NightmareRespawn(ctx, mobj);
        Assert.True(mobj.Destroyed);
    }

    [Fact]
    public void NightmareRespawn_SucceedsWhenPositionClear()
    {
        var mobj = new VmPMobjFallNightmare.NightmareActor { SkillRespawnCount = 0 };
        var ctx = new VmPMobjFallNightmare.NightmareContext
        {
            StaticSpawn = _ => new object(),
            CheckPosition = _ => true,
            ClearCounters = _ => { },
            FindFloorCeiling = (_, _) => { },
        };
        VmPMobjFallNightmare.NightmareRespawn(ctx, mobj);
        Assert.Equal(1, mobj.SkillRespawnCount);
        Assert.True(mobj.Destroyed);
    }
}
