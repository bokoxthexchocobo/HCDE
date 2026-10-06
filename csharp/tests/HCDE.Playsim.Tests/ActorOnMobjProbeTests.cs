using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorOnMobjProbeTests
{
    [Theory]
    [InlineData(-4, true)]
    [InlineData(0, false)]
    [InlineData(4, false)]
    public void VerticalPredictionRestoresAllState(int velocity, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var support = sim.AddBot(0, 0);
        actor.Z = Fixed.FromInt(60); actor.VelocityZ = Fixed.FromInt(velocity);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Fact]
    public void FallingProbeSelectsHighestOfMultipleOverlaps()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(0, 0); var upper = sim.AddBot(0, 0);
        upper.Z = Fixed.FromInt(10); actor.Z = Fixed.FromInt(70); actor.VelocityZ = Fixed.FromInt(-20);
        Assert.Same(upper, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(Fixed.FromInt(70), actor.Z);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PredictionClipsFloorAndCeiling(bool ceiling)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var blocker = sim.AddBot(0, 0);
        actor.Z = Fixed.FromInt(ceiling ? 120 : 10);
        actor.VelocityZ = Fixed.FromInt(ceiling ? 100 : -100);
        blocker.Z = Fixed.FromInt(ceiling ? 190 : 0);
        Assert.Same(blocker, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(Fixed.FromInt(ceiling ? 120 : 10), actor.Z);
    }

    [Theory]
    [InlineData(false, 4, false)]
    [InlineData(true, 4, true)]
    [InlineData(false, 0, true)]
    [InlineData(false, -4, true)]
    public void FloatingAdjustmentHonorsInFloatAndSignedSpeed(bool inFloat, double speed, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(50, 0); var support = sim.AddBot(0, 0);
        actor.Floating = true; actor.InFloat = inFloat; actor.FloatSpeed = speed;
        actor.Z = Fixed.FromInt(56); target.Z = Fixed.FromInt(100); actor.Brain!.SetSpecialTarget(target);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(20, false)]
    [InlineData(60, true)]
    public void FlyingPlayerPredictionUsesLevelTicForBob(int tic, bool blocked)
    {
        var sim = Room(); var player = sim.Players.Single(); var support = sim.AddBot(500, 0);
        player.NoGravity = true; player.Z = Fixed.FromInt(56); sim.Thinkers.Clock.Restore(tic);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, player));
        Assert.Equal(Fixed.FromInt(56), player.Z);
    }

    [Fact]
    public void InvalidFloatSpeedFailsWithoutMutatingPosition()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        actor.Floating = true; actor.FloatSpeed = double.NaN; target.Z = Fixed.FromInt(100);
        actor.Brain!.SetSpecialTarget(target);
        Assert.Throws<InvalidOperationException>(() => ActorPhysics.CheckOnMobj(sim, actor));
        Assert.Equal(default, actor.Z);
    }

    [Theory]
    [InlineData(215, false)]
    [InlineData(216, true)]
    public void FloatingDistanceThresholdIsStrict(int distance, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(distance, 0); var support = sim.AddBot(0, 0);
        actor.Floating = true; actor.Z = Fixed.FromInt(56); target.Z = Fixed.FromInt(100);
        actor.Brain!.SetSpecialTarget(target);
        Assert.Equal(blocked ? support : null, ActorPhysics.CheckOnMobj(sim, actor));
    }

    [Fact]
    public void SkullChargeSuppressesFloatingAdjustment()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(50, 0); var support = sim.AddBot(0, 0);
        actor.Floating = true; actor.Z = Fixed.FromInt(56); target.Z = Fixed.FromInt(100);
        actor.Brain!.StartCharge(actor, target); actor.VelocityZ = default;
        Assert.Same(support, ActorPhysics.CheckOnMobj(sim, actor));
        Assert.True(actor.Brain.Charging);
    }

    [Fact]
    public void StateActionCanUseProbeWithoutMovingItsCaller()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(0, 0);
        actor.Z = Fixed.FromInt(60); actor.VelocityZ = Fixed.FromInt(-4);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorPhysics.CheckOnMobj(sim, self) is null ? null : 2), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        Assert.Equal(Fixed.FromInt(60), actor.Z);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
