namespace HCDE.Playsim.Tests;

public class ConsumedPickupMaximumQueryTests
{
    [Theory]
    [InlineData("healthbonus", 200)]
    [InlineData("Soulsphere", 200)]
    [InlineData("MegasphereHealth", 200)]
    [InlineData("MegaSphere", 1)]
    [InlineData("Stimpack", 0)]
    [InlineData("Medikit", 0)]
    public void MaximumUsesClassDefaultsForPlayerAndNonplayer(string type, int maximum)
    {
        Actor[] actors = [new PlayerPawn { Health = 250, MaxHealth = 300 }, new Actor { Health = 250 }];
        foreach (var actor in actors)
        {
            Assert.Equal(maximum, AcsPlayerInventory.Count(actor, type, true));
            Assert.Equal(0, AcsPlayerInventory.Count(actor, type, false));
            Assert.Equal(maximum, AcsPlayerInventory.Count(actor, [type], 0, true));
        }
    }

    [Theory]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    [InlineData("MegasphereHealth")]
    [InlineData("Megasphere")]
    public void MissingActivatorCannotQueryClassMaximum(string type)
    {
        Assert.Equal(0, AcsPlayerInventory.Count(null, type, true));
        Assert.Equal(0, AcsPlayerInventory.Count(null, [type], 0, true));
    }

    [Fact]
    public void GrantedPickupIsConsumedButItsClassMaximumRemainsAvailable()
    {
        var player = new PlayerPawn { Health = 10 };
        AcsPlayerInventory.Give(player, "Soulsphere", 25);
        Assert.Equal(35, player.Health);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "Soulsphere", false));
        Assert.Equal(200, AcsPlayerInventory.Count(player, "Soulsphere", true));
        Assert.Equal(0, AcsPlayerInventory.Count(player, ["Soulsphere"], -1, true));
        Assert.Equal(0, AcsPlayerInventory.Count(player, "UnknownHealthPickup", true));
    }
}
