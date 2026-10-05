namespace HCDE.Playsim.Tests;

public class DeathTypeNameCaseTests
{
    [Theory]
    [InlineData("fire")]
    [InlineData("FIRE")]
    public void TypedDeathAndAvailabilityUseNativeNameCase(string type)
    {
        var actor = new Actor { Health = 20, DeathState = -1 };
        actor.SetTypedDeath("Fire", 3);
        Assert.True(ActorDamage.Apply(actor, 20, damageType: type).Killed);
        Assert.Equal(3, actor.States.Current);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("EXTREME")]
    public void ReservedTypedDeathNamesAreRejectedRegardlessOfCase(string type) =>
        Assert.Throws<ArgumentException>(() => new Actor().SetTypedDeath(type, 2));

    [Fact]
    public void MixedCaseMassacreBypassesTypedOnlyRestriction()
    {
        var actor = new Actor { Health = 100, DeathState = -1 }; actor.SetTypedDeath("Fire", 2);
        Assert.Equal(20, ActorDamage.Apply(actor, 20, damageType: "mAsSaCrE").HealthLost);
    }
}
