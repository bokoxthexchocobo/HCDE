using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ScreenSeekerTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(21, true)]
    [InlineData(22, false)]
    [InlineData(339, true)]
    [InlineData(338, false)]
    [InlineData(180, false)]
    public void UsesNativeHistoricalAngularThreshold(double yaw, bool accepted)
    {
        var (_, owner, missile, target) = Room(); owner.Angle = BamAngle.FromDegrees(yaw);
        missile.Angle = BamAngle.FromDegrees(180); missile.ScreenSeeker = true;
        Assert.Equal(accepted, missile.IsOkayToAttack(target));
    }

    [Fact]
    public void AngularThresholdMatchesNativeBamRoundingAtAdjacentValues()
    {
        var (_, owner, missile, target) = Room(); missile.ScreenSeeker = true;
        var threshold = 30 * (256.0 / 360); var raw = BamAngle.FromDegrees(threshold).Raw;
        foreach (var value in new[] { raw - 1, raw, raw + 1 })
        {
            owner.Angle = new BamAngle(value);
            Assert.Equal(owner.Angle.ToDegrees() < threshold, missile.IsOkayToAttack(target));
        }
    }

    [Fact]
    public void UsesOwnerPositionRatherThanMissilePosition()
    {
        var (_, owner, missile, target) = Room(); owner.Angle = default;
        missile.SetXYZ(100, 200, 0); missile.ScreenSeeker = true;
        Assert.True(missile.IsOkayToAttack(target));
        owner.Angle = BamAngle.FromDegrees(90); missile.Angle = BamAngle.FromDegrees(270);
        Assert.False(missile.IsOkayToAttack(target));
    }

    [Fact]
    public void SummonedObserverUsesTracerFacingBeforeProjectileOwner()
    {
        var (sim, owner, missile, target) = Room(); var summoner = sim.AddBot(0, 100);
        missile.SummonedMonster = missile.ScreenSeeker = true; missile.TracerId = summoner.Id;
        owner.Angle = default; summoner.Angle = BamAngle.FromDegrees(90);
        Assert.False(missile.IsOkayToAttack(target));
        summoner.Angle = BamAngle.FromDegrees(-45); Assert.True(missile.IsOkayToAttack(target));
    }

    [Fact]
    public void OrdinaryObserverUsesItsOwnFacingAndFlagClearingRestoresUnrestrictedEligibility()
    {
        var (_, owner, _, target) = Room(); owner.ScreenSeeker = true; owner.Angle = BamAngle.FromDegrees(90);
        Assert.False(owner.IsOkayToAttack(target)); owner.ScreenSeeker = false;
        Assert.True(owner.IsOkayToAttack(target));
    }

    [Fact]
    public void OutOfConeCandidateStillConsumesNativeVisibilityDrawBeforeAngleRejection()
    {
        var (sim, owner, missile, target) = Room(); var (reference, _, _, _) = Room();
        missile.ScreenSeeker = true; owner.Angle = BamAngle.FromDegrees(180); target.Invisible = true;
        reference.NextCheckSightByte(); Assert.False(missile.IsOkayToAttack(target));
        Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SeekerLookUsesScreenSeekerAcquisitionFilter()
    {
        var (sim, owner, missile, target) = Room(); missile.ScreenSeeker = true;
        var rejected = sim.AddBot(0, 100); rejected.Brain = null;
        owner.Angle = default;
        ActorSeekerActions.SeekerMissile(missile, 9999, 9999, ActorSeekerFlags.Look, 256);
        Assert.Equal(target.Id, missile.TracerId);
    }

    [Fact]
    public void FlagAndBehaviorRoundTripWithLegacyDefault()
    {
        var (sim, owner, missile, target) = Room(); owner.Angle = BamAngle.FromDegrees(90);
        var legacy = SimSavegame.Write(sim, true); missile.ScreenSeeker = true;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(329, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        missile.ScreenSeeker = false; SimSavegame.Apply(sim, bytes);
        Assert.True(missile.ScreenSeeker); Assert.False(missile.IsOkayToAttack(target));
        Assert.Equal(checksum, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, legacy); Assert.False(missile.ScreenSeeker); Assert.True(missile.IsOkayToAttack(target));
    }

    [Fact]
    public void FlagChangesChecksum()
    {
        var (sim, _, missile, _) = Room(); sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        missile.ScreenSeeker = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Fact]
    public void AcsSetsQueriesAndClearsFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "SCREENSEEKER", true));
        Assert.True(AcsActorFlags.TryGet(actor, "screenseeker", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "ScreenSeeker", false)); Assert.False(actor.ScreenSeeker);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidSavedFlagIsRejected(int value)
    {
        var (sim, _, missile, _) = Room(); missile.ScreenSeeker = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-screenseeker-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Owner, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(-256, -256, 6, 6), []), Things = [] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        return (sim, owner, missile, target);
    }
}
