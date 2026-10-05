namespace HCDE.Playsim.Tests;

public class SpecialDeathClassificationTests
{
    [Fact]
    public void ExtremeOnlyDeathCountsAsSpecialAndFiltersOtherTypes()
    {
        var actor = new Actor { Health = 100, DeathState = -1, ExtremeDeathState = 2 };
        Assert.True(actor.HasSpecialDeathStates());
        Assert.Equal(0, ActorDamage.Apply(actor, 20, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(actor, 20, damageType: "Extreme").HealthLost);
    }

    [Fact]
    public void NestedExtremeOnlyLabelDoesNotCountAsDirectDeathChild()
    {
        var actor = new Actor { Health = 100, DeathState = -1 };
        actor.SetTypedDeath("Fire", 2, extreme: true);
        Assert.False(actor.HasSpecialDeathStates());
        Assert.Equal(20, ActorDamage.Apply(actor, 20, damageType: "Normal").HealthLost);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(99, false)]
    [InlineData(2, true)]
    public void OnlyAvailableDirectTypedStatesCount(int state, bool expected)
    {
        var actor = new Actor { DeathState = -1 }; actor.SetTypedDeath("Fire", state);
        Assert.Equal(expected, actor.HasSpecialDeathStates());
    }
}
