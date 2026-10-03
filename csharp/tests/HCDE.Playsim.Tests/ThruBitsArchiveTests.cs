using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruBitsArchiveTests
{
    [Theory]
    [InlineData(0u, false, false)]
    [InlineData(1u, false, false)]
    [InlineData(1u, true, false)]
    [InlineData(0x80000000u, true, false)]
    [InlineData(0u, false, true)]
    [InlineData(1u, false, true)]
    [InlineData(1u, true, true)]
    [InlineData(0x80000000u, true, true)]
    public void SavedMaskAndFlagControlBlastCollision(uint mask, bool enabled, bool serialized)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        mover.Brain = target.Brain = null; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(3);
        mover.ThruBits = target.ThruBits = mask; target.AllowThruBits = enabled;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion32(state); Assert.Equal(32, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        mover.ThruBits = target.ThruBits = ~mask; target.AllowThruBits = !enabled;
        sim.RestoreState(state); Assert.Equal(mask, mover.ThruBits); Assert.Equal(mask, target.ThruBits);
        Assert.Equal(enabled, target.AllowThruBits);
        var passes = enabled && mask != 0;
        Assert.Equal(passes, ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(passes ? 0 : 3, target.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireEnableFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion32(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 12), flags);
        actor.ThruBits = uint.MaxValue; actor.AllowThruBits = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-thrubits-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.Equal(uint.MaxValue, actor.ThruBits);
        Assert.True(actor.AllowThruBits); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion32(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 32 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-thrubits-size" : "save-thrubits-header", error);
    }

    [Fact]
    public void IncompleteZeroTableRejectsRestoreBeforeMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null; sim.AddBot(100, 0).Brain = null;
        var state = sim.CaptureState(); state.Actors[1].ThruBits = null;
        actor.ThruBits = 8; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion32(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.Equal(8u, actor.ThruBits);
    }

    [Fact]
    public void Version31PreservesCurrentMaskAndFlag()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Actors[0].ThruBits = null;
        var bytes = WriteVersion32(state); Assert.Equal(31, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.ThruBits = uint.MaxValue; actor.AllowThruBits = true;
        SimSavegame.Apply(sim, bytes); Assert.Equal(uint.MaxValue, actor.ThruBits); Assert.True(actor.AllowThruBits);
    }

    [Fact]
    public void NestedContactFlagsAndFullMaskRoundTrip()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.ThruBits = uint.MaxValue;
        actor.AllowThruBits = actor.ThruSpecies = actor.MThruSpecies = actor.ThruActors = true;
        var bytes = WriteVersion32(sim);
        actor.ThruBits = 0; actor.AllowThruBits = actor.ThruSpecies = actor.MThruSpecies = actor.ThruActors = false;
        SimSavegame.Apply(sim, bytes); Assert.Equal(uint.MaxValue, actor.ThruBits);
        Assert.True(actor.AllowThruBits); Assert.True(actor.ThruSpecies); Assert.True(actor.MThruSpecies); Assert.True(actor.ThruActors);
    }

    private static byte[] WriteVersion32(AuthoritySimulation sim) => WriteVersion32(sim.CaptureState());
    private static byte[] WriteVersion32(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.GhostFlags = null; pose.NonShootableFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
