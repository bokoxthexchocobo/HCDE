namespace HCDE.Playsim;

/// <summary>Whole static <c>DamageMobj</c> from <c>p_interaction.cpp</c> (1098–1571).</summary>
public static class VmPInteractionDamageMobj
{
    public const int DmgForced = 1;
    public const int DmgNoArmor = 1 << 1;
    public const int DmgNoEnhance = 1 << 2;
    public const int DmgNoProtect = 1 << 3;
    public const int DmgNoFactor = 1 << 4;
    public const int DmgThrustless = 1 << 5;
    public const int DmgFoilinvul = 1 << 6;
    public const int DmgFoilbuddha = 1 << 7;

    public const int MfShootable = 1;
    public const int Mf6Vulnerable = 1;
    public const int Mf4Spectral = 1;
    public const int Mf2Invulnerable = 1;
    public const int Mf2Dormant = 2;
    public const int MfStealth = 1;
    public const int MfSkullfly = 1;
    public const int MfFriendly = 1;
    public const int MfNoclip = 1;
    public const int Mf5Nodamage = 1;
    public const int Mf5Piercearmor = 1;
    public const int Mf5Dontdrain = 1;
    public const int Mf5Specialfiredamage = 1;
    public const int Mf5Summonedmonster = 1;
    public const int Mf2Nodmgthrust = 1;
    public const int Mf7Laxtelefragdmg = 1;
    public const int Mf7Dontthrust = 1;
    public const int Mf7Buddha = 1;
    public const int Mf7Foilbuddha = 1;
    public const int Mf9Ispuff = 1;
    public const int Mf3Foilinvul = 1;
    public const int MfIcecorpse = 1;
    public const int Mf6Shattering = 1;
    public const int Mf7Iceshatter = 1 << 2;

    public const int CfGodmode = 1;
    public const int CfGodmode2 = 1 << 1;
    public const int CfBuddha2 = 1 << 2;
    public const int SecfEndlevel = 1;

    public sealed class DamageActor
    {
        public int Health { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasInventory { get; set; }
        public bool AlternativeNull { get; set; } = true;
        public double SelfDamageFactor { get; set; } = 1;
        public double TeamDamage { get; set; } = 1;
        public int SectorFlags { get; set; }
        public int Special1 { get; set; }
        public int DamageTypeReceived { get; set; }
        public int DamageType { get; set; }
        public int Tics { get; set; }
        public double Alpha { get; set; } = 1;
        public int VisDir { get; set; }
        public bool VelZeroed { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerCheats { get; set; }
        public int PlayerDamageCount { get; set; }
        public int PlayerLastDamageType { get; set; }
        public object? PlayerAttacker { get; set; }
        public bool BotAllround { get; set; }
        public bool Died { get; set; }
    }

    public sealed class DamageContext
    {
        public required DamageActor Target { get; init; }
        public bool TargetNull { get; init; }
        public bool InflictorNull { get; init; } = true;
        public bool SourceNull { get; init; } = true;
        public bool SameTargetSource { get; init; }
        public bool Teammate { get; init; }
        public bool Deathmatch { get; init; }
        public int FriendPlayer { get; init; }
        public bool Hostile { get; init; }
        public int TelefragThreshold { get; init; } = 1_000_000;
        public int Mod { get; init; }
        public int ModIce { get; init; }
        public int ModFire { get; init; }
        public int ModReflection { get; init; }
        public int Flags { get; set; }
        public double SkillDamageFactor { get; init; } = 1;
        public double MobjDamageFactor { get; init; } = 1;
        public double FriendlyDamageFactor { get; init; } = 1;
        public double SourceDamageMultiply { get; init; } = 1;
        public bool AutoUseHealthSkill { get; init; }
        public bool VoodooCompat { get; init; }
        public int BuddhaLevel { get; init; }
        public bool PlayerNoThrustWhenInvul { get; init; }
        public bool SourceSamePlayerVictim { get; init; }
        public double BestDrainStrength { get; init; }
        public int InflictorFlags2 { get; init; }
        public int InflictorFlags3 { get; init; }
        public int InflictorFlags4 { get; init; }
        public int InflictorFlags5 { get; init; }
        public int InflictorFlags7 { get; init; }
        public int InflictorFlags9 { get; init; }
        public int InflictorDeathType { get; init; }
        public bool InflictorDeathTypeSet { get; init; }
        public int SourceFlags2 { get; init; }
        public bool SourceHasPlayer { get; init; }
        public bool SourceSummoned { get; init; }
        public bool SourceTracerPlayer { get; init; }
        public bool SourceTracerIsMo { get; init; }
        public object? Inflictor { get; init; }
        public object? Source { get; set; }
        public object? KillCreditSource { get; set; }
        public Func<int, int>? CallDoSpecialDamage { get; init; }
        public Func<int, bool, int>? GetModifiedDamage { get; init; }
        public Func<int, int>? ApplyDamageFactor { get; init; }
        public Func<int>? CallTakeSpecialDamage { get; init; }
        public Func<int, int>? AbsorbDamage { get; init; }
        public Action? ApplyKickback { get; init; }
        public Action<int, int>? ReflectDamage { get; init; }
        public Func<(int Reflect, bool ReflectType)>? PickReflection { get; init; }
        public Action<int>? AutoUseHealthItems { get; init; }
        public Action? AutoUseStrifeHealth { get; init; }
        public Func<int, bool>? GiveBody { get; init; }
        public Action? PlayDrainSound { get; init; }
        public Func<int, int>? OnDrainVirtual { get; init; }
        public Action<object?, object?, object?, int, int, int>? WorldThingDamaged { get; init; }
        public Action<object?, object?, int, int>? CallDie { get; init; }
    }

    public static int DamageMobj(DamageContext ctx, int damage, ref bool needEvent)
    {
        var target = ctx.Target;
        var flags = ctx.Flags;
        var rawDamage = damage;
        if (damage < 0) damage = 0;

        if (ctx.TargetNull || !Shootable(target.Flags, target.Flags6))
            return -1;

        var telefrag = rawDamage >= ctx.TelefragThreshold;
        var mod = ctx.Mod;

        if ((target.Flags4 & Mf4Spectral) != 0 && (flags & DmgForced) == 0 && !telefrag)
        {
            if (ctx.InflictorNull || (ctx.InflictorFlags4 & Mf4Spectral) == 0)
                return -1;
        }

        if (target.Health <= 0)
        {
            if (!ctx.InflictorNull && mod == ctx.ModIce && (ctx.InflictorFlags7 & Mf7Iceshatter) == 0)
                return -1;
            if ((target.Flags & MfIcecorpse) != 0)
            {
                target.Tics = 1;
                target.Flags6 |= Mf6Shattering;
                target.VelZeroed = true;
            }
            return -1;
        }

        if (ctx.SameTargetSource && (!telefrag || (target.Flags7 & Mf7Laxtelefragdmg) != 0))
            damage = (int)(damage * target.SelfDamageFactor);

        if ((target.Flags2 & Mf2Invulnerable) != 0 && !telefrag && (flags & DmgForced) == 0)
        {
            if (!target.HasPlayer)
            {
                if (ctx.InflictorNull || ((ctx.InflictorFlags3 & Mf3Foilinvul) == 0 && (flags & DmgFoilinvul) == 0))
                    return 0;
            }
            else if (ctx.PlayerNoThrustWhenInvul)
                return 0;
        }

        if (!ctx.InflictorNull && (ctx.InflictorFlags5 & Mf5Piercearmor) != 0)
            flags |= DmgNoArmor;

        if ((target.Flags & MfStealth) != 0)
        {
            target.Alpha = 1;
            target.VisDir = -1;
        }

        if ((target.Flags & MfSkullfly) != 0)
            target.VelZeroed = true;

        if ((flags & DmgForced) == 0)
        {
            if ((target.Flags2 & Mf2Dormant) != 0)
                return -1;

            if (!telefrag || (target.Flags7 & Mf7Laxtelefragdmg) != 0)
            {
                damage = ScaleSkillDamage(target.HasPlayer, (target.Flags & MfFriendly) != 0, damage,
                    ctx.SkillDamageFactor, ctx.MobjDamageFactor, ctx.FriendlyDamageFactor);

                if (!ctx.InflictorNull)
                {
                    if ((ctx.InflictorFlags4 & Mf4Spectral) != 0 && (ctx.InflictorFlags9 & Mf9Ispuff) == 0)
                    {
                        if (target.HasPlayer && !ctx.Deathmatch && ctx.FriendPlayer > 0)
                            return -1;
                        if (!target.HasPlayer && (target.Flags4 & Mf4Spectral) != 0
                            && ctx.FriendPlayer == 0 && !ctx.Hostile)
                            return -1;
                    }

                    damage = ctx.CallDoSpecialDamage?.Invoke(damage) ?? damage;
                    if (damage < 0) return -1;

                    var oldDam = damage;
                    if (damage > 0 && !ctx.SourceNull)
                    {
                        damage = (int)(damage * ctx.SourceDamageMultiply);
                        if (damage > 0 && (flags & DmgNoEnhance) == 0)
                            damage = ctx.GetModifiedDamage?.Invoke(damage, false) ?? damage;
                    }

                    if (damage > 0 && (flags & DmgNoProtect) == 0)
                        damage = ctx.GetModifiedDamage?.Invoke(damage, true) ?? damage;
                    if (damage > 0 && (flags & DmgNoFactor) == 0)
                        damage = ctx.ApplyDamageFactor?.Invoke(damage) ?? damage;
                    if (damage >= 0)
                        damage = ctx.CallTakeSpecialDamage?.Invoke() ?? damage;
                    if (damage == 0 && oldDam > 0) return 0;
                }
            }

            if ((target.Flags5 & Mf5Nodamage) != 0)
                damage = 0;
        }

        if (damage < 0) return -1;

        mod = ApplyReflection(ctx, target, damage, mod);
        ctx.Flags = flags;

        if (KickbackEligible(ctx, target, flags))
            ctx.ApplyKickback?.Invoke();

        if ((flags & DmgForced) == 0 && !ctx.SourceNull && ctx.Teammate)
        {
            if (!telefrag || (target.Flags7 & Mf7Laxtelefragdmg) != 0)
            {
                damage = (int)(damage * target.TeamDamage);
                if (damage <= 0) return damage < 0 ? -1 : 0;
            }
        }

        if (target.HasPlayer)
        {
            var playerOutcome = ApplyPlayerDamage(ctx, ref flags, ref damage, telefrag);
            ctx.Flags = flags;
            if (playerOutcome.HasValue) return playerOutcome.Value;
        }
        else
        {
            if ((flags & (DmgNoArmor | DmgForced)) == 0 && target.HasInventory && damage > 0)
            {
                damage = ctx.AbsorbDamage?.Invoke(damage) ?? damage;
                if (damage <= 0) return damage < 0 ? -1 : 0;
            }
            target.Health -= damage;
        }

        target.DamageTypeReceived = mod;

        if (!ctx.SourceNull && ctx.SourceHasPlayer && (target.Flags5 & Mf5Dontdrain) == 0)
            ApplyDrain(ctx, damage);

        if (target.Health <= 0)
        {
            if (TryMonsterBuddha(ctx, flags, telefrag))
                target.Health = 1;
            else
                return FinishDeath(ctx, target, damage, mod, ref needEvent);
        }

        return Math.Max(0, damage);
    }

    public static bool Shootable(int flags, int flags6) =>
        (flags & MfShootable) != 0 || (flags6 & Mf6Vulnerable) != 0;

    public static int ScaleSkillDamage(bool player, bool friendly, int damage,
        double skill, double mobj, double friendlyFactor)
    {
        if (damage <= 1) return damage;
        if (player) return (int)(damage * skill);
        if (friendly) return (int)(damage * friendlyFactor);
        return (int)(damage * mobj);
    }

    public static bool KickbackEligible(DamageContext ctx, DamageActor target, int flags) =>
        !ctx.InflictorNull && !ctx.SameTargetSource
        && (target.Flags & MfNoclip) == 0
        && (ctx.InflictorFlags2 & Mf2Nodmgthrust) == 0
        && (flags & DmgThrustless) == 0
        && (target.Flags7 & Mf7Dontthrust) == 0
        && (ctx.SourceNull || !ctx.SourceHasPlayer || (ctx.SourceFlags2 & Mf2Nodmgthrust) == 0);

    public static int ApplyReflection(DamageContext ctx, DamageActor target, int damage, int mod)
    {
        if (!target.HasPlayer || damage <= 0 || ctx.SourceNull || mod == ctx.ModReflection || ctx.SameTargetSource)
            return mod;

        var pick = ctx.PickReflection?.Invoke() ?? (0, false);
        if (pick.Reflect <= 0) return mod;

        ctx.ReflectDamage?.Invoke(pick.Reflect, pick.ReflectType ? mod : ctx.ModReflection);
        return ctx.Mod;
    }

    public static int? ApplyPlayerDamage(DamageContext ctx, ref int flags, ref int damage, bool telefrag)
    {
        var target = ctx.Target;
        if ((target.PlayerCheats & (CfGodmode2 | CfBuddha2)) != 0 || (target.Flags5 & Mf5Nodamage) != 0)
            flags &= ~DmgForced;

        if (target.HasPlayer)
            target.BotAllround = true;

        if ((target.SectorFlags & SecfEndlevel) != 0 && damage >= target.Health)
            damage = target.Health - 1;

        if ((flags & DmgForced) == 0)
        {
            if ((!telefrag && ((target.Flags2 & Mf2Invulnerable) != 0 || (target.PlayerCheats & CfGodmode) != 0))
                || (target.PlayerCheats & CfGodmode2) != 0 || (target.Flags5 & Mf5Nodamage) != 0)
                return 0;

            if ((flags & DmgNoArmor) == 0 && target.HasInventory && damage > 0)
            {
                var newDam = ctx.AbsorbDamage?.Invoke(damage) ?? damage;
                if (!telefrag || (target.Flags7 & Mf7Laxtelefragdmg) != 0)
                    damage = newDam;
                if (damage <= 0) return damage < 0 ? -1 : 0;
            }

            if (damage >= target.PlayerHealth && !telefrag
                && (ctx.AutoUseHealthSkill || ctx.Deathmatch) && target.AlternativeNull)
                ctx.AutoUseHealthItems?.Invoke(damage - target.PlayerHealth + 1);
        }

        target.PlayerHealth -= damage;
        if (ctx.VoodooCompat)
            target.Health -= damage;
        else
            target.Health = target.PlayerHealth;

        if (target.PlayerHealth < 50 && !ctx.Deathmatch && (flags & DmgForced) == 0)
        {
            ctx.AutoUseStrifeHealth?.Invoke();
            if (!ctx.VoodooCompat)
                target.PlayerHealth = target.PlayerHealth;
        }

        if (target.PlayerHealth <= 0)
        {
            var buddha = ctx.BuddhaLevel;
            if ((flags & DmgForced) != 0) buddha = 0;
            if (telefrag && buddha == 1) buddha = 0;
            if (buddha != 0)
                target.PlayerHealth = target.Health = 1;
            else
                target.PlayerHealth = 0;
        }

        target.PlayerLastDamageType = ctx.Mod;
        target.PlayerAttacker = ctx.Source;
        target.PlayerDamageCount += damage;
        if (target.PlayerDamageCount > 100) target.PlayerDamageCount = 100;
        return null;
    }

    public static void ApplyDrain(DamageContext ctx, int damage)
    {
        if (ctx.SourceSamePlayerVictim) return;
        if (ctx.BestDrainStrength <= 0) return;
        var drain = (int)(ctx.BestDrainStrength * damage);
        if (ctx.OnDrainVirtual != null)
            drain = ctx.OnDrainVirtual(drain);
        if (ctx.GiveBody?.Invoke(drain) == true)
            ctx.PlayDrainSound?.Invoke();
    }

    public static bool TryMonsterBuddha(DamageContext ctx, int flags, bool telefrag) =>
        (flags & DmgForced) == 0 && (ctx.Target.Flags7 & Mf7Buddha) != 0 && !telefrag
        && (ctx.InflictorNull || (ctx.InflictorFlags7 & Mf7Foilbuddha) == 0)
        && (flags & DmgFoilbuddha) == 0;

    public static int FinishDeath(DamageContext ctx, DamageActor target, int damage, int mod, ref bool needEvent)
    {
        target.Special1 = damage;
        if (!ctx.InflictorNull && ctx.InflictorDeathTypeSet)
            mod = ctx.InflictorDeathType;

        target.DamageType = mod == ctx.ModFire
            ? ResolveFireDeath(ctx, target, damage)
            : mod;

        var killSource = ctx.Source;
        if (!ctx.SourceNull && ctx.SourceSummoned && ctx.SourceTracerPlayer && ctx.SourceTracerIsMo)
            killSource = ctx.KillCreditSource;

        var real = Math.Max(0, damage);
        ctx.WorldThingDamaged?.Invoke(target, ctx.Inflictor, killSource, real, mod, ctx.Flags);
        needEvent = false;
        ctx.CallDie?.Invoke(killSource, ctx.Inflictor, ctx.Flags, mod);
        target.Died = true;
        return real;
    }

    public static int ResolveFireDeath(DamageContext ctx, DamageActor target, int damage)
    {
        if (target.HasPlayer && target.AlternativeNull)
        {
            if (ctx.InflictorNull || (target.Health > -50 && damage > 25)
                || (ctx.InflictorFlags5 & Mf5Specialfiredamage) == 0)
                return ctx.ModFire;
            return ctx.Mod;
        }
        return ctx.ModFire;
    }
}
