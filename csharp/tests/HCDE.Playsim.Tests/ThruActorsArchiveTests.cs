using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruActorsArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RestoredFlagControlsBlastCollision(bool enabled, bool serialized)
    {
        var sim = Room(); var source = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        source.Brain = target.Brain = null; source.Blasted = true; source.VelocityX = Fixed.FromInt(3);
        target.ThruActors = enabled; var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion29(state);
            Assert.Equal(29, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        target.ThruActors = !enabled; sim.RestoreState(state);
        Assert.Equal(enabled, target.ThruActors);
        Assert.Equal(enabled, ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(enabled ? 0 : 3, target.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredProjectileUsesSavedFlag(bool enabled)
    {
        var sim = Room(); var owner = sim.AddBot(-100, 0); var target = sim.AddBot(30, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.ThruActors = enabled; var bytes = WriteVersion29(sim);
        missile.ThruActors = !enabled; SimSavegame.Apply(sim, bytes); sim.Tick();
        Assert.Equal(!enabled, missile.Destroyed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagsRejectApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion29(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.ThruActors = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-thruactors-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.ThruActors); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].ThruActorsFlags = flags;
        actor.ThruActors = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion29(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.ThruActors);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion29(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 29 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-thruactors-size" : "save-thruactors-header", error);
    }

    [Fact]
    public void PartialZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].ThruActorsFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion29(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version28PreservesCurrentThruActorsFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var saved = sim.CaptureState();
        saved.Actors[0].ThruActorsFlags = null; var bytes = WriteVersion29(saved);
        Assert.Equal(28, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.ThruActors = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.ThruActors);
    }

    [Fact]
    public void NestedBlastEligibilityFlagsRestoreAlongsideThruActors()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = true;
        var bytes = WriteVersion29(sim);
        actor.ThruActors = actor.Boss = actor.DontBlast = actor.Blasted = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.ThruActors); Assert.True(actor.Boss); Assert.True(actor.DontBlast); Assert.True(actor.Blasted);
    }

    private static byte[] WriteVersion29(AuthoritySimulation sim) => WriteVersion29(sim.CaptureState());
    private static byte[] WriteVersion29(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.MissileThruSpeciesFlags = null; pose.ThruSpeciesFlags = null; pose.ThruBits = null; pose.GhostFlags = null; pose.NonShootableFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
