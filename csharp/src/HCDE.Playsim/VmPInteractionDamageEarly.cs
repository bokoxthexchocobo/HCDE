namespace HCDE.Playsim;

/// <summary>Early <c>DamageMobj</c> gates from <c>p_interaction.cpp</c> (1098–1280).</summary>
public static class VmPInteractionDamageEarly
{
    public const int DmgForced = 1;
    public const int Mf2Dormant = 1;
    public const int Mf7Laxtelefragdmg = 1;
    public const int MfFriendly = 1;
    public const int Mf4Spectral = 1;
    public const int Mf9Ispuff = 1;

    public static bool TelefragDamage(int raw, int threshold) => raw >= threshold;

    public static bool LaxTelefragAllowsReduction(bool telefrag, int flags7) =>
        !telefrag || (flags7 & Mf7Laxtelefragdmg) != 0;

    public static bool DormantBlocks(bool dormant, int flags) =>
        dormant && (flags & DmgForced) == 0;

    public static bool PlayerSkillScale(bool player, int damage) => player && damage > 1;

    public static bool MonsterScale(bool player, bool friendly, int damage) =>
        !player && damage > 1;

    public static bool SpectralPlayerBlock(bool spectralPuff, bool player, bool dm, int friendPlayer) =>
        spectralPuff && player && !dm && friendPlayer > 0;

    public static bool SpectralActorBlock(bool spectralPuff, bool spectralTarget, int friendPlayer, bool hostile) =>
        spectralPuff && spectralTarget && friendPlayer == 0 && !hostile;

    public static bool SpecialDamageCancelled(int damage) => damage < 0;

public static bool InvulnBlocks(bool invuln, int flags) => invuln && (flags & DmgForced) == 0;

public readonly record struct DamageEarlyContext(
        bool TargetNull,
        bool Shootable,
        bool Vulnerable,
        bool Dormant,
        int Flags,
        int RawDamage,
        int TelefragThreshold,
        int TargetFlags7,
        bool Player,
        bool Friendly,
        bool SpectralInflictor,
        bool SpectralTarget,
        bool Deathmatch,
        int FriendPlayer,
        bool Hostile,
        int SpecialDamage);

    public static bool NonShootableTarget(bool shootable, bool vulnerable) => shootable || vulnerable;

    public static int ScalePlayerDamage(int damage, double factor) => (int)(damage * factor);

    public static int ScaleMonsterDamage(int damage, double factor) => (int)(damage * factor);

    public static int ScaleFriendlyDamage(int damage, double factor) => (int)(damage * factor);

public static bool ApplyTelefragUnreduced(bool telefrag, int flags7) =>
        telefrag && (flags7 & Mf7Laxtelefragdmg) == 0;

    public readonly record struct SkillScaleContext(
        bool Player,
        bool Friendly,
        int Damage,
        double SkillFactor,
        double MobjFactor,
        double FriendlyFactor);

    public static int EvaluateSkillScaledDamage(SkillScaleContext ctx)
    {
        if (ctx.Damage <= 1) return ctx.Damage;
        if (ctx.Player) return ScalePlayerDamage(ctx.Damage, ctx.SkillFactor);
        if (ctx.Friendly) return ScaleFriendlyDamage(ctx.Damage, ctx.FriendlyFactor);
        return ScaleMonsterDamage(ctx.Damage, ctx.MobjFactor);
    }

    public static int EvaluateEarlyDamage(DamageEarlyContext ctx)
    {
        if (ctx.TargetNull || !NonShootableTarget(ctx.Shootable, ctx.Vulnerable)) return -1;
        if (DormantBlocks(ctx.Dormant, ctx.Flags)) return -1;
        var telefrag = TelefragDamage(ctx.RawDamage, ctx.TelefragThreshold);
        if (ApplyTelefragUnreduced(telefrag, ctx.TargetFlags7)) { }
        if (SpectralPlayerBlock(ctx.SpectralInflictor, ctx.Player, ctx.Deathmatch, ctx.FriendPlayer)) return -1;
        if (SpectralActorBlock(ctx.SpectralInflictor, ctx.SpectralTarget, ctx.FriendPlayer, ctx.Hostile)) return -1;
        if (SpecialDamageCancelled(ctx.SpecialDamage)) return -1;
        return ctx.RawDamage;
    }
}
