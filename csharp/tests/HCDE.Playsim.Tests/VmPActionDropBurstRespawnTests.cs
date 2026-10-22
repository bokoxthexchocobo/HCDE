namespace HCDE.Playsim.Tests;

public class VmPActionDropBurstRespawnTests
{
    [Fact]
    public void CountdownArg_ExplodesMissile()
    {
        var exploded = false;
        var actor = new VmPActionDropBurstRespawn.CountdownActor { Flags = VmPActionDropBurstRespawn.MfMissile };
        actor.Args[0] = 1;
        VmPActionDropBurstRespawn.CountdownArg(new()
        {
            ExplodeMissile = () => exploded = true,
        }, actor, 0, null);
        Assert.True(exploded);
    }

    [Fact]
    public void Burst_DestroysSelf()
    {
        var destroyed = false;
        VmPActionDropBurstRespawn.Burst(new()
        {
            Radius = 20,
            Height = 64,
            BurstRandom = _ => 0,
            SpawnChunk = (_, _, _) => new object(),
            CopyFriendliness = _ => { },
            DestroySelf = () => destroyed = true,
        });
        Assert.True(destroyed);
    }

    [Fact]
    public void Respawn_FailsWhenBlocked()
    {
        var self = new VmPActionDropBurstRespawn.RespawnActor();
        Assert.False(VmPActionDropBurstRespawn.Respawn(new()
        {
            CheckPosition = () => false,
            RestoreSpecialPosition = () => { },
        }, self, 0));
    }
}
