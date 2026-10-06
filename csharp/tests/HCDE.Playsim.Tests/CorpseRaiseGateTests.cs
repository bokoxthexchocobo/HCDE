using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CorpseRaiseGateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void RaiseEligibilityUsesExplicitCorpseFlag(bool corpseFlag, bool raises)
    {
        var sim = Room(); Actor actor = sim.AddBot(0, 0, 3004); Ready(actor);
        actor.Corpse = corpseFlag;
        Assert.Equal(raises, ActorRaise.CanRaise(sim, actor));
        Assert.Equal(raises, ActorRaiseActions.RaiseSelf(actor));
        Assert.Equal(!raises, actor.IsDead);
        Assert.False(actor.Corpse);
    }

    [Fact]
    public void DontCorpseDeathCannotBeRaisedEvenWithPositionBypass()
    {
        var sim = Room(); Actor actor = sim.AddBot(0, 0, 3004); actor.DontCorpse = true; Ready(actor);
        Assert.False(actor.Corpse);
        Assert.False(ActorRaiseActions.RaiseSelf(actor, 2));
        Assert.True(actor.IsDead);
    }

    [Fact]
    public void ExplicitLivingCorpseWithRaiseSupportCanBeRevived()
    {
        var sim = Room(); Actor actor = sim.AddBot(0, 0, 3004);
        actor.Corpse = true; actor.States.Enter(actor, ActorStateMachine.Corpse);
        Assert.True(ActorRaise.CanRaise(sim, actor));
        Assert.True(ActorRaiseActions.RaiseSelf(actor));
        Assert.False(actor.Corpse); Assert.False(actor.IsDead);
    }

    [Fact]
    public void SavedClearedCorpseFlagStillBlocksResurrection()
    {
        var sim = Room(); Actor actor = sim.AddBot(0, 0, 3004); Ready(actor); actor.Corpse = false;
        var saved = SimSavegame.Write(sim); actor.Corpse = true; SimSavegame.Apply(sim, saved);
        actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        Assert.False(ActorRaiseActions.RaiseSelf(actor)); Assert.True(actor.IsDead);
    }

    private static void Ready(Actor actor)
    {
        actor.Health = 0; actor.Solid = false; actor.States.Enter(actor, ActorStateMachine.Corpse);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }]
    });
}
