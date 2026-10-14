using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffDefaultTests
{
    [Fact]
    public void NativeDefaultsAreVisibleThroughActorFlagsAndRenderProperties()
    {
        var puff = new PuffActor();
        Assert.Equal(ActorRenderStyle.Translucent, puff.RenderStyle); Assert.Equal(0.5, puff.Alpha);
        Assert.Equal(puff.RenderStyle, puff.DefaultRenderStyle); Assert.Equal(puff.Alpha, puff.DefaultAlpha);
        Assert.True(AcsActorFlags.TryGet(puff, "NOBLOCKMAP", out var noBlockmap)); Assert.True(noBlockmap);
        Assert.True(AcsActorFlags.TryGet(puff, "NOGRAVITY", out var noGravity)); Assert.True(noGravity);
        Assert.False(puff.Solid); Assert.False(puff.Shootable); Assert.False(puff.IsBlockmapActor);
        Assert.Equal(5, puff.Mass); Assert.Equal(Fixed.FromInt(1), puff.VelocityZ);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void DefaultsAndOverridesSurviveSaveAndReconstruction(bool modified, bool reconstruct)
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        if (modified)
        {
            puff.Alpha = 0.75; puff.RenderStyle = ActorRenderStyle.Normal;
            Assert.True(AcsActorFlags.TrySet(puff, "NOBLOCKMAP", false));
        }
        var before = SimSavegame.Write(sim, true);
        var expectedAlpha = puff.Alpha; var expectedStyle = puff.RenderStyle; var expectedFlag = puff.NoBlockmap;
        if (reconstruct) { puff.Destroy(); sim.Tick(); }
        else
        {
            puff.Alpha = 0; puff.RenderStyle = ActorRenderStyle.OptFuzzy;
            Assert.True(AcsActorFlags.TrySet(puff, "NOBLOCKMAP", !expectedFlag));
        }
        SimSavegame.Apply(sim, before);
        var restored = sim.Actors.OfType<PuffActor>().Single();
        Assert.Equal(expectedAlpha, restored.Alpha); Assert.Equal(expectedStyle, restored.RenderStyle);
        Assert.Equal(expectedFlag, restored.NoBlockmap); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageSourceReadBarrierClearsDestroyedActor(bool remove)
    {
        var sim = Room(); var source = sim.AddBot(200, 0); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        puff.DamageSource = source; Assert.Same(source, puff.DamageSource);
        source.Destroy(); if (remove) sim.Tick();
        Assert.Null(puff.DamageSource); Assert.Null(puff.DamageSource);
        var before = SimSavegame.Write(sim, true); puff.Destroy(); SimSavegame.Apply(sim, before);
        Assert.Null(sim.Actors.OfType<PuffActor>().Single().DamageSource);
    }

    [Fact]
    public void SourceReferenceRebindsToRestoredActorRatherThanDestroyedInstance()
    {
        var sim = Room(); var source = sim.AddBot(200, 0); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        puff.DamageSource = source; var before = SimSavegame.Write(sim, true);
        source.Destroy(); Assert.Null(puff.DamageSource); sim.Tick(); SimSavegame.Apply(sim, before);
        var restoredSource = sim.Actors.Single(actor => actor.Id == source.Id);
        Assert.NotSame(source, restoredSource); Assert.Same(restoredSource, puff.DamageSource);
        restoredSource.Destroy(); Assert.Null(puff.DamageSource);
    }

    [Fact]
    public void OrdinarySaveRestoresNativeDefaultsAfterLaterOverrides()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        var before = SimSavegame.Write(sim);
        puff.Alpha = 0; puff.RenderStyle = ActorRenderStyle.Normal; puff.NoBlockmap = false;
        SimSavegame.Apply(sim, before);
        Assert.Equal(ActorRenderStyle.Translucent, puff.RenderStyle); Assert.Equal(0.5, puff.Alpha);
        Assert.True(puff.NoBlockmap); Assert.Equal(before, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
