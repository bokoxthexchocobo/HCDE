using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorOrientationActionTests
{
    [Theory]
    [InlineData(-90, 270)]
    [InlineData(450, 90)]
    public void AngleAndRollAreAbsoluteAndWrap(double value, double expected)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(20) };
        ActorOrientationActions.SetAngle(actor, value); ActorOrientationActions.SetRoll(actor, value);
        Assert.Equal(expected, actor.Angle.ToDegrees()); Assert.Equal(expected, actor.Roll.ToDegrees());
    }

    [Theory]
    [InlineData(0, 120)]
    [InlineData(1, 89)]
    [InlineData(6, 120)]
    public void MonsterPitchOnlyClampsWhenRequested(int flags, double expected)
    {
        var actor = new Actor(); ActorOrientationActions.SetPitch(actor, 120, flags);
        Assert.Equal(expected, actor.PitchDegrees);
    }

    [Fact]
    public void PlayerPitchClampsAndInterpolationFailsBeforeMutation()
    {
        var player = Room().Players.Single();
        ActorOrientationActions.SetPitch(player, -120); Assert.Equal(-89, player.PitchDegrees);
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.SetPitch(player, 20, 2));
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.SetAngle(player, 20, 4));
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.SetRoll(player, 20, 2));
        Assert.Equal(-89, player.PitchDegrees); Assert.Equal(0u, player.Angle.Raw); Assert.Equal(0u, player.Roll.Raw);
    }

    [Fact]
    public void FrameSetsTargetOrientationAndSaveRestoresAllFields()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
        {
            ActorOrientationActions.SetAngle(self, 90, pointerSelector: AcsActorPointer.Target);
            ActorOrientationActions.SetPitch(self, 30, pointerSelector: AcsActorPointer.Target);
            ActorOrientationActions.SetRoll(self, 180, pointerSelector: AcsActorPointer.Target);
        })], 0);
        caller.States.Enter(caller, 1);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        target.Angle = default; target.PitchDegrees = 0; target.Roll = default; sim.RestoreState(state);
        Assert.Equal(90, target.Angle.ToDegrees()); Assert.Equal(30, target.PitchDegrees);
        Assert.Equal(180, target.Roll.ToDegrees()); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void NullSelectionIsNoopAndInvalidInputsFailBeforeMutation()
    {
        var actor = new Actor(); ActorOrientationActions.SetAngle(actor, 90, pointerSelector: AcsActorPointer.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.SetAngle(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.SetPitch(actor, double.PositiveInfinity));
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.SetRoll(actor, 90, 8));
        Assert.Equal(0u, actor.Angle.Raw); Assert.Equal(0, actor.PitchDegrees); Assert.Equal(0u, actor.Roll.Raw);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
