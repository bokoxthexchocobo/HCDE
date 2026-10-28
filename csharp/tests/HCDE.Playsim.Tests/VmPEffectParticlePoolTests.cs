namespace HCDE.Playsim.Tests;

public class VmPEffectParticlePoolTests
{
    [Fact]
    public void NewParticle_LinksInactiveChain()
    {
        var level = new VmPEffectParticlePool.LevelParticles();
        VmPEffectParticlePool.InitParticles(level, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 4 });
        var p = VmPEffectParticlePool.NewParticle(level);
        Assert.NotNull(p);
        Assert.Equal(0, level.ActiveParticles);
        Assert.Equal(1, level.InactiveParticles);
    }

    [Fact]
    public void FreeParticle_ReturnsToInactiveList()
    {
        var level = new VmPEffectParticlePool.LevelParticles();
        VmPEffectParticlePool.InitParticles(level, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 2 });
        VmPEffectParticlePool.NewParticle(level);
        VmPEffectParticlePool.FreeParticle(level, 0);
        Assert.Equal(VmPEffectParticlePool.NoParticle, level.ActiveParticles);
        Assert.Equal(0, level.InactiveParticles);
    }

    [Fact]
    public void InitParticles_ClampsCount()
    {
        var level = new VmPEffectParticlePool.LevelParticles();
        VmPEffectParticlePool.InitParticles(level, new VmPEffectParticlePool.InitParticlesArgs { NumParticlesArg = "10" });
        Assert.Equal(100, level.Particles.Count);
    }

    [Fact]
    public void FindParticleSubsectors_BucketsActiveParticles()
    {
        var level = new VmPEffectParticlePool.LevelParticles();
        VmPEffectParticlePool.InitParticles(level, new VmPEffectParticlePool.InitParticlesArgs { RMaxParticles = 4 });
        var ss = new VmPEffectParticlePool.SubsectorBucket { Index = 0 };
        level.Subsectors.Add(ss);
        var p = VmPEffectParticlePool.NewParticle(level)!;
        p.Subsector = ss;
        p.PosX = 1;
        VmPEffectParticlePool.FindParticleSubsectors(level, true);
        Assert.Equal(0, level.ParticlesInSubsec[0]);
    }

    [Fact]
    public void InitEffects_WritesNamedPalette()
    {
        var args = new VmPEffectParticlePool.InitEffectsArgs { DefaultBloodParticleColor = 0xff0000 };
        VmPEffectParticlePool.InitEffects(args);
        Assert.True(args.NamedColors.ContainsKey("orange"));
        Assert.NotEqual(0, args.Blood1);
    }
}
