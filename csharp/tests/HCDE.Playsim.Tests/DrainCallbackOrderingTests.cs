namespace HCDE.Playsim.Tests;

public class DrainCallbackOrderingTests
{
    private sealed class ObservingPlayer : PlayerPawn
    {
        public int ObservedHealth { get; private set; }
        public int ObservedState { get; private set; }
        public int ObservedDeaths { get; private set; }
        public int Calls { get; private set; }
        public override int OnDrain(Actor target, int amount, string? damageType)
        {
            Calls++;
            ObservedHealth = target.Health;
            ObservedState = target.States.Current;
            ObservedDeaths = target.DeathCount;
            return amount;
        }
    }

    [Theory]
    [InlineData(false, -10)]
    [InlineData(true, 1)]
    public void BuddhaSurvivalUsesNativePlayerAndMonsterStages(bool playerTarget, int observedHealth)
    {
        var source = new ObservingPlayer { Health = 50, DrainStrength = 1 };
        Actor target = playerTarget ? new PlayerPawn() : new Actor();
        target.Health = 10;
        target.Buddha = true;
        ActorDamage.Apply(target, 20, source);
        Assert.Equal(observedHealth, source.ObservedHealth);
        Assert.Equal(1, source.Calls);
        Assert.Equal(0, source.ObservedDeaths);
        Assert.Equal(1, target.Health);
        Assert.Equal(0, target.DeathCount);
        Assert.Equal(70, source.Health);
    }

    [Theory]
    [InlineData(20, 0, -10, 2)]
    [InlineData(5, 0, 5, 1)]
    [InlineData(5, 9, 5, 3)]
    public void DrainObservesDamageBeforeDeathPainOrWound(int damage, int woundHealth, int health, int finalState)
    {
        var source = new ObservingPlayer { Health = 50, DrainStrength = 1 };
        var target = new Actor { Health = 10, PainChance = 256, WoundHealth = woundHealth };
        target.SetTypedWound("Fire", 3);
        var initialState = target.States.Current;
        ActorDamage.Apply(target, damage, source, damageType: "Fire");
        Assert.Equal(1, source.Calls);
        Assert.Equal(health, source.ObservedHealth);
        Assert.Equal(initialState, source.ObservedState);
        Assert.Equal(0, source.ObservedDeaths);
        Assert.Equal(finalState, target.States.Current);
        Assert.Equal(50 + damage, source.Health);
    }
}
