using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeAcquisitionSightTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AutomaticNativePolicyAcquiresPartiallyVisiblePlayer(bool native)
    {
        var (sim, actor) = Room(native); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            false, false, nativeMovement: native);
        Assert.Equal(native ? sim.Players.Single().Id : (uint?)null, actor.TargetId);
    }

    [Fact]
    public void NativePolicyAppliesVisibilityProbabilityOnlyAfterViewAndRangeGates()
    {
        var (sim, actor) = Room(true); var target = sim.Players.Single();
        target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        Assert.False(ActorVisibility.IsVisible(sim, actor, target, true, new ActorLookParameters(MaxDistance: 127)));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
        var (reference, _) = Room(true);
        for (var i = 0; i < 256; i++)
        {
            Assert.Equal(reference.NextCheckSightByte() <= 50, ActorVisibility.IsVisible(sim, actor, target, true));
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Fact]
    public void NativeLostSightGateRetainsPartiallyVisibleEnemyBeforeSearch()
    {
        var (sim, actor) = Room(false); var enemy = sim.Players.Single();
        enemy.ThingId = actor.TidToHate = 123;
        var replacement = sim.AddBot(-64, 100); replacement.ThingId = 123;
        actor.SetTargetPointer(enemy); actor.LastLookActorId = enemy.Id; actor.MoveCount = 10;
        sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            false, false, nativeMovement: true);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(9, actor.MoveCount);
        Assert.Equal(enemy.Id, actor.LastLookActorId);
    }

    [Fact]
    public void PositiveThresholdDoesNotAddLostSightRandomDraw()
    {
        var (sim, actor) = Room(false); var enemy = sim.Players.Single();
        actor.SetTargetPointer(enemy); actor.TidToHate = 123; actor.SetChaseThreshold(2, false);
        enemy.RenderStyle = ActorRenderStyle.Translucent; enemy.Alpha = 0;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            false, false, nativeMovement: true);
        Assert.Null(sim.CaptureState().CheckSightRandomState); Assert.Equal(enemy.Id, actor.TargetId);
    }

    [Fact]
    public void SavedNativeAcquisitionReplaysRandomDecision()
    {
        var (sim, actor) = Room(true); var target = sim.Players.Single();
        target.RenderStyle = ActorRenderStyle.Translucent; target.Alpha = 0;
        var saved = SimSavegame.Write(sim, true);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, false, false, nativeMovement: true);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        actor.Brain.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, false, false, nativeMovement: true);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation, Actor) Room(bool native)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = 64 }] },
            spawnOptions: new SpawnOptions(NativeMonsterMovement: native));
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
