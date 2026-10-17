using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PreciseSeekerTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(0)]
    public void PreciseModeAimsBetweenCentersAtConstantTotalSpeed(int z)
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(z);
        Assert.True(ActorSeekerActions.Seek(missile, 180, 0, precise: true));
        AssertVector(missile, target.Center() - missile.Center(), 100, 10);
    }

    [Fact]
    public void PreciseModeCorrectsPitchEvenWhenHeightIntervalsOverlap()
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(0);
        missile.VelocityZ = Fixed.FromInt(-2);
        ActorSeekerActions.Seek(missile, 180, 0, precise: true);
        Assert.True(missile.VelocityZ.Raw > 0);
    }

    [Fact]
    public void HorizontalDistanceHasNativeOneUnitMinimum()
    {
        var (_, missile, target) = Room(); target.SetXYZ(0.25, 0, 10);
        ActorSeekerActions.Seek(missile, 180, 0, precise: true);
        AssertVector(missile, target.Center() - missile.Center(), 1, 10);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PreciseHuggersFlattenExistingVerticalMotion(bool ceiling)
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(100);
        missile.CeilingHugger = ceiling; missile.FloorHugger = !ceiling; missile.VelocityZ = Fixed.FromInt(4);
        ActorSeekerActions.Seek(missile, 180, 0, precise: true);
        Assert.Equal(0, missile.VelocityZ.Raw); Assert.Equal(10, missile.VelXYToSpeed(), 5);
    }

    [Fact]
    public void CurrentSpeedModePreservesThreeDimensionalMagnitude()
    {
        var (_, missile, _) = Room(); missile.VelocityX = Fixed.FromInt(3); missile.VelocityY = Fixed.FromInt(4);
        missile.VelocityZ = Fixed.FromInt(12);
        ActorSeekerActions.Seek(missile, 180, 0, useCurrentSpeed: true, precise: true);
        Assert.InRange(Math.Abs(missile.VelToSpeed() - 13), 0, 2.0 / 65536);
    }

    [Fact]
    public void PlayerTargetUsesEyeHeightAndBobInsteadOfBodyCenter()
    {
        var (sim, missile, _) = Room(); var player = sim.Players.Single(); player.SetXYZ(100, 0, 0);
        player.ViewHeight = 35; player.ViewBobOffset = 3; player.FloorClip = 5;
        missile.TracerId = player.Id;
        ActorSeekerActions.Seek(missile, 180, 0, precise: true);
        AssertVector(missile, 33 - missile.Center(), 100, 10);
        Assert.Equal(35, player.ViewHeight); Assert.Equal(3, player.ViewBobOffset);
    }

    [Theory]
    [InlineData(0, 41, 0, 8, 33)]
    [InlineData(10, 41, 0, 8, 51)]
    [InlineData(0, 2, -5, 0, 4)]
    [InlineData(0, 300, 0, 0, 252)]
    public void EyeHeightAppliesGroundClipAndNativeBounds(int z, int height, int bob, int clip, int expected)
    {
        var (sim, _, _) = Room(); var player = sim.Players.Single(); player.Z = Fixed.FromInt(z);
        player.ViewHeight = height; player.ViewBobOffset = bob; player.FloorClip = clip;
        Assert.Equal(expected, player.GetViewZ());
    }

    [Fact]
    public void DeadPlayerEyeDoesNotSubtractFloorClip()
    {
        var (sim, _, _) = Room(); var player = sim.Players.Single(); player.Health = 0;
        player.ViewHeight = 20; player.ViewBobOffset = 0; player.FloorClip = 8;
        Assert.Equal(20, player.GetViewZ());
    }

    [Fact]
    public void PreciseSteeringResumesIdenticallyAfterSaveRestore()
    {
        var (sim, missile, target) = Room(); target.Z = Fixed.FromInt(100);
        ActorSeekerActions.Seek(missile, 0, 20, precise: true); var bytes = SimSavegame.Write(sim, true);
        ActorSeekerActions.Seek(missile, 0, 20, true, true);
        var expected = (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ);
        SimSavegame.Apply(sim, bytes); ActorSeekerActions.Seek(missile, 0, 20, true, true);
        Assert.Equal(expected, (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ));
    }

    private static void AssertVector(Actor missile, double vertical, double distance, double speed)
    {
        var scale = speed / Math.Sqrt(distance * distance + vertical * vertical);
        Assert.InRange(Math.Abs(missile.VelXYToSpeed() - distance * scale), 0, 2.0 / 65536);
        Assert.InRange(Math.Abs(missile.VelocityZ.ToDouble() - vertical * scale), 0, 1.0 / 65536);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = 500 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        missile.SetXYZ(0, 0, 0); missile.Angle = default; missile.TracerId = target.Id;
        return (sim, missile, target);
    }
}
