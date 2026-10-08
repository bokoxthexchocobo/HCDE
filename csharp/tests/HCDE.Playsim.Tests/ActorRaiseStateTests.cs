using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRaiseStateTests
{
    [Theory]
    [InlineData(false, -1, false, 1, false)]
    [InlineData(true, -1, false, 1, true)]
    [InlineData(true, 4, false, 1, false)]
    [InlineData(true, 4, true, 1, true)]
    [InlineData(true, -1, false, -1, false)]
    [InlineData(true, -1, false, 99, false)]
    public void QueryUsesCorpseFramePermissionAndValidRaiseLabel(bool corpse, int tics,
        bool canRaise, int raiseState, bool expected)
    {
        var actor = new Actor { RaiseState = raiseState };
        actor.States.Configure(actor, [new(tics, 0, CanRaise: canRaise), new(-1, 1)], 0);
        actor.Corpse = corpse;
        Assert.Equal(expected ? 1 : (int?)null, actor.GetRaiseState());
        Assert.Null(actor.Brain);
        Assert.Equal(0, actor.States.Current);
    }

    [Fact]
    public void PlayerCannotReturnRaiseState()
    {
        var player = new PlayerPawn { Corpse = true, RaiseState = 0 };
        Assert.Null(player.GetRaiseState());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothRaiseRoutesEnterConfiguredStateAfterRevivalAndFriendship(bool archvile)
    {
        var sim = Room(); var corpse = sim.Actors[1]; var raiser = new Actor();
        corpse.Health = 0; corpse.RaiseState = 4;
        var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3),
            new(7, 0, self =>
            {
                calls++; Assert.False(self.Corpse); Assert.True(self.Health > 0);
                Assert.True(self.Friendly); Assert.True(self.FullBright);
            }, FullBright: true)], 3);
        raiser.Friendly = true;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, 3));
        Assert.Equal(1, calls); Assert.Equal(4, corpse.States.Current);
        Assert.Equal(7, corpse.States.RemainingTics);
        Assert.Equal(0, corpse.Brain!.RaiseTics);
        var saved = SimSavegame.Write(sim);
        corpse.States.Enter(corpse, 0);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, calls); Assert.Equal(4, corpse.States.Current);
        Assert.Equal(7, corpse.States.RemainingTics); Assert.True(corpse.FullBright);
        sim.Tick();
        Assert.Equal(4, corpse.States.Current); Assert.Equal(6, corpse.States.RemainingTics);
        Assert.Equal(MonsterMode.Raise, corpse.Brain.Mode);
    }

    [Fact]
    public void RaiseLabelQueriesDoNotRequireCurrentEligibility()
    {
        var actor = new Actor { RaiseState = 0 };
        Assert.Null(actor.GetRaiseState());
        Assert.True(AcsActorStates.TryFindNamedState(actor, "Raise", true, out var state));
        Assert.Equal(0, state);
        actor.RaiseState = 99;
        Assert.False(AcsActorStates.HasNamedState(actor, "Raise", true));
    }

    [Fact]
    public void InvalidExplicitStateDoesNotFallBackToDuration()
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.RaiseState = 99;
        Assert.True(corpse.RaiseDuration > 0);
        Assert.False(ActorRaise.CanRaise(sim, corpse));
        Assert.False(ActorRaiseActions.RaiseSelf(corpse, 2));
        Assert.True(corpse.IsDead);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
