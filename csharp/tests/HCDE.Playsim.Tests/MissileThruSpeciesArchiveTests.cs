using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileThruSpeciesArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SavedFlagControlsResumedSpeciesPassage(bool enabled, bool serialized)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3003); var target = sim.AddBot(30, 0, 69);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.MThruSpecies = enabled; var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion30(state);
            Assert.Equal(30, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        missile.MThruSpecies = !enabled; sim.RestoreState(state);
        Assert.Equal(enabled, missile.MThruSpecies); sim.Tick(); Assert.Equal(!enabled, missile.Destroyed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion30(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.MThruSpecies = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-missile-thruspecies-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.MThruSpecies); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].MissileThruSpeciesFlags = flags;
        actor.MThruSpecies = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion30(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.MThruSpecies);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void InvalidTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion30(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 30 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-missile-thruspecies-size" : "save-missile-thruspecies-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].MissileThruSpeciesFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion30(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version29PreservesCurrentFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors[0].MissileThruSpeciesFlags = null; var bytes = WriteVersion30(state);
        Assert.Equal(29, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.MThruSpecies = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.MThruSpecies);
    }

    [Fact]
    public void NestedContactFlagsRoundTripTogether()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.MThruSpecies = actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = true;
        var bytes = WriteVersion30(sim);
        actor.MThruSpecies = actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.MThruSpecies); Assert.True(actor.ThruActors); Assert.True(actor.Boss);
        Assert.True(actor.DontBlast); Assert.True(actor.Blasted);
    }

    private static byte[] WriteVersion30(AuthoritySimulation sim) => WriteVersion30(sim.CaptureState());
    private static byte[] WriteVersion30(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.ThruSpeciesFlags = null; pose.ThruBits = null; pose.GhostFlags = null; pose.NonShootableFlags = null; pose.HitOwnerFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
