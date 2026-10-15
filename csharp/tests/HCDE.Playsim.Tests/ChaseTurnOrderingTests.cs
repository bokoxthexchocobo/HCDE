using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseTurnOrderingTests
{
    [Theory]
    [InlineData(false, 45)]
    [InlineData(true, 0)]
    public void PostAttackReturnStillPerformsInitialTurn(bool suppress, int expected)
    {
        var (sim, actor) = Room(); actor.SetTargetPointer(sim.Players.Single()); actor.JustAttacked = true;
        Chase(sim, actor, suppress ? MonsterChaseFlags.NoDirectionTurn : 0);
        Assert.Equal(BamAngle.FromDegrees(expected), actor.Angle);
        Assert.False(actor.JustAttacked);
    }

    [Fact]
    public void TargetlessReturnStillTurns()
    {
        var (sim, actor) = Room(); sim.Players.Single().NoTarget = true;
        Chase(sim, actor);
        Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
        Assert.Null(actor.TargetId);
    }

    [Fact]
    public void ChaseAndWanderEachTurnDuringTargetlessFallback()
    {
        var (sim, actor) = Room(); sim.Players.Single().NoTarget = true;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontIdle, nativeMovement: true);
        Assert.Equal(BamAngle.FromDegrees(90), actor.Angle);
    }

    [Fact]
    public void OrdinaryChaseTurnsOnlyOnce()
    {
        var (sim, actor) = Room(); actor.SetTargetPointer(sim.Players.Single());
        Chase(sim, actor); Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
    }

    [Fact]
    public void ConversationPreventsInitialTurn()
    {
        var (sim, actor) = Room(); actor.InConversation = true;
        Chase(sim, actor); Assert.Equal(default, actor.Angle);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = 0) => actor.Brain!.Tick(sim, actor,
        flags | MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, false, false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MoveDirection = 2; actor.MoveCount = 10;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
