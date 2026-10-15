using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseLostSightRetargetTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HateSearchReplacesHiddenEnemyBeforeMovementAndRestoresSightFlag(bool noSightCheck)
    {
        var (sim, actor, old, visible) = Room(); actor.NoSightCheck = noSightCheck;
        Chase(sim, actor);
        Assert.Equal(visible.Id, actor.TargetId); Assert.NotEqual(old.Id, actor.TargetId);
        Assert.Equal(noSightCheck, actor.NoSightCheck);
        Assert.Equal(Fixed.FromInt(-32), actor.X); Assert.Equal(10, actor.MoveCount);
        Assert.Equal(5, actor.StrafeCount);
    }

    [Fact]
    public void PositiveThresholdDefersReplacement()
    {
        var (sim, actor, old, _) = Room(); actor.SetChaseThreshold(2, setDefault: false);
        Chase(sim, actor); Assert.Equal(old.Id, actor.TargetId); Assert.Equal(1, actor.Brain!.Threshold);
        Assert.Equal(9, actor.MoveCount);
    }

    [Fact]
    public void FailedReplacementRetainsHiddenEnemyAndContinuesMovement()
    {
        var (sim, actor, old, visible) = Room(); visible.Shootable = false;
        Chase(sim, actor);
        Assert.Equal(old.Id, actor.TargetId); Assert.Equal(9, actor.MoveCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyOrSinglePlayerWithoutHateKeepsExistingTarget(bool legacy)
    {
        var (sim, actor, old, _) = Room(); if (!legacy) actor.TidToHate = 0;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            false, false, nativeMovement: !legacy);
        Assert.Equal(old.Id, actor.TargetId);
    }

    [Fact]
    public void SavedSearchReplaysNewTargetAndSearchRandomState()
    {
        var (sim, actor, _, _) = Room(); var saved = SimSavegame.Write(sim, true);
        Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplacementSearchHonorsDontLookAllAround(bool forwardOnly)
    {
        var (sim, actor, old, visible) = Room(); visible.X = Fixed.FromInt(-100);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive
            | (forwardOnly ? MonsterChaseFlags.DontLookAllAround : 0), false, false, nativeMovement: true);
        Assert.Equal(forwardOnly ? old.Id : visible.Id, actor.TargetId);
    }

    [Fact]
    public void CooperativeSearchCanReplaceHiddenEnemyWithVisiblePlayer()
    {
        var (sim, actor, _, _) = Room(SpawnGameMode.Cooperative); actor.TidToHate = 0;
        Chase(sim, actor);
        Assert.Equal(sim.Players.Single(p => p.PlayerNum == 1).Id, actor.TargetId);
        Assert.Equal(10, actor.MoveCount);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, false, false, nativeMovement: true);

    private static (AuthoritySimulation, Actor, Actor, Actor) Room(SpawnGameMode mode = SpawnGameMode.Single)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var lines = geometry.Lines.ToArray();
        lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = lines, Things = mode == SpawnGameMode.Cooperative
                ? [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 2, X = -64, Y = 100 }]
                : [new LevelThing { Type = 1, X = 64 }] }, spawnOptions: new SpawnOptions(Mode: mode));
        sim.Players.Single(p => p.PlayerNum == 0).NoTarget = true;
        var actor = sim.AddBot(-32, 0); var old = sim.AddBot(64, 0); var visible = sim.AddBot(-64, 100);
        old.ThingId = visible.ThingId = 123;
        actor.Brain = new MonsterBrain(MonsterAttack.Melee); actor.SetTargetPointer(old);
        actor.TidToHate = 123; actor.LastLookActorId = old.Id;
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.StrafeCount = 5;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor, old, visible);
    }
}
