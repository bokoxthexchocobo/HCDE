namespace HCDE.Playsim.Tests;

public class AcsDamagePowerInventoryTests
{
    [Theory]
    [InlineData("pOwErDaMaGe", false)]
    [InlineData("pOwErPrOtEcTiOn", true)]
    public void GrantQueryTakeControlDamage(string name, bool protection)
    {
        var player = new PlayerPawn { Health = 100 };
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name, 5);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(875, AcsActorPowerups.RemainingTics(player, name));
        var target = protection ? (Actor)player : new Actor { Health = 100 };
        Assert.Equal(protection ? 5 : 80, ActorDamage.Apply(target, 20, protection ? null : player).HealthLost);
        AcsPlayerInventory.Take(player, name, 1);
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, name));
        target.Health = 100;
        Assert.Equal(20, ActorDamage.Apply(target, 20, protection ? null : player).HealthLost);
    }

    [Theory]
    [InlineData("PowerDamage")]
    [InlineData("PowerProtection")]
    public void NonpositiveGiveAndTakeDoNotMutate(string name)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, name, 0);
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        AcsPlayerInventory.Give(player, name, 1);
        AcsPlayerInventory.Take(player, name, 0);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        AcsPlayerInventory.Take(player, name, -1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
    }
}
