using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTuningActionTests
{
    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    [InlineData(2.5)]
    public void SpeedSettersRetainSignedValuesWithoutClamping(double speed)
    {
        var actor = new Actor(); ActorTuningActions.SetSpeed(actor, speed);
        ActorTuningActions.SetFloatSpeed(actor, speed);
        Assert.Equal(speed, actor.MovementSpeed.ToDouble()); Assert.Equal(speed, actor.FloatSpeed);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void PainThresholdPreservesSignedValue(int value)
    {
        var actor = new Actor(); ActorTuningActions.SetPainThreshold(actor, value);
        Assert.Equal(value, actor.PainThreshold);
    }

    [Fact]
    public void FrameActionTunesSelectedTargetOnly()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7); var previous = caller.MovementSpeed;
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
        {
            ActorTuningActions.SetSpeed(self, 2.5, AcsActorPointer.Target);
            ActorTuningActions.SetFloatSpeed(self, 1.5, AcsActorPointer.Target);
            ActorTuningActions.SetPainThreshold(self, 10, AcsActorPointer.Target);
        })], 0);
        caller.States.Enter(caller, 1);
        Assert.Equal(2.5, target.MovementSpeed.ToDouble()); Assert.Equal(1.5, target.FloatSpeed);
        Assert.Equal(10, target.PainThreshold); Assert.Equal(previous, caller.MovementSpeed);
    }

    [Fact]
    public void NullSelectionAndInvalidInputsPreserveProperties()
    {
        var actor = new Actor();
        ActorTuningActions.SetSpeed(actor, 8, AcsActorPointer.Null);
        ActorTuningActions.SetFloatSpeed(actor, 8, AcsActorPointer.Null);
        ActorTuningActions.SetPainThreshold(actor, 8, AcsActorPointer.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorTuningActions.SetSpeed(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorTuningActions.SetFloatSpeed(actor, double.PositiveInfinity));
        Assert.Equal(4, actor.MovementSpeed.ToDouble()); Assert.Equal(4, actor.FloatSpeed); Assert.Equal(0, actor.PainThreshold);
    }

    [Fact]
    public void PainThresholdActionControlsFlinchAtExactDamageBoundary()
    {
        var actor = new Actor { Health = 100 };
        ActorTuningActions.SetPainThreshold(actor, 10);
        ActorDamage.Apply(actor, 9);
        Assert.Equal(91, actor.Health); Assert.Equal(0, actor.States.Current);
        ActorDamage.Apply(actor, 10);
        Assert.Equal(81, actor.Health); Assert.Equal(1, actor.States.Current);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
