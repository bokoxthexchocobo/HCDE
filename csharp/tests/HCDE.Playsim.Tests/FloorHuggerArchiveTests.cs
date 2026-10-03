using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorHuggerArchiveTests
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
    public void RestoredFlagsControlFloorContact(int flags, bool serialized)
    {
        var sim = Room(); var missile = Shoot(sim);
        missile.FloorHugger = (flags & 1) != 0; missile.NoDropOff = (flags & 2) != 0;
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = LegacyActorArchiveFixture.Write(state); Assert.Equal(flags == 0 ? 24 : 26, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        missile.FloorHugger = !missile.FloorHugger; missile.NoDropOff = !missile.NoDropOff;
        sim.RestoreState(state); sim.Tick();
        Assert.Equal((flags & 1) != 0, missile.FloorHugger); Assert.Equal((flags & 2) != 0, missile.NoDropOff);
        Assert.Equal(flags != 1, missile.Destroyed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidSerializedBitsAreRejected(int flags)
    {
        var sim = Room(); Shoot(sim).FloorHugger = true; var bytes = LegacyActorArchiveFixture.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-floor-hugger-flags", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void InvalidMemoryBitsRejectRestoreBeforeMutation()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        state.Actors[0].FloorHuggerFlags = 4; missile.NoExplodeFloor = true; sim.Tick();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.True(missile.NoExplodeFloor); Assert.Equal(174, missile.RemainingTics);
    }

    [Fact]
    public void IncompleteTableCannotBeWritten()
    {
        var sim = Room(); Shoot(sim).FloorHugger = true; var state = sim.CaptureState();
        state.Actors[0].FloorHuggerFlags = null;
        Assert.Throws<InvalidOperationException>(() => LegacyActorArchiveFixture.Write(state));
    }

    [Fact]
    public void FlagsComposeWithCeilingAndProjectileArchives()
    {
        var sim = Room(); var missile = Shoot(sim); missile.FloorHugger = missile.CeilingHugger = true;
        Assert.True(SimSavegame.TryRead(LegacyActorArchiveFixture.Write(sim), out var state, out var error), error);
        missile.FloorHugger = missile.CeilingHugger = false; missile.NoDropOff = true;
        sim.RestoreState(state); sim.Tick(); Assert.False(missile.Destroyed); Assert.True(missile.CeilingHugger);
        Assert.Equal(0, missile.Z.ToDouble()); Assert.Equal(174, missile.RemainingTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(2);
        missile.VelocityX = missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(-10);
        return missile;
    }
}
