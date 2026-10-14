namespace HCDE.Playsim.Tests;

public class PoisonDeathInflictorTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    public void PoisonDeathUsesSourceExtremeFlags(bool extreme, bool noExtreme, bool expected)
    {
        var target = new PlayerPawn { ExtremeDeathState = ActorStateMachine.Corpse };
        var source = new Actor { ExtremeDeath = extreme, NoExtremeDeath = noExtreme };
        ActorDamage.ApplyPoisonDamage(target, 100, source);
        Assert.True(target.IsDead); Assert.Equal(expected, target.ExtremelyDead);
        Assert.Equal(expected ? ActorStateMachine.Corpse : ActorStateMachine.Death, target.States.Current);
        Assert.Null(target.DeathInflictor); Assert.Null(target.DamageTypeReceived);
    }

    [Fact]
    public void SourceDeathTypeCannotReplaceStoredPoisonType()
    {
        var target = new PlayerPawn { PoisonDamageType = "Ice" };
        target.SetTypedDeath("Ice", ActorStateMachine.Corpse);
        var source = new Actor { DeathType = "Fire" };
        ActorDamage.ApplyPoisonDamage(target, 100, source);
        Assert.Equal(ActorStateMachine.Corpse, target.States.Current); Assert.Equal("Ice", target.DamageType);
    }

    [Fact]
    public void SourceSpecialFireFlagDoesNotSuppressPoisonFireDeath()
    {
        var target = new PlayerPawn { PoisonDamageType = "Fire" };
        target.SetTypedDeath("Fire", ActorStateMachine.Corpse);
        ActorDamage.ApplyPoisonDamage(target, 200, new Actor { SpecialFireDamage = true });
        Assert.Equal(ActorStateMachine.Corpse, target.States.Current); Assert.Equal("Fire", target.DamageType);
    }

    [Fact]
    public void PoisonSourceIsVisibleDuringDeathCallback()
    {
        var source = new Actor(); var target = new ObservingPlayer();
        ActorDamage.ApplyPoisonDamage(target, 100, source);
        Assert.Same(source, target.Observed); Assert.Null(target.DeathInflictor);
    }

    private sealed class ObservingPlayer : PlayerPawn
    {
        public Actor? Observed { get; private set; }
        public override Fixed GetDeathHeight() { Observed = DeathInflictor; return base.GetDeathHeight(); }
    }
}
