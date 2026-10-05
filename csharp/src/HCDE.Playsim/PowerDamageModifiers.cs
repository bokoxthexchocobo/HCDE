namespace HCDE.Playsim;

/// <summary>Native powerup class damage factors, separate from actor DamageFactor.</summary>
public abstract class PowerDamageModifier : InventoryDamageModifier
{
    private readonly Dictionary<string, double> _factors = new(StringComparer.OrdinalIgnoreCase);

    public void SetDamageFactor(string damageType, double factor)
    {
        ArgumentException.ThrowIfNullOrEmpty(damageType);
        if (!double.IsFinite(factor)) throw new ArgumentOutOfRangeException(nameof(factor));
        _factors[damageType] = factor;
    }

    protected int ApplyFactors(string? damageType, int damage, long defaultDamage)
    {
        if (_factors.Count == 0) return (int)Math.Clamp(defaultDamage, int.MinValue, int.MaxValue);
        if (!_factors.TryGetValue(string.IsNullOrEmpty(damageType) ? "None" : damageType, out var factor)
            && !_factors.TryGetValue("None", out factor)) return damage;
        if (factor < 0) return damage;
        return (int)Math.Clamp(damage * factor, int.MinValue, int.MaxValue);
    }
}

/// <summary>Native PowerDamage.ModifyDamage calculation; lifecycle is separate.</summary>
public sealed class PowerDamage : PowerDamageModifier
{
    public override int ModifyDamage(int damage, string? damageType, bool passive,
        Actor? inflictor, Actor? source, DamageFlags flags) =>
        !passive && damage > 0 ? Math.Max(1, ApplyFactors(damageType, damage, (long)damage * 4)) : damage;
}

/// <summary>Native PowerProtection.ModifyDamage calculation; lifecycle is separate.</summary>
public sealed class PowerProtection : PowerDamageModifier
{
    public override int ModifyDamage(int damage, string? damageType, bool passive,
        Actor? inflictor, Actor? source, DamageFlags flags) =>
        passive && damage > 0 ? Math.Max(0, ApplyFactors(damageType, damage, damage / 4)) : damage;
}
