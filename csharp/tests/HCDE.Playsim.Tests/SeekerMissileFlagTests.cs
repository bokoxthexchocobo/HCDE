using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeekerMissileFlagTests
{
    [Theory]
    [InlineData(ProjectileKind.Plasma, false)]
    [InlineData(ProjectileKind.RevenantTracer, true)]
    public void SupportedProjectileDefaultsDoNotRequireOverrideArchive(ProjectileKind kind, bool seeker)
    {
        var (sim, missile, _) = Room(kind); Assert.Equal(seeker, missile.SeekerMissile);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == missile.Id).SeekerMissileFlag);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BothReflectionRoutesUseFlagRatherThanProjectileKind(bool through, bool seeker)
    {
        var (_, missile, target) = Room(); var oldOwner = missile.Owner;
        missile.SeekerMissile = seeker; missile.ThruReflect = through; target.Reflective = target.ThruReflect = true;
        missile.Tick(); Assert.Same(target, missile.Owner);
        Assert.Equal(seeker ? oldOwner!.Id : (uint?)null, missile.TracerId);
    }

    [Fact]
    public void ClearingRevenantFlagPreventsReflectionTracerOverwrite()
    {
        var (_, missile, target) = Room(ProjectileKind.RevenantTracer);
        missile.SeekerMissile = false; target.Reflective = target.ThruReflect = true;
        missile.Tick(); Assert.Same(target, missile.Owner); Assert.Null(missile.TracerId);
    }

    [Theory]
    [InlineData(ProjectileKind.Plasma, true)]
    [InlineData(ProjectileKind.RevenantTracer, false)]
    public void ExplicitOverridesRoundTripAndOldSavesRestoreClassDefaults(ProjectileKind kind, bool value)
    {
        var (sim, missile, _) = Room(kind); var older = SimSavegame.Write(sim, true);
        missile.SeekerMissile = value; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(325, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        missile.SeekerMissile = !value; SimSavegame.Apply(sim, saved);
        Assert.Equal(value, missile.SeekerMissile); Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.Equal(kind == ProjectileKind.RevenantTracer, missile.SeekerMissile);
    }

    [Fact]
    public void AdjustmentCallbackCanEnableSeekerBeforeTracerTransfer()
    {
        var (_, missile, target) = Room(); var oldOwner = missile.Owner; target.Reflective = true;
        missile.AdjustReflectionAngleFunction = (Actor self, Actor _, ref double angle) =>
        { self.SeekerMissile = true; angle = 90; return false; };
        missile.Tick(); Assert.Equal(oldOwner!.Id, missile.TracerId); Assert.Same(target, missile.Owner);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidSavedFlagIsRejected(int value)
    {
        var (sim, missile, _) = Room(); missile.SeekerMissile = true;
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-seekermissile-value", error);
    }

    [Fact]
    public void AcsSetsAndQueriesSeekerFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "SEEKERMISSILE", true));
        Assert.True(AcsActorFlags.TryGet(actor, "seekermissile", out var value)); Assert.True(value);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(ProjectileKind kind = ProjectileKind.Plasma)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Invulnerable = true;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        sim.Thinkers.Clock.Restore(1);
        return (sim, missile, target);
    }
}
