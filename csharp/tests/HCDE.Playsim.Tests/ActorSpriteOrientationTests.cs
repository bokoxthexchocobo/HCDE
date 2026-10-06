using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpriteOrientationTests
{
    [Theory]
    [InlineData(-90, 450)]
    [InlineData(1.23456789, -0.125)]
    [InlineData(0, 0)]
    public void AssignmentAndSavePreserveNativeDegreeValues(double angle, double rotation)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Angle = BamAngle.FromDegrees(90);
        Assert.True(ActorOrientationActions.SetSpriteAngle(actor, angle));
        Assert.True(ActorOrientationActions.SetSpriteRotation(actor, rotation));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.SpriteAngle = actor.SpriteRotation = 42; sim.RestoreState(state);
        Assert.Equal(angle, actor.SpriteAngle); Assert.Equal(rotation, actor.SpriteRotation);
        Assert.Equal(90, actor.Angle.ToDegrees()); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameActionSelectsTargetAndNullReturnsFalse()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
        {
            Assert.True(ActorOrientationActions.SetSpriteAngle(self, 45, AcsActorPointer.Target));
            Assert.True(ActorOrientationActions.SetSpriteRotation(self, -90, AcsActorPointer.Target));
        })], 0);
        caller.States.Enter(caller, 1);
        Assert.False(ActorOrientationActions.SetSpriteAngle(target, 99, AcsActorPointer.Null));
        Assert.False(ActorOrientationActions.SetSpriteRotation(target, 99, AcsActorPointer.Null));
        Assert.Equal(45, target.SpriteAngle); Assert.Equal(-90, target.SpriteRotation);
        Assert.Equal(0, caller.SpriteAngle); Assert.Equal(0, caller.SpriteRotation);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DirectValuesPersistAndContributeToChecksum(bool angle)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var other = Room(); other.AddBot(0, 0).Brain = null;
        if (angle) actor.SpriteAngle = 30; else actor.SpriteRotation = 30;
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.SpriteAngle = actor.SpriteRotation = 0; sim.RestoreState(state);
        Assert.Equal(angle ? 30 : 0, actor.SpriteAngle); Assert.Equal(angle ? 0 : 30, actor.SpriteRotation);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void InvalidInputAndMemoryFailBeforeMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.SetSpriteAngle(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.SetSpriteRotation(actor, double.PositiveInfinity));
        Assert.Equal(0, actor.SpriteAngle); Assert.Equal(0, actor.SpriteRotation);
        var state = sim.CaptureState(); state.Actors.Single().SpriteOrientation = new(double.NaN, 0); actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    [Theory]
    [InlineData(0, 78)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var bytes = Saved(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(Start(bytes) + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-spriteorientation-", error);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(20)]
    public void NonfiniteWireValueIsRejected(int offset)
    {
        var bytes = Saved(); BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(Start(bytes) + offset), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-spriteorientation-value", error);
    }

    [Fact]
    public void LegacyAbsenceRestoresDefaultValues()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.SpriteAngle = 90; actor.SpriteRotation = 180; sim.RestoreState(state);
        Assert.Equal(0, actor.SpriteAngle); Assert.Equal(0, actor.SpriteRotation);
    }

    private static int Start(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static byte[] Saved()
    { var sim = Room(); ActorOrientationActions.SetSpriteAngle(sim.AddBot(0, 0), 90); return SimSavegame.Write(sim); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
