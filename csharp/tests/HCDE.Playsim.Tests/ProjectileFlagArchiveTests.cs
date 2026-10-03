using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFlagArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RestoredFlagDeterminesFloorSurvival(bool enabled, bool serialized)
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoExplodeFloor = enabled;
        var state = sim.CaptureState();
        if (serialized)
        {
            foreach (var pose in state.Actors) pose.ProjectileLifetime = null;
            var bytes = SimSavegame.Write(state);
            Assert.Equal(enabled ? 22 : 18, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        missile.NoExplodeFloor = !enabled; sim.RestoreState(state); sim.Tick();
        Assert.Equal(enabled, missile.NoExplodeFloor); Assert.Equal(!enabled, missile.Destroyed);
        Assert.Equal(enabled, sim.Level.Sectors[0].HealthFloor == 1000);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void MalformedFlagIsRejected(int flags)
    {
        var sim = Room(); Shoot(sim).NoExplodeFloor = true; var bytes = WriteFlagsOnly(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-projectile-flags", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void InvalidMemoryFlagRejectsRestoreBeforeMutation()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        state.Actors[0].ProjectileFlags = 2; missile.NoExplodeFloor = true; sim.Tick();
        var tic = sim.Thinkers.Clock.Tic; var health = sim.Players.Single().Health;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(tic, sim.Thinkers.Clock.Tic); Assert.Equal(health, sim.Players.Single().Health);
        Assert.True(missile.NoExplodeFloor);
    }

    [Fact]
    public void IncompleteFlagTableCannotBeWritten()
    {
        var sim = Room(); Shoot(sim).NoExplodeFloor = true; var state = sim.CaptureState();
        state.Actors[0].ProjectileFlags = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    [Fact]
    public void ExtensionComposesWithPainDeathTimerArchive()
    {
        var sim = Room(true); var parent = sim.Actors.Single(a => a.DoomEdNum == 71);
        parent.Health = 0; for (var i = 0; i < 10; i++) sim.Tick();
        var missile = Shoot(sim); missile.NoExplodeFloor = true;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        Assert.Equal(10, state.Actors.Single(a => a.Id == parent.Id).PainDeath!.Value.Tics);
        missile.NoExplodeFloor = false; parent.Brain!.RestorePainDeath(new(31, null)); sim.RestoreState(state);
        Assert.True(missile.NoExplodeFloor); Assert.Equal(10, parent.Brain.CapturePainDeath()!.Value.Tics);
    }

    [Theory]
    [InlineData(13, "save-projectile-size")]
    [InlineData(0, "save-projectile-size")]
    public void InvalidTrailerSizeIsRejected(int size, string expected)
    {
        var sim = Room(); Shoot(sim).NoExplodeFloor = true; var bytes = WriteFlagsOnly(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal(expected, error); Assert.Empty(state.Actors);
    }

    private static byte[] WriteFlagsOnly(AuthoritySimulation sim)
    {
        var state = sim.CaptureState(); foreach (var pose in state.Actors) pose.ProjectileLifetime = null;
        return SimSavegame.Write(state);
    }

    private static AuthoritySimulation Room(bool pain = false) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512, HealthFloor = 1000 }],
        Things = pain ? [new LevelThing { Type = 1 }, new LevelThing { Type = 71, X = 512 }]
            : [new LevelThing { Type = 1 }] });

    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(2);
        missile.VelocityX = missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(-10);
        return missile;
    }
}
