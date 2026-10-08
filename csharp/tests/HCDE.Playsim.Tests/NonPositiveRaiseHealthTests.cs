using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonPositiveRaiseHealthTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, -7)]
    [InlineData(false, 100)]
    [InlineData(true, 0)]
    [InlineData(true, -7)]
    [InlineData(true, 100)]
    public void ExplicitStateRestoresCapturedHealthWithoutPositiveHealthGate(bool archvile, int spawnHealth)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1]; corpse.Health = 0;
        corpse.Brain = null; corpse.IsMonster = false; corpse.ResurrectionCollisionFlags = 3;
        corpse.ResurrectionHealth = spawnHealth; corpse.RaiseState = 4;
        var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3),
            new(7, 0, self =>
            {
                calls++; Assert.Equal(spawnHealth, self.Health);
                Assert.False(self.Corpse); Assert.True(self.Friendly);
            })], 3);
        Assert.True(ActorRaise.CanRaise(sim, corpse));
        var raiser = new Actor { Friendly = true };
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, 1));
        Assert.Equal(1, calls); Assert.Equal(spawnHealth, corpse.Health);
        Assert.Equal(spawnHealth <= 0, corpse.IsDead); Assert.False(corpse.Corpse);
        Assert.Equal(4, corpse.States.Current);
        var saved = SimSavegame.Write(sim);
        corpse.RestoreHealth(200); corpse.Corpse = true;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, calls); Assert.Equal(spawnHealth, corpse.Health);
        Assert.False(corpse.Corpse); Assert.Equal(4, corpse.States.Current);
    }
}
