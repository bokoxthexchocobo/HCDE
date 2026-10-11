namespace HCDE.Playsim.Tests;

public class ZeroOrdinaryWoundTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void InitiallyZeroDamageCanSelectWoundWithoutPainFlags(int damage)
    {
        var target = new Actor { Health = 50, WoundHealth = 50, ReactionTime = 12 };
        target.SetTypedWound("Fire", 3);
        Assert.Equal(default, ActorDamage.Apply(target, damage, damageType: "Fire"));
        Assert.Equal(50, target.Health); Assert.Equal(3, target.States.Current);
        Assert.Equal(12, target.ReactionTime); Assert.Null(target.LastDamageSourceId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PositiveDamageReducedOrCancelledBySpecialDamageDoesNotSelectWound(bool negative)
    {
        var target = new CancellingActor { Health = 50, WoundHealth = 50, Negative = negative };
        target.SetTypedWound("Fire", 3); var initial = target.States.Current;
        ActorDamage.Apply(target, 7, damageType: "Fire");
        Assert.Equal(initial, target.States.Current); Assert.Equal(50, target.Health);
    }

    private sealed class CancellingActor : Actor
    {
        public bool Negative { get; init; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
            => Negative ? -1 : 0;
    }
}
