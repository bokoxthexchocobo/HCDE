using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloatingTargetParityTests
{
    [Theory]
    [InlineData(4, 100, 60)]
    [InlineData(0, 100, 56)]
    [InlineData(-4, 100, 52)]
    [InlineData(4, 0, 52)]
    [InlineData(-4, 0, 60)]
    public void RuntimeUsesSignedFloatSpeed(double speed, int targetZ, int expected)
    {
        var (sim, actor, target) = Setup(); target.Z = Fixed.FromInt(targetZ);
        ActorTuningActions.SetFloatSpeed(actor, speed);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.Equal(default, actor.VelocityZ);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(1, 60)]
    [InlineData(2, 60)]
    [InlineData(3, 56)]
    public void AssignedTargetNeedNotBeShootableOrAliveButMustExist(int mode, int expected)
    {
        var (sim, actor, target) = Setup();
        target.Shootable = mode != 0; target.Health = mode == 1 ? 0 : 20;
        target.Solid = mode != 2; if (mode == 3) target.Destroy();
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false, 60)]
    [InlineData(true, 56)]
    public void DormancySuppressesRuntimeAdjustment(bool dormant, int expected)
    {
        var (sim, actor, _) = Setup(); actor.Dormant = dormant;
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Fact]
    public void SignedSpeedAndTargetSurviveSaveBeforeMovement()
    {
        var (sim, actor, _) = Setup(); ActorTuningActions.SetFloatSpeed(actor, -4);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, _) = Setup(); restored.RestoreState(state);
        ActorPhysics.Step(restored, loaded);
        Assert.Equal(52, loaded.Z.ToDouble()); Assert.Equal(-4, loaded.FloatSpeed);
    }

    [Theory]
    [InlineData(0, false, false, 60)]
    [InlineData(-1, false, false, 60)]
    [InlineData(0, true, false, 56)]
    [InlineData(0, false, true, 56)]
    public void DeadFloaterUsesFlagsRatherThanHealthGate(int health, bool dormant, bool inFloat, int expected)
    {
        var (sim, actor, _) = Setup(); actor.Health = health; actor.Dormant = dormant; actor.InFloat = inFloat;
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.Z.ToDouble());
    }

    [Fact]
    public void DeadFloaterWithoutTargetDoesNotAdjust()
    {
        var (sim, actor, _) = Setup(); actor.Health = 0; actor.Brain!.SetSpecialTarget(null);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(56, actor.Z.ToDouble());
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var actor = sim.AddBot(0, 0, 3005); var target = sim.AddBot(50, 0);
        actor.Z = Fixed.FromInt(56); target.Z = Fixed.FromInt(100);
        actor.Brain!.SetSpecialTarget(target); actor.Brain.Enabled = false;
        return (sim, actor, target);
    }
}
