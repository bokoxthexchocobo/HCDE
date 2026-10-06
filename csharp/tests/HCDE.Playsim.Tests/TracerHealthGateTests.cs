using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TracerHealthGateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void HuggerFlagsSuppressVerticalHomingButRetainYawAfterSave(bool floor, bool ceiling)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800);
        missile.FloorHugger = floor; missile.CeilingHugger = ceiling;
        missile.VelocityZ = default;
        var bytes = SimSavegame.Write(sim);
        missile.FloorHugger = missile.CeilingHugger = false;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        sim.Tick();
        Assert.Equal(BamAngle.FromDegrees(16.875).Raw, missile.Angle.Raw);
        Assert.Equal(floor || ceiling ? 0 : -0.125, missile.VelocityZ.ToDouble());
        Assert.False(missile.Destroyed);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HealthControlsTrackingIndependentlyOfShootabilityAfterSave(bool dead, bool shootable)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800);
        if (dead) target.Health = 0;
        target.Shootable = shootable;
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        var angle = missile.Angle; var vertical = missile.VelocityZ;
        sim.Tick();
        Assert.Equal(dead ? angle.Raw : BamAngle.FromDegrees(16.875).Raw, missile.Angle.Raw);
        if (dead) Assert.Equal(vertical, missile.VelocityZ);
        else Assert.NotEqual(vertical, missile.VelocityZ);
        Assert.Equal(target.Id, missile.TracerTargetId);
    }

    [Fact]
    public void ZeroSpeedDoesNotTurnOrAdjustVerticalVelocity()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(-200, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800);
        missile.MovementSpeed = default;
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        var angle = missile.Angle;
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        sim.Tick();
        Assert.Equal(angle, missile.Angle); Assert.Equal(default, missile.VelocityZ);
    }
}
