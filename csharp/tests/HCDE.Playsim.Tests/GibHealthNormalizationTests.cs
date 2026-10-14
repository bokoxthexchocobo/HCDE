namespace HCDE.Playsim.Tests;

public class GibHealthNormalizationTests
{
    [Theory]
    [InlineData(100, -100)]
    [InlineData(-100, -100)]
    [InlineData(0, 0)]
    [InlineData(int.MaxValue, -int.MaxValue)]
    public void ExplicitThresholdUsesNegativeMagnitude(int value, int expected)
    {
        var actor = new Actor { GibHealth = value };
        Assert.Equal(expected, actor.GetGibHealth()); Assert.Equal(value, actor.GibHealth);
    }

    [Theory]
    [InlineData(100, 101, ActorStateMachine.Death)]
    [InlineData(-100, 101, ActorStateMachine.Death)]
    [InlineData(100, 200, ActorStateMachine.Death)]
    [InlineData(-100, 200, ActorStateMachine.Death)]
    [InlineData(100, 201, ActorStateMachine.Corpse)]
    [InlineData(-100, 201, ActorStateMachine.Corpse)]
    public void ExtremeDeathRequiresHealthStrictlyBelowNormalizedThreshold(int threshold, int damage, int state)
    {
        var actor = new Actor { GibHealth = threshold, ExtremeDeathState = ActorStateMachine.Corpse };
        ActorDamage.Apply(actor, damage); Assert.Equal(state, actor.States.Current);
    }

    [Fact]
    public void ForcedExtremeDeathClampsBelowNormalizedPositiveThreshold()
    {
        var actor = new Actor { GibHealth = 100, ExtremeDeathState = ActorStateMachine.Corpse };
        ActorDamage.Apply(actor, 100, damageType: "Extreme");
        Assert.Equal(-101, actor.Health); Assert.Equal(ActorStateMachine.Corpse, actor.States.Current);
    }
}
