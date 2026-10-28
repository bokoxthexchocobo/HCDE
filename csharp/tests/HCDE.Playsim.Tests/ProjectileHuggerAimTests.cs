using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileHuggerAimTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 100)]
    [InlineData(false, -100)]
    [InlineData(true, -100)]
    public void MonsterHuggersIgnoreElevationAndShortTargetCorrection(bool ceiling, int z)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(30, 40);
        target.Z = Fixed.FromInt(z); target.Height = Fixed.FromInt(16);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        SetHugger(missile, ceiling); missile.Aim(target);
        Assert.Equal(Fixed.FromDouble(missile.Speed * 0.6), missile.VelocityX);
        Assert.Equal(Fixed.FromDouble(missile.Speed * 0.8), missile.VelocityY);
        Assert.Equal(0, missile.VelocityZ.Raw);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MonsterCoincidentHorizontalAimRemainsZero(bool ceiling)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0); target.Z = Fixed.FromInt(100);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        SetHugger(missile, ceiling); missile.Aim(target);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void PlayerHuggersIgnorePitchAndTargetElevation(bool ceiling, bool aimed)
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = -45;
        player.Angle = BamAngle.FromDegrees(90);
        var target = sim.AddBot(100, 100); target.Z = Fixed.FromInt(200);
        var missile = sim.SpawnProjectile(player, ProjectileKind.Rocket);
        SetHugger(missile, ceiling); missile.Aim(aimed ? target : null);
        Assert.Equal(0, missile.VelocityZ.Raw);
        Assert.InRange(Math.Abs(missile.VelXYToSpeed() - missile.Speed), 0, 2.0 / 65536);
        if (aimed) Assert.InRange(missile.Angle.ToDegrees(), 40, 46);
        else Assert.Equal(90, missile.Angle.ToDegrees(), 1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearingHuggerRestoresVerticalAimAfterSaveRoundTrip(bool ceiling)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(100, 0); target.Z = Fixed.FromInt(100);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        SetHugger(missile, ceiling); missile.Aim(target); var bytes = SimSavegame.Write(sim, true);
        missile.FloorHugger = missile.CeilingHugger = false; missile.Aim(target); Assert.True(missile.VelocityZ.Raw > 0);
        SimSavegame.Apply(sim, bytes); Assert.Equal(ceiling, missile.CeilingHugger); Assert.Equal(!ceiling, missile.FloorHugger);
        missile.Aim(target); Assert.Equal(0, missile.VelocityZ.Raw);
        missile.FloorHugger = missile.CeilingHugger = false; missile.Aim(target); Assert.True(missile.VelocityZ.Raw > 0);
    }

    private static void SetHugger(ProjectileActor missile, bool ceiling)
    { missile.CeilingHugger = ceiling; missile.FloorHugger = !ceiling; }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
