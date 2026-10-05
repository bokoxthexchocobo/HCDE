namespace HCDE.Playsim.Tests;

public class PlayerDamagePowerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GrantRefreshAndExpirationControlGameplayDamage(bool protection)
    {
        var player = new PlayerPawn { Health = 100 };
        if (protection) player.GivePowerProtection(); else player.GivePowerDamage();
        var name = protection ? "PowerProtection" : "PowerDamage";
        Assert.Equal(875, AcsActorPowerups.RemainingTics(player, name));
        player.Tick();
        if (protection) player.GivePowerProtection(); else player.GivePowerDamage();
        Assert.Equal(874, AcsActorPowerups.RemainingTics(player, name));
        if (protection) player.PowerProtectionTics = 128; else player.PowerDamageTics = 128;
        if (protection) player.GivePowerProtection(); else player.GivePowerDamage();
        Assert.Equal(875, AcsActorPowerups.RemainingTics(player, name));
        var target = protection ? (Actor)player : new Actor { Health = 100 };
        Assert.Equal(protection ? 5 : 80, ActorDamage.Apply(target, 20, protection ? null : player).HealthLost);
        if (protection) player.PowerProtectionTics = 1; else player.PowerDamageTics = 1;
        player.Tick();
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, name));
        target.Health = 100;
        Assert.Equal(20, ActorDamage.Apply(target, 20, protection ? null : player).HealthLost);
    }
}
