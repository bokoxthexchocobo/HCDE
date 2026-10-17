using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NormalReflectionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(123)]
    public void ReflectionUsesNamedStreamAngleJitterAndHalfClassSpeed(int seed)
    {
        var (sim, missile, target) = Room(seed); var expected = NativeStateRandom.Seed((uint)seed, 0x4139f7d5u);
        var jitter = (NativeStateRandom.Next(ref expected) & 255) % 16 - 8.0;
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(BamAngle.FromDegrees(180 + jitter), missile.Angle);
        Assert.InRange(missile.VelXYToSpeed(), 12.4999, 12.5001); Assert.Equal(-2, missile.VelocityZ.ToDouble());
        Assert.Same(target, missile.Owner); Assert.Equal(expected, sim.CaptureState().ReflectRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DontReflectStopsBeforeRandomConsumption(bool optOut)
    {
        var (sim, missile, target) = Room(); var owner = missile.Owner; missile.DontReflect = optOut;
        Assert.Equal(!optOut, ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(!optOut, sim.CaptureState().ReflectRandomState.HasValue);
        Assert.Same(optOut ? owner : target, missile.Owner);
    }

    [Fact]
    public void ReflectorThruFlagBypassesDontReflectAndRandomness()
    {
        var (sim, missile, target) = Room(); missile.DontReflect = target.ThruReflect = true;
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.Equal(8, missile.VelocityX.ToDouble()); Assert.Equal(4, missile.VelocityZ.ToDouble());
        Assert.Null(sim.CaptureState().ReflectRandomState);
    }

    [Fact]
    public void ReflectionRandomStreamIsIndependentOfDamageStreams()
    {
        var (sim, missile, target) = Room(); ActorReflectionActions.ReflectOffActor(missile, target);
        var state = sim.CaptureState(); Assert.Null(state.MissileDamageRandomState); Assert.Null(state.PoisonDamageRandomState);
        sim.NextMissileDamageByte(); sim.NextPoisonDamageByte();
        Assert.Equal(state.ReflectRandomState, sim.CaptureState().ReflectRandomState);
    }

    [Fact]
    public void RandomSaveReplayAndLegacyDefaultPreserveReflectionSequence()
    {
        var (sim, missile, target) = Room(); var older = SimSavegame.Write(sim, true);
        ActorReflectionActions.ReflectOffActor(missile, target); var firstAngle = missile.Angle;
        var saved = SimSavegame.Write(sim, true); Assert.Equal(324, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        ActorReflectionActions.ReflectOffActor(missile, target); var nextAngle = missile.Angle;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
        ActorReflectionActions.ReflectOffActor(missile, target); Assert.Equal(nextAngle, missile.Angle);
        sim.RestoreState(sim.CaptureState()); Assert.Equal(checksum, sim.Checksum);
        SimSavegame.Apply(sim, older); Assert.Null(sim.CaptureState().ReflectRandomState);
        ActorReflectionActions.ReflectOffActor(missile, target); Assert.Equal(firstAngle, missile.Angle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidRandomTrailerIsRejected(bool header)
    {
        var (sim, missile, target) = Room(); ActorReflectionActions.ReflectOffActor(missile, target);
        var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(header ? bytes.Length - 16 : bytes.Length - 4), header ? 324 : 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(header ? "save-reflectrandom-header" : "save-reflectrandom-size", error);
    }

    [Fact]
    public void DontReflectFlagIsAvailableThroughAcsAndSaved()
    {
        var (sim, missile, _) = Room(); Assert.True(AcsActorFlags.TrySet(missile, "DONTREFLECT", true));
        var saved = SimSavegame.Write(sim, true); missile.DontReflect = false; SimSavegame.Apply(sim, saved);
        Assert.True(AcsActorFlags.TryGet(missile, "dontreflect", out var value)); Assert.True(value);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(int seed = 123)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true; target.Reflective = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(8); missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(4);
        missile.DamageExpression = _ => 7;
        return (sim, missile, target);
    }
}
