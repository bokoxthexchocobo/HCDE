using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonShootableArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SavedFlagControlsResumedMissileContact(bool enabled, bool serialized)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null; target.NonShootable = enabled;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion34(state); Assert.Equal(34, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        target.NonShootable = !enabled; sim.RestoreState(state);
        Assert.Equal(enabled, target.NonShootable); Assert.True(target.Shootable);
        var health = target.Health; sim.Tick(); Assert.Equal(!enabled, missile.Destroyed);
        if (enabled) Assert.Equal(health, target.Health); else Assert.True(target.Health < health);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion34(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.NonShootable = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-nonshootable-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.NonShootable); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].NonShootableFlags = flags;
        actor.NonShootable = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion34(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.NonShootable);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion34(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 34 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-nonshootable-size" : "save-nonshootable-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].NonShootableFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion34(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version33PreservesCurrentFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Actors[0].NonShootableFlags = null;
        var bytes = WriteVersion34(state); Assert.Equal(33, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.NonShootable = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.NonShootable);
    }

    [Fact]
    public void NestedGhostFlagsAndFullPassageMaskRoundTrip()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.NonShootable = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = true;
        actor.ThruBits = uint.MaxValue; var bytes = WriteVersion34(sim);
        actor.NonShootable = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = false;
        actor.ThruBits = 0; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.NonShootable); Assert.True(actor.Ghost); Assert.True(actor.ThruGhost);
        Assert.True(actor.AllowThruBits); Assert.Equal(uint.MaxValue, actor.ThruBits);
    }

    private static byte[] WriteVersion34(AuthoritySimulation sim) => WriteVersion34(sim.CaptureState());
    private static byte[] WriteVersion34(SimSaveState state)
    {
        foreach (var pose in state.Actors) pose.HitOwnerFlags = null;
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
