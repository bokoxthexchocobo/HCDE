using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileSightTests
{
    [Theory]
    [InlineData(true, 80, 0, null, true)]
    [InlineData(true, 226, 0, null, true)]
    [InlineData(true, 80, 1, null, true)]
    [InlineData(true, 80, 2, null, false)]
    [InlineData(false, 80, 0, null, false)]
    [InlineData(true, 81, 0, null, false)]
    [InlineData(true, 80, 42, 42, true)]
    [InlineData(true, 80, 1, 42, false)]
    public void MissileSightOnlyPassesCurrentMapImpactScriptLines(bool impact, int special, int map, int? levelNumber, bool sees)
    {
        var (sim, actor) = Room(impact, special, map, levelNumber);
        var target = sim.Players.Single(); actor.JustHit = true;
        Assert.False(CombatTrace.HasLineOfSight(sim, actor, target));
        Assert.Equal(sees, CombatTrace.HasMissileLineOfSight(sim, actor, target));
        Assert.Equal(sees, actor.Brain!.CheckMissileRange(sim, actor));
        Assert.Equal(!sees, actor.JustHit);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void SightBlockingAndClosedOpeningsRemainOpaque(bool blocksSight, bool closed)
    {
        var (sim, actor) = Room(true, 80, 0, null, blocksSight);
        if (closed) sim.Ceilings[1] = 10;
        Assert.False(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
    }

    [Fact]
    public void NativeChaseCanSelectMissileThroughQualifyingLine()
    {
        var (sim, actor) = Room(true, 80, 0); actor.JustHit = true;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            useMelee: false, nativeMovement: true);
        Assert.Equal(8, actor.Brain.WindupTics);
    }

    [Fact]
    public void RestoringLineSpecialRestoresMissileSight()
    {
        var (sim, actor) = Room(true, 80, 0); var saved = SimSavegame.Write(sim, true);
        sim.Level.Lines[6].Special = 0;
        Assert.False(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
        SimSavegame.Apply(sim, saved);
        Assert.True(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
    }

    private static (AuthoritySimulation, Actor) Room(bool impact, int special, int map, int? levelNumber = null, bool blocksSight = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level; var lines = geometry.Lines.ToArray();
        lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
            Flags = LevelLine.TwoSidedFlag | LevelLine.BlockEverythingFlag | (blocksSight ? LevelLine.BlockSightFlag : 0),
            Impact = impact, Special = special, Arg1 = map };
        var sim = AuthoritySimulation.Start(new PlayLevel { MapName = "MAP01", LevelNumber = levelNumber,
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1; actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
