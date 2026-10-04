namespace HCDE.Playsim.Tests;

public class NonplayerHealthGrantTests
{
    [Theory]
    [InlineData("Health")]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    [InlineData("MegasphereHealth")]
    public void HealthClassesGrantRequestedAmountToNonplayer(string name)
    {
        var actor = new Actor { Health = 20, ResurrectionHealth = 60 };
        AcsPlayerInventory.Give(actor, name.ToLowerInvariant(), 5);
        Assert.Equal(25, actor.Health);
        AcsPlayerInventory.Give(actor, [name], 0, 1000);
        Assert.Equal(60, actor.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(80)]
    public void GrantDoesNotReviveOrReduceOverCapHealth(int health)
    {
        var actor = new Actor { Health = health, ResurrectionHealth = 60 };
        AcsPlayerInventory.Give(actor, "Health", 1000);
        Assert.Equal(health, actor.Health);
    }

    [Fact]
    public void NonplayerIgnoresExplicitBonusCapAndClampsLargeBaseAmount()
    {
        var actor = new Actor { Health = 1, ResurrectionHealth = 100000 };
        AcsPlayerInventory.Give(actor, "HealthBonus", int.MaxValue);
        Assert.Equal(65537, actor.Health);
    }

    [Fact]
    public void NonpositiveAndNonhealthGrantsLeaveHealthUnchanged()
    {
        var actor = new Actor { Health = 20, ResurrectionHealth = 60 };
        AcsPlayerInventory.Give(actor, "Health", 0);
        AcsPlayerInventory.Give(actor, "Health", -1);
        AcsPlayerInventory.Give(actor, "Clip", 10);
        AcsPlayerInventory.Give(actor, "UnknownHealth", 10);
        Assert.Equal(20, actor.Health);
    }
}
