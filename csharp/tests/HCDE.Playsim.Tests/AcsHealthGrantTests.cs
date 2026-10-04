namespace HCDE.Playsim.Tests;

public class AcsHealthGrantTests
{
    [Theory]
    [InlineData(50, 10, 60)]
    [InlineData(90, 50, 100)]
    [InlineData(50, int.MaxValue, 100)]
    [InlineData(100, 10, 100)]
    [InlineData(150, 10, 150)]
    [InlineData(0, 50, 0)]
    [InlineData(-10, 50, -10)]
    public void HealthGrantUsesNormalLimitAndDoesNotRevive(int health, int amount, int expected)
    {
        var player = new PlayerPawn { Health = health };
        AcsPlayerInventory.Give(player, "Health", amount);
        Assert.Equal(expected, player.Health);
    }
}
