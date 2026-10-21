namespace HCDE.Playsim;

/// <summary>Fade-out/to, debris, and particle setup from <c>p_actionfunctions.cpp</c> (1598–1775).</summary>
public static class VmActionFadeSpawn
{
    public const int FtfRemove = 1 << 0;
    public const int FtfClamp = 1 << 1;
    public const int SpfRelPos = 1 << 0;
    public const int SpfRelVel = 1 << 1;
    public const int SpfRelAccel = 1 << 2;
    public const int SpfRelAng = 1 << 3;

    public static double FadeReduceDefault(double reduce) => reduce == 0 ? 0.1 : reduce;

    public static (double alpha, bool clampToZero, bool shouldRemove) StepFadeOut(double alpha, double reduce, int flags)
    {
        alpha -= reduce;
        if (alpha <= 0)
        {
            var clamp = (flags & FtfClamp) != 0;
            var remove = (flags & FtfRemove) != 0;
            return (clamp ? 0.0 : alpha, clamp, remove);
        }

        return (alpha, false, false);
    }

    public static double StepFadeToward(double alpha, double target, double amount)
    {
        if (alpha > target)
        {
            alpha -= amount;
            return alpha < target ? target : alpha;
        }

        if (alpha < target)
        {
            alpha += amount;
            return alpha > target ? target : alpha;
        }

        return alpha;
    }

    public static (double alpha, bool shouldRemove) FinishFadeTo(double alpha, double target, int flags)
    {
        if ((flags & FtfClamp) != 0)
            alpha = Math.Clamp(alpha, 0.0, 1.0);
        var remove = alpha == target && (flags & FtfRemove) != 0;
        return (alpha, remove);
    }

    public static (double multH, double multV) NormalizeDebrisMultipliers(double multH, double multV)
    {
        if (multV <= 0) multV = 1;
        if (multH <= 0) multH = 1;
        return (multH, multV);
    }

public static (double xo, double yo) DebrisOffsetFromPr(int prValue) =>
        ((prValue - 128) / 16.0, (prValue - 128) / 16.0);

    public static double ClampParticleStartAlpha(double startAlpha) => Math.Clamp(startAlpha, 0.0, 1.0);

    public static double ClampPositiveFadeStep(double fadestep) =>
        fadestep > 0 ? Math.Clamp(fadestep, 0.0, 1.0) : fadestep;

    public static double AbsParticleSize(double size) => Math.Abs(size);

    public static (double x, double y) RotateRelPos(double xoff, double yoff, double sin, double cos) =>
        (xoff * cos + yoff * sin, xoff * sin - yoff * cos);

    public static bool ShouldSpawnParticle(int lifetime) => lifetime != 0;

    public static bool DebrisTypeIsNull(object? debrisType) => debrisType is null;
}
