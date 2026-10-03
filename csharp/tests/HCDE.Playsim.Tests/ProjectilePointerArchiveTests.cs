using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePointerArchiveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RestoreResumesSavedHomingDirection(bool serialized, bool targetless)
    {
        var sim = Room(); var target = sim.AddBot(800, 200); target.Brain = null;
        var other = sim.AddBot(800, -200); other.Brain = null;
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.RevenantTracer, targetless ? null : target);
        for (var i = 0; i < 3; i++) sim.Tick();
        var state = sim.CaptureState();
        if (serialized)
        {
            var bytes = SimSavegame.Write(state); Assert.Equal(24, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
            Assert.True(SimSavegame.TryRead(bytes, out state, out var error), error);
        }
        sim.Tick(); var angle = missile.Angle; var vz = missile.VelocityZ;
        missile.RestoreTracerTarget(other.Id); sim.RestoreState(state);
        Assert.Equal(targetless ? (uint?)null : target.Id, missile.TracerTargetId);
        sim.Tick(); Assert.Equal(angle, missile.Angle); Assert.Equal(vz, missile.VelocityZ);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedSavedTargetClearsTracer(bool remove)
    {
        var sim = Room(); var target = sim.AddBot(800, 200); target.Brain = null;
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.RevenantTracer, target);
        var state = sim.CaptureState(); target.Destroy(); if (remove) sim.Tick();
        sim.RestoreState(state); Assert.Null(missile.TracerTargetId);
    }

    [Fact]
    public void OwnerMismatchRejectsRestoreBeforeClockMutation()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var state = sim.CaptureState(); state.Actors.Single(a => a.Id == missile.Id).ProjectilePointers = new(999, null);
        sim.Tick(); Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(1, sim.Thinkers.Clock.Tic); Assert.Equal(174, missile.RemainingTics);
    }

    [Fact]
    public void NonHomingMissileCannotRestoreTracer()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var state = sim.CaptureState(); state.Actors.Single(a => a.Id == missile.Id).ProjectilePointers = new(missile.Owner.Id, missile.Owner.Id);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Fact]
    public void TracerOnAbsentSerializedRecordIsRejected()
    {
        var sim = Room(); sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var bytes = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 12), 99);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-pointer-state", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void LegacyArchivePreservesExistingTracer()
    {
        var sim = Room(); var target = sim.AddBot(800, 200); target.Brain = null;
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.RevenantTracer, target);
        var state = sim.CaptureState(); foreach (var pose in state.Actors) pose.ProjectilePointers = null;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(state), out state, out var error), error);
        missile.RestoreTracerTarget(null); sim.RestoreState(state); Assert.Null(missile.TracerTargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
