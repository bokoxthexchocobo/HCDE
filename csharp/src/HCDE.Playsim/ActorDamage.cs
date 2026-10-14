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

    /// <summary>Native P_PoisonDamage bypasses armor and ordinary hit reactions.</summary>
    public static DamageResult ApplyPoisonDamage(PlayerPawn target, int damage, Actor? source = null, bool playPainSound = true)
    {
        if (target.Health <= 0) return default;
        var telefrag = damage >= TelefragDamage && !target.LaxTelefragDamage;
        if (target.GodMode2 || target.NoDamage || !telefrag && (target.Invulnerable || target.GodMode)) return default;
        if (target.Simulation is { } simulation)
            damage = (int)Math.Clamp(damage * simulation.SkillDamageFactor * simulation.PlayerDamageFactor, 0, int.MaxValue);
        damage = target.GetModifiedDamage(target.PoisonDamageType, damage, true, null, source, DamageFlags.None);
        if (damage > 0) damage = ScaleDamage(damage, target.DamageFactor);
        if (damage > 0) damage = target.ApplyTypedDamageFactor(damage, target.PoisonDamageType);
        if (damage <= 0) return default;
        AutoUseEmergencyHealth(target, damage);
        var before = target.Health;
        var next = (long)before - damage;
        if (next < 50 && target.Simulation is { GameMode: not SpawnGameMode.Deathmatch, DisableAutoHealth: false } strifeSimulation)
            next = target.Inventory.AutoUseStrifePoisonHealth(before, damage, PickupCatalog.ResolveHealthMaximum(target), strifeSimulation.HealthFactor);
        if (next <= 0 && SurvivesByBuddha(target, telefrag ? TelefragDamage : 0, DamageFlags.None, null)) next = 1;
        target.LastDamageSourceId = source?.Id;
        target.DeathDamageAmount = damage;
        target.DamageTypeReceived = target.PoisonDamageType;
        // P_PoisonDamage selects its stored type directly, bypassing ordinary inflictor overrides.
        target.DeathInflictor = source;
        try { target.SetDamageHealth((int)Math.Clamp(next, int.MinValue, int.MaxValue), source, () => target.Health,
            useInflictorDeathType: false); }
        finally { target.DamageTypeReceived = null; target.DeathInflictor = null; }
        if (!target.IsDead && playPainSound && (target.Simulation?.Thinkers.Clock.Tic ?? 0) % 64 == 0)
        {
            var state = target.PainStateFor(target.PoisonPainType);
            if (target.States.HasState(state))
            {
                var previous = target.PainDamageTypeContext;
                target.PainDamageTypeContext = target.PoisonPainType;
                try { target.States.Enter(target, state); }
                finally { target.PainDamageTypeContext = previous; }
            }
        }
        return new(before - target.Health, 0, target.IsDead);
    }

    public static DamageResult Apply(Actor target, int damage, Actor? source = null, DamageFlags flags = DamageFlags.None, string? damageType = null, Actor? inflictor = null)
    {
        var forced = flags.HasFlag(DamageFlags.Forced);
        // God mode and invulnerability stop ordinary hits. A telefrag goes through both.
        var telefrag = damage >= TelefragDamage;
        var reducible = !telefrag || target.LaxTelefragDamage;
        damage = Math.Max(damage, 0);
        var originalDamage = damage;
        if (target.Spectral && !forced && !telefrag && inflictor is not { Spectral: true })
            return default;
        if (target.IsDead)
        {
            if (!target.Shootable && !target.Vulnerable)
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
        if (!target.CanTakeDamage) return default;
        if (ReferenceEquals(target, source) && reducible)
            damage = ScaleDamage(damage, target.SelfDamageFactor);
        if ((target.Invulnerable || target is PlayerPawn { GodMode: true })
            && !telefrag && !forced && !foilInvulnerability && !flags.HasFlag(DamageFlags.BypassInvulnerability))
            return (target is not PlayerPawn || target.NoDamage && target is not PlayerPawn { GodMode: true })
                && IsFakePain(target, inflictor, originalDamage)
                ? ReactToDamage(target, source, inflictor, flags, damageType, 0, 0, 0, originalDamage) : default;
        var attacker = source ?? inflictor;
        if (inflictor is { PierceArmor: true }) flags |= DamageFlags.BypassArmor;
        if (target.Brain is { Charging: true })
            target.VelocityX = target.VelocityY = target.VelocityZ = default;
        if (target.Dormant && !forced) return default;
        if (!forced && !telefrag && attacker != null && !target.CanAttackHurtFrom(attacker))
            return default;
        if (!forced && reducible)
        {
            if (damage > 1 && target.Simulation is { } skillSimulation)
            {
                var factor = target is PlayerPawn ? skillSimulation.PlayerDamageFactor
                    : target.Friendly ? skillSimulation.FriendlyDamageFactor : skillSimulation.MonsterDamageFactor;
                var scaled = target is PlayerPawn ? damage * skillSimulation.SkillDamageFactor * factor : damage * factor;
                damage = (int)Math.Clamp(scaled, 0, int.MaxValue);
            }
            if (inflictor != null)
            {
                if (inflictor.Spectral && !inflictor.IsPuff
                    && (target is PlayerPawn
                        ? target.Simulation?.GameMode != SpawnGameMode.Deathmatch && inflictor.FriendPlayer > 0
                        : target.Spectral && inflictor.FriendPlayer == 0 && !target.IsHostile(inflictor)))
                    return default;
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
            if (damage < 0) return default;
            damage = target.TakeSpecialDamage(inflictor, source, damage, damageType, flags);
            if (damage <= 0)
            {
                if (damage == 0 && (IsFakePain(target, inflictor, originalDamage)
                    || originalModifierDamage == 0))
                    return ReactToDamage(target, source, inflictor, flags, damageType, 0, 0, 0, originalDamage);
                return default;
            }
        }
        // Native clears DMG_FORCED at the player protection boundary, after special damage.
        if (target is PlayerPawn && target.Simulation is { } sectorSimulation
            && target.SectorIndex >= 0 && target.SectorIndex < sectorSimulation.Level.Sectors.Count
            && sectorSimulation.Level.Sectors[target.SectorIndex].DamageEndsLevel && damage >= target.Health)
            damage = target.Health - 1;
        if (target is PlayerPawn protectedPlayer && (protectedPlayer.GodMode2 || protectedPlayer.Buddha2 || protectedPlayer.NoDamage))
        {
            forced = false;
            flags &= ~DamageFlags.Forced;
            if (protectedPlayer.GodMode2) return default;
            if (!telefrag && (protectedPlayer.Invulnerable || protectedPlayer.GodMode)
                && !flags.HasFlag(DamageFlags.BypassInvulnerability))
                return default;
        }
        if (target.NoDamage && !forced)
            return ReactToDamage(target, source, inflictor, flags, damageType, 0, 0, 0, originalDamage);
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
        var healthDamage = telefrag && !target.LaxTelefragDamage && target is PlayerPawn ? damage : damage - absorbed;
        if (healthDamage <= 0) return IsFakePain(target, inflictor, originalDamage) || forced
            ? ReactToDamage(target, source, inflictor, flags, damageType, 0, absorbed, 0, originalDamage)
            : new DamageResult(0, absorbed, false);
        if (!forced && (!telefrag || target.LaxTelefragDamage) && target is PlayerPawn emergencyPlayer)
            AutoUseEmergencyHealth(emergencyPlayer, healthDamage);
        var before = target.Health;
        if (target is PlayerPawn flashPlayer)
            flashPlayer.DamageCount = (int)Math.Min(100L, (long)flashPlayer.DamageCount + healthDamage);
        target.LastDamageSourceId = source?.Id;
        if (target is PlayerPawn damagedPlayer) damagedPlayer.LastDamageType = damageType;
        target.DamageTypeReceived = damageType;
        target.DeathInflictor = inflictor;
        // P_DamageMobj subtracts the post-armor remainder and keeps the negative overkill.
        // GetGibHealth is -spawn health. Health equal to that threshold is not an extreme death.
        var next = (long)before - healthDamage;
        if (!forced && next is > 0 and < 50 && target is PlayerPawn strifePlayer
            && target.Simulation is { GameMode: not SpawnGameMode.Deathmatch, DisableAutoHealth: false } strifeSimulation)
            next = strifePlayer.Inventory.AutoUseStrifeHealth((int)next, PickupCatalog.ResolveHealthMaximum(strifePlayer), strifeSimulation.HealthFactor);
        target.DeathDamageAmount = healthDamage;
        // Clamp before assigning Health. The setter enters death as soon as health crosses 0.
        if (next <= 0 && target is PlayerPawn && SurvivesByBuddha(target, telefrag ? TelefragDamage : damage, flags, inflictor))
            next = 1;
        try
        {
            target.SetDamageHealth((int)Math.Clamp(next, int.MinValue, int.MaxValue), source,
                () =>
                {
                    Drain(target, source, healthDamage, damageType);
                    return target.Health <= 0 && target is not PlayerPawn && SurvivesByBuddha(target, damage, flags, inflictor)
                        ? 1 : target.Health;
                });
        }
        finally
        {
            target.DamageTypeReceived = null;
            target.DeathInflictor = null;
        }
        var lost = before - target.Health;
        var dealt = healthDamage;
        return ReactToDamage(target, source, inflictor, flags, damageType, lost, absorbed, dealt, originalDamage);
    }

    private static DamageResult ReactToDamage(Actor target, Actor? source, Actor? inflictor,
        DamageFlags flags, string? damageType, int lost, int absorbed, int dealt, int originalDamage)
    {
        // Native player protection rejects reactions before wound-state selection.
        if (target is PlayerPawn player && (player.GodMode2 || player.NoPain
            || player.GodMode && dealt < TelefragDamage))
            return new DamageResult(lost, absorbed, target.IsDead);
        var forcedPain = inflictor is { ForcePain: true };
        var fakePain = IsFakePain(target, inflictor, originalDamage);
        if (!target.IsDead && target.WoundHealth > 0 && target.Health <= target.WoundHealth)
        {
            var wound = target.WoundStateFor(damageType);
            if (wound >= 0)
            {
                target.States.Enter(target, wound);
                return new DamageResult(lost, absorbed, target.IsDead);
            }
        }
        var painType = !string.IsNullOrEmpty(inflictor?.PainType)
            && !string.Equals(inflictor.PainType, "None", StringComparison.OrdinalIgnoreCase)
            ? inflictor.PainType : damageType;
        var painChance = target.PainChanceFor(painType);
        var painState = target.PainStateFor(painType);
        // MF6_FORCEPAIN skips the threshold and the pain roll. MF5_NOPAIN, MF5_PAINLESS, and DMG_NO_PAIN still block.
        var painless = target.NoPain || inflictor is { Painless: true } || flags.HasFlag(DamageFlags.NoPain);
        var painReaction = !target.IsDead && !painless && target.Brain?.Charging != true
            && (target is PlayerPawn || target.Simulation?.NoMonsterPain != true)
            && (forcedPain || ((dealt > 0 || fakePain) && dealt >= target.PainThreshold
                && RollPainChance(target, painChance)))
            && TryReactToPain(target, painState, painType);
        if (!target.IsDead)
        {
            if (target is not PlayerPawn && (dealt > 0 || forcedPain || fakePain))
                target.ReactionTime = 0;
            target.Brain?.WakeOnDamage(target, source, dealt, forcedPain || fakePain);
        }
        if (painReaction && ShouldMarkJustHit(target, source))
            target.JustHit = true;
        return new DamageResult(lost, absorbed, target.IsDead);
    }

    private static bool IsFakePain(Actor target, Actor? inflictor, int originalDamage) =>
        (target.AllowPain || target.NoDamage) && originalDamage > 0 || inflictor is { CausePain: true };

    private static int ScaleDamage(int damage, Fixed factor) =>
        (int)Math.Clamp((long)damage * factor.Raw / 65536, int.MinValue, int.MaxValue);

    private static bool RollPainChance(Actor target, int chance) => target.Simulation is { } simulation
        ? simulation.NextPainChanceByte() < chance
        : chance >= 256;

    internal static bool TriggerPainChance(Actor target, string? damageType, bool forcedPain)
    {
        if (target.NoPain || target.Health < 1) return false;
        if (!forcedPain && !RollPainChance(target, target.PainChanceFor(damageType))) return false;
        var state = target.PainStateFor(damageType);
        var hasAnimation = target.States.HasState(state);
        return TryReactToPain(target, state, damageType) && hasAnimation;
    }

    /// <summary>Electric and poison cloud pain use independent native random streams.</summary>
    private static bool TryReactToPain(Actor target, int painState, string? damageType)
    {
        var previous = target.PainDamageTypeContext;
        target.PainDamageTypeContext = damageType;
        try { return ReactToPain(target, painState, damageType); }
        finally { target.PainDamageTypeContext = previous; }
    }

    private static bool ReactToPain(Actor target, int painState, string? damageType)
    {
        if (!string.Equals(damageType, "Electric", StringComparison.OrdinalIgnoreCase))
        {
            if (target.States.HasState(painState)) target.States.Enter(target, painState);
            if (string.Equals(damageType, "PoisonCloud", StringComparison.OrdinalIgnoreCase)
                && target.IsMonster && target.Simulation is { } simulation && simulation.NextPoisonDamageByte() < 128)
                ActorSoundActions.Howl(target);
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
        if (target.IsMonster && target.Simulation.NextFlickerRandom() < 128) ActorSoundActions.Howl(target);
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

    private static void AutoUseEmergencyHealth(PlayerPawn target, int damage)
    {
        if (damage < target.Health || target.Simulation is not { } simulation
            || !simulation.AutoUseHealth && simulation.GameMode != SpawnGameMode.Deathmatch) return;
        var gain = target.Inventory.AutoUseHealth(damage - target.Health + 1, simulation.AutoUseHealth);
        if (gain > 0) target.Health = (int)Math.Min(int.MaxValue, (long)target.Health + gain);
    }

    /// <summary>Ordinary Buddha is defeated by forced damage and telefrags; Buddha2 survives both.</summary>
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
        if (postArmor <= 0 || source is not PlayerPawn player || player.EffectiveDrainStrength <= 0
            || target.DontDrain || ReferenceEquals(target, player))
            return;
        var amount = (int)(player.EffectiveDrainStrength * postArmor);
        amount = player.OnDrain(target, amount, damageType);
        if (PickupCatalog.GiveHealth(player, amount) && player.Simulation is not null)
            ActorSoundActions.StartSound(player, "*drainhealth", channel: 3);
    }
}
