using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpectralArchiveTests
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
    public void SavedFlagsControlResumedMissileContact(bool targetSpectral, bool missileSpectral, bool serialized)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null; target.PainChance = 0;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        target.Spectral = targetSpectral; missile.Spectral = missileSpectral; var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = SimSavegame.Write(state); Assert.Equal(36, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        target.Spectral = !targetSpectral; missile.Spectral = !missileSpectral;
        sim.RestoreState(state); Assert.Equal(targetSpectral, target.Spectral); Assert.Equal(missileSpectral, missile.Spectral);
        var health = target.Health; sim.Tick(); var passes = targetSpectral && !missileSpectral;
        Assert.Equal(!passes, missile.Destroyed);
        if (passes) Assert.Equal(health, target.Health); else Assert.True(target.Health < health);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidWireFlagRejectsApplyBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = SimSavegame.Write(sim.CaptureState());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(TrailerStart(bytes) + 8), flags);
        actor.Spectral = true; sim.Tick(); var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-spectral-flags", error); Assert.Empty(state.Actors);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Spectral); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidMemoryFlagRejectsWriteAndRestore(int flags)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var state = sim.CaptureState(); state.Actors[0].SpectralFlags = flags;
        actor.Spectral = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(actor.Spectral);
    }

    [Theory]
    [InlineData("size")]
    [InlineData("prior")]
    [InlineData("count")]
    public void MalformedTrailerIsRejected(string field)
    {
        var sim = Room(); sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim.CaptureState());
        var offset = field == "size" ? bytes.Length - 4 : TrailerStart(bytes) + (field == "count" ? 4 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), field == "prior" ? 36 : 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(field == "size" ? "save-spectral-size" : "save-spectral-header", error);
    }

    [Fact]
    public void IncompleteZeroTableIsRejected()
    {
        var sim = Room(); sim.AddBot(0, 0); sim.AddBot(100, 0);
        var state = sim.CaptureState(); state.Actors[1].SpectralFlags = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Version35PreservesCurrentFlag(bool enabled)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState(); state.Actors[0].SpectralFlags = null;
        var bytes = SimSavegame.Write(state); Assert.Equal(35, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.Spectral = enabled; SimSavegame.Apply(sim, bytes); Assert.Equal(enabled, actor.Spectral);
    }

    [Fact]
    public void NestedGhostFlagsAndFullPassageMaskRoundTrip()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Spectral = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = true;
        actor.ThruBits = uint.MaxValue; var bytes = SimSavegame.Write(sim.CaptureState());
        actor.Spectral = actor.Ghost = actor.ThruGhost = actor.AllowThruBits = false;
        actor.ThruBits = 0; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.Spectral); Assert.True(actor.Ghost); Assert.True(actor.ThruGhost);
        Assert.True(actor.AllowThruBits); Assert.Equal(uint.MaxValue, actor.ThruBits);
    }

    private static int TrailerStart(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
