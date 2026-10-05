namespace HCDE.Playsim.Tests;

public class DamageModifierStagesTests
{
    private sealed class ModifierActor : Actor
    {
        public List<string> Calls { get; init; } = [];
        public int Modifier { get; init; } = 2;
        public override int GetModifiedDamage(string? type, int damage, bool passive, Actor? inflictor, Actor? other, DamageFlags flags)
        {
            Calls.Add($"{(passive ? "protect" : "enhance")}:{damage}");
            return passive ? damage / Modifier : damage * Modifier;
        }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? type, DamageFlags flags)
        {
            Calls.Add($"special:{damage}");
            return damage;
        }
    }

    [Theory]
    [InlineData(DamageFlags.None, 20)]
    [InlineData(DamageFlags.NoEnhance, 10)]
    [InlineData(DamageFlags.NoProtect, 40)]
    [InlineData(DamageFlags.NoEnhance | DamageFlags.NoProtect, 20)]
    public void BypassFlagsPreserveBaseScalingAndArmor(DamageFlags flags, int expected)
    {
        var calls = new List<string>();
        var source = new ModifierActor { Calls = calls, DamageMultiplier = Fixed.FromInt(2) };
        var target = new ModifierActor { Calls = calls, Health = 100, DamageFactor = Fixed.FromDouble(0.5), Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(expected, ActorDamage.Apply(target, 40, source, flags).HealthLost);
        Assert.Equal(expected, 100 - target.Armor);
        var wanted = new List<string>();
        if (!flags.HasFlag(DamageFlags.NoEnhance)) wanted.Add("enhance:80");
        if (!flags.HasFlag(DamageFlags.NoProtect)) wanted.Add(flags.HasFlag(DamageFlags.NoEnhance) ? "protect:80" : "protect:160");
        wanted.Add($"special:{expected * 2}");
        Assert.Equal(wanted, calls);
    }

    [Theory]
    [InlineData(DamageFlags.Forced, 40)]
    [InlineData(DamageFlags.None, ActorDamage.TelefragDamage)]
    public void ForcedAndTelefragSkipModifiers(DamageFlags flags, int damage)
    {
        var calls = new List<string>();
        var target = new ModifierActor { Calls = calls, Health = 100 };
        ActorDamage.Apply(target, damage, new ModifierActor { Calls = calls }, flags);
        Assert.Empty(calls);
    }

    [Fact]
    public void PassiveModifierRunsWithoutSource()
    {
        var target = new ModifierActor { Health = 100 };
        Assert.Equal(10, ActorDamage.Apply(target, 20).HealthLost);
        Assert.Equal(new[] { "protect:20", "special:10" }, target.Calls);
    }
}
