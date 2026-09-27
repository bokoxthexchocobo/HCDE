namespace HCDE.Playsim.Tests;

public class ArmorRoundingTests
{
    [Theory]
    [InlineData(2, 100, 0, 2)]
    [InlineData(3, 100, 1, 2)]
    [InlineData(6, 100, 2, 4)]
    [InlineData(6, 1, 1, 5)]
    public void GreenArmorSavesAnExactThirdLimitedByRemainingArmor(int damage, int armor, int absorbed, int lost)
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = armor;
        player.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var result = ActorDamage.Apply(player, damage);
        Assert.Equal(absorbed, result.ArmorLost);
        Assert.Equal(lost, result.HealthLost);
        Assert.Equal(armor - absorbed, player.Inventory.Armor);
        Assert.Equal(100 - lost, player.Health);
    }
}
