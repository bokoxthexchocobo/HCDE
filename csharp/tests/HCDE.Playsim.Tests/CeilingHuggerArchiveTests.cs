using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingHuggerArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RestoreResumesSavedCeilingBehavior(bool enabled, bool serialized)
    {
        var sim = Room(); var missile = Shoot(sim); missile.CeilingHugger = enabled;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = LegacyActorArchiveFixture.Write(state);
            Assert.Equal(enabled ? 25 : 24, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        missile.CeilingHugger = !enabled; missile.RestoreTracerTarget(null); missile.RestoreRemainingTics(1);
        sim.RestoreState(state); sim.Tick();
        Assert.Equal(enabled, missile.CeilingHugger); Assert.Equal(!enabled, missile.Destroyed);
        if (enabled) Assert.Equal(174, missile.RemainingTics);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void MalformedFlagIsRejected(int flags)
    {
        var sim = Room(); Shoot(sim).CeilingHugger = true; var bytes = LegacyActorArchiveFixture.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-ceiling-hugger-flags", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void InvalidMemoryFlagRejectsRestoreBeforeMutation()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        state.Actors[0].CeilingFlags = 2; missile.CeilingHugger = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(missile.CeilingHugger); Assert.Equal(174, missile.RemainingTics);
    }

    [Fact]
    public void IncompleteTableCannotBeWritten()
    {
        var sim = Room(); Shoot(sim).CeilingHugger = true; var state = sim.CaptureState();
        state.Actors[0].CeilingFlags = null;
        Assert.Throws<InvalidOperationException>(() => LegacyActorArchiveFixture.Write(state));
    }

    [Fact]
    public void MalformedSizeIsRejected()
    {
        var sim = Room(); Shoot(sim).CeilingHugger = true; var bytes = LegacyActorArchiveFixture.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-ceiling-hugger-size", error); Assert.Empty(state.Actors);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.RevenantTracer);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(118);
        missile.VelocityX = missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(10);
        return missile;
    }
}
