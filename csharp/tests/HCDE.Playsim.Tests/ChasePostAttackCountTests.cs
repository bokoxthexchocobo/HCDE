using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChasePostAttackCountTests
{
    [Theory]
    [InlineData(false, 256)]
    [InlineData(true, 256)]
    [InlineData(false, 272)]
    [InlineData(false, 384)]
    [InlineData(false, 288)]
    public void StopIfBlockedResetsCountWithExactlyOneWalkDraw(bool fast, int flags)
    {
        var (expected, _) = Room(fast);
        var count = expected.NextTryWalkByte() & 15;
        var random = expected.CaptureState().TryWalkRandomState;
        var (sim, actor) = Room(fast);
        actor.Brain!.Tick(sim, actor, (MonsterChaseFlags)flags, nativeMovement: true);
        Assert.Equal(count, actor.MoveCount);
        Assert.Equal(random, sim.CaptureState().TryWalkRandomState);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
        Assert.False(actor.JustAttacked);
        Assert.Equal(default, actor.X);
        Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void NoPostAttackTurnAloneDoesNotResetWalkCount()
    {
        var (sim, actor) = Room(false);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPostAttackTurn, nativeMovement: true);
        Assert.Equal(123, actor.MoveCount);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void ResetIsLimitedToPostAttackGuard()
    {
        var (sim, actor) = Room(false); actor.JustAttacked = false;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.StopIfBlocked | MonsterChaseFlags.DontMove,
            useMelee: false, useMissile: false, nativeMovement: true);
        Assert.Equal(122, actor.MoveCount);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void SavedGuardReplaysCountAndRandomState()
    {
        var (sim, actor) = Room(false);
        var saved = SimSavegame.Write(sim, true);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.StopIfBlocked, nativeMovement: true);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        actor.Brain.Tick(sim, actor, MonsterChaseFlags.StopIfBlocked, nativeMovement: true);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor) Room(bool fast)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = true }],
            Things = [new LevelThing { Type = 1, X = 300 }]
        }, spawnOptions: new SpawnOptions(FastMonsters: fast));
        var actor = sim.AddBot(0, 0);
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MoveCount = 123; actor.JustAttacked = true;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
