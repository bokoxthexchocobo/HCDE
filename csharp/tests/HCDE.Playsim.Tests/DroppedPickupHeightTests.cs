using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupHeightTests
{
    [Theory]
    [InlineData(0, 0, 56, 28)]
    [InlineData(0, 80, 56, 108)]
    [InlineData(32, 32, 40, 52)]
    [InlineData(-32, -32, 0, -32)]
    [InlineData(0, 10.5, 25.5, 23.25)]
    public void DefaultDoomDropPreservesSourceHeightAndInterpolation(double floor, double z, double height, double expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { FloorHeight = floor, CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single();
        source.Z = Fixed.FromDouble(z); source.Height = Fixed.FromDouble(height);
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(Fixed.FromDouble(expected), drop.Z);
        Assert.Equal(drop.Z, drop.PreviousZ);
        Assert.Equal(expected <= floor, drop.OnGround);
        Assert.False(drop.NoGravity);
        Assert.Equal(source.X, drop.X); Assert.Equal(source.Y, drop.Y);
    }

    [Fact]
    public void AirborneDropFallsAndLandsOnSectorFloor()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(80);
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        source.X = Fixed.FromInt(200);
        var initial = drop.Z;
        sim.Tick(); Assert.True(drop.Z.Raw > initial.Raw);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(Fixed.FromInt(0), drop.Z); Assert.True(drop.OnGround);
    }
}
