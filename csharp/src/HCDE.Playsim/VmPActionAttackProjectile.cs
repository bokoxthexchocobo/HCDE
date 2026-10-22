namespace HCDE.Playsim;

/// <summary>Bullet/jump/radius/spawn-projectile actions from <c>p_actionfunctions.cpp</c> (894–1165).</summary>
public static class VmPActionAttackProjectile
{
    public const int RdsfBfgdamage = 1;

    public sealed class BulletContext
    {
        public object? Target { get; init; }
        public Action? FaceTarget { get; init; }
        public Func<double>? AimSlope { get; init; }
        public Action? PlayAttackSound { get; init; }
        public Func<int>? MissileDamageCount { get; init; }
        public Func<int, int>? RandomBulletDamage { get; init; }
        public Action<double, double, double, int>? LineAttack { get; init; }
    }

    public static bool BulletAttack(BulletContext ctx)
    {
        if (ctx.Target == null)
            return false;
        ctx.FaceTarget?.Invoke();
        var slope = ctx.AimSlope?.Invoke() ?? 0;
        ctx.PlayAttackSound?.Invoke();
        var shots = ctx.MissileDamageCount?.Invoke(0) ?? 0;
        for (var i = shots; i > 0; i--)
        {
            var damage = ctx.RandomBulletDamage?.Invoke(i) ?? 3;
            ctx.LineAttack?.Invoke(0, 0, slope, damage);
        }

        return true;
    }

    public static object? Jump(int maxChance, int roll, object? jumpState, object? nullState) =>
        maxChance >= 256 || roll < maxChance ? jumpState : nullState;

    public sealed class RadiusSelfContext
    {
        public object? Target { get; init; }
        public double Distance { get; init; }
        public int Damage { get; init; }
        public int Flags { get; init; }
        public double ActualDistance { get; init; }
        public Func<int, int>? BfgSelfDamageRoll { get; init; }
        public Action<object?, int, int>? DamageTarget { get; init; }
        public Action<int, int>? TraceBleed { get; init; }
    }

    public static bool RadiusDamageSelf(RadiusSelfContext ctx)
    {
        if (ctx.Target == null || ctx.ActualDistance >= ctx.Distance)
            return false;
        var steps = ctx.Damage - (int)(ctx.Damage * ctx.ActualDistance / ctx.Distance);
        int actual;
        if ((ctx.Flags & RdsfBfgdamage) != 0)
        {
            actual = 0;
            for (var i = 0; i < steps; i++)
                actual += (ctx.BfgSelfDamageRoll?.Invoke(i) ?? 0) + 1;
        }
        else
        {
            actual = steps;
        }

        var newDam = ctx.DamageTarget?.Invoke(ctx.Target, actual, 0) ?? 0;
        ctx.TraceBleed?.Invoke(newDam > 0 ? newDam : actual, actual);
        return true;
    }

    public const int CmfAbsoluteangle = 128;
    public const int CmfAbsolutepitch = 16;
    public const int CmfOffsetpitch = 32;
    public const int CmfSavepitch = 64;
    public const int CmfChecktargetdead = 8;
    public const int Mf2Seekermissile = 1;

    public sealed class SpawnProjectileContext
    {
        public bool HasType { get; init; }
        public object? Ref { get; init; }
        public int AimMode { get; init; }
        public Func<object?>? SpawnMissile { get; init; }
        public Action<object?, object?>? OnSpawned { get; init; }
        public Action? AbortToSeeState { get; init; }
        public bool CheckTargetDead { get; init; }
    }

    public static object? SpawnProjectile(SpawnProjectileContext ctx)
    {
        if (ctx.Ref != null || ctx.AimMode == 2)
        {
            if (!ctx.HasType)
                return null;
            var missile = ctx.SpawnMissile?.Invoke();
            if (missile != null)
                ctx.OnSpawned?.Invoke(missile, ctx.Ref);
            return missile;
        }

        if (ctx.CheckTargetDead)
            ctx.AbortToSeeState?.Invoke();
        return null;
    }
}
