namespace HCDE.Playsim.Tests;

public class ActorMaxHealthMethodTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BaseMethodReturnsNativeDefault(bool upgrades)
    {
        Assert.Equal(100, new Actor().GetMaxHealth(upgrades));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(150, 150)]
    public void PlayerHealingUsesResolvedMaximum(int configured, int expected)
    {
        var player = new PlayerPawn { MaxHealth = configured, Health = 80 };
        Assert.Equal(expected, player.GetMaxHealth(true));
        Assert.True(player.GiveBody(200)); Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void ExplicitHealingMaximumStillTakesPrecedence()
    {
        var player = new PlayerPawn { MaxHealth = 150, Health = 80 };
        Assert.True(player.GiveBody(200, 120)); Assert.Equal(120, player.Health);
    }
}
