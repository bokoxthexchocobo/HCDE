using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruSpeciesArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SavedFlagControlsResumedSpeciesPassage(bool enabled, bool serialized)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0, 3003); var target = sim.AddBot(50, 0, 69);
        mover.Brain = target.Brain = null; mover.ThruSpecies = enabled;
        mover.Blasted = true; mover.VelocityX = Fixed.FromInt(3); var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion31(state);
            Assert.Equal(31, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        mover.ThruSpecies = !enabled; sim.RestoreState(state);
        Assert.Equal(enabled, mover.ThruSpecies);
        Assert.Equal(enabled, ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(enabled ? 0 : 3, target.VelocityX.ToDouble());
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion31(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.ThruSpecies = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-thruspecies-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.ThruSpecies); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].ThruSpeciesFlags = flags;
        actor.ThruSpecies = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion31(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.ThruSpecies);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void InvalidTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion31(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 31 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-thruspecies-size" : "save-thruspecies-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].ThruSpeciesFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion31(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version30PreservesCurrentFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors[0].ThruSpeciesFlags = null; var bytes = WriteVersion31(state);
        Assert.Equal(30, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.ThruSpecies = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.ThruSpecies);
    }

    [Fact]
    public void RestoredTargetOnlyFlagStillDoesNotGrantPassage()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        target.ThruSpecies = true; var bytes = WriteVersion31(sim);
        mover.ThruSpecies = true; target.ThruSpecies = false; SimSavegame.Apply(sim, bytes);
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RestoredFlagDeterminesBridgeSupport(bool enabled, bool serialized)
    {
        var sim = Room(); var carrier = sim.AddBot(0, 0); var rider = sim.AddBot(0, 0);
        carrier.Brain = rider.Brain = null; carrier.Height = Fixed.FromInt(16); carrier.ActsLikeBridge = true;
        rider.Z = carrier.Height; rider.OnGround = true; rider.ThruSpecies = enabled;
        var state = sim.CaptureState();
        if (serialized) Assert.True(SimSavegame.TryRead(WriteVersion31(state), out state, out var error), error);
        rider.ThruSpecies = !enabled; sim.RestoreState(state);
        ActorPhysics.FitToSector(sim, rider); Assert.Equal(enabled ? 0 : 16, rider.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredFlagDeterminesOccupancy(bool enabled)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); sim.AddBot(0, 0);
        mover.ThruSpecies = enabled; var bytes = WriteVersion31(sim);
        mover.ThruSpecies = !enabled; SimSavegame.Apply(sim, bytes);
        Assert.Equal(enabled, ActorPhysics.CanOccupy(sim, mover));
    }

    [Fact]
    public void NestedContactFlagsRoundTripTogether()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.MThruSpecies = actor.ThruSpecies = actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = true;
        var bytes = WriteVersion31(sim);
        actor.MThruSpecies = actor.ThruSpecies = actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.MThruSpecies); Assert.True(actor.ThruSpecies); Assert.True(actor.ThruActors); Assert.True(actor.Boss);
        Assert.True(actor.DontBlast); Assert.True(actor.Blasted);
    }

    private static byte[] WriteVersion31(AuthoritySimulation sim) => WriteVersion31(sim.CaptureState());
    private static byte[] WriteVersion31(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.ThruBits = null; pose.GhostFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
