using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupHorizontalReachTests
{
    [Theory]
    [InlineData(15, 15, true)]
    [InlineData(-15, -15, true)]
    [InlineData(20, 0, false)]
    [InlineData(-20, 0, false)]
    [InlineData(0, 20, false)]
    [InlineData(0, -20, false)]
    [InlineData(19.9999847412109375, 0, true)]
    [InlineData(20.0000152587890625, 0, false)]
    public void NativeSquareContactUsesStrictAxisBounds(double x, double y, bool expected)
    {
        var player = new Actor { Radius = Fixed.FromInt(8) };
        var pickup = new Actor { X = Fixed.FromDouble(x), Y = Fixed.FromDouble(y), Radius = Fixed.FromInt(12) };
        Assert.Equal(expected, PickupCatalog.IsWithinHorizontalReach(player, pickup));
    }

    [Theory]
    [InlineData(15, 15, true)]
    [InlineData(20, 0, false)]
    [InlineData(0, -20, false)]
    public void SimulationCollectsDiagonalContactButRejectsExactEdge(double x, double y, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip }],
        });
        var player = sim.Players.Single(); player.Radius = Fixed.FromInt(8);
        var pickup = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        pickup.Radius = Fixed.FromInt(12); pickup.X = Fixed.FromDouble(x); pickup.Y = Fixed.FromDouble(y);
        var before = player.Inventory.Bullets;
        sim.Tick();
        Assert.Equal(expected, pickup.Destroyed);
        Assert.Equal(before + (expected ? 10 : 0), player.Inventory.Bullets);
    }
}
