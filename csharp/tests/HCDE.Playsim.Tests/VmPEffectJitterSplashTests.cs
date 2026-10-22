namespace HCDE.Playsim.Tests;

public class VmPEffectJitterSplashTests
{
    [Fact]
    public void JitterParticle_SetsTtlAndAlpha()
    {
        var level = new VmPEffectJitterSplash.SplashLevel { Random = _ => 0 };
        VmPEffectParticlePool.InitParticles(level.Particles, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 2 });
        var p = VmPEffectJitterSplash.JitterParticle(level, 20);
        Assert.NotNull(p);
        Assert.Equal(20, p!.Ttl);
        Assert.Equal(1f, p.Alpha);
    }

    [Fact]
    public void DrawSplash_SpawnsSparkParticles()
    {
        var level = new VmPEffectJitterSplash.SplashLevel { Random = new Random(1).Next };
        VmPEffectParticlePool.InitParticles(level.Particles, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 8 });
        VmPEffectJitterSplash.DrawSplash(level, 3, 0, 0, 0, 0, 1, new VmPEffectJitterSplash.SplashColors { Orange = 1, Yorange = 2 });
        Assert.Equal(3, level.Particles.Particles.Count(p => p.Ttl == 10));
    }

    [Fact]
    public void MakeFountain_SkipsOddTics()
    {
        var actor = new VmPEffectJitterSplash.FountainActor
        {
            Level = new VmPEffectJitterSplash.SplashLevel { Random = _ => 64 },
            LevelTime = 1,
            Radius = 32,
            Height = 16,
            Vec3Angle = (r, a, z) => (r, 0, z),
        };
        VmPEffectParticlePool.InitParticles(actor.Level.Particles, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 2 });
        VmPEffectJitterSplash.MakeFountain(actor, 1, 2);
        Assert.Equal(VmPEffectParticlePool.NoParticle, actor.Level.Particles.ActiveParticles);
    }
}
