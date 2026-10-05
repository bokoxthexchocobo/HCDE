namespace HCDE.Playsim.Tests;

public class PlayerTelefragArmorTests
{
    [Fact]
    public void PlayerArmorIsConsumedWithoutReducingRawTelefragHealthLoss()
    {
        var player = new PlayerPawn { Health = 2_000_000 };
        player.Inventory.Armor = 200; player.Inventory.ArmorSavePercent = 50;
        var result = ActorDamage.Apply(player, ActorDamage.TelefragDamage);
        Assert.Equal(200, result.ArmorLost); Assert.Equal(1_000_000, result.HealthLost);
        Assert.Equal(1_000_000, player.Health); Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void MonsterArmorStillReducesTelefragHealthLoss()
    {
        var actor = new Actor { Health = 2_000_000, Armor = 200, ArmorSavePercent = 50 };
        var result = ActorDamage.Apply(actor, ActorDamage.TelefragDamage);
        Assert.Equal(200, result.ArmorLost); Assert.Equal(999800, result.HealthLost);
    }

    [Fact]
    public void OrdinaryPlayerDamageStillUsesArmorRemainder()
    {
        var player = new PlayerPawn { Health = 100 };
        player.Inventory.Armor = 200; player.Inventory.ArmorSavePercent = 50;
        var result = ActorDamage.Apply(player, 20);
        Assert.Equal(10, result.ArmorLost); Assert.Equal(10, result.HealthLost);
    }
}
