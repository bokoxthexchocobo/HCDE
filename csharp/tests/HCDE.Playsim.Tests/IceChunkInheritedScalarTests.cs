using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkInheritedScalarTests
{
    [Fact]
    public void DebrisInheritsNativeActorScalarDefaults()
    {
        var chunk = new IceChunkActor(10);
        Assert.Equal(1000, chunk.Health); Assert.Equal(1000, chunk.SpawnHealth());
        Assert.Equal(-1000, chunk.GetGibHealth()); Assert.Equal(0, chunk.MovementSpeed.Raw);
        Assert.Equal(8, chunk.ReactionTime); Assert.Equal(0, chunk.PainChance);
        Assert.False(chunk.CanTakeDamage);
    }

    [Fact]
    public void RevivingDebrisRestoresPositiveNativeSpawnHealth()
    {
        var chunk = new IceChunkActor(10); chunk.RestoreHealth(0);
        ActorRaise.ReviveSupported(chunk);
        Assert.Equal(1000, chunk.Health); Assert.False(chunk.IsDead);
        Assert.False(chunk.Solid); Assert.False(chunk.Shootable);
    }

    [Fact]
    public void RuntimeScalarsAndBaselineReloadUseNativeDefaults()
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunk = sim.Actors.OfType<IceChunkActor>().First(); var baseline = SimSavegame.Write(sim, true);
        chunk.RestoreHealth(75); chunk.ReactionTime = 23; chunk.PainChance = 17;
        chunk.MovementSpeed = Fixed.FromInt(9); chunk.HasTuningOverride = true;
        var changed = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, changed);
        var loaded = Assert.IsType<IceChunkActor>(fresh.Actors.Single(a => a.Id == chunk.Id));
        Assert.Equal(75, loaded.Health); Assert.Equal(23, loaded.ReactionTime);
        Assert.Equal(17, loaded.PainChance); Assert.Equal(9, loaded.MovementSpeed.ToDouble());
        Assert.Equal(changed, SimSavegame.Write(fresh, true));
        SimSavegame.Apply(sim, baseline); SimSavegame.Apply(fresh, baseline);
        Assert.Equal(1000, loaded.Health); Assert.Equal(8, loaded.ReactionTime);
        Assert.Equal(0, loaded.PainChance); Assert.Equal(0, loaded.MovementSpeed.Raw);
        Assert.Equal(baseline, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
