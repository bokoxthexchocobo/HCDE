using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargeIdleTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(-20)]
    public void NoHorizontalMovementStopsChargeAndClearsVerticalVelocity(int vertical)
    {
        var sim = Room();
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        soul.VelocityX = soul.VelocityY = default;
        soul.VelocityZ = Fixed.FromInt(vertical);
        var z = soul.Z;
        sim.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(MonsterMode.Chase, soul.Brain.Mode);
        Assert.Equal(default, soul.VelocityX);
        Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ);
        Assert.Equal(z, soul.Z);
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Fact]
    public void TinyNonzeroHorizontalMovementKeepsChargeActive()
    {
        var sim = Room();
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        soul.VelocityX = Fixed.FromDouble(1.0 / 65536);
        soul.VelocityY = soul.VelocityZ = default;
        sim.Tick();
        Assert.True(soul.Brain.Charging);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006, Z = 100 }],
    });
}
