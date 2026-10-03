using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileLifetimeArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoringLifetimeRestoresExpiryDelay(bool serialized)
    {
        var sim = Room(); var missile = Shoot(sim);
        for (var i = 0; i < 100; i++) sim.Tick();
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = SimSavegame.Write(state); Assert.Equal(23, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        for (var i = 0; i < 30; i++) sim.Tick();
        sim.RestoreState(state); Assert.Equal(75, missile.RemainingTics);
        for (var i = 0; i < 74; i++) sim.Tick();
        Assert.False(missile.Destroyed); sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(176)]
    public void MalformedTimerIsRejected(int tics)
    {
        var sim = Room(); Shoot(sim); var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 16), tics);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-lifetime-state", error); Assert.Empty(state.Actors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestroyedOrRemovedProjectileRejectsRestore(bool remove)
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        missile.Destroy(); if (remove) sim.Tick(); var tic = sim.Thinkers.Clock.Tic;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(tic, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void WrongProjectileKindRejectsRestoreBeforeMutation()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        state.Actors.Single(a => a.Id == missile.Id).ProjectileLifetime = new(100, ProjectileKind.Rocket);
        sim.Tick(); var tic = sim.Thinkers.Clock.Tic;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(tic, sim.Thinkers.Clock.Tic); Assert.Equal(174, missile.RemainingTics);
    }

    [Fact]
    public void LegacyArchivePreservesExistingCountdown()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.ProjectileLifetime = null;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(state), out state, out var error), error);
        for (var i = 0; i < 10; i++) sim.Tick(); sim.RestoreState(state);
        Assert.Equal(165, missile.RemainingTics);
    }

    [Fact]
    public void InvalidMemoryTimerRejectsRestoreBeforeClockMutation()
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        state.Actors.Single(a => a.Id == missile.Id).ProjectileLifetime = new(0, missile.Kind);
        sim.Tick(); Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.Equal(174, missile.RemainingTics);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(175)]
    public void LifetimeBoundaryRoundTripsAndExpiresExactly(int tics)
    {
        var sim = Room(); var missile = Shoot(sim); missile.RestoreRemainingTics(tics);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        missile.RestoreRemainingTics(50); sim.RestoreState(state);
        for (var i = 0; i < tics - 1; i++) sim.Tick();
        Assert.False(missile.Destroyed); sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Fact]
    public void LifetimeRecordOnOrdinaryActorRejectsRestore()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.Actors[0].ProjectileLifetime = new(100, ProjectileKind.Plasma);
        sim.Tick(); Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(64);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
