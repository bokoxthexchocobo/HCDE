namespace HCDE.Playsim.Tests;

public class InventoryDamageModifierTests
{
    private sealed class Modifier : InventoryDamageModifier
    {
        public Func<int, bool, int> Change { get; init; } = (damage, _) => damage;
        public int Calls { get; private set; }
        public override int ModifyDamage(int damage, string? type, bool passive, Actor? inflictor, Actor? source, DamageFlags flags)
        {
            Calls++;
            Assert.Equal("Fire", type);
            return Change(damage, passive);
        }
    }

    [Fact]
    public void ChainUsesPreviousResultInInventoryOrder()
    {
        var second = new Modifier { Change = (damage, _) => damage / 3 };
        var first = new Modifier { Next = second, Change = (damage, _) => damage + 5 };
        var actor = new Actor { DamageModifiers = first };
        Assert.Equal(5, actor.GetModifiedDamage("Fire", 10, false, null, null, DamageFlags.None));
        Assert.Equal(1, first.Calls);
        Assert.Equal(1, second.Calls);
    }

    [Fact]
    public void SelfRemovalKeepsCapturedNextPointer()
    {
        var second = new Modifier { Change = (damage, _) => damage / 2 };
        var first = new Modifier();
        first = new Modifier { Next = second, Change = (damage, _) => { first.Next = null; first.Destroyed = true; return damage; } };
        var actor = new Actor { DamageModifiers = first };
        Assert.Equal(10, actor.GetModifiedDamage("Fire", 20, true, null, null, DamageFlags.None));
        Assert.Equal(1, second.Calls);
    }

    [Fact]
    public void DestroyedItemStopsTraversal()
    {
        var last = new Modifier();
        var destroyed = new Modifier { Destroyed = true, Next = last };
        var first = new Modifier { Next = destroyed };
        Assert.Equal(20, new Actor { DamageModifiers = first }.GetModifiedDamage("Fire", 20, true, null, null, DamageFlags.None));
        Assert.Equal(1, first.Calls);
        Assert.Equal(0, destroyed.Calls);
        Assert.Equal(0, last.Calls);
    }

    [Fact]
    public void ZeroDamageStillVisitsRemainingItems()
    {
        var last = new Modifier { Change = (damage, _) => damage + 3 };
        var first = new Modifier { Next = last, Change = (_, _) => 0 };
        Assert.Equal(3, new Actor { DamageModifiers = first }.GetModifiedDamage("Fire", 20, true, null, null, DamageFlags.None));
    }

    [Fact]
    public void SharedDamageRunsActiveAndPassiveChains()
    {
        var active = new Modifier { Change = (damage, passive) => passive ? damage : damage * 4 };
        var passive = new Modifier { Change = (damage, isPassive) => isPassive ? damage / 4 : damage };
        var source = new Actor { DamageModifiers = active };
        var target = new Actor { Health = 100, DamageModifiers = passive };
        Assert.Equal(20, ActorDamage.Apply(target, 20, source, damageType: "Fire").HealthLost);
        Assert.Equal(1, active.Calls);
        Assert.Equal(1, passive.Calls);
    }
}
