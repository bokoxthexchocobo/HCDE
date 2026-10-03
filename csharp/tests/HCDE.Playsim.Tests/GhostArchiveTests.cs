using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GhostArchiveTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void SavedFlagsDetermineResumedMissileImpact(bool ghost, bool thruGhost, bool serialized)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null; target.Ghost = ghost;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.ThruGhost = thruGhost; var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion33(state); Assert.Equal(33, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        target.Ghost = !ghost; missile.ThruGhost = !thruGhost;
        sim.RestoreState(state); Assert.Equal(ghost, target.Ghost); Assert.Equal(thruGhost, missile.ThruGhost);
        sim.Tick(); Assert.Equal(!(ghost && thruGhost), missile.Destroyed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidWireFlagsRejectApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion33(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.Ghost = actor.ThruGhost = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-ghost-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Ghost); Assert.True(actor.ThruGhost);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidMemoryFlagsRejectWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].GhostFlags = flags;
        actor.Ghost = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion33(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Ghost);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void InvalidTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion33(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 33 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-ghost-size" : "save-ghost-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].GhostFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion33(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Fact]
    public void Version32PreservesCurrentGhostFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Actors[0].GhostFlags = null;
        var bytes = WriteVersion33(state); Assert.Equal(32, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.Ghost = actor.ThruGhost = true; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.Ghost); Assert.True(actor.ThruGhost);
    }

    [Fact]
    public void NestedContactMetadataRoundTripsAlongsideGhostFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Ghost = actor.ThruGhost = actor.AllowThruBits = actor.ThruSpecies = actor.MThruSpecies = actor.ThruActors = true;
        actor.ThruBits = uint.MaxValue; var bytes = WriteVersion33(sim);
        actor.Ghost = actor.ThruGhost = actor.AllowThruBits = actor.ThruSpecies = actor.MThruSpecies = actor.ThruActors = false;
        actor.ThruBits = 0; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.Ghost); Assert.True(actor.ThruGhost); Assert.True(actor.AllowThruBits);
        Assert.True(actor.ThruSpecies); Assert.True(actor.MThruSpecies); Assert.True(actor.ThruActors);
        Assert.Equal(uint.MaxValue, actor.ThruBits);
    }

    private static byte[] WriteVersion33(AuthoritySimulation sim) => WriteVersion33(sim.CaptureState());
    private static byte[] WriteVersion33(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.NonShootableFlags = null; pose.HitOwnerFlags = null; pose.SpectralFlags = null; }
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
