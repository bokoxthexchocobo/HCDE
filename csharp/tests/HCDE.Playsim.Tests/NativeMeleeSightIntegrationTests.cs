using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeMeleeSightIntegrationTests
{
    [Fact]
    public void MeleeActionsSeePartialTargetAndReturnMatchingJumpDestinations()
    {
        var (sim, actor) = Room(); sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        Assert.False(CombatTrace.HasLineOfSight(sim, actor, sim.Players.Single()));
        Assert.True(ActorJumpActions.CheckMeleeRange(actor));
        Assert.Equal(7, ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 7));
        Assert.Null(ActorJumpActions.JumpIfTargetOutsideMeleeRange(actor, 7));
    }

    [Fact]
    public void InvisibleMeleeChecksUseNativeSightProbability()
    {
        var (sim, actor) = Room(); var (reference, _) = Room(); sim.Players.Single().Invisible = true;
        for (var i = 0; i < 256; i++)
        {
            Assert.Equal(reference.NextCheckSightByte() <= 50, ActorJumpActions.CheckMeleeRange(actor));
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EarlierMeleeGatesRejectBeforeSightDraw(int gate)
    {
        var (sim, actor) = Room(noAttack: gate == 2); var target = sim.Players.Single(); target.Invisible = true;
        if (gate == 0) actor.MeleeRange = Fixed.FromInt(32);
        if (gate == 1) target.Z = Fixed.FromInt(200);
        if (gate == 3) actor.Friendly = true;
        Assert.False(ActorJumpActions.CheckMeleeRange(actor));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void NearbyGoalBypassesVerticalSectorAndSightGates()
    {
        var (sim, actor) = Room(noAttack: true); var target = sim.Players.Single(); actor.GoalId = target.Id;
        target.Z = Fixed.FromInt(200); target.MVisBlocked = target.Invisible = true;
        Assert.True(ActorJumpActions.CheckMeleeRange(actor));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void StrictDistanceEqualityRejectsBeforeSightDraw()
    {
        var (sim, actor) = Room(); actor.MeleeRange = Fixed.FromInt(112);
        sim.Players.Single().Invisible = true;
        Assert.False(ActorJumpActions.CheckMeleeRange(actor));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    internal static (AuthoritySimulation, Actor) Room(bool noAttack = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = noAttack
                ? [new LevelSector { CeilingHeight = 128, NoAttack = true }, new LevelSector { CeilingHeight = 128 }]
                : geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var actor = sim.AddBot(-64, 0, 3001); actor.SetTargetPointer(sim.Players.Single());
        actor.MeleeRange = Fixed.FromInt(256);
        return (sim, actor);
    }
}
