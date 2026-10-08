using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BrainIndependentRaiseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitRaiseWorksWithoutMonsterOrBrain(bool archvile)
    {
        var sim = Room(); var corpse = ConfigureCorpse(sim);
        var raiser = new Actor { Friendly = true };
        var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3),
            new(7, 0, self =>
            {
                calls++; Assert.Null(self.Brain); Assert.False(self.Corpse);
                Assert.True(self.Friendly); Assert.Null(self.LastDamageSourceId);
                Assert.Equal(self.SpawnHealth(), self.Health);
            })], 3);
        Assert.True(ActorRaise.CanRaise(sim, corpse));
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, 1));
        Assert.Equal(1, calls); Assert.Equal(4, corpse.States.Current);
        Assert.Null(corpse.Brain);
        var saved = SimSavegame.Write(sim);
        corpse.States.Enter(corpse, 0);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, calls); Assert.Null(corpse.Brain);
        Assert.Equal(4, corpse.States.Current); Assert.Equal(7, corpse.States.RemainingTics);
        sim.Tick(); Assert.Equal(6, corpse.States.RemainingTics);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    [InlineData(int.MinValue)]
    public void MissingOrInvalidStateDoesNotEnableBrainIndependentRaise(int state)
    {
        var sim = Room(); var corpse = ConfigureCorpse(sim);
        corpse.RaiseState = state;
        corpse.RaiseDuration = 20;
        Assert.False(ActorRaise.CanRaise(sim, corpse));
        Assert.False(ActorRaiseActions.RaiseSelf(corpse, 2));
        Assert.True(corpse.Corpse); Assert.True(corpse.IsDead);
    }

    [Fact]
    public void BrainIndependentRaiseStillRequiresFreePosition()
    {
        var sim = Room(); var corpse = ConfigureCorpse(sim);
        corpse.X = sim.Players.Single().X;
        Assert.False(ActorRaise.CanRaise(sim, corpse));
        Assert.False(ActorRaiseActions.RaiseSelf(corpse));
        Assert.True(corpse.IsDead); Assert.Equal(3, corpse.States.Current);
        Assert.True(ActorRaiseActions.RaiseSelf(corpse, 2));
        Assert.Equal(4, corpse.States.Current);
    }

    private static Actor ConfigureCorpse(AuthoritySimulation sim)
    {
        var corpse = sim.Actors[1]; corpse.Health = 0;
        corpse.Brain = null; corpse.IsMonster = false; corpse.RaiseDuration = 0; corpse.RaiseState = 4;
        corpse.LastDamageSourceId = sim.Players.Single().Id;
        corpse.States.Configure(corpse, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 0)], 3);
        return corpse;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
