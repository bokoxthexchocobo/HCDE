using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkInheritedFreezeTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void InheritedIceDeathRespectsClassificationAndNoIceDeath(bool monster, bool noIceDeath, bool freezes)
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunk = sim.Actors.OfType<IceChunkActor>().First();
        ActorPropertyActions.ChangeFlag(chunk, "SHOOTABLE", true);
        ActorPropertyActions.ChangeFlag(chunk, "ISMONSTER", monster);
        ActorPropertyActions.ChangeFlag(chunk, "NOICEDEATH", noIceDeath);
        chunk.RestoreHealth(10); chunk.SetXYZ(200, 0, 0);
        chunk.VelocityX = chunk.VelocityY = chunk.VelocityZ = default;
        Assert.True(AcsActorStates.TryFindNamedState(chunk, "GenericFreezeDeath", true, out var state));
        ActorDamage.Apply(chunk, 20, damageType: "Ice");
        Assert.Equal(!freezes, chunk.Destroyed);
        if (!freezes) return;
        Assert.True(chunk.IceCorpse); Assert.Equal(state, chunk.States.Current);
        Assert.Equal(4, chunk.Height.ToDouble()); Assert.False(chunk.FullBright);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var loaded = fresh.Actors.Single(a => a.Id == chunk.Id);
        chunk.States.ForceRemainingTics(1); loaded.States.ForceRemainingTics(1);
        var count = sim.Actors.OfType<IceChunkActor>().Count();
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
        Assert.True(sim.Actors.OfType<IceChunkActor>().Count() > count);
        sim.Tick(); fresh.Tick(); Assert.True(chunk.Destroyed); Assert.True(loaded.Destroyed);
        Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
