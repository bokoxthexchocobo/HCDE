namespace HCDE.Playsim.Tests;

public class PowerupDefinitionDurationTests
{
    [Theory]
    [InlineData(-25, 875)]
    [InlineData(-60, 2100)]
    [InlineData(-1, 35)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void NativeDefinitionUnits(int duration, int expected)
    {
        Assert.Equal(expected, PowerupDuration.FromDefinition(duration));
    }

    [Theory]
    [InlineData(ManagedPowerupKind.Damage, "PowerDamage")]
    [InlineData(ManagedPowerupKind.Protection, "PowerProtection")]
    [InlineData(ManagedPowerupKind.Buddha, "PowerBuddha")]
    public void DefinitionGrantUsesUnitsBeforeRefresh(ManagedPowerupKind kind, string name)
    {
        var player = new PlayerPawn();
        player.GivePowerup(kind, -10);
        Assert.Equal(350, AcsActorPowerups.RemainingTics(player, name));
        player.GivePowerup(kind, -1, additiveTime: true);
        Assert.Equal(385, AcsActorPowerups.RemainingTics(player, name));
        player.GivePowerup(kind, -20, alwaysPickup: true);
        Assert.Equal(700, AcsActorPowerups.RemainingTics(player, name));
        player.GivePowerup(kind, 0, additiveTime: true);
        Assert.Equal(700, AcsActorPowerups.RemainingTics(player, name));
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-61356676)]
    public void UnrepresentableDurationDoesNotMutate(int duration)
    {
        var player = new PlayerPawn { PowerDamageTics = 100 };
        Assert.Throws<OverflowException>(() => player.GivePowerup(ManagedPowerupKind.Damage, duration));
        Assert.Equal(100, player.PowerDamageTics);
    }

    [Fact]
    public void UnknownPowerKindIsRejected()
    {
        var player = new PlayerPawn();
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerup((ManagedPowerupKind)99, -25));
        Assert.Equal(0, player.PowerDamageTics);
        Assert.Equal(0, player.PowerProtectionTics);
        Assert.Equal(0, player.PowerBuddhaTics);
    }
}
