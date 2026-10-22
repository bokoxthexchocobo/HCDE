namespace HCDE.Playsim;

/// <summary>Debris and particle spawn actions from <c>p_actionfunctions.cpp</c> (1675–1846).</summary>
public static class VmPActionDebrisParticle
{
    public const int SpfRelang = 1;
    public const int SpfRelpos = 2;
    public const int SpfRelvel = 4;
    public const int SpfRelaccel = 8;

    public sealed class DebrisContext
    {
        public int DefaultHealth { get; init; }
        public Func<int, int>? RandomDebris { get; init; }
        public Func<double, double, double, object?>? SpawnDebris { get; init; }
        public Action<object?, int>? SetOwnedState { get; init; }
        public Action<object?, double, double, double>? SetVelocity { get; init; }
        public bool TransferTranslation { get; init; }
        public Action<object?, int>? CopyTranslation { get; init; }
    }

    public static int SpawnDebris(DebrisContext ctx, double multH, double multV)
    {
        if (multH <= 0) multH = 1;
        if (multV <= 0) multV = 1;
        var count = 0;
        for (var i = 0; i < ctx.DefaultHealth; i++)
        {
            var mo = ctx.SpawnDebris?.Invoke(0, 0, 0);
            if (mo == null)
                continue;
            count++;
            if (ctx.TransferTranslation)
                ctx.CopyTranslation?.Invoke(mo, 0);
            ctx.SetOwnedState?.Invoke(mo, i);
            var vx = multH * (ctx.RandomDebris?.Invoke(0) ?? 0) / 64.0;
            var vy = multH * (ctx.RandomDebris?.Invoke(1) ?? 0) / 64.0;
            var vz = multV * (((ctx.RandomDebris?.Invoke(2) ?? 0) & 7) + 5);
            ctx.SetVelocity?.Invoke(mo, vx, vy, vz);
        }

        return count;
    }

    public sealed class ParticleContext
    {
        public int Lifetime { get; init; }
        public int Flags { get; init; }
        public double ActorYaw { get; init; }
        public double Size { get; init; }
        public double StartAlpha { get; init; }
        public Action? SpawnParticle { get; init; }
        public Action? SpawnParticleEx { get; init; }
    }

    public static bool SpawnParticle(ParticleContext ctx)
    {
        if (ctx.Lifetime == 0)
            return false;
        var alpha = Math.Clamp(ctx.StartAlpha, 0, 1);
        var size = Math.Abs(ctx.Size);
        ctx.SpawnParticle?.Invoke();
        return alpha >= 0 && size >= 0;
    }

    public static bool SpawnParticleEx(ParticleContext ctx, int style)
    {
        if (ctx.Lifetime == 0)
            return false;
        if (style < 0)
            style = 0;
        ctx.SpawnParticleEx?.Invoke();
        return true;
    }

    public static (double X, double Y) RotateOffset(double xoff, double yoff, double yawSin, double yawCos, bool relative) =>
        relative
            ? (xoff * yawCos + yoff * yawSin, xoff * yawSin - yoff * yawCos)
            : (xoff, yoff);

    public static double DebrisZo(int random, double height, double bob) =>
        random * height / 256 + bob;
}
