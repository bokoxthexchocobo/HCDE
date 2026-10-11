using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseRetaliationTests
{
    [Theory]
    [InlineData(false, 200)]
    [InlineData(false, 4096)]
    [InlineData(true, 200)]
    [InlineData(true, 4096)]
    public void HostileRetaliationConsumesJustHitAndBypassesMissileDelayAndRange(bool friendly, int distance)
    {
        var (sim, hunter, target) = Room(distance);
        hunter.Friendly = friendly;
        hunter.JustHit = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(9, hunter.ReactionTime);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.False(hunter.JustHit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DeferredMissileCheckPreservesJustHit(int gate)
    {
        var (sim, hunter, target) = Room(200);
        hunter.JustHit = true;
        if (gate == 0) hunter.GoalId = target.Id;
        if (gate == 1) target.Health = 0;
        if (gate == 2) hunter.Brain!.Enabled = false;
        hunter.Brain!.Tick(sim, hunter, useMissile: gate != 3);
        Assert.True(hunter.JustHit);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Fact]
    public void BlockedSightPreservesRetaliationUntilSightReturns()
    {
        var (sim, hunter, _) = Room(200);
        var lines = (List<LevelLine>)sim.Level.Lines;
        lines.Add(new LevelLine { X1 = 100, X2 = 100, Y1 = -200, Y2 = 200, SideBack = -1 });
        hunter.JustHit = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.True(hunter.JustHit);
        Assert.Equal(0, hunter.Brain.WindupTics);
        lines.Clear();
        hunter.Brain.Tick(sim, hunter);
        Assert.False(hunter.JustHit);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Fact]
    public void MeleeSelectionDoesNotConsumeMissileRetaliation()
    {
        var (sim, hunter, target) = Room(40);
        hunter.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        hunter.ReactionTime = 10;
        hunter.Brain.SetTargetThingId(sim, target.ThingId);
        hunter.JustHit = true;
        hunter.MeleeRange = Fixed.FromInt(64);
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.True(hunter.JustHit);
    }

    [Fact]
    public void NoAttackSectorDefersRetaliationWithoutConsumingFlag()
    {
        var (sim, hunter, _) = Room(200, noAttack: true);
        hunter.SectorIndex = 0;
        hunter.JustHit = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.True(hunter.JustHit);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Fact]
    public void GenericRetaliationFinishesWhileReactionTimerIsStillNonzero()
    {
        var (sim, hunter, target) = Room(200);
        hunter.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        hunter.ReactionTime = 100;
        hunter.Brain.SetTargetThingId(sim, target.ThingId);
        hunter.JustHit = true;
        var health = target.Health;
        for (var tic = 0; tic < 9; tic++) hunter.Brain.Tick(sim, hunter);
        Assert.Equal(91, hunter.ReactionTime);
        Assert.Equal(MonsterMode.Recovery, hunter.Brain.Mode);
        Assert.True(target.Health < health);
        Assert.False(hunter.JustHit);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(int distance, bool noAttack = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = noAttack }],
            Lines = new List<LevelLine>(),
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = distance, Id = 7 }],
        });
        var hunter = sim.Actors[0]; var target = sim.Actors[1];
        hunter.ReactionTime = 10;
        hunter.Brain!.SetTargetThingId(sim, 7);
        return (sim, hunter, target);
    }
}
