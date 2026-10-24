using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TracerTurnParameterTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(90, 45)]
    [InlineData(-5, 355)]
    public void DirectTracerActionUsesSignedTurnParameterAndNativeOvershootRule(double turn, double expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800);
        missile.VelocityZ = default;
        var bytes = SimSavegame.Write(sim);
        missile.Trace(turn);
        Assert.InRange(missile.Angle.ToDegrees(), expected - 1, expected + 1);
        Assert.Equal(missile.Speed, missile.VelXYToSpeed(), 4);
        Assert.Equal(-0.125, missile.VelocityZ.ToDouble());
        var angle = missile.Angle; var velocity = (missile.VelocityX, missile.VelocityY, missile.VelocityZ);
        SimSavegame.Apply(sim, bytes);
        missile.Trace(turn);
        Assert.Equal(angle, missile.Angle);
        Assert.Equal(velocity, (missile.VelocityX, missile.VelocityY, missile.VelocityZ));
    }
}
