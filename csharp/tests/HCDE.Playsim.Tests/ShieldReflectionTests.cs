using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ShieldReflectionTests
{
    [Theory]
    [InlineData(180, true)]
    [InlineData(135, true)]
    [InlineData(225, true)]
    [InlineData(134.9999, false)]
    [InlineData(225.0001, false)]
    [InlineData(0, false)]
    public void ShieldConeIncludesExactBoundaryAndRejectsBeforeRandomDraw(double facing, bool reflects)
    {
        var (sim, missile, target) = Room(); var owner = missile.Owner; target.Angle = BamAngle.FromDegrees(facing);
        var random = NativeStateRandom.Seed(123, 0x4139f7d5u);
        var offset = (NativeStateRandom.Next(ref random) & 255) < 128 ? 45 : -45;
        Assert.Equal(reflects, ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(reflects, sim.CaptureState().ReflectRandomState.HasValue);
        Assert.Same(reflects ? target : owner, missile.Owner);
        if (reflects) Assert.Equal(BamAngle.FromDegrees(180 + offset), missile.Angle);
    }

    [Fact]
    public void ShieldAngleGateHandlesYawWrapping()
    {
        var (_, missile, target) = Room(); missile.X = Fixed.FromInt(20); target.Angle = BamAngle.FromDegrees(315);
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
    }

    [Fact]
    public void ShieldGatePrecedesDeflectAndMirror()
    {
        var (sim, missile, target) = Room(); target.Angle = BamAngle.FromDegrees(0);
        target.Deflect = target.MirrorReflect = true;
        Assert.False(ActorReflectionActions.ReflectOffActor(missile, target)); Assert.Null(sim.CaptureState().ReflectRandomState);
        Assert.Equal(8, missile.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void NoShieldReflectOnlyRejectsShieldVariant(bool shield, bool reflects)
    {
        var (sim, missile, target) = Room(); target.ShieldReflect = shield; missile.NoShieldReflect = true;
        Assert.Equal(reflects, ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(reflects, sim.CaptureState().ReflectRandomState.HasValue);
    }

    [Fact]
    public void ReflectorThruFlagBypassesShieldConeAndOptOut()
    {
        var (sim, missile, target) = Room(); target.ThruReflect = missile.NoShieldReflect = true;
        target.Angle = BamAngle.FromDegrees(0);
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Null(sim.CaptureState().ReflectRandomState); Assert.Equal(8, missile.VelocityX.ToDouble());
    }

    [Fact]
    public void ShieldFlagsAndRandomReplaySurviveSaveRestore()
    {
        var (sim, missile, target) = Room(); missile.NoShieldReflect = true;
        var saved = SimSavegame.Write(sim, true); target.ShieldReflect = missile.NoShieldReflect = false;
        SimSavegame.Apply(sim, saved); Assert.True(target.ShieldReflect); Assert.True(missile.NoShieldReflect);
        Assert.False(ActorReflectionActions.ReflectOffActor(missile, target));
        missile.NoShieldReflect = false; var eligible = SimSavegame.Write(sim, true);
        ActorReflectionActions.ReflectOffActor(missile, target); var angle = missile.Angle;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, eligible); ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(angle, missile.Angle); sim.RestoreState(sim.CaptureState()); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData("SHIELDREFLECT")]
    [InlineData("NOSHIELDREFLECT")]
    public void ShieldFlagsAreExposedThroughAcs(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToLowerInvariant(), out var value)); Assert.True(value);
    }

    [Theory]
    [InlineData(180, false)]
    [InlineData(0, true)]
    public void ProjectileMovementReflectsFrontContactsAndExplodesAtRear(double facing, bool explodes)
    {
        var (sim, missile, target) = Room(); target.Invulnerable = true; target.Angle = BamAngle.FromDegrees(facing);
        missile.Tick(); Assert.Equal(explodes, missile.Destroyed);
        Assert.Equal(!explodes, sim.CaptureState().ReflectRandomState.HasValue);
        if (!explodes) Assert.Same(target, missile.Owner);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: 123);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Reflective = target.ShieldReflect = true;
        target.Angle = BamAngle.FromDegrees(180);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(8); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
