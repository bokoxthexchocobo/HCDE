using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ReflectionVariantTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MirrorReversesCurrentVelocityAndConsumesAdjustmentDraw(bool deflect)
    {
        var (sim, missile, target) = Room(); target.MirrorReflect = true; target.Deflect = deflect;
        var expectedRandom = NativeStateRandom.Seed(123, 0x4139f7d5u); NativeStateRandom.Next(ref expectedRandom);
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(BamAngle.FromDegrees(210), missile.Angle);
        Assert.Equal(-4, missile.VelocityX.ToDouble()); Assert.Equal(-3, missile.VelocityY.ToDouble());
        Assert.Equal(-2, missile.VelocityZ.ToDouble()); Assert.Equal(expectedRandom, sim.CaptureState().ReflectRandomState);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(123)]
    public void DeflectUsesSignedFortyFiveDegreeAdjustment(int seed)
    {
        var (sim, missile, target) = Room(seed); target.Deflect = true;
        var random = NativeStateRandom.Seed((uint)seed, 0x4139f7d5u);
        var offset = (NativeStateRandom.Next(ref random) & 255) < 128 ? 45 : -45;
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(BamAngle.FromDegrees(180 + offset), missile.Angle);
        Assert.InRange(missile.VelXYToSpeed(), 12.4999, 12.5001); Assert.Equal(-2, missile.VelocityZ.ToDouble());
        Assert.Equal(random, sim.CaptureState().ReflectRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DontReflectPrecedesBothVariants(bool mirror)
    {
        var (sim, missile, target) = Room(); missile.DontReflect = true; target.MirrorReflect = mirror; target.Deflect = !mirror;
        Assert.False(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Null(sim.CaptureState().ReflectRandomState); Assert.Equal(8, missile.VelocityX.ToDouble());
    }

    [Fact]
    public void ReflectorThruFlagBypassesMirrorAndDeflectWithoutRandomDraw()
    {
        var (sim, missile, target) = Room(); target.ThruReflect = target.MirrorReflect = target.Deflect = true;
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(8, missile.VelocityX.ToDouble()); Assert.Equal(BamAngle.FromDegrees(30), missile.Angle);
        Assert.Null(sim.CaptureState().ReflectRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagsAndRandomContinuationReplayAfterRestore(bool mirror)
    {
        var (sim, missile, target) = Room(); target.MirrorReflect = mirror; target.Deflect = !mirror;
        var saved = SimSavegame.Write(sim, true); ActorReflectionActions.ReflectOffActor(missile, target);
        var angle = missile.Angle; var vx = missile.VelocityX; var vy = missile.VelocityY;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        target.MirrorReflect = target.Deflect = false; SimSavegame.Apply(sim, saved);
        Assert.Equal(mirror, target.MirrorReflect); Assert.Equal(!mirror, target.Deflect);
        ActorReflectionActions.ReflectOffActor(missile, target);
        Assert.Equal(angle, missile.Angle); Assert.Equal(vx, missile.VelocityX); Assert.Equal(vy, missile.VelocityY);
        sim.RestoreState(sim.CaptureState()); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData("MIRRORREFLECT")]
    [InlineData("DEFLECT")]
    public void VariantFlagsAreAvailableThroughAcs(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToLowerInvariant(), out var value)); Assert.True(value);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(int seed = 123)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Reflective = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20); missile.Angle = BamAngle.FromDegrees(30);
        missile.VelocityX = Fixed.FromInt(8); missile.VelocityY = Fixed.FromInt(6); missile.VelocityZ = Fixed.FromInt(4);
        return (sim, missile, target);
    }
}
