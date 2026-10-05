using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RipperPushTests
{
    [Theory]
    [InlineData(true, false, 0.25, 7.5)]
    [InlineData(true, false, 0.5, 15)]
    [InlineData(true, false, 0, 0)]
    [InlineData(true, false, -0.5, -15)]
    [InlineData(false, false, 0.25, 0)]
    [InlineData(true, true, 0.25, 0)]
    public void HorizontalMomentumTransfersOncePerMovementPass(bool pushable, bool cannotPush, double factor, double expected)
    {
        var (_, missile, target) = Setup(); target.Pushable = pushable; missile.CannotPush = cannotPush;
        target.PushFactor = factor; target.VelocityX = Fixed.FromInt(1); target.VelocityY = Fixed.FromInt(2); target.VelocityZ = Fixed.FromInt(3);
        missile.Tick();
        Assert.Equal(1 + expected, target.VelocityX.ToDouble()); Assert.Equal(2, target.VelocityY.ToDouble());
        Assert.Equal(3, target.VelocityZ.ToDouble()); Assert.Equal(993, target.Health);
        Assert.False(missile.Destroyed);
    }

    [Fact]
    public void ExtendedArchiveAndPosePreservePushRules()
    {
        var (sim, missile, target) = Setup(); target.Pushable = true; target.PushFactor = 0.5;
        missile.CannotPush = true; var pose = sim.CaptureState();
        missile.CannotPush = false; target.PushFactor = 0; sim.RestoreState(pose);
        Assert.True(missile.CannotPush); Assert.Equal(0.5, target.PushFactor);
        var bytes = SimSavegame.Write(sim); Assert.Equal(60, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, victim) = Setup(); restored.RestoreState(state);
        Assert.True(victim.Pushable); Assert.True(loaded.CannotPush); Assert.Equal(0.5, victim.PushFactor);
        Assert.True(AcsActorFlags.TrySet(loaded, "Actor.CANNOTPUSH", false));
        Assert.True(AcsActorFlags.TryGet(victim, "pushable", out var value)); Assert.True(value);
        loaded.DamageExpression = _ => 7; loaded.Tick(); Assert.Equal(15, victim.VelocityX.ToDouble());
    }

    [Fact]
    public void LegacyClearsPushFlagsAndRestoresDefaultFactor()
    {
        var (sim, missile, target) = Setup(); var legacy = SimSavegame.Write(sim);
        target.Pushable = true; target.PushFactor = -0.5; missile.CannotPush = true;
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error); sim.RestoreState(state);
        Assert.False(target.Pushable); Assert.False(missile.CannotPush); Assert.Equal(0.25, target.PushFactor);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void NonFinitePushFactorIsRejected()
    {
        var (sim, _, target) = Setup(); Assert.Throws<ArgumentOutOfRangeException>(() => target.PushFactor = double.NaN);
        target.Pushable = true; var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - size + 24), double.PositiveInfinity);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-rip-push-factor", error);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.Rip = true; missile.DamageExpression = _ => 7;
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
