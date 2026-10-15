using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileSightApertureTests
{
    [Theory]
    [InlineData(20, 25, true)]
    [InlineData(30, 35, true)]
    [InlineData(0, 10, false)]
    [InlineData(100, 110, false)]
    [InlineData(20, 20, false)]
    public void VisibilityUsesTargetHeightAndObserverEye(int floor, int ceiling, bool visible)
    {
        var (sim, actor) = Room(); sim.Floors[1] = floor; sim.Ceilings[1] = ceiling;
        Assert.Equal(visible, CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
        Assert.False(CombatTrace.HasLineOfSight(sim, actor, sim.Players.Single()));
    }

    [Fact]
    public void ChangingObserverHeightChangesVisibleAperture()
    {
        var (sim, actor) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        actor.Height = Fixed.FromInt(56);
        Assert.True(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
        actor.Height = Fixed.FromInt(128);
        Assert.False(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
    }

    [Fact]
    public void CallableCanRetaliateWhenOnlyBottomOfTargetIsVisible()
    {
        var (sim, actor) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25; actor.JustHit = true;
        Assert.True(actor.Brain!.CheckMissileRange(sim, actor)); Assert.False(actor.JustHit);
    }

    [Fact]
    public void SightQueryDoesNotMutateSavedSimulation()
    {
        var (sim, actor) = Room(); var before = SimSavegame.Write(sim, true);
        Assert.True(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void NativeChaseRejectsCenterRayWhenTallerObserversEyeCannotSeeTarget()
    {
        var (sim, actor) = Room(); actor.Height = Fixed.FromInt(128);
        sim.Floors[1] = 45; sim.Ceilings[1] = 47;
        Assert.True(CombatTrace.HasLineOfSight(sim, actor, sim.Players.Single()));
        Assert.False(CombatTrace.HasMissileLineOfSight(sim, actor, sim.Players.Single()));
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.JustHit = true;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive,
            useMelee: false, nativeMovement: true);
        Assert.Equal(0, actor.Brain.WindupTics); Assert.True(actor.JustHit);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var actor = sim.AddBot(-64, 0); actor.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        return (sim, actor);
    }
}
