using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloatingPlaneOrderTests
{
    [Theory]
    [InlineData(2, -4, 4, 2, -4)]
    [InlineData(2, -4, 0, 0, 0)]
    [InlineData(2, -4, -4, 0, 0)]
    public void FloatingAdjustmentPrecedesFloorClip(int z, int velocity, double speed, int expectedZ, int expectedVelocity)
    {
        var (sim, actor, _) = Setup(false);
        actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorTuningActions.SetFloatSpeed(actor, speed);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expectedZ, actor.Z.ToDouble()); Assert.Equal(expectedVelocity, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 0)]
    [InlineData(10, 1, 7)]
    public void FlyingFrictionUsesHeightAfterFloating(int z, int velocity, int expectedZ)
    {
        var (sim, actor, target) = Setup(true); target.Height = default;
        actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expectedZ, actor.Z.ToDouble());
        Assert.Equal(expectedZ > 0 ? Fixed.FromDouble(ActorPhysics.FlyingFriction) : Fixed.FromInt(1), actor.VelocityZ);
    }

    [Fact]
    public void SaveRestoredCrossingContinuesIdentically()
    {
        var (sim, actor, _) = Setup(false); actor.Z = Fixed.FromInt(2); actor.VelocityZ = Fixed.FromInt(-4);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, _) = Setup(false); restored.RestoreState(state);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(restored, loaded);
        Assert.Equal(Fixed.FromInt(2), actor.Z); Assert.Equal(actor.Z, loaded.Z);
        Assert.Equal(actor.VelocityZ, loaded.VelocityZ);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Target) Setup(bool player)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1, X = 100 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
        Actor actor = player ? sim.Players.Single() : sim.AddBot(0, 0, 3005);
        var target = sim.AddBot(actor.X.ToDouble(), 0); target.Solid = false;
        actor.Brain ??= new MonsterBrain(MonsterAttack.Melee);
        actor.Brain.SetSpecialTarget(target); actor.Brain.Enabled = false;
        actor.Floating = true; actor.NoGravity = true;
        return (sim, actor, target);
    }
}
