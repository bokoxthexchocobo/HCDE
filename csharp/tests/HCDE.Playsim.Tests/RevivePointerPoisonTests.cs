using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RevivePointerPoisonTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RevivalClearsTargetForBrainBackedAndBrainlessActors(bool brain)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); if (!brain) actor.Brain = null;
        actor.SetTargetPointer(sim.Players.Single()); actor.StoredTargetId = sim.Players.Single().Id;
        ActorRaise.ReviveSupported(actor);
        Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId); Assert.Null(actor.Brain?.LastEnemyId);
    }

    [Fact]
    public void ExplicitRaiseClearsNonMonsterStoredKiller()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        actor.Brain = null; actor.IsMonster = false; actor.RaiseDuration = 0; actor.RaiseState = ActorStateMachine.Spawn;
        ActorDamage.Apply(actor, 1000, sim.Players.Single()); actor.States.Enter(actor, ActorStateMachine.Corpse);
        Assert.NotNull(actor.TargetId); Assert.True(ActorRaiseActions.RaiseSelf(actor)); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void PlayerPoisonAndTargetAreClearedBeforeOnRevive()
    {
        var player = new ObservingPlayer { PoisonCount = 50, PoisonSourceId = 3,
            PoisonDamageType = "Fire", PoisonPainType = "Ice", ResurrectionHealth = 100 };
        player.StoredTargetId = 4; player.RestoreHealth(0);
        ActorRaise.ReviveSupported(player); Assert.True(player.Observed); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void BlockedRaiseDoesNotClearStoredTarget()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.RaiseState = ActorStateMachine.Spawn; actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.SetTargetPointer(sim.Players.Single()); sim.Ceilings[0] = 10;
        Assert.False(ActorRaiseActions.RaiseSelf(actor)); Assert.Equal(sim.Players.Single().Id, actor.TargetId);
    }

    [Fact]
    public void ClearedStoredTargetSurvivesSaveRestoration()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); actor.Brain = null;
        actor.SetTargetPointer(sim.Players.Single()); ActorRaise.ReviveSupported(actor);
        var saved = SimSavegame.Write(sim); actor.SetTargetPointer(sim.Players.Single()); SimSavegame.Apply(sim, saved);
        Assert.Null(actor.TargetId); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    private sealed class ObservingPlayer : PlayerPawn
    {
        public bool Observed { get; private set; }
        public override void OnRevive()
        {
            Assert.Null(TargetId); Assert.Equal(0, PoisonCount); Assert.Null(PoisonSourceId);
            Assert.Null(PoisonDamageType); Assert.Null(PoisonPainType); Observed = true;
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
