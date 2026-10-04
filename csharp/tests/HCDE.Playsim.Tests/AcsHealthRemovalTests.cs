namespace HCDE.Playsim.Tests;

public class AcsHealthRemovalTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(0)]
    [InlineData(-10)]
    public void TakingHealthDoesNotChangeActorHealth(int health)
    {
        var player = new PlayerPawn { Health = health };
        AcsPlayerInventory.Take(player, "Health", int.MaxValue);
        Assert.Equal(health, player.Health);
        Assert.Equal(health, AcsPlayerInventory.Count(player, "Health", false));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(0)]
    public void HealthQueryAliasIsNotDroppableInventory(int health)
    {
        var player = new PlayerPawn { Health = health };
        Assert.False(AcsPlayerInventory.Drop(player, "health"));
        Assert.Equal(health, player.Health);
    }
}
