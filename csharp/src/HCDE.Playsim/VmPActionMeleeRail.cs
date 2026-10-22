namespace HCDE.Playsim;

/// <summary>Custom melee/combo/rail actions from <c>p_actionfunctions.cpp</c> (1172–1374).</summary>
public static class VmPActionMeleeRail
{
    public const int CrfDontaim = 0;
    public const int CrfAimparallel = 1;
    public const int CrfAimdirect = 2;
    public const int MfStealth = 1;

    public sealed class MeleeContext
    {
        public object? Target { get; init; }
        public Action? FaceTarget { get; init; }
        public Func<bool>? InMeleeRange { get; init; }
        public Action? PlayMeleeSound { get; init; }
        public Action? PlayMissSound { get; init; }
        public Func<int>? DamageTarget { get; init; }
        public bool Bleed { get; init; }
        public Action<int>? TraceBleed { get; init; }
    }

    public static bool CustomMeleeAttack(MeleeContext ctx, int damage)
    {
        if (ctx.Target == null)
            return false;
        ctx.FaceTarget?.Invoke();
        if (ctx.InMeleeRange?.Invoke() == true)
        {
            ctx.PlayMeleeSound?.Invoke();
            var newDam = ctx.DamageTarget?.Invoke(damage) ?? 0;
            if (ctx.Bleed)
                ctx.TraceBleed?.Invoke(newDam > 0 ? newDam : damage);
        }
        else
        {
            ctx.PlayMissSound?.Invoke();
        }

        return true;
    }

    public sealed class ComboContext : MeleeContext
    {
        public bool HasMissileType { get; init; }
        public Func<object?>? SpawnMissile { get; init; }
        public Action<object?>? CheckMissileSpawn { get; init; }
    }

    public static bool CustomComboAttack(ComboContext ctx, int damage)
    {
        if (ctx.Target == null)
            return false;
        ctx.FaceTarget?.Invoke();
        if (ctx.InMeleeRange?.Invoke() == true)
            return CustomMeleeAttack(ctx, damage);
        if (!ctx.HasMissileType)
            return true;
        var missile = ctx.SpawnMissile?.Invoke();
        if (missile != null)
            ctx.CheckMissileSpawn?.Invoke(missile);
        return true;
    }

    public sealed class RailContext
    {
        public object? Target { get; init; }
        public int Aim { get; init; }
        public int Flags { get; init; }
        public bool Stealth { get; init; }
        public Action? ClearAmbush { get; init; }
        public Action? RevealStealth { get; init; }
        public Action<double>? AimLineAttack { get; init; }
        public Action? RailAttack { get; init; }
        public Action? RestorePose { get; init; }
    }

    public static bool CustomRailgun(RailContext ctx)
    {
        if (ctx.Aim != 0 && ctx.Target == null)
            return false;
        if (ctx.Stealth)
            ctx.RevealStealth?.Invoke();
        ctx.ClearAmbush?.Invoke();
        if (ctx.Aim != 0)
            ctx.AimLineAttack?.Invoke(0);
        ctx.RailAttack?.Invoke();
        ctx.RestorePose?.Invoke();
        return true;
    }
}
