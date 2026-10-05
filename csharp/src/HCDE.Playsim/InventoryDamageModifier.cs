namespace HCDE.Playsim;

/// <summary>Managed subset of the native inventory ModifyDamage chain.</summary>
public class InventoryDamageModifier
{
    public InventoryDamageModifier? Next { get; set; }
    public bool Destroyed { get; set; }

    public virtual int ModifyDamage(int damage, string? damageType, bool passive,
        Actor? inflictor, Actor? source, DamageFlags flags) => damage;

    internal static int Apply(InventoryDamageModifier? item, int damage, string? damageType,
        bool passive, Actor? inflictor, Actor? source, DamageFlags flags)
    {
        while (item is { Destroyed: false })
        {
            // Native captures the next inventory pointer before invoking script code.
            var next = item.Next;
            damage = item.ModifyDamage(damage, damageType, passive, inflictor, source, flags);
            item = next;
        }
        return damage;
    }
}
