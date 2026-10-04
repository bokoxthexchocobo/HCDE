namespace HCDE.Playsim.Tests;

public class HealthInventoryUseTests
{
    [Theory]
    [InlineData("Health")]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    public void HealthUseDoesNotHealWithoutPersistentInventory(string name)
    {
        var player = new PlayerPawn { Health = 50 };
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        Assert.Equal(50, player.Health);
        AcsPlayerInventory.Give(player, name, 5);
        Assert.Equal(55, player.Health);
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        Assert.Equal(55, player.Health);
    }
}
