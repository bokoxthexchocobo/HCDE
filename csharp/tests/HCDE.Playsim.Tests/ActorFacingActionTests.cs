using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFacingActionTests
{
    [Theory]
    [InlineData(0, 0, 90)]
    [InlineData(20, 5, 25)]
    [InlineData(0, 5, 95)]
    public void TargetFacingLimitsYawAndPreservesDefaultPitch(double limit, double offset, double expected)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(0, 100);
        actor.Brain!.SetSpecialTarget(target); actor.PitchDegrees = 17; actor.Ambush = true;
        ActorOrientationActions.FaceTarget(actor, maxTurn: limit, angleOffset: offset);
        Assert.Equal(expected, actor.Angle.ToDegrees(), 5); Assert.Equal(17, actor.PitchDegrees); Assert.False(actor.Ambush);
    }

    [Fact]
    public void MasterFacingAndPitchRoundTrip()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var master = sim.AddBot(100, 0); actor.MasterId = master.Id;
        master.Z = Fixed.FromInt(100);
        ActorOrientationActions.FaceMaster(actor, maxPitch: 0);
        Assert.Equal(-45, actor.PitchDegrees, 5);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.PitchDegrees = 0; sim.RestoreState(state);
        Assert.Equal(-45, actor.PitchDegrees, 5); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void TracerFacingAndMissingPointer()
    {
        var sim = Room(); var target = sim.AddBot(0, 100);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        missile.X = missile.Y = default; missile.RestoreTracerTarget(target.Id);
        ActorOrientationActions.FaceTracer(missile);
        Assert.Equal(90, missile.Angle.ToDegrees(), 5);
        missile.RestoreTracerTarget(null); missile.Ambush = true;
        ActorOrientationActions.FaceTracer(missile); Assert.True(missile.Ambush);
    }

    [Fact]
    public void PitchLimitAndOffsetApplyAfterAiming()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0); target.Z = Fixed.FromInt(100);
        ActorOrientationActions.Face(actor, target, maxPitch: 10, pitchOffset: 2);
        Assert.Equal(-8, actor.PitchDegrees, 5);
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(1, 0)]
    [InlineData(2, 50)]
    [InlineData(3, 50)]
    [InlineData(4, 100)]
    [InlineData(5, 100)]
    [InlineData(6, 100)]
    [InlineData(7, 100)]
    public void BodyFlagsUseNativePrecedenceBeforeZOffset(int flags, double targetHeight)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Height = Fixed.FromInt(64); target.Height = Fixed.FromInt(100);
        actor.MasterId = target.Id;
        ActorOrientationActions.FaceMaster(actor, maxPitch: 0, flags: flags, zOffset: 10, pitchOffset: 3);
        var expected = -Math.Atan2(targetHeight + 10 - 32, 100) * 180 / Math.PI + 3;
        Assert.InRange(Math.Abs(expected - actor.PitchDegrees), 0, 1.0 / 65536);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.PitchDegrees = 0; sim.RestoreState(state);
        Assert.InRange(Math.Abs(expected - actor.PitchDegrees), 0, 1.0 / 65536); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ShortBodiesUseCenterAndPitchDisabledIgnoresBodyFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Height = Fixed.FromInt(16); target.Height = Fixed.FromInt(24);
        ActorOrientationActions.Face(actor, target, maxPitch: 0);
        Assert.InRange(Math.Abs(-Math.Atan2(4, 100) * 180 / Math.PI - actor.PitchDegrees), 0, 1.0 / 65536);
        actor.PitchDegrees = 17;
        ActorOrientationActions.Face(actor, target, flags: 7, zOffset: 100);
        Assert.Equal(17, actor.PitchDegrees);
    }

    [Fact]
    public void UnknownBodyFlagFailsBeforeMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(0, 100);
        actor.Ambush = true; actor.PitchDegrees = 17;
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.Face(actor, target, flags: 16));
        Assert.True(actor.Ambush); Assert.Equal(0, actor.Angle.ToDegrees()); Assert.Equal(17, actor.PitchDegrees);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
