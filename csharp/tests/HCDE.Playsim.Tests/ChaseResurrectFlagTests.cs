using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseResurrectFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagRaisesWithoutEnemyBeforeOrdinaryChaseGuards(bool conversation)
    {
        var (sim, actor, corpse) = Room(); actor.InConversation = conversation;
        actor.ReactionTime = 7; actor.JustAttacked = true; actor.MoveCount = 10;
        Chase(sim, actor);
        Assert.False(corpse.IsDead); Assert.Null(actor.TargetId);
        Assert.Equal(7, actor.ReactionTime); Assert.Equal(10, actor.MoveCount);
        Assert.True(actor.JustAttacked); Assert.Equal(Fixed.FromInt(0), actor.X);
    }

    [Fact]
    public void SuccessfulRaiseUsesActorsOwnHealState()
    {
        var (sim, actor, corpse) = Room(); actor.HealState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        Chase(sim, actor);
        Assert.False(corpse.IsDead); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void NoHealLabelKeepsCurrentState()
    {
        var (sim, actor, corpse) = Room(); actor.HealState = -1;
        Chase(sim, actor); Assert.False(corpse.IsDead); Assert.Equal(0, actor.States.Current);
    }

    [Fact]
    public void FailedRaiseFallsThroughToOrdinaryChase()
    {
        var (sim, actor, corpse) = Room(); actor.MoveDirection = 8;
        actor.SetTargetPointer(sim.Players.Single()); actor.MoveCount = 4;
        Chase(sim, actor);
        Assert.True(corpse.IsDead); Assert.Equal(3, actor.MoveCount);
    }

    [Fact]
    public void LegacyUseRejectsBeforeCorpseMutation()
    {
        var (sim, actor, corpse) = Room();
        Assert.Throws<NotSupportedException>(() => actor.Brain!.Tick(sim, actor, MonsterChaseFlags.Resurrect));
        Assert.True(corpse.IsDead);
    }

    [Fact]
    public void SavedCorpseReplaysRevivalAndSound()
    {
        var (sim, actor, _) = Room(); var saved = SimSavegame.Write(sim, true);
        Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        var sound = Assert.Single(sim.DrainSoundRequests());
        SimSavegame.Apply(sim, saved); Chase(sim, actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        Assert.Equal(sound, Assert.Single(sim.DrainSoundRequests()));
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.Resurrect | MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
        useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, Y = 45 }]
        });
        var corpse = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.MovementSpeed = Fixed.FromInt(8); actor.Radius = Fixed.FromInt(20); actor.MoveDirection = 2;
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        sim.DrainSoundRequests();
        return (sim, actor, corpse);
    }
}
