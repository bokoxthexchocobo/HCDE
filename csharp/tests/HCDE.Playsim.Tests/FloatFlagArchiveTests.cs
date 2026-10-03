using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class FloatFlagArchiveTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FlagsRestoreAndResumeIdenticalMovement(int flags)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Brain = null;
        actor.Z = Fixed.FromInt(100); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        actor.InFloat = (flags & 1) != 0; actor.VerticalFriction = (flags & 2) != 0;
        var bytes = WriteWithoutPainTimer(sim);
        Assert.Equal(flags == 0 ? 18 : 19, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.Tick(); var z = actor.Z; var velocity = actor.VelocityZ; var hash = sim.Checksum;
        actor.InFloat = !actor.InFloat; actor.VerticalFriction = !actor.VerticalFriction;
        sim.RestoreState(state);
        Assert.Equal((flags & 1) != 0, actor.InFloat); Assert.Equal((flags & 2) != 0, actor.VerticalFriction);
        sim.Tick(); Assert.Equal(z, actor.Z); Assert.Equal(velocity, actor.VelocityZ); Assert.Equal(hash, sim.Checksum);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidSerializedBitsAreRejected(int flags)
    {
        var sim = Room(); Assert.Single(sim.Actors).InFloat = true;
        var bytes = WriteWithoutPainTimer(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-float-flags", error); Assert.Empty(state.Actors);
    }
    [Fact]
    public void InvalidInMemoryFlagsFailBeforePoseMutation()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        var state = sim.CaptureState(); state.Actors[0].FloatFlags = 4;
        var before = actor.X;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, actor.X);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }
    private static byte[] WriteWithoutPainTimer(AuthoritySimulation sim)
    {
        var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.PainDeath = null;
        return SimSavegame.Write(state);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 71 }] });
}