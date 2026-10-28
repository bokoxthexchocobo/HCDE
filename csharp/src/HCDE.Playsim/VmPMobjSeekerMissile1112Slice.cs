namespace HCDE.Playsim;

/// <summary>
/// Floor-bounce rest tail, <c>P_FaceMobj</c>, <c>CanSeek</c>, and <c>P_SeekerMissile</c> turn/aim from
/// <c>p_mobj.cpp</c> (**2251–2400**). Chunk **1112**.
/// </summary>
public static class VmPMobjSeekerMissile1112Slice
{
    public const int BounceMbf = 512;
    public const int BounceAutoOff = 1;
    public const int BounceAutoOffFloorOnly = 2;
    public const int BounceTypeMask = 0xFF00;
    public const int MfNoGravity = 0x40;
    public const int Mf5CantSeek = 1;
    public const int Mf2DontSeekInvisible = 2;
    public const int MfShadow = 4;
    public const int RfInvisible = 8;
    public const int Mf8MinVisible = 16;
    public const int MfShootable = 0x4;
    public const int Mf3FloorHugger = 0x20;
    public const int Mf3CeilingHugger = 0x80;

    public static bool ShouldZeroMbfVerticalSpeed(double velZ, double mass, double gravity) =>
        Math.Abs(velZ) < mass * gravity / 64.0;

    public static bool ShouldClearBounceType(int bounceFlags, double normalZ, int actorFlags, double velocityDotNormal)
    {
        if ((bounceFlags & BounceMbf) != 0)
            return false;
        if ((bounceFlags & (BounceAutoOff | BounceAutoOffFloorOnly)) == 0)
            return false;
        if (normalZ <= 0 && (bounceFlags & BounceAutoOff) == 0)
            return false;
        if ((actorFlags & MfNoGravity) != 0)
            return false;
        return normalZ < 0 || velocityDotNormal < 3;
    }

    public static int FaceMobjDirection(double yawDelta)
    {
        if (yawDelta > 0)
            return 1;
        return 0;
    }

    public static double FaceMobjTurnAmount(double yawDelta) => Math.Abs(yawDelta);

    public sealed class CanSeekContext
    {
        public int SeekerFlags2 { get; init; }
        public int TargetFlags5 { get; init; }
        public int TargetFlags { get; init; }
        public int TargetRenderFlags { get; init; }
        public int TargetFlags8 { get; init; }
        public bool RenderStyleVisible { get; init; }
    }

    public static bool CanSeek(CanSeekContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.TargetFlags5 & Mf5CantSeek) != 0)
            return false;
        if ((ctx.SeekerFlags2 & Mf2DontSeekInvisible) != 0
            && ((ctx.TargetFlags & MfShadow) != 0
                || (ctx.TargetRenderFlags & RfInvisible) != 0
                || (ctx.TargetFlags8 & Mf8MinVisible) != 0
                || !ctx.RenderStyleVisible))
        {
            return false;
        }

        return true;
    }

    public sealed class SeekerTurnContext
    {
        public bool HasTarget { get; init; }
        public bool CanSeekTarget { get; init; }
        public bool TargetShootable { get; init; }
        public double Speed { get; init; }
        public bool UseCurrentSpeed { get; init; }
        public double CurrentSpeed { get; init; }
        public double ActorSpeed { get; init; }
        public double Delta { get; set; }
        public int FaceDirection { get; init; }
        public double Threshold { get; init; }
        public double TurnMax { get; init; }
        public bool Precise { get; init; }
        public int Flags3 { get; init; }
        public bool NeedsVerticalSeek { get; init; }
        public bool TargetIsPlayer { get; init; }
        public double Yaw { get; set; }
        public bool ClearTracer { get; set; }
        public bool Tracked { get; set; }
        public bool ApplyPlanarVelocity { get; set; }
        public bool ApplyVerticalVelocity { get; set; }
        public bool ApplyPrecisePitch { get; set; }
    }

    public static bool RunSeekerMissile(SeekerTurnContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var speed = ctx.UseCurrentSpeed ? ctx.CurrentSpeed : ctx.ActorSpeed;
        if (!ctx.HasTarget || !ctx.CanSeekTarget)
            return false;
        if (!ctx.TargetShootable)
        {
            ctx.ClearTracer = true;
            return false;
        }

        if (speed == 0)
        {
            ctx.Tracked = true;
            return true;
        }

        var delta = ctx.Delta;
        if (delta > ctx.Threshold)
        {
            delta /= 2;
            if (delta > ctx.TurnMax)
                delta = ctx.TurnMax;
        }

        ctx.Yaw += ctx.FaceDirection != 0 ? delta : -delta;
        ctx.Delta = delta;
        if (!ctx.Precise)
        {
            ctx.ApplyPlanarVelocity = true;
            var hugger = (ctx.Flags3 & (Mf3FloorHugger | Mf3CeilingHugger)) != 0;
            ctx.ApplyVerticalVelocity = !hugger && ctx.NeedsVerticalSeek;
        }
        else
        {
            var hugger = (ctx.Flags3 & (Mf3FloorHugger | Mf3CeilingHugger)) != 0;
            ctx.ApplyPrecisePitch = !hugger;
        }

        ctx.Tracked = true;
        return true;
    }

    public static double PreciseAimHeight(bool targetIsPlayer, double height, double viewZ, double targetZ) =>
        targetIsPlayer ? viewZ - targetZ : height / 2.0;

    public static double ClampSeekDistance(double distance2D) => Math.Max(1.0, distance2D);
}
