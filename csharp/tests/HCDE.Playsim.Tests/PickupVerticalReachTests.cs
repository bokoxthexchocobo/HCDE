using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupVerticalReachTests
{
    [Theory]
    [InlineData(56, 16, true)]
    [InlineData(56.0000152587890625, 16, false)]
    [InlineData(-32, 16, true)]
    [InlineData(-32.0000152587890625, 16, false)]
    [InlineData(-64, 64, true)]
    [InlineData(-64.0000152587890625, 64, false)]
    [InlineData(0, 0, true)]
    public void ReachUsesNativeHeightBoundsWithoutRounding(double delta, double pickupHeight, bool expected)
    {
        var toucher = new Actor { Z = Fixed.FromInt(100), Height = Fixed.FromInt(56) };
        var pickup = new Actor { Z = Fixed.FromDouble(100 + delta), Height = Fixed.FromDouble(pickupHeight) };
        Assert.Equal(expected, PickupCatalog.IsWithinVerticalReach(toucher, pickup));
    }

    [Theory]
    [InlineData(100, false)]
    [InlineData(56, true)]
    [InlineData(20, true)]
    public void SimulationOnlyCollectsHorizontallyOverlappingPickupWithinVerticalReach(double z, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip }],
        });
        var player = sim.Players.Single();
        player.FullHeight = 56; player.Height = Fixed.FromInt(56);
        var pickup = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        pickup.Z = Fixed.FromDouble(z); pickup.NoGravity = true; pickup.OnGround = false;
        var before = player.Inventory.Bullets;
        sim.Tick();
        Assert.True(expected == pickup.Destroyed, $"Player Z={player.Z.ToDouble()} Height={player.Height.ToDouble()}, pickup Z={pickup.Z.ToDouble()}");
        Assert.Equal(before + (expected ? 10 : 0), player.Inventory.Bullets);
    }
}
