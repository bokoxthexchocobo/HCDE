using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileHitPointerFlagTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void HitFlagsIndependentlyAssignPointersAfterDirectDamage(int flags)
    {
        var (sim, missile, target) = Setup(); var owner = missile.Owner;
        SetFlags(missile, flags);
        missile.DamageExpression = actor => { Assert.Same(owner, actor.Owner); return 7; };
        sim.DrainSoundRequests();
        sim.SoundRequestsReady += () =>
        {
            Assert.Same((flags & 1) != 0 ? target : owner, missile.Owner);
            Assert.Equal((flags & 2) != 0 ? target.Id : (uint?)null, missile.MasterId);
            Assert.Equal((flags & 4) != 0 ? target.Id : (uint?)null, missile.TracerId);
        };
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NondamagingBlockedContactStillAssignsHitPointers(bool overlap)
    {
        var (_, missile, target) = Setup(); SetFlags(missile, 7);
        if (overlap) { missile.CanPass = true; missile.DontOverlap = target.DontOverlap = true; }
        else missile.SpecialMissileHitFunction = (_, _) => 0;
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(1000, target.Health);
        Assert.Same(target, missile.Owner); Assert.Equal(target.Id, missile.MasterId); Assert.Equal(target.Id, missile.TracerId);
    }

    [Fact]
    public void ExplosionCountDefersPointerAssignment()
    {
        var (_, missile, target) = Setup(); var owner = missile.Owner; SetFlags(missile, 7);
        missile.ExplodeCount = true; ActorPropertyActions.SetChaseThreshold(missile, 2, setDefault: true);
        missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(owner, missile.Owner); Assert.Null(missile.MasterId);
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Same(target, missile.Owner);
        Assert.Equal(target.Id, missile.MasterId); Assert.Equal(target.Id, missile.TracerId);
    }

    [Fact]
    public void ArchivePreservesFlagsAndRejectsUnknownBits()
    {
        var (sim, missile, _) = Setup(); var legacy = SimSavegame.Write(sim); SetFlags(missile, 7);
        var bytes = SimSavegame.Write(sim); Assert.Equal(197, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        SetFlags(missile, 0); SimSavegame.Apply(sim, bytes);
        Assert.True(missile.HitTarget && missile.HitMaster && missile.HitTracer); Assert.Equal(bytes, SimSavegame.Write(sim));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 8);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-missile-hit-pointer-flags-value", error);
        SimSavegame.Apply(sim, legacy); Assert.False(missile.HitTarget || missile.HitMaster || missile.HitTracer);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    private static void SetFlags(Actor actor, int flags)
    {
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.HITTARGET", (flags & 1) != 0));
        Assert.True(AcsActorFlags.TrySet(actor, "hitmaster", (flags & 2) != 0));
        Assert.True(AcsActorFlags.TrySet(actor, "HITTRACER", (flags & 4) != 0));
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
