using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HitOwnerArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SavedFlagControlsResumedMissileContact(bool enabled, bool serialized)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var owner = sim.Players.Single(); owner.X = Fixed.FromInt(30);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.HitOwner = enabled; var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = WriteVersion35(state); Assert.Equal(35, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        missile.HitOwner = !enabled; sim.RestoreState(state); Assert.Equal(enabled, missile.HitOwner);
        var health = owner.Health; sim.Tick(); Assert.Equal(enabled, missile.Destroyed);
        if (enabled) Assert.True(owner.Health < health); else Assert.Equal(health, owner.Health);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = WriteVersion35(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.HitOwner = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-hitowner-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.HitOwner); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].HitOwnerFlags = flags;
        actor.HitOwner = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => WriteVersion35(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.HitOwner);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = WriteVersion35(sim);
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 35 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-hitowner-size" : "save-hitowner-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].HitOwnerFlags = null;
        Assert.Throws<InvalidOperationException>(() => WriteVersion35(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version34PreservesCurrentFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Actors[0].HitOwnerFlags = null;
        var bytes = WriteVersion35(state); Assert.Equal(34, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.HitOwner = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.HitOwner);
    }

    [Fact]
    public void NestedGhostFlagsAndFullPassageMaskRoundTrip()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.HitOwner = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = true;
        actor.ThruBits = uint.MaxValue; var bytes = WriteVersion35(sim);
        actor.HitOwner = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = false;
        actor.ThruBits = 0; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.HitOwner); Assert.True(actor.Ghost); Assert.True(actor.ThruGhost);
        Assert.True(actor.AllowThruBits); Assert.Equal(uint.MaxValue, actor.ThruBits);
    }

    private static byte[] WriteVersion35(AuthoritySimulation sim) => WriteVersion35(sim.CaptureState());
    private static byte[] WriteVersion35(SimSaveState state)
    {
        foreach (var pose in state.Actors) pose.SpectralFlags = null;
        return SimSavegame.Write(state);
    }
    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
