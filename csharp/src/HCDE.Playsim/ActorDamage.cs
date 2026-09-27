namespace HCDE.Playsim;

[Flags]
public enum DamageFlags
{
    None = 0,
    BypassArmor = 1,
    BypassInvulnerability = 2,
    NoPain = 4,
}

public readonly record struct DamageResult(int HealthLost, int ArmorLost, bool Killed);

/// <summary>Shared damage/lifecycle boundary used by weapons and environmental damage.</summary>
public static class ActorDamage
{
    public static DamageResult Apply(Actor target, int damage, Actor? source = null, DamageFlags flags = DamageFlags.None)
    {
        if (damage <= 0 || !target.CanTakeDamage
            || (target.Invulnerable && !flags.HasFlag(DamageFlags.BypassInvulnerability)))
            return default;
        var absorbed = 0;
        if (target is PlayerPawn player && !flags.HasFlag(DamageFlags.BypassArmor))
        {
            var inventory = player.Inventory;
            var percent = Math.Clamp(inventory.ArmorSavePercent, 0, 100);
            // The legacy value 33 identifies Doom green armor's exact one-third save.
            // Multiplying by 33/100 incorrectly saves zero against three damage.
            var saved = percent == PlayerInventory.GreenSavePercent ? damage / 3 : (long)damage * percent / 100;
            absorbed = (int)Math.Min(Math.Max(0, inventory.Armor), saved);
            inventory.Armor -= absorbed;
        }
        var before = target.Health;
        target.LastDamageSourceId = source?.Id;
        target.Health = Math.Max(0, before - (damage - absorbed));
        var lost = before - target.Health;
        if (lost > 0 && !target.IsDead && !flags.HasFlag(DamageFlags.NoPain) && target.States.HasState(target.PainState)
            && (target.PainChance >= 256 || target.PainChance > 0 && target.Simulation != null
                && target.Simulation.NextCombatRandom() % 256 < target.PainChance))
            target.States.Enter(target, target.PainState);
        return new DamageResult(lost, absorbed, target.IsDead);
    }
}
