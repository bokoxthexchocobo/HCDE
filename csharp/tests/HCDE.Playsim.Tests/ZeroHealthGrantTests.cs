namespace HCDE.Playsim.Tests;

public class ZeroHealthGrantTests
{
    [Theory]
    [InlineData(10, 100, true)]
    [InlineData(100, 100, false)]
    [InlineData(150, 100, false)]
    [InlineData(0, 100, false)]
    [InlineData(-10, 100, false)]
    public void NonplayerZeroGrantReturnsNativeSuccessWithoutChangingHealth(int health, int maximum, bool success)
    {
        var actor = new Actor { Health = health, ResurrectionHealth = maximum };
        Assert.Equal(success, actor.GiveBody(0, 999));
        Assert.Equal(health, actor.Health);
    }

    [Fact]
    public void PlayerZeroGrantFailsEvenBelowMaximum()
    {
        var player = new PlayerPawn { Health = 10 };
        Assert.False(player.GiveBody(0));
        Assert.Equal(10, player.Health);
    }

    [Fact]
    public void AcsNonpositiveHealthGrantStillDoesNothing()
    {
        var actor = new Actor { Health = 10, ResurrectionHealth = 100 };
        AcsPlayerInventory.Give(actor, "Health", 0);
        AcsPlayerInventory.Give(actor, "Health", -100);
        Assert.Equal(10, actor.Health);
    }
}
