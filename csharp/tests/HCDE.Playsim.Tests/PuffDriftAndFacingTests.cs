using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffDriftAndFacingTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(90, false)]
    [InlineData(180, false)]
    [InlineData(270, false)]
    [InlineData(0, true)]
    [InlineData(90, true)]
    [InlineData(180, true)]
    [InlineData(270, true)]
    public void SpawnedPuffFacesOppositeShotAndDriftsUp(double yaw, bool negative)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); var angle = BamAngle.FromDegrees(yaw);
        var radians = yaw * Math.PI / 180; var sign = negative ? -1 : 1;
        var target = sim.AddBot(sign * 64 * Math.Cos(radians), sign * 64 * Math.Sin(radians));
        var health = target.Health;
        Assert.Equal(1, AcsLineAttack.Attack(sim, source, unchecked((int)angle.Raw), 0, 7, "None",
            negative ? -128 : 128, puffTid: 42, absoluteAngles: true));
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        Assert.Equal(unchecked(angle.Raw + 0x80000000u), puff.Angle.Raw);
        Assert.Equal(1, puff.VelocityZ.ToDouble()); Assert.True(puff.NoGravity);
        Assert.False(puff.Solid); Assert.False(puff.Shootable); Assert.Equal(42, puff.ThingId);
        var x = puff.X; var y = puff.Y; var z = puff.Z;
        puff.Tick();
        Assert.Equal(x, puff.X); Assert.Equal(y, puff.Y); Assert.Equal(z.ToDouble() + 1, puff.Z.ToDouble());
        Assert.Equal(1, puff.VelocityZ.ToDouble()); Assert.Equal(health - 7, target.Health);
    }
}
