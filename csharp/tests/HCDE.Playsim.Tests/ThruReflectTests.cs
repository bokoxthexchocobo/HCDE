using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruReflectTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void ContactDamagesBeforeOptionalPassThroughOwnerTransfer(bool through, bool reflective, bool passes)
    {
        var (_, missile, target) = Room(); var owner = missile.Owner;
        missile.ThruReflect = through; target.Reflective = reflective;
        missile.Tick();
        Assert.Equal(993, target.Health); Assert.Equal(owner!.Id, target.LastDamageSourceId);
        Assert.Equal(!reflective, missile.Destroyed);
        if (passes) { Assert.Same(target, missile.Owner); Assert.Equal(2, missile.X.ToDouble()); Assert.Equal(2, missile.VelocityX.ToDouble()); }
    }

    [Fact]
    public void NewOwnerImmunityPreventsRepeatedDamageWhilePassingThrough()
    {
        var (_, missile, target) = Room(); missile.ThruReflect = target.Reflective = true;
        missile.Tick(); missile.Tick();
        Assert.Equal(993, target.Health); Assert.False(missile.Destroyed); Assert.Equal(4, missile.X.ToDouble());
    }

    [Fact]
    public void SupportedSeekerRetargetsTracerToPreviousOwner()
    {
        var (_, missile, target) = Room(ProjectileKind.RevenantTracer); var previousOwner = missile.Owner;
        missile.ThruReflect = target.Reflective = true; missile.Tick();
        Assert.Same(target, missile.Owner); Assert.Equal(previousOwner!.Id, missile.TracerId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReflectionFlagsAreReadAfterDamageExpression(bool enable)
    {
        var (_, missile, target) = Room(); target.Reflective = true; missile.ThruReflect = !enable;
        missile.DamageExpression = _ => { missile.ThruReflect = enable; return 7; };
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Equal(enable ? 2 : 0, missile.X.ToDouble()); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void FlagsAndTransferredOwnerRoundTripAndOldSaveClearsFlags()
    {
        var (sim, missile, target) = Room(); var older = SimSavegame.Write(sim, true);
        missile.ThruReflect = target.Reflective = true; missile.Tick();
        var saved = SimSavegame.Write(sim, true); Assert.Equal(323, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        missile.Tick(); var expectedX = missile.X;
        SimSavegame.Apply(sim, saved);
        Assert.True(missile.ThruReflect); Assert.True(target.Reflective); Assert.Same(target, missile.Owner);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); missile.Tick(); Assert.Equal(expectedX, missile.X);
        SimSavegame.Apply(sim, older); Assert.False(missile.ThruReflect); Assert.False(target.Reflective);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(256)]
    public void InvalidReflectionArchiveFlagsAreRejected(int value)
    {
        var (sim, missile, _) = Room(); missile.ThruReflect = true;
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-reflection-value", error);
    }

    [Theory]
    [InlineData("reflective")]
    [InlineData("thrureflect")]
    public void AcsExposesReflectionFlags(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToUpperInvariant(), out var enabled)); Assert.True(enabled);
    }

    [Fact]
    public void RipperPassagePrecedesReflectiveOwnerTransfer()
    {
        var (_, missile, target) = Room(); var owner = missile.Owner;
        missile.Rip = missile.ThruReflect = target.Reflective = true;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(owner, missile.Owner); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void AttackImmunityBlocksDamageButOrdinaryReflectionStillTransfersOwner()
    {
        var (_, missile, target) = Room(); var owner = Assert.IsAssignableFrom<Actor>(missile.Owner);
        owner.ProjectileGroup = target.ProjectileGroup = 7; missile.ThruReflect = target.Reflective = true;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void ZeroDamageStillAllowsReflectionTransfer()
    {
        var (_, missile, target) = Room(); missile.ThruReflect = target.Reflective = true;
        missile.DamageExpression = _ => 0; missile.Tick();
        Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(1000, target.Health);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(ProjectileKind kind = ProjectileKind.Plasma)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        missile.DamageExpression = _ => 7; sim.Thinkers.Clock.Restore(1);
        return (sim, missile, target);
    }
}
