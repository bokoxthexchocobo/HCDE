namespace HCDE.Playsim.Tests;

public class VmPActionDebrisParticleTests
{
    [Fact]
    public void SpawnDebris_SpawnsPerDefaultHealth()
    {
        var n = 0;
        var ctx = new VmPActionDebrisParticle.DebrisContext
        {
            DefaultHealth = 3,
            SpawnDebris = (_, _, _) => new object(),
            SetOwnedState = (_, _) => n++,
        };
        Assert.Equal(3, VmPActionDebrisParticle.SpawnDebris(ctx, 1, 1));
        Assert.Equal(3, n);
    }

    [Fact]
    public void SpawnParticle_SkipsZeroLifetime()
    {
        Assert.False(VmPActionDebrisParticle.SpawnParticle(new() { Lifetime = 0 }));
    }
}
