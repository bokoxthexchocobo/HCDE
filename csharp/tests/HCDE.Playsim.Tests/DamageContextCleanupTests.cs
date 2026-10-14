namespace HCDE.Playsim.Tests;

public class DamageContextCleanupTests
{
    [Theory]
    [InlineData("gib")]
    [InlineData("height")]
    [InlineData("state")]
    public void ThrowingDeathCallbackClearsContextAndPropagatesSameException(string stage)
    {
        var actor = new ThrowingActor(stage); var inflictor = new Actor { ExtremeDeath = true };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, _ => actor.StateAction()), new(-1, 2)], 0);
        actor.DeathState = 1;
        Assert.Same(actor.Failure, Assert.Throws<InvalidOperationException>(() =>
            ActorDamage.Apply(actor, 100, damageType: "Fire", inflictor: inflictor)));
        Assert.Null(actor.DamageTypeReceived); Assert.Null(actor.DeathInflictor);
        actor.Throw = false; actor.ExtremeDeathState = 2; actor.Health = 100; actor.Health = 0;
        Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void ThrowingDrainCallbackClearsVictimContext()
    {
        var source = new ThrowingDrainPlayer { DrainStrength = 0.5 };
        var victim = new Actor(); var inflictor = new Actor();
        Assert.Same(source.Failure, Assert.Throws<InvalidOperationException>(() =>
            ActorDamage.Apply(victim, 10, source, damageType: "Fire", inflictor: inflictor)));
        Assert.Null(victim.DamageTypeReceived); Assert.Null(victim.DeathInflictor);
        Assert.Equal(90, victim.Health);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void SuccessfulDamageAlsoClearsTransientContext(int damage)
    {
        var target = new Actor(); ActorDamage.Apply(target, damage, damageType: "Fire", inflictor: new Actor());
        Assert.Null(target.DamageTypeReceived); Assert.Null(target.DeathInflictor);
    }

    private sealed class ThrowingActor(string stage) : Actor
    {
        public bool Throw { get; set; } = true;
        public InvalidOperationException Failure { get; } = new("death-callback-test");
        public override int GetGibHealth()
        {
            if (Throw && stage == "gib") throw Failure;
            return base.GetGibHealth();
        }
        public override Fixed GetDeathHeight()
        {
            if (Throw && stage == "height") throw Failure;
            return base.GetDeathHeight();
        }
        public void StateAction() { if (Throw && stage == "state") throw Failure; }
    }

    private sealed class ThrowingDrainPlayer : PlayerPawn
    {
        public InvalidOperationException Failure { get; } = new("drain-callback-test");
        public override int OnDrain(Actor target, int amount, string? damageType) => throw Failure;
    }
}
