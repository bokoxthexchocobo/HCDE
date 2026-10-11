namespace HCDE.Playsim.Tests;

public class NegativeDamageModifierCancellationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NegativeModifierCannotBeReplacedByTakeSpecialDamage(bool passive, bool painFlags)
    {
        var target = new ModifierActor
        {
            Health = 100, Armor = 50, ArmorSavePercent = 50, ReactionTime = 12,
            CancelPassive = passive, AllowPain = painFlags, NoDamage = painFlags,
        };
        var source = new ModifierActor { CancelActive = !passive };
        var inflictor = new Actor { CausePain = painFlags, ForcePain = painFlags };
        var result = ActorDamage.Apply(target, 7, source, damageType: "Fire", inflictor: inflictor);
        Assert.Equal(default, result); Assert.Equal(100, target.Health); Assert.Equal(50, target.Armor);
        Assert.Equal(12, target.ReactionTime); Assert.Equal(target.SpawnState, target.States.Current);
        Assert.Equal(0, target.SpecialCalls); Assert.Null(target.LastDamageSourceId);
        Assert.Equal(1, source.ActiveCalls);
        Assert.Equal(passive ? 1 : 0, target.PassiveCalls);
    }

    private sealed class ModifierActor : Actor
    {
        public bool CancelActive { get; init; }
        public bool CancelPassive { get; init; }
        public int ActiveCalls { get; private set; }
        public int PassiveCalls { get; private set; }
        public int SpecialCalls { get; private set; }

        public override int GetModifiedDamage(string? damageType, int damage, bool passive,
            Actor? inflictor, Actor? other, DamageFlags flags)
        {
            if (passive) PassiveCalls++; else ActiveCalls++;
            return (passive ? CancelPassive : CancelActive) ? -1 : damage;
        }

        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        {
            SpecialCalls++;
            return 10;
        }
    }
}
