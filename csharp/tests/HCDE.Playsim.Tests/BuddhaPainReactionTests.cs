namespace HCDE.Playsim.Tests;

public class BuddhaPainReactionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PositiveDamageStillTriggersPainAtOneHealth(bool playerTarget, bool powerBuddha)
    {
        Actor target = playerTarget ? new PlayerPawn() : new Actor();
        target.Health = 1; target.PainChance = 256; target.PainThreshold = 7;
        target.Buddha = !powerBuddha;
        if (powerBuddha) ((PlayerPawn)target).PowerBuddhaTics = 10;
        var result = ActorDamage.Apply(target, 7);
        Assert.Equal(0, result.HealthLost); Assert.False(result.Killed);
        Assert.Equal(1, target.Health); Assert.Equal(target.PainState, target.States.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ThresholdAndNoPainStillSuppressProtectedPain(bool noPain)
    {
        var target = new Actor { Health = 1, Buddha = true, PainChance = 256,
            PainThreshold = noPain ? 0 : 8, NoPain = noPain, ReactionTime = 12 };
        ActorDamage.Apply(target, 7);
        Assert.Equal(1, target.Health); Assert.Equal(target.SpawnState, target.States.Current);
        Assert.Equal(0, target.ReactionTime);
    }
}
