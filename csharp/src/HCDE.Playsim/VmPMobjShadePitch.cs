namespace HCDE.Playsim;

/// <summary><c>SetShade</c>, <c>ClampPitch</c>, and view-angle setters from <c>p_mobj.cpp</c> (4021–4168).</summary>
public static class VmPMobjShadePitch
{
    public const int SpfForceclamp = 1;
    public const int SpfScalednolerp = 1;
    public const int SpfInterpolate = 1;
    public const int CfScalednolerp = 1;
    public const int CfInterpview = 1;
    public const int CfInterpviewangles = 1;

    public static uint ShadeRgbMask(uint rgb) => rgb & 0xffffff;

public static double NonPlayerMinPitch() => -89;

    public static double NonPlayerMaxPitch() => 89;

    public static double ClampAngle(double p, double min, double max) => Math.Clamp(p, min, max);

    public static bool PitchChanged(double oldPitch, double newPitch) => oldPitch != newPitch;

    public static bool ApplyForceClamp(bool player, int flags) =>
        player || (flags & SpfForceclamp) != 0;

    public static bool ScaledNoLerpPitch(bool player, int flags) =>
        player && (flags & SpfScalednolerp) != 0;

    public static bool InterpolateView(bool player, int flags) =>
        player && (flags & SpfInterpolate) != 0;

    public static bool YawChanged(double oldYaw, double newYaw) => oldYaw != newYaw;

    public static bool RollChanged(double oldRoll, double newRoll) => oldRoll != newRoll;

    public static bool ViewPitchChanged(double oldP, double newP) => oldP != newP;

    public static bool ViewYawChanged(double oldY, double newY) => oldY != newY;

    public static bool InterpolateViewAngles(bool player, int flags) =>
        player && (flags & SpfInterpolate) != 0;

    public readonly record struct ClampPitchContext(bool HasPlayer, double Min, double Max, double Pitch);

    public static double EvaluateClampPitch(ClampPitchContext ctx)
    {
        if (ctx.HasPlayer) return ClampAngle(ctx.Pitch, ctx.Min, ctx.Max);
        return ClampAngle(ctx.Pitch, NonPlayerMinPitch(), NonPlayerMaxPitch());
    }

    public readonly record struct SetPitchContext(
        bool HasPlayer,
        int Flags,
        double OldPitch,
        double NewPitch);

    public static bool EvaluateSetPitch(SetPitchContext ctx)
    {
        if (!PitchChanged(ctx.OldPitch, ctx.NewPitch)) return false;
        if (ScaledNoLerpPitch(ctx.HasPlayer, ctx.Flags)) return true;
        if (InterpolateView(ctx.HasPlayer, ctx.Flags)) return true;
        return true;
    }

    public readonly record struct SetAngleContext(bool HasPlayer, int Flags, double OldYaw, double NewYaw);

    public static bool EvaluateSetAngle(SetAngleContext ctx)
    {
        if (!YawChanged(ctx.OldYaw, ctx.NewYaw)) return false;
        return ScaledNoLerpPitch(ctx.HasPlayer, ctx.Flags) || InterpolateView(ctx.HasPlayer, ctx.Flags) || !ctx.HasPlayer;
    }

    public readonly record struct SetRollContext(bool HasPlayer, int Flags, double OldRoll, double NewRoll);

    public static bool EvaluateSetRoll(SetRollContext ctx)
    {
        if (!RollChanged(ctx.OldRoll, ctx.NewRoll)) return false;
        return ScaledNoLerpPitch(ctx.HasPlayer, ctx.Flags) || InterpolateView(ctx.HasPlayer, ctx.Flags) || !ctx.HasPlayer;
    }

    public readonly record struct SetViewPitchContext(
        bool HasPlayer,
        int Flags,
        double OldPitch,
        double NewPitch);

    public static bool EvaluateSetViewPitch(SetViewPitchContext ctx)
    {
        if (!ApplyForceClamp(ctx.HasPlayer, ctx.Flags)) return false;
        if (!ViewPitchChanged(ctx.OldPitch, ctx.NewPitch)) return false;
        return InterpolateViewAngles(ctx.HasPlayer, ctx.Flags);
    }

    public readonly record struct SetViewAngleContext(bool HasPlayer, int Flags, double OldYaw, double NewYaw);

    public static bool EvaluateSetViewAngle(SetViewAngleContext ctx)
    {
        if (!ViewYawChanged(ctx.OldYaw, ctx.NewYaw)) return false;
        return InterpolateViewAngles(ctx.HasPlayer, ctx.Flags);
    }

    public const int CfNofovinterp = 1;

    public static bool NoFovInterpCheat(int cheats) => (cheats & CfNofovinterp) != 0;

    public static double LerpFov(double prev, double cur, double frac) => prev * (1 - frac) + cur * frac;

    public readonly record struct GetFovContext(bool HasPlayer, int Cheats, double PlayerFov, double CameraFov, double PrevFov, double TicFrac);

    public static double EvaluateGetFov(GetFovContext ctx)
    {
        if (ctx.HasPlayer && NoFovInterpCheat(ctx.Cheats)) return ctx.PlayerFov;
        var fov = ctx.HasPlayer ? ctx.PlayerFov : ctx.CameraFov;
        return LerpFov(ctx.PrevFov, fov, ctx.TicFrac);
    }

    public readonly record struct SetViewRollContext(bool HasPlayer, int Flags, double OldRoll, double NewRoll);

    public static bool EvaluateSetViewRoll(SetViewRollContext ctx)
    {
        if (!RollChanged(ctx.OldRoll, ctx.NewRoll)) return false;
        return InterpolateViewAngles(ctx.HasPlayer, ctx.Flags);
    }
}
