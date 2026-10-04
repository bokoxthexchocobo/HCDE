namespace HCDE.Playsim.Tests;

public class ArmorAbsorptionTickTests
{
    [Fact]
    public void TickRestoresAbsorptionAllowanceAfterSameTickHits()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 100, maxAbsorb: 5);
        Assert.Equal(5, ActorDamage.Apply(player, 10).ArmorLost);
        Assert.Equal(0, ActorDamage.Apply(player, 10).ArmorLost);
        Assert.Equal(5, player.Inventory.AbsorbCount);
        player.Tick();
        Assert.Equal(0, player.Inventory.AbsorbCount);
        Assert.Equal(5, ActorDamage.Apply(player, 10).ArmorLost);
    }

    [Fact]
    public void DeadPlayerTickAlsoResetsRetainedArmorCounter()
    {
        var player = new PlayerPawn { Health = 0 };
        player.Inventory.AbsorbCount = 7;
        player.Tick();
        Assert.Equal(0, player.Inventory.AbsorbCount);
    }
}
