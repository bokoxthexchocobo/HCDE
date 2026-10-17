using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloatBobTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(16, 8)]
    [InlineData(48, -8)]
    [InlineData(80, 8)]
    public void PhaseUsesNativeSixtyFourTicCycle(int phase, double expected)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.FloatBob = true; actor.FloatBobPhase = phase;
        Assert.Equal(expected, actor.GetBobOffset(), 10);
        actor.FloatBob = false; Assert.Equal(0, actor.GetBobOffset());
    }

    [Fact]
    public void RateStrengthAndFractionUseNativeOrder()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.Thinkers.Clock.Restore(5);
        actor.FloatBob = true; actor.FloatBobPhase = 2; actor.FloatBobFactor = -0.5; actor.FloatBobStrength = 2;
        Assert.Equal(Math.Sin((2 - 2.5 + 0.75) * Math.PI / 32) * 16, actor.GetBobOffset(0.75), 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.FloatBobFactor = double.NaN);
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.FloatBobStrength = double.PositiveInfinity);
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.GetBobOffset(double.NaN));
    }

    [Theory]
    [InlineData(0, 40)]
    [InlineData(1, 8)]
    [InlineData(2, 58)]
    [InlineData(4, 108)]
    [InlineData(7, 108)]
    public void FacingIncludesBothBobOffsetsAndBodyFlagPrecedence(int flags, double targetZ)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Height = Fixed.FromInt(100); target.Height = Fixed.FromInt(100);
        actor.FloatBob = target.FloatBob = true; actor.FloatBobPhase = 48; target.FloatBobPhase = 16;
        ActorOrientationActions.Face(actor, target, maxPitch: 0, flags: flags);
        var expected = -Math.Atan2(targetZ - 24, 100) * 180 / Math.PI;
        Assert.InRange(Math.Abs(expected - actor.PitchDegrees), 0, 1.0 / 65536);
    }

    [Fact]
    public void DefaultAimHeightFallsBackToUnbobbedCenterForShortActors()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Height = Fixed.FromInt(16); target.Height = Fixed.FromInt(24);
        actor.FloatBob = target.FloatBob = true; actor.FloatBobPhase = target.FloatBobPhase = 16;
        ActorOrientationActions.Face(actor, target, maxPitch: 0);
        Assert.InRange(Math.Abs(-Math.Atan2(4, 100) * 180 / Math.PI - actor.PitchDegrees), 0, 1.0 / 65536);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SettingsRoundTripIncludingDisabledTuningAndOlderDefaults(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var old = SimSavegame.Write(sim, true);
        actor.FloatBob = enabled; actor.FloatBobFactor = -2; actor.FloatBobStrength = 0.75;
        ActorPropertyActions.SetFloatBobPhase(actor, 80); sim.Thinkers.Clock.Restore(9);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var offset = actor.GetBobOffset();
        var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(326, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.FloatBob = !enabled; actor.FloatBobFactor = 1; actor.FloatBobStrength = 1;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(enabled, actor.FloatBob); Assert.Equal(-2, actor.FloatBobFactor); Assert.Equal(0.75, actor.FloatBobStrength);
        Assert.Equal(offset, actor.GetBobOffset()); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old);
        Assert.False(actor.FloatBob); Assert.Equal(1, actor.FloatBobFactor); Assert.Equal(1, actor.FloatBobStrength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EveryBobSettingAffectsChecksum(int setting)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var baseline = sim.Checksum;
        if (setting == 0) actor.FloatBob = true;
        if (setting == 1) actor.FloatBobStrength = 2;
        if (setting == 2) actor.FloatBobFactor = 2;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(12)]
    public void CorruptArchiveValuesAreRejected(int offset)
    {
        var sim = Room(); sim.Players.Single().FloatBob = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var item = bytes.AsSpan(bytes.Length - size + 8);
        if (offset == 0) BinaryPrimitives.WriteInt32LittleEndian(item, 2);
        else BinaryPrimitives.WriteDoubleLittleEndian(item[offset..], double.NaN);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-floatbob-value", error);
    }

    [Fact]
    public void AcsCanEnableAndDisableFloatBob()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "FLOATBOB", true));
        Assert.True(AcsActorFlags.TryGet(actor, "floatbob", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "FLOATBOB", false)); Assert.False(actor.FloatBob);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
