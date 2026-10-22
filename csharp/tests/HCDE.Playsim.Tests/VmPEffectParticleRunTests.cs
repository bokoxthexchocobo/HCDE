namespace HCDE.Playsim.Tests;

public class VmPEffectParticleRunTests
{
    [Fact]
    public void ThinkParticles_SkipsFadeWhenFrozen()
    {
        var level = new VmPEffectParticleRun.LevelParticles { Frozen = true };
        var p = new VmPEffectParticleRun.Particle { Ttl = 5, Alpha = 0.8f, FadeStep = 0.5f };
        level.Particles.Add(p);
        level.ActiveParticles = 0;
        VmPEffectParticleRun.ThinkParticles(level);
        Assert.Equal(0.8f, p.Alpha);
    }

    [Fact]
    public void ThinkParticles_ExpiresWhenTtlZero()
    {
        var level = new VmPEffectParticleRun.LevelParticles();
        var p = new VmPEffectParticleRun.Particle { Ttl = 1, Alpha = 0.5f, FadeStep = 0.01f, Size = 2 };
        level.Particles.Add(p);
        level.ActiveParticles = 0;
        VmPEffectParticleRun.ThinkParticles(level);
        Assert.True(p.Ttl <= 0 || p.Alpha <= 0);
    }

    [Fact]
    public void SpawnParticle_SetsFadeFromTtlWhenNegativeStep()
    {
        var level = new VmPEffectParticleRun.LevelParticles();
        var created = VmPEffectParticleRun.SpawnParticle(
            level, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 10, 2, -2, 0,
            VmPEffectParticleRun.SpfFadeInOut, null, null, 0, 0, 0, 0.1);
        Assert.NotNull(created);
        Assert.True(created!.FadeStep < 0);
    }

    [Fact]
    public void RunEffect_SpawnsGrenadeTrailParticles()
    {
        var st = new VmPEffectParticleRun.RunEffectState
        {
            Effects = VmPEffectParticleRun.FxGrenade,
            Radius = 8,
            Height = 20,
            Random = _ => 32,
            JitterParticle = (_, _) => new VmPEffectParticleRun.Particle(),
            Vec3Offset = (x, y, z) => (x, y, z),
        };
        VmPEffectParticleRun.RunEffect(st);
        Assert.NotEmpty(st.Spawned);
    }

    [Fact]
    public void RunEffect_SpawnsRocketTrailParticles()
    {
        var st = new VmPEffectParticleRun.RunEffectState
        {
            Effects = VmPEffectParticleRun.FxRocket,
            Radius = 16,
            Height = 40,
            Random = _ => 64,
            JitterParticle = (_, _) => new VmPEffectParticleRun.Particle(),
            Vec3Offset = (x, y, z) => (x, y, z),
        };
        VmPEffectParticleRun.RunEffect(st);
        Assert.NotEmpty(st.Spawned);
    }
}
