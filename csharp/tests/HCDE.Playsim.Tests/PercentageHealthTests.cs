namespace HCDE.Playsim.Tests;

public class PercentageHealthTests
{
    [Theory]
    [InlineData(-50, 200, 100)]
    [InlineData(-150, 200, 300)]
    [InlineData(-33, 101, 33)]
    [InlineData(int.MinValue, 100, 65536)]
    public void PlayerPercentageUsesMaximumAndTruncates(int amount, int maximum, int expected)
    {
        var player = new PlayerPawn { Health = 1, MaxHealth = maximum };
        Assert.True(player.GiveBody(amount));
        Assert.Equal(expected, player.Health);
        Assert.False(player.GiveBody(amount));
    }

    [Fact]
    public void ExplicitPlayerMaximumOverridesDefault()
    {
        var player = new PlayerPawn { Health = 10, MaxHealth = 200 };
        Assert.True(player.GiveBody(-50, 300));
        Assert.Equal(150, player.Health);
    }

    [Fact]
    public void NonplayerIgnoresExplicitMaximum()
    {
        var actor = new Actor { Health = 10, ResurrectionHealth = 200 };
        Assert.True(actor.GiveBody(-150, 1000));
        Assert.Equal(300, actor.Health);
    }

    [Fact]
    public void PercentageDoesNotReviveDeadActorOrReduceHealth()
    {
        var dead = new PlayerPawn { Health = 0 };
        Assert.False(dead.GiveBody(-100));
        var alive = new PlayerPawn { Health = 150 };
        Assert.False(alive.GiveBody(-100));
        Assert.Equal(150, alive.Health);
    }
}
