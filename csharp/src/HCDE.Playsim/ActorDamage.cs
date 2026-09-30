namespace HCDE.Playsim;

[Flags]
public enum DamageFlags
{
    None = 0,
    BypassArmor = 1,
    BypassInvulnerability = 2,
    NoPain = 4,
    /// <summary>Native <c>DMG_FORCED</c>. Skips armor, invulnerability, and Buddha.</summary>
    Forced = 8,
    /// <summary>Native <c>DMG_FOILBUDDHA</c>. Kills a non-player Buddha. Players ignore it.</summary>
    FoilBuddha = 16,
}

public readonly record struct DamageResult(int HealthLost, int ArmorLost, bool Killed);

/// <summary>Shared damage/lifecycle boundary used by weapons and environmental damage.</summary>
public static class ActorDamage
{
    /// <summary>Native <c>TELEFRAG_DAMAGE</c>. A hit this large kills through ordinary Buddha.</summary>
    public const int TelefragDamage = 1_000_000;

    public static DamageResult Apply(Actor target, int damage, Actor? source = null, DamageFlags flags = DamageFlags.None, string? damageType = null, Actor? inflictor = null)
    {
        var forced = flags.HasFlag(DamageFlags.Forced);
        // CF_BUDDHA2 clears DMG_FORCED before invulnerability and armor.
        if (target is PlayerPawn { Buddha2: true })
            forced = false;
        // God mode and invulnerability stop ordinary hits. A telefrag goes through both.
        var telefrag = damage >= TelefragDamage;
        if (damage <= 0)
            return default;
        if (target.IsDead)
        {
            if (!target.Shootable)
                return default;
            if (inflictor is { } && string.Equals(damageType, "Ice", StringComparison.Ordinal) && !inflictor.IceShatter)
                return default;
            if (target.IceCorpse)
            {
                target.Shattering = true;
                target.VelocityX = target.VelocityY = target.VelocityZ = default;
                target.States.ForceRemainingTics(1);
                target.Simulation?.SpawnIceChunks(target);
            }
            return default;
        }
        if (!target.CanTakeDamage
            || ((target.Invulnerable || target is PlayerPawn { GodMode: true })
                && !telefrag && !forced && !flags.HasFlag(DamageFlags.BypassInvulnerability)))
            return default;
        var attacker = source ?? inflictor;
        if (attacker != null && !target.CanAttackHurtFrom(attacker))
            return default;
        var absorbed = 0;
        if (!forced && !flags.HasFlag(DamageFlags.BypassArmor) && !IgnoresArmor(damageType))
        {
            if (target is PlayerPawn player)
            {
                absorbed = AbsorbArmor(damage, player.Inventory.Armor, player.Inventory.ArmorSavePercent,
                    player.Inventory.MaxAbsorb, player.Inventory.MaxFullAbsorb, player.Inventory.AbsorbCount);
                player.Inventory.Armor -= absorbed;
                player.Inventory.AbsorbCount += absorbed;
                // Amount 0 clears the percent, then uses the best stored pickup.
                if (player.Inventory.Armor == 0)
                    player.Inventory.PromoteSpareArmor();
            }
            else
            {
                // p_interaction.cpp calls AbsorbDamage for a monster with inventory.
                // BasicArmor clears its save percent when the amount reaches 0.
                absorbed = AbsorbArmor(damage, target.Armor, target.ArmorSavePercent,
                    target.MaxAbsorb, target.MaxFullAbsorb, target.AbsorbCount);
                target.Armor -= absorbed;
                target.AbsorbCount += absorbed;
                if (absorbed > 0 && target.Armor == 0)
                    target.ArmorSavePercent = 0;
            }
        }
        var before = target.Health;
        target.LastDamageSourceId = source?.Id;
        target.DamageTypeReceived = damageType;
        target.DeathInflictor = inflictor;
        // P_DamageMobj subtracts the post-armor remainder and keeps the negative overkill.
        // GetGibHealth is -spawn health. Health equal to that threshold is not an extreme death.
        var next = (long)before - (damage - absorbed);
        // Clamp before assigning Health. The setter enters death as soon as health crosses 0.
        if (next <= 0 && SurvivesByBuddha(target, damage, flags, inflictor))
            next = 1;
        target.Health = (int)Math.Clamp(next, int.MinValue, int.MaxValue);
        target.DamageTypeReceived = null;
        target.DeathInflictor = null;
        var lost = before - target.Health;
        var dealt = damage - absorbed;
        var forcedPain = inflictor is { ForcePain: true };
        if (!target.IsDead && target.WoundHealth > 0 && target.Health <= target.WoundHealth)
        {
            var wound = target.WoundStateFor(damageType);
            if (wound >= 0)
            {
                target.States.Enter(target, wound);
                Drain(target, source, damage - absorbed);
                return new DamageResult(lost, absorbed, target.IsDead);
            }
        }
        var painChance = target.PainChanceFor(damageType);
        var painState = target.PainStateFor(damageType);
        // MF6_FORCEPAIN skips the threshold and the pain roll. MF5_NOPAIN, MF5_PAINLESS, and DMG_NO_PAIN still block.
        var painless = target.NoPain || inflictor is { Painless: true } || flags.HasFlag(DamageFlags.NoPain);
        var painFlinch = !target.IsDead && !painless && target.States.HasState(painState)
            && (forcedPain || (lost > 0 && dealt >= target.PainThreshold
                && (painChance >= 256 || painChance > 0 && target.Simulation != null
                    && target.Simulation.NextCombatRandom() % 256 < painChance)))
            && TryEnterPain(target, painState, damageType, forcedPain);
        if (!target.IsDead)
            target.Brain?.WakeOnDamage(target, source, dealt, forcedPain);
        if (painFlinch && source != null && ShouldMarkJustHit(target, source))
            target.JustHit = true;
        Drain(target, source, damage - absorbed);
        return new DamageResult(lost, absorbed, target.IsDead);
    }

    /// <summary>Electric pain rolls <c>pr_lightning</c> on the flicker stream. Poison howling is absent.</summary>
    private static bool TryEnterPain(Actor target, int painState, string? damageType, bool forcedPain)
    {
        if (!string.Equals(damageType, "Electric", StringComparison.Ordinal))
        {
            target.States.Enter(target, painState);
            return true;
        }
        if (target.Simulation == null)
        {
            target.States.Enter(target, painState);
            return true;
        }
        if (forcedPain || target.Simulation.NextFlickerRandom() < 96)
        {
            target.FullBright = false;
            target.States.Enter(target, painState);
            return true;
        }
        target.FullBright = true;
        return false;
    }

    /// <summary>Native <c>MF_JUSTHIT</c> gate. Teamplay and designated teams are absent.</summary>
    private static bool ShouldMarkJustHit(Actor target, Actor source)
    {
        if (target.Brain is not { } brain) return true;
        if (brain.TargetId == source.Id) return true;
        if (brain.TargetId == null) return true;
        var chase = target.Simulation?.Actors.FirstOrDefault(actor => actor.Id == brain.TargetId);
        return chase != null && !target.IsFriend(chase);
    }

    /// <summary>Mapinfo <c>DamageType Drowning</c> is the only <c>NoArmor</c> type in this port.</summary>
    internal static bool IgnoresArmor(string? damageType) =>
        string.Equals(damageType, "Drowning", StringComparison.Ordinal);

    /// <summary>
    /// BasicArmor's integer save. <c>MaxFullAbsorb</c> is saved in full before the
    /// percent. Percent 33 is green armor's <c>damage / 3</c>. Other percents use
    /// <c>damage * percent / 100</c>. <c>MaxAbsorb</c> caps the running total.
    /// The armor amount caps the save. Both caps at 0 leave the older formula.
    /// </summary>
    internal static int AbsorbArmor(int damage, int armor, int percent, int maxAbsorb, int maxFullAbsorb, int absorbCount)
    {
        if (damage <= 0 || armor <= 0) return 0;
        percent = Math.Clamp(percent, 0, 100);
        var full = Math.Max(0, maxFullAbsorb - absorbCount);
        var saved = damage < full
            ? damage
            : full + PercentSave(damage - full, percent);
        if (maxAbsorb > 0 && saved + absorbCount > maxAbsorb)
            saved = Math.Max(0, maxAbsorb - absorbCount);
        return Math.Min(armor, Math.Max(0, saved));
    }

    private static int PercentSave(int damage, int percent)
    {
        if (damage <= 0 || percent <= 0) return 0;
        return percent == PlayerInventory.GreenSavePercent ? damage / 3 : (int)((long)damage * percent / 100);
    }

    /// <summary>
    /// <c>MF7_BUDDHA</c> and a live <c>PowerBuddha</c> leave health at 1. Telefrag
    /// damage and <see cref="DamageFlags.Forced"/> still kill those two.
    /// <see cref="DamageFlags.FoilBuddha"/> and an inflictor with
    /// <c>MF7_FOILBUDDHA</c> kill a non-player only. A null inflictor does not.
    /// <c>CF_BUDDHA2</c> also survives a telefrag and forced damage.
    /// </summary>
    private static bool SurvivesByBuddha(Actor target, int damage, DamageFlags flags, Actor? inflictor)
    {
        if (target is PlayerPawn { Buddha2: true })
            return true;
        var protectedByItem = target is PlayerPawn { PowerBuddhaTics: > 0 };
        if ((!target.Buddha && !protectedByItem) || flags.HasFlag(DamageFlags.Forced) || damage >= TelefragDamage)
            return false;
        if (target is PlayerPawn)
            return true;
        return !flags.HasFlag(DamageFlags.FoilBuddha) && inflictor is not { FoilBuddha: true };
    }

    /// <summary>
    /// PowerDrain. The source must be a living player, and the amount is
    /// <c>int(strength * post-armor damage)</c>, capped at <see cref="PlayerPawn.DrainMaxHealth"/>.
    /// </summary>
    private static void Drain(Actor target, Actor? source, int postArmor)
    {
        if (postArmor <= 0 || source is not PlayerPawn player || player.DrainStrength <= 0
            || target.DontDrain || ReferenceEquals(target, player)
            || player.Health <= 0 || player.Health >= PlayerPawn.DrainMaxHealth)
            return;
        var amount = (int)(player.DrainStrength * postArmor);
        if (amount <= 0) return;
        player.Health = Math.Min(PlayerPawn.DrainMaxHealth, player.Health + amount);
    }
}
