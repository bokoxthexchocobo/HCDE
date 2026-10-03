using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class BlastedArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RestoredMotionUsesSavedBlastedDropoffPolicy(bool blasted, bool serialized)
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 128); var actor = sim.AddBot(-1, 80);
        actor.Brain = null; actor.AllowDropOff = false; actor.NoDropOff = true;
        actor.Blasted = blasted; actor.VelocityX = Fixed.FromInt(4);
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = LegacyActorArchiveFixture.Write(state); Assert.Equal(blasted ? 27 : 26, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        actor.Blasted = !blasted; actor.VelocityX = default; sim.RestoreState(state); sim.Tick();
        Assert.Equal(blasted ? 1 : 0, actor.SectorIndex);
        if (blasted) Assert.True(actor.Blasted);
    }

    [Fact]
    public void RestoredStationaryBlastedActorClearsFlagOnNextTic()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); var actor = sim.AddBot(-64, 80);
        actor.Brain = null; actor.Blasted = true;
        Assert.True(SimSavegame.TryRead(LegacyActorArchiveFixture.Write(sim), out var state, out var error), error);
        actor.Blasted = false; sim.RestoreState(state); Assert.True(actor.Blasted);
        sim.Tick(); Assert.False(actor.Blasted);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidSerializedFlagIsRejected(int flags)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); sim.AddBot(-64, 80).Blasted = true;
        var bytes = LegacyActorArchiveFixture.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-blasted-flags", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void InvalidMemoryFlagRejectsRestoreBeforeMutation()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); var actor = sim.AddBot(-64, 80); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].BlastedFlags = 2;
        actor.VelocityX = Fixed.FromInt(1); actor.Blasted = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Blasted);
    }

    [Fact]
    public void IncompleteTableCannotBeWritten()
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128); sim.AddBot(-64, 80).Blasted = true;
        var state = sim.CaptureState(); state.Actors[0].BlastedFlags = null;
        Assert.Throws<InvalidOperationException>(() => LegacyActorArchiveFixture.Write(state));
    }
}
