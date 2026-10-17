using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeekerAcquisitionTests
{
    [Fact]
    public void LookAcquiresAndSteersToSeekableMonster()
    {
        var (_, missile, target) = Room(); target.SetXYZ(192, 320, 100);
        ActorSeekerActions.SeekerMissile(missile, 0, 20, ActorSeekerFlags.Look | ActorSeekerFlags.Precise, 256);
        Assert.Equal(target.Id, missile.TracerId); Assert.Equal(20, missile.Angle.ToDegrees(), 5);
        Assert.True(missile.VelocityZ.Raw > 0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ImpossibleChanceStillConsumesOneDraw(int chance)
    {
        var (sim, missile, _) = Room(); var (reference, _, _) = Room(); reference.NextSeekerMissileByte();
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, chance);
        Assert.Null(missile.TracerId);
        Assert.Equal(reference.CaptureState().SeekerMissileRandomState, sim.CaptureState().SeekerMissileRandomState);
    }

    [Fact]
    public void ChanceComparisonIsStrictAndNotPercentScaled()
    {
        var (sim, missile, _) = Room(); var (reference, _, _) = Room();
        var value = reference.NextSeekerMissileByte();
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, value);
        Assert.Null(missile.TracerId);
        Assert.Equal(reference.CaptureState().SeekerMissileRandomState, sim.CaptureState().SeekerMissileRandomState);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void DistanceArgumentReachesExpectedRing(int distance, bool acquired)
    {
        var (_, missile, target) = Room();
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, 256, distance);
        Assert.Equal(acquired ? target.Id : (uint?)null, missile.TracerId);
    }

    [Fact]
    public void UnseekableTracerClearsWithoutDrawThenReacquiresOnNextCall()
    {
        var (sim, missile, target) = Room(); var rejected = sim.AddBot(210, 210); rejected.CantSeek = true;
        missile.TracerId = rejected.Id;
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, 256);
        Assert.Null(missile.TracerId); Assert.Null(sim.CaptureState().SeekerMissileRandomState);
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, 256);
        Assert.Equal(target.Id, missile.TracerId); Assert.NotNull(sim.CaptureState().SeekerMissileRandomState);
    }

    [Fact]
    public void NoLookNeverConsumesAcquisitionRandomness()
    {
        var (sim, missile, _) = Room();
        ActorSeekerActions.SeekerMissile(missile, 10, 20);
        Assert.Null(sim.CaptureState().SeekerMissileRandomState); Assert.Null(missile.TracerId);
    }

    [Fact]
    public void NamedStreamMatchesNativeSeedAndIsIndependentOfOtherDraws()
    {
        var (sim, _, _) = Room(); var state = NativeStateRandom.Seed(0, 0x85890f4bu);
        for (var i = 0; i < 64; i++)
        {
            sim.NextReflectByte(); sim.NextCheckSightByte(); sim.NextCombatRandom();
            Assert.Equal((byte)(NativeStateRandom.Next(ref state) & 255), sim.NextSeekerMissileByte());
        }
        Assert.Equal(state, sim.CaptureState().SeekerMissileRandomState);
    }

    [Fact]
    public void RandomStateChangesChecksumAndRoundTripsWithLegacyReset()
    {
        var (sim, _, _) = Room(); var legacy = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        sim.NextSeekerMissileByte(); sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(327, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 32).Select(_ => sim.NextSeekerMissileByte()).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Enumerable.Range(0, 32).Select(_ => sim.NextSeekerMissileByte()).ToArray());
        SimSavegame.Apply(sim, legacy); Assert.Null(sim.CaptureState().SeekerMissileRandomState);
        var (reference, _, _) = Room(); Assert.Equal(reference.NextSeekerMissileByte(), sim.NextSeekerMissileByte());
    }

    [Fact]
    public void AcquisitionSequenceResumesAfterSaveRestore()
    {
        var (sim, missile, _) = Room();
        ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, 50);
        var bytes = SimSavegame.Write(sim, true);
        uint? Next()
        {
            missile.TracerId = null;
            ActorSeekerActions.SeekerMissile(missile, 10, 20, ActorSeekerFlags.Look, 50);
            return missile.TracerId;
        }
        var expected = Enumerable.Range(0, 64).Select(_ => Next()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, Enumerable.Range(0, 64).Select(_ => Next()).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void InvalidArchiveTrailerIsRejected(int corrupt)
    {
        var (sim, _, _) = Room(); sim.NextSeekerMissileByte(); var bytes = SimSavegame.Write(sim, true);
        if (corrupt == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        else BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 327);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(corrupt == 0 ? "save-seekerrandom-size" : "save-seekerrandom-header", error);
    }

    [Fact]
    public void RandomStateRequiresGeometryBeforeRestoreMutatesSimulation()
    {
        var (sim, _, _) = Room(); var state = sim.CaptureState(); state.SeekerMissileRandomState = 42; state.GeometryHealth = null;
        var before = sim.CaptureState().SeekerMissileRandomState;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, sim.CaptureState().SeekerMissileRandomState);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 5, 5), []), Things = [] });
        var owner = sim.AddBot(192, 192); var target = sim.AddBot(320, 192);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall); missile.SetXYZ(192, 192, 0); missile.Angle = default;
        return (sim, missile, target);
    }
}
