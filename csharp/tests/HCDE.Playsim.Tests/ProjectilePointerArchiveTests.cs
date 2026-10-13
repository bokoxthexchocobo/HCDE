using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePointerArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearedOwnerAndTracerSurviveSaveAndReconstruction(bool reconstruct)
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(800, 200);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        missile.X = Fixed.FromInt(100);
        ActorPointerActions.RearrangePointers(missile, AcsActorPointer.Null);
        Assert.Null(AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target));
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(190, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        if (reconstruct) missile.Destroy();
        else missile.ReassignOwner(owner);
        sim.RestoreState(state);
        var restored = Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == missile.Id));
        Assert.Null(restored.Owner); Assert.Equal(target.Id, restored.TracerTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        sim.Tick(); Assert.False(restored.Destroyed);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void TargetChainGuardHonorsUnsafeTargetFlag(int flags, bool retained)
    {
        var sim = Room(); var player = sim.Players.Single();
        var first = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        var second = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        first.MasterId = second.Id; second.MasterId = first.Id;
        ActorPointerActions.RearrangePointers(first, AcsActorPointer.Master);
        ActorPointerActions.RearrangePointers(second, AcsActorPointer.Master, flags: flags);
        Assert.Same(second, first.Owner);
        Assert.Equal(retained ? first : null, second.Owner);
    }

    [Fact]
    public void TargetChainGuardRejectsDownstreamCycle()
    {
        var sim = Room(); var player = sim.Players.Single();
        var first = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        var second = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        var third = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        first.ReassignOwner(second); second.ReassignOwner(first); third.MasterId = first.Id;
        ActorPointerActions.RearrangePointers(third, AcsActorPointer.Master);
        Assert.Null(third.Owner); Assert.Same(second, first.Owner); Assert.Same(first, second.Owner);
    }

    [Fact]
    public void RestoreReconstructsBothSidesOfUnsafeOwnerCycle()
    {
        var sim = Room(); var player = sim.Players.Single();
        var first = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        var second = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        first.ReassignOwner(second); second.ReassignOwner(first);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        first.Destroy(); second.Destroy();
        sim.RestoreState(state);
        var restoredFirst = Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == first.Id));
        var restoredSecond = Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == second.Id));
        Assert.Same(restoredSecond, restoredFirst.Owner); Assert.Same(restoredFirst, restoredSecond.Owner);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void OwnerlessDamageAndMovementAreSafeWithSpeciesFiltering()
    {
        var sim = Room(); var victim = sim.AddBot(800, 200);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.MThruSpecies = true;
        missile.X = Fixed.FromInt(100);
        ActorPointerActions.RearrangePointers(missile, AcsActorPointer.Null);
        var health = victim.Health;
        missile.DoMissileDamage(victim);
        Assert.True(victim.Health < health); Assert.Null(victim.LastDamageSourceId);
        sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void MalformedNullableOwnerPresenceIsRejected()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.ReassignOwner(null);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-nullable-pointer-value", error);
    }

    [Fact]
    public void ClearingOwnerRemovesFormerOwnerCollisionImmunity()
    {
        var sim = Room(); var player = sim.Players.Single();
        var missile = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        missile.ReassignOwner(null);
        var health = player.Health;
        sim.Tick();
        Assert.True(missile.Destroyed); Assert.True(player.Health < health);
        Assert.Null(player.LastDamageSourceId);
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Bfg)]
    public void OwnerlessExplosionHandlesRadiusDamageAndBfgSpray(ProjectileKind kind)
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        missile.ReassignOwner(null);
        missile.ExplodeForAction();
        Assert.True(missile.Destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReassignedOwnerSurvivesSaveAndProjectileReconstruction(bool reconstruct)
    {
        var sim = Room(); var original = sim.Players.Single(); var owner = sim.AddBot(800, 0, 3001);
        var victim = sim.AddBot(900, 0);
        var missile = sim.SpawnProjectile(original, ProjectileKind.Plasma);
        missile.MasterId = owner.Id;
        ActorPointerActions.RearrangePointers(missile, AcsActorPointer.Master);
        Assert.Same(owner, missile.Owner);
        Assert.Same(owner, AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target));
        missile.DoMissileDamage(victim);
        Assert.Equal(owner.Id, victim.LastDamageSourceId);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        if (reconstruct) missile.Destroy();
        else missile.ReassignOwner(original);
        sim.RestoreState(state);
        var restored = Assert.IsType<ProjectileActor>(sim.Actors.Single(a => a.Id == missile.Id));
        Assert.Same(owner, restored.Owner);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void OwnerClearRearrangesOriginalPointers()
    {
        var sim = Room(); var owner = sim.Players.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        ActorPointerActions.RearrangePointers(missile,
            AcsActorPointer.Null, AcsActorPointer.Target, AcsActorPointer.Target);
        Assert.Null(missile.Owner); Assert.Equal(owner.Id, missile.MasterId); Assert.Equal(owner.Id, missile.TracerId);
    }

    [Fact]
    public void ProjectileProximityTargetAssignmentChangesOwner()
    {
        var sim = Room(); var owner = sim.AddBot(20, 0, 3001);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.True(ActorProximity.Check(missile, "DoomImp", 100, flags: 64));
        Assert.Same(owner, missile.Owner);
    }

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
            var bytes = LegacyActorArchiveFixture.Write(state); Assert.Equal(24, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
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
    public void NonHomingMissileCanRestoreAssignedTracer()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var state = sim.CaptureState(); state.Actors.Single(a => a.Id == missile.Id).ProjectilePointers = new(Assert.IsAssignableFrom<Actor>(missile.Owner).Id, Assert.IsAssignableFrom<Actor>(missile.Owner).Id);
        var bytes = LegacyActorArchiveFixture.Write(state);
        Assert.True(SimSavegame.TryRead(bytes, out var restored, out var error), error);
        sim.RestoreState(restored);
        Assert.Equal(Assert.IsAssignableFrom<Actor>(missile.Owner).Id, missile.TracerTargetId);
    }

    [Fact]
    public void TracerOnAbsentSerializedRecordIsRejected()
    {
        var sim = Room(); sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var bytes = LegacyActorArchiveFixture.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
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
        Assert.True(SimSavegame.TryRead(LegacyActorArchiveFixture.Write(state), out state, out var error), error);
        missile.RestoreTracerTarget(null); sim.RestoreState(state); Assert.Null(missile.TracerTargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
