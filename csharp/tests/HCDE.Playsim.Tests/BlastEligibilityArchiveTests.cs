using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastEligibilityArchiveTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void RestoredFlagsControlBlastTransfer(int flags, bool serialized)
    {
        var sim = Room(); var source = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        source.Brain = target.Brain = null; source.Blasted = true; source.VelocityX = Fixed.FromInt(3);
        target.Boss = (flags & 1) != 0; target.DontBlast = (flags & 2) != 0;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion28(state);
            Assert.Equal(28, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        target.Boss = !target.Boss; target.DontBlast = !target.DontBlast;
        sim.RestoreState(state);
        Assert.Equal((flags & 1) != 0, target.Boss); Assert.Equal((flags & 2) != 0, target.DontBlast);
        Assert.False(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(flags == 0 ? 3 : 0, target.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(7)]
    [InlineData(16)]
    public void ExplicitlyClearedBossDefaultSurvivesSerialization(int type)
    {
        var sim = Room(); var boss = sim.AddBot(0, 0, type); Assert.True(boss.Boss);
        boss.Boss = false; var bytes = WriteVersion28(sim); boss.Boss = true;
        SimSavegame.Apply(sim, bytes); Assert.False(boss.Boss);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidWireFlagsRejectApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion28(sim); var start = TrailerStart(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), flags);
        actor.Boss = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-blast-eligibility-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Boss); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidMemoryFlagsRejectWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].BlastEligibilityFlags = flags;
        actor.DontBlast = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion28(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.DontBlast);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion28(sim);
        var start = TrailerStart(bytes);
        var offset = field == "size" ? bytes.Length - 4 : start + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 28 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-blast-eligibility-size" : "save-blast-eligibility-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].BlastEligibilityFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion28(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Fact]
    public void NestedMovementFlagExtensionsRoundTripTogether()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Boss = actor.DontBlast = actor.Blasted = actor.NoDropOff = actor.FloorHugger = true;
        actor.CeilingHugger = actor.NoExplodeFloor = actor.InFloat = true;
        var bytes = WriteVersion28(sim);
        actor.Boss = actor.DontBlast = actor.Blasted = actor.NoDropOff = actor.FloorHugger = false;
        actor.CeilingHugger = actor.NoExplodeFloor = actor.InFloat = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.Boss); Assert.True(actor.DontBlast); Assert.True(actor.Blasted);
        Assert.True(actor.NoDropOff); Assert.True(actor.FloorHugger); Assert.True(actor.CeilingHugger);
        Assert.True(actor.NoExplodeFloor); Assert.True(actor.InFloat);
    }

    [Fact]
    public void LegacyArchivePreservesCurrentFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 7);
        var bytes = LegacyActorArchiveFixture.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Actors[0].BlastEligibilityFlags);
        actor.DontBlast = true; sim.RestoreState(state);
        Assert.True(actor.Boss); Assert.True(actor.DontBlast);
    }

    private static byte[] WriteVersion28(AuthoritySimulation sim) => WriteVersion28(sim.CaptureState());
    private static byte[] WriteVersion28(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.ThruActorsFlags = null; pose.MissileThruSpeciesFlags = null; pose.ThruSpeciesFlags = null; pose.ThruBits = null; pose.GhostFlags = null; pose.NonShootableFlags = null; pose.HitOwnerFlags = null; pose.SpectralFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
