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
    /// <summary>Native DMG_NO_FACTOR. Skips the target's damage factor.</summary>
    NoFactor = 32,
    /// <summary>Native DMG_FOILINVUL. With an inflictor, bypasses monster invulnerability.</summary>
    FoilInvulnerability = 64,
    /// <summary>Native DMG_NO_ENHANCE. Skips active modifiers, preserving DamageMultiplier.</summary>
    NoEnhance = 128,
    /// <summary>Native DMG_NO_PROTECT. Skips passive modifiers, preserving DamageFactor.</summary>
    NoProtect = 256,
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
        damage = Math.Max(damage, 0);
        if (damage == 0 && forced)
            return default;
        if (target.Spectral && !forced && !telefrag && inflictor is not { Spectral: true })
            return default;
        if (target.IsDead)
        {
            if (!target.Shootable)
                return default;
            if (inflictor is { } && string.Equals(damageType, "Ice", StringComparison.OrdinalIgnoreCase) && !inflictor.IceShatter)
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
        var foilInvulnerability = target is not PlayerPawn && inflictor != null
            && (inflictor.FoilInvul || flags.HasFlag(DamageFlags.FoilInvulnerability));
        if (!target.CanTakeDamage
            || ((target.Invulnerable || target is PlayerPawn { GodMode: true })
                && !telefrag && !forced && !foilInvulnerability && !flags.HasFlag(DamageFlags.BypassInvulnerability)))
            return default;
        var attacker = source ?? inflictor;
        if (inflictor is { PierceArmor: true }) flags |= DamageFlags.BypassArmor;
        if (target.Brain is { Charging: true })
            target.VelocityX = target.VelocityY = target.VelocityZ = default;
        if (target.Dormant && !forced) return default;
        if (!forced && !telefrag && attacker != null && !target.CanAttackHurtFrom(attacker))
            return default;
        if (!forced && !telefrag)
        {
            if (inflictor != null)
            {
                damage = inflictor.DoSpecialDamage(target, damage, damageType, flags);
                if (damage < 0) return default;
            }
            var originalModifierDamage = damage;
            // Native truncates each multiplication separately. Only source, not inflictor, enhances damage.
            if (damage > 0 && source != null) damage = ScaleDamage(damage, source.DamageMultiplier);
            if (damage > 0 && source != null && !flags.HasFlag(DamageFlags.NoEnhance))
                damage = source.GetModifiedDamage(damageType, damage, false, inflictor, target, flags);
            if (damage > 0 && !flags.HasFlag(DamageFlags.NoProtect))
                damage = target.GetModifiedDamage(damageType, damage, true, inflictor, source, flags);
            if (damage > 0 && !flags.HasFlag(DamageFlags.NoFactor))
            {
                damage = ScaleDamage(damage, target.DamageFactor);
                if (damage > 0) damage = target.ApplyTypedDamageFactor(damage, damageType);
            }
            damage = target.TakeSpecialDamage(inflictor, source, damage, damageType, flags);
            if (damage <= 0)
            {
                if (damage == 0 && originalModifierDamage == 0 && inflictor is { ForcePain: true })
                    return ReactToDamage(target, source, inflictor, flags, damageType, 0, 0, 0);
                return default;
            }
        }
        var absorbed = 0;
        if (!forced && !flags.HasFlag(DamageFlags.BypassArmor) && !IgnoresArmor(damageType, target.Simulation))
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
                if (target.Armor == 0)
                    target.ArmorSavePercent = 0;
            }
        }
        // Native armor cancellation keeps armor consumption but stops later damage effects.
        var healthDamage = telefrag && target is PlayerPawn ? damage : damage - absorbed;
        if (healthDamage <= 0) return new DamageResult(0, absorbed, false);
        var before = target.Health;
        target.LastDamageSourceId = source?.Id;
        target.DamageTypeReceived = damageType;
        target.DeathInflictor = inflictor;
        // P_DamageMobj subtracts the post-armor remainder and keeps the negative overkill.
        // GetGibHealth is -spawn health. Health equal to that threshold is not an extreme death.
        var next = (long)before - healthDamage;
        target.DeathDamageAmount = healthDamage;
        // Clamp before assigning Health. The setter enters death as soon as health crosses 0.
        if (next <= 0 && target is PlayerPawn && SurvivesByBuddha(target, damage, flags, inflictor))
            next = 1;
        target.SetDamageHealth((int)Math.Clamp(next, int.MinValue, int.MaxValue), source,
            () =>
            {
                Drain(target, source, healthDamage, damageType);
                return target.Health <= 0 && target is not PlayerPawn && SurvivesByBuddha(target, damage, flags, inflictor)
                    ? 1 : target.Health;
            });
        target.DamageTypeReceived = null;
        target.DeathInflictor = null;
        var lost = before - target.Health;
        var dealt = healthDamage;
        return ReactToDamage(target, source, inflictor, flags, damageType, lost, absorbed, dealt);
    }

    private static DamageResult ReactToDamage(Actor target, Actor? source, Actor? inflictor,
        DamageFlags flags, string? damageType, int lost, int absorbed, int dealt)
    {
        var forcedPain = inflictor is { ForcePain: true };
        if (!target.IsDead && target.WoundHealth > 0 && target.Health <= target.WoundHealth)
        {
            var wound = target.WoundStateFor(damageType);
            if (wound >= 0)
            {
                target.States.Enter(target, wound);
                return new DamageResult(lost, absorbed, target.IsDead);
            }
        }
        var painChance = target.PainChanceFor(damageType);
        var painState = target.PainStateFor(damageType);
        // MF6_FORCEPAIN skips the threshold and the pain roll. MF5_NOPAIN, MF5_PAINLESS, and DMG_NO_PAIN still block.
        var painless = target.NoPain || inflictor is { Painless: true } || flags.HasFlag(DamageFlags.NoPain);
        var painReaction = !target.IsDead && !painless && target.Brain?.Charging != true
            && (forcedPain || (lost > 0 && dealt >= target.PainThreshold
                && RollPainChance(target, painChance)))
            && TryReactToPain(target, painState, damageType);
        if (!target.IsDead)
        {
            if (target is not PlayerPawn && (dealt > 0 || forcedPain))
                target.ReactionTime = 0;
            target.Brain?.WakeOnDamage(target, source, dealt, forcedPain);
        }
        if (painReaction && ShouldMarkJustHit(target, source))
            target.JustHit = true;
        return new DamageResult(lost, absorbed, target.IsDead);
    }

    private static int ScaleDamage(int damage, Fixed factor) =>
        (int)Math.Clamp((long)damage * factor.Raw / 65536, int.MinValue, int.MaxValue);

    private static bool RollPainChance(Actor target, int chance) => target.Simulation is { } simulation
        ? simulation.NextCombatRandom() % 256 < chance
        : chance >= 256;

    internal static bool TriggerPainChance(Actor target, string? damageType, bool forcedPain)
    {
        if (target.NoPain || target.Health < 1) return false;
        if (!forcedPain && !RollPainChance(target, target.PainChanceFor(damageType))) return false;
        var state = target.PainStateFor(damageType);
        var hasAnimation = target.States.HasState(state);
        return TryReactToPain(target, state, damageType) && hasAnimation;
    }

    /// <summary>Electric pain rolls <c>pr_lightning</c> on the flicker stream. Poison howling is absent.</summary>
    private static bool TryReactToPain(Actor target, int painState, string? damageType)
    {
        if (!string.Equals(damageType, "Electric", StringComparison.OrdinalIgnoreCase))
        {
            if (target.States.HasState(painState)) target.States.Enter(target, painState);
            return true;
        }
        if (target.Simulation == null)
        {
            if (target.States.HasState(painState)) target.States.Enter(target, painState);
            return true;
        }
        if (target.Simulation.NextFlickerRandom() < 96)
        {
            if (target.States.HasState(painState))
            {
                target.FullBright = false;
                target.States.Enter(target, painState);
            }
            return true;
        }
        target.FullBright = true;
        // Native monster electrocution consumes another lightning draw for the howl.
        if (target.IsMonster) target.Simulation.NextFlickerRandom();
        return false;
    }

    /// <summary>Native <c>MF_JUSTHIT</c> gate. Teamplay and designated teams are absent.</summary>
    private static bool ShouldMarkJustHit(Actor target, Actor? source)
    {
        if (target.Brain is not { } brain) return true;
        if (brain.TargetId == source?.Id) return true;
        if (brain.TargetId == null) return true;
        var chase = target.Simulation?.Actors.FirstOrDefault(actor => actor.Id == brain.TargetId);
        return chase != null && !target.IsFriend(chase);
    }

    /// <summary>Native DamageTypeDefinition.IgnoreArmor, with built-in Drowning for detached actors.</summary>
    internal static bool IgnoresArmor(string? damageType, AuthoritySimulation? simulation = null) =>
        simulation is not null
            ? !string.IsNullOrEmpty(damageType) && simulation.DamageTypes.Find(damageType)?.NoArmor == true
            : string.Equals(damageType, "Drowning", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// BasicArmor's integer save. <c>MaxFullAbsorb</c> is saved in full before the
    /// percent. Percent 33 represents native green armor's 33.335%. Other percents use
    /// <c>damage * percent / 100</c>. <c>MaxAbsorb</c> caps the running total only
    /// when damage reaches the remaining full-absorption allowance.
    /// The armor amount caps the save. Both caps at 0 leave the older formula.
    /// </summary>
    internal static int AbsorbArmor(int damage, int armor, int percent, int maxAbsorb, int maxFullAbsorb, int absorbCount)
    {
        if (damage <= 0 || armor <= 0) return 0;
        percent = Math.Clamp(percent, 0, 100);
        var full = Math.Max(0, maxFullAbsorb - absorbCount);
        int saved;
        if (damage < full)
            saved = damage;
        else
        {
            saved = full + PercentSave(damage - full, percent);
            if (maxAbsorb > 0 && saved + absorbCount > maxAbsorb)
                saved = Math.Max(0, maxAbsorb - absorbCount);
        }
        return Math.Min(armor, Math.Max(0, saved));
    }

    private static int PercentSave(int damage, int percent)
    {
        if (damage <= 0 || percent <= 0) return 0;
        return percent == PlayerInventory.GreenSavePercent
            ? (int)(damage * PlayerInventory.ArmorSaveFraction(percent))
            : (int)((long)damage * percent / 100);
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
    /// PowerDrain. The source must be a player, and the amount is
    /// <c>int(strength * post-armor damage)</c>, limited by the player's maximum health.
    /// </summary>
    private static void Drain(Actor target, Actor? source, int postArmor, string? damageType)
    {
        // Native armor returns before drain when the damage remainder reaches zero.
        if (postArmor <= 0 || source is not PlayerPawn player || player.DrainStrength <= 0
            || target.DontDrain || ReferenceEquals(target, player))
            return;
        var amount = (int)(player.DrainStrength * postArmor);
        amount = player.OnDrain(target, amount, damageType);
        PickupCatalog.GiveHealth(player, amount);
    }
}
