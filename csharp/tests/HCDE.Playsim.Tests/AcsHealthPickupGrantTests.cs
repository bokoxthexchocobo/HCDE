namespace HCDE.Playsim.Tests;

public class AcsHealthPickupGrantTests
{
    [Theory]
    [InlineData("Stimpack", 50, 3, 53)]
    [InlineData("Medikit", 50, 3, 53)]
    [InlineData("HealthBonus", 150, 10, 160)]
    [InlineData("Soulsphere", 150, 10, 160)]
    [InlineData("Stimpack", 90, int.MaxValue, 100)]
    [InlineData("HealthBonus", 150, int.MaxValue, 200)]
    public void GrantUsesRequestedAmountAndClassCap(string name, int health, int amount, int expected)
    {
        var player = new PlayerPawn { Health = health };
        AcsPlayerInventory.Give(player, name, amount);
        Assert.Equal(expected, player.Health);
    }

    [Theory]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    public void GrantDoesNotReviveDeadPlayer(string name)
    {
        var player = new PlayerPawn { Health = 0 };
        AcsPlayerInventory.Give(player, name, 50);
        Assert.Equal(0, player.Health);
    }
}
