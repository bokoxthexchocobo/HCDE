using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkMissingStateLabelsTests
{
    [Theory]
    [InlineData("Spawn", true)]
    [InlineData("Pain", false)]
    [InlineData("Death", false)]
    [InlineData("Corpse", false)]
    [InlineData("See", false)]
    public void DebrisStateLookupDoesNotExposeGenericActorFrameAliases(string label, bool expected)
    {
        var chunk = new IceChunkActor(10);
        Assert.Equal(expected, AcsActorStates.HasNamedState(chunk, label, true));
        Assert.Equal(-1, chunk.PainState); Assert.Equal(-1, chunk.DeathState);
    }

    [Fact]
    public void ShootableDebrisKeepsFrameOnPainAndRemovesOnFatalDamageAfterRestore()
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunk = sim.Actors.OfType<IceChunkActor>().First();
        ActorPropertyActions.ChangeFlag(chunk, "SHOOTABLE", true);
        chunk.RestoreHealth(10); chunk.PainChance = 256;
        var frame = chunk.States.Current; var tics = chunk.States.RemainingTics;
        Assert.False(chunk.TriggerPainChance()); ActorDamage.Apply(chunk, 1);
        Assert.Equal(9, chunk.Health); Assert.Equal(frame, chunk.States.Current); Assert.Equal(tics, chunk.States.RemainingTics);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == chunk.Id);
        Assert.False(loaded.TriggerPainChance()); Assert.False(chunk.TriggerPainChance());
        ActorDamage.Apply(chunk, 20); ActorDamage.Apply(loaded, 20);
        Assert.True(chunk.Destroyed); Assert.True(loaded.Destroyed);
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
