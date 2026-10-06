using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTuningSaveTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3006)]
    public void AbsentTuningRestoresRecordedClassDefaults(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }],
        });
        var actor = Assert.Single(sim.Actors);
        var speed = actor.MovementSpeed;
        var bytes = SimSavegame.Write(sim);
        Assert.Null(sim.CaptureState().Actors.Single().Tuning);
        ActorTuningActions.SetSpeed(actor, -2);
        ActorTuningActions.SetFloatSpeed(actor, -0.125);
        ActorTuningActions.SetPainThreshold(actor, int.MaxValue);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(speed, actor.MovementSpeed);
        Assert.Equal(4, actor.FloatSpeed);
        Assert.Equal(0, actor.PainThreshold);
        Assert.False(actor.HasTuningOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DirectTuningChangesRoundTripWithoutActionMarker()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.MovementSpeed = Fixed.FromDouble(-2.5);
        actor.FloatSpeed = -0.125; actor.PainThreshold = int.MinValue;
        Assert.False(actor.HasTuningOverride);
        var bytes = SimSavegame.Write(sim);
        actor.MovementSpeed = default; actor.FloatSpeed = 99; actor.PainThreshold = 99;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(-2.5, actor.MovementSpeed.ToDouble());
        Assert.Equal(-0.125, actor.FloatSpeed); Assert.Equal(int.MinValue, actor.PainThreshold);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(4, 4, 0)]
    [InlineData(-2, -0.125, int.MinValue)]
    [InlineData(0, 0, int.MaxValue)]
    [InlineData(2.5, 1.23456789, 10)]
    public void ActionsRoundTripAllFieldsAndExplicitDefaults(double speed, double floating, int pain)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorTuningActions.SetSpeed(actor, speed); ActorTuningActions.SetFloatSpeed(actor, floating);
        ActorTuningActions.SetPainThreshold(actor, pain);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.MovementSpeed = default; actor.FloatSpeed = 5; actor.PainThreshold = 7;
        sim.RestoreState(state);
        Assert.Equal(speed, actor.MovementSpeed.ToDouble()); Assert.Equal(floating, actor.FloatSpeed);
        Assert.Equal(pain, actor.PainThreshold); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void AcsSpeedResetRetainsRawValue()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.Speed, 262144);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.MovementSpeed = default; sim.RestoreState(state);
        Assert.Equal(262144, actor.MovementSpeed.Raw); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 73)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void MalformedTrailerIsRejected(int offset, int value)
    {
        var bytes = Saved(); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-tuning-", error);
    }

    [Fact]
    public void NonfiniteWireValueIsRejected()
    {
        var bytes = Saved(); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(bytes.Length - size + 16), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-tuning-value", error);
    }

    [Fact]
    public void NonfiniteMemoryRejectsRestoreBeforeHealthMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors.Single().Tuning = new(0, double.NaN, 0); actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    private static byte[] Saved()
    { var sim = Room(); ActorTuningActions.SetSpeed(sim.AddBot(0, 0), 2); return SimSavegame.Write(sim); }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
