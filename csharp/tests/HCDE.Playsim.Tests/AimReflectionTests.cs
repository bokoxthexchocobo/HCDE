using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AimReflectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AimUsesTargetCenterFullSpeedAndOverridesMirror(bool mirror)
    {
        var (sim, missile, target) = Room(); var owner = Assert.IsAssignableFrom<Actor>(missile.Owner);
        owner.X = Fixed.FromInt(3); owner.Y = Fixed.FromInt(4); owner.Z = Fixed.FromInt(4);
        missile.MovementSpeed = Fixed.FromInt(26); missile.Ambush = true; missile.PitchDegrees = 17;
        target.MirrorReflect = target.Deflect = mirror;
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(6, missile.VelocityX.ToDouble()); Assert.Equal(8, missile.VelocityY.ToDouble()); Assert.Equal(24, missile.VelocityZ.ToDouble());
        Assert.Equal(BamAngle.FromDegrees(Math.Atan2(4, 3) * 180 / Math.PI), missile.Angle);
        Assert.False(missile.Ambush); Assert.Equal(17, missile.PitchDegrees); Assert.NotNull(sim.CaptureState().ReflectRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TargetSelectionPrefersMissileOwnerThenReflectorTarget(bool removeOwner)
    {
        var (sim, missile, target) = Room(); var fallback = sim.AddBot(0, 100); fallback.Brain = null;
        target.SetTargetPointer(fallback);
        if (removeOwner) missile.SetTargetPointer(null);
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(BamAngle.FromDegrees(removeOwner ? 90 : 180), missile.Angle);
        Assert.Same(target, missile.Owner);
    }

    [Fact]
    public void MissingTargetsRetainMirrorFallbackAndRandomDraw()
    {
        var (sim, missile, target) = Room(); missile.SetTargetPointer(null); target.MirrorReflect = true;
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(-4, missile.VelocityX.ToDouble()); Assert.Equal(-3, missile.VelocityY.ToDouble());
        Assert.Equal(-2, missile.VelocityZ.ToDouble()); Assert.NotNull(sim.CaptureState().ReflectRandomState);
    }

    [Fact]
    public void ShieldRejectionStillPrecedesAimAndRandomDraw()
    {
        var (sim, missile, target) = Room(); target.ShieldReflect = true; target.Angle = BamAngle.FromDegrees(0);
        Assert.False(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Null(sim.CaptureState().ReflectRandomState); Assert.Equal(8, missile.VelocityX.ToDouble());
    }

    [Fact]
    public void ReflectorThruFlagSkipsAimingAndPreservesAmbush()
    {
        var (sim, missile, target) = Room(); target.ThruReflect = true; missile.Ambush = true;
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.True(missile.Ambush); Assert.Equal(8, missile.VelocityX.ToDouble()); Assert.Equal(4, missile.VelocityZ.ToDouble());
        Assert.Null(sim.CaptureState().ReflectRandomState);
    }

    [Fact]
    public void CoincidentTargetCenterProducesZeroVector()
    {
        var (_, missile, target) = Room(); var owner = Assert.IsAssignableFrom<Actor>(missile.Owner);
        owner.X = owner.Y = default; owner.Z = Fixed.FromInt(-8);
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
    }

    [Fact]
    public void AimFlagAndRandomReplaySurviveRestore()
    {
        var (sim, missile, target) = Room(); var saved = SimSavegame.Write(sim, true);
        ActorReflectionActions.ReflectOffActor(missile, target);
        var vx = missile.VelocityX; var vz = missile.VelocityZ;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        target.AimReflect = false; SimSavegame.Apply(sim, saved); Assert.True(target.AimReflect);
        ActorReflectionActions.ReflectOffActor(missile, target); Assert.Equal(vx, missile.VelocityX); Assert.Equal(vz, missile.VelocityZ);
        sim.RestoreState(sim.CaptureState()); Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void AcsExposesAimReflectFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "AIMREFLECT", true));
        Assert.True(AcsActorFlags.TryGet(actor, "aimreflect", out var enabled)); Assert.True(enabled);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] }, rngSeed: 123);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Reflective = target.AimReflect = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(8); missile.VelocityY = Fixed.FromInt(6); missile.VelocityZ = Fixed.FromInt(4);
        return (sim, missile, target);
    }
}
