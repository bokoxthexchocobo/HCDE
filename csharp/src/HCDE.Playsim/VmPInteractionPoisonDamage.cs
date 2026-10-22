namespace HCDE.Playsim;

/// <summary><c>P_PoisonMobj</c>, <c>P_PoisonPlayer</c>, and <c>P_PoisonDamage</c> from <c>p_interaction.cpp</c>.</summary>
public static class VmPInteractionPoisonDamage
{
    public const int Mf2Invulnerable = 1;
    public const int Mf3Foilinvul = 1;
    public const int Mf6Poisonalways = 1;
    public const int Mf6Additivepoisondamage = 1;
    public const int Mf6Additivepoisonduration = 1;
    public const int Mf5Nodamage = 1;
    public const int Mf7Laxtelefragdmg = 1;
    public const int CfGodmode = 1;
    public const int CfGodmode2 = 1 << 1;

    public sealed class PoisonActor
    {
        public int Flags2 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public int Health { get; set; }
        public int PoisonDamageReceived { get; set; }
        public int PoisonDurationReceived { get; set; }
        public int PoisonPeriodReceived { get; set; }
        public int PoisonDamageTypeReceived { get; set; }
        public object? Poisoner { get; set; }
        public bool AlternativeNull { get; set; } = true;
        public int DamageType { get; set; }
        public int Special1 { get; set; }
        public bool Died { get; set; }
    }

    public sealed class PoisonPlayerState
    {
        public int Cheats { get; set; }
        public int Health { get; set; }
        public int PoisonCount { get; set; }
        public object? Poisoner { get; set; }
        public int PoisonType { get; set; }
        public int PoisonPainType { get; set; }
        public object? Attacker { get; set; }
    }

    public sealed class PoisonContext
    {
        public bool MirrorTarget { get; init; }
        public bool MirrorInflictor { get; init; }
        public bool MirrorSource { get; init; }
        public bool VisualTarget { get; init; }
        public bool VisualInflictor { get; init; }
        public bool VisualSource { get; init; }
        public int TelefragThreshold { get; init; } = 1_000_000;
        public double SkillFactor { get; init; } = 1;
        public double PlayerDamageFactor { get; init; } = 1;
        public bool AutoUseHealth { get; init; }
        public bool Deathmatch { get; init; }
        public int ModFire { get; init; }
        public int LevelTime { get; init; }
        public bool PlayPainSound { get; init; }
        public Func<int, int>? GetModifiedDamage { get; init; }
        public Func<int, int>? ApplyDamageFactor { get; init; }
        public Action<int>? AutoUseHealthItems { get; init; }
        public Action? AutoUseStrifeHealth { get; init; }
        public Func<string?, string?>? FindPainState { get; init; }
        public Action<string?>? SetState { get; init; }
        public Action<object?, object?>? CallDie { get; init; }
        public int BuddhaLevel { get; init; }
    }

    public static bool MirrorOrVisualSkips(PoisonContext ctx) =>
        ctx.MirrorTarget || ctx.MirrorInflictor || ctx.MirrorSource
        || ctx.VisualTarget || ctx.VisualInflictor || ctx.VisualSource;

    public static void PoisonMobj(PoisonContext ctx, PoisonActor target, int inflictorFlags6, int inflictorFlags2,
        int inflictorFlags3, object? source, int damage, int duration, int period, int type)
    {
        if (MirrorOrVisualSkips(ctx)) return;

        if ((inflictorFlags6 & Mf6Poisonalways) == 0)
        {
            if ((target.Flags2 & Mf2Invulnerable) != 0)
            {
                if ((inflictorFlags3 & Mf3Foilinvul) == 0)
                    return;
            }
        }

        target.Poisoner = source;
        target.PoisonDamageTypeReceived = type;
        target.PoisonPeriodReceived = period;

        if ((inflictorFlags6 & Mf6Additivepoisondamage) != 0)
            target.PoisonDamageReceived += damage;
        else
            target.PoisonDamageReceived = damage;

        if ((inflictorFlags6 & Mf6Additivepoisonduration) != 0)
            target.PoisonDurationReceived += duration;
        else
            target.PoisonDurationReceived = duration;
    }

    public static bool PoisonPlayer(PoisonPlayerState player, PoisonActor mo, object? poisoner, object? source,
        int poison, bool teammate, double teamDamage, int poisonerDamageType, int poisonerPainType, bool poisonerNull)
    {
        if ((player.Cheats & CfGodmode) != 0 || (mo.Flags2 & Mf2Invulnerable) != 0
            || (player.Cheats & CfGodmode2) != 0 || (mo.Flags5 & Mf5Nodamage) != 0)
            return false;

        if (source != null && teammate)
            poison = (int)(poison * teamDamage);

        if (poison <= 0) return true;

        player.PoisonCount += poison;
        player.Poisoner = source;
        if (poisonerNull)
        {
            player.PoisonType = 0;
            player.PoisonPainType = 0;
        }
        else
        {
            player.PoisonType = poisonerDamageType;
            player.PoisonPainType = poisonerPainType != 0 ? poisonerPainType : poisonerDamageType;
        }

        if (player.PoisonCount > 100)
            player.PoisonCount = 100;
        return true;
    }

    public static void PoisonDamage(PoisonContext ctx, PoisonPlayerState player, PoisonActor target, object? source,
        int damage, int poisonType, int poisonPainType)
    {
        if (player.Health <= 0 || target.Health <= 0) return;

        var telefrag = damage >= ctx.TelefragThreshold && (target.Flags7 & Mf7Laxtelefragdmg) == 0;

        if ((player.Cheats & CfGodmode2) != 0 || (target.Flags5 & Mf5Nodamage) != 0
            || (!telefrag && ((target.Flags2 & Mf2Invulnerable) != 0 || (player.Cheats & CfGodmode) != 0)))
            return;

        damage = (int)(damage * ctx.SkillFactor * ctx.PlayerDamageFactor);
        damage = ctx.GetModifiedDamage?.Invoke(damage) ?? damage;
        damage = ctx.ApplyDamageFactor?.Invoke(damage) ?? damage;
        if (damage <= 0) return;

        if (damage >= player.Health && (ctx.AutoUseHealth || ctx.Deathmatch) && target.AlternativeNull)
            ctx.AutoUseHealthItems?.Invoke(damage - player.Health + 1);

        player.Health -= damage;
        if (player.Health < 50 && !ctx.Deathmatch)
            ctx.AutoUseStrifeHealth?.Invoke();
        if (player.Health < 0) player.Health = 0;
        player.Attacker = source;

        target.Health -= damage;
        if (target.Health <= 0)
        {
            var buddha = ctx.BuddhaLevel;
            if (telefrag && buddha == 1) buddha = 0;
            if (buddha != 0)
            {
                player.Health = target.Health = 1;
                return;
            }

            target.Special1 = damage;
            if (target.AlternativeNull)
            {
                if (poisonType == ctx.ModFire && target.Health > -50 && damage > 25)
                    target.DamageType = ctx.ModFire;
                else
                    target.DamageType = poisonType;
            }

            ctx.CallDie?.Invoke(source, source);
            target.Died = true;
            return;
        }

        if ((ctx.LevelTime & 63) == 0 && ctx.PlayPainSound)
        {
            var painState = ctx.FindPainState?.Invoke(poisonPainType.ToString());
            if (painState != null)
                ctx.SetState?.Invoke(painState);
        }
    }
}
