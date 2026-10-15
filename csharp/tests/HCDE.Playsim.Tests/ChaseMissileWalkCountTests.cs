using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseMissileWalkCountTests
{
    [Theory]
    [InlineData(false, 0, true)]
    [InlineData(false, 1, false)]
    [InlineData(false, -1, false)]
    [InlineData(false, int.MinValue, false)]
    [InlineData(true, 5, true)]
    [InlineData(true, -5, true)]
    public void NativeMissileSelectionRequiresZeroCountUnlessFast(bool fast, int count, bool attacks)
    {
        var (sim, actor) = Room(fast); actor.MoveCount = count;
        Chase(sim, actor);
        Assert.Equal(attacks ? 8 : 0, actor.Brain!.WindupTics);
        Assert.Equal(attacks, actor.JustAttacked);
    }

    [Fact]
    public void BlockedSelectionDoesNotConsumeJustHitRetaliation()
    {
        var (sim, actor) = Room(false); actor.MoveCount = 5; actor.JustHit = true;
        Chase(sim, actor); Assert.True(actor.JustHit); Assert.Equal(0, actor.Brain!.WindupTics);
        actor.MoveCount = 0; Chase(sim, actor);
        Assert.False(actor.JustHit); Assert.Equal(8, actor.Brain.WindupTics);
    }

    [Fact]
    public void WalkCountDoesNotInterruptExistingWindup()
    {
        var (sim, actor) = Room(false); Chase(sim, actor);
        actor.MoveCount = 5; Chase(sim, actor);
        Assert.Equal(7, actor.Brain!.WindupTics);
    }

    [Fact]
    public void MeleeSelectionBypassesMissileWalkCountGate()
    {
        var (sim, actor) = Room(false); actor.X = Fixed.FromInt(280); actor.MoveCount = 5;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            nativeMovement: true);
        Assert.Equal(8, actor.Brain.WindupTics);
    }

    [Fact]
    public void LegacySelectionRemainsIndependentOfWalkCount()
    {
        var (sim, actor) = Room(false); actor.MoveCount = 5;
        actor.Brain!.Tick(sim, actor, useMelee: false);
        Assert.Equal(8, actor.Brain.WindupTics);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, useMelee: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room(bool fast)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 300 }]
        }, spawnOptions: new SpawnOptions(FastMonsters: fast));
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
