namespace HCDE.Playsim;

/// <summary><c>A_FaceMovementDirection</c>, <c>A_CopySpriteFrame</c>, <c>A_SetVisibleRotation</c>, <c>A_SetTranslation</c> from <c>p_actionfunctions.cpp</c> (4897–5082).</summary>
public static class VmActorFaceMovementCopySprite
{
    [Flags]
    public enum FmdfFlags
    {
        NoPitch = 1 << 0,
        Interpolate = 1 << 1,
        NoAngle = 1 << 2,
    }

    [Flags]
    public enum CpsfFlags
    {
        NoSprite = 1,
        NoFrame = 1 << 1,
    }

    [Flags]
    public enum VrfFlags
    {
        NoAngleStart = 1,
        NoAngleEnd = 1 << 1,
        NoPitchStart = 1 << 2,
        NoPitchEnd = 1 << 3,
    }

    public static bool EarlyFaceMovementFail(bool hasMobj, FmdfFlags flags) =>
        !hasMobj || (flags & FmdfFlags.NoPitch) != 0 && (flags & FmdfFlags.NoAngle) != 0;

    public static bool ShouldAdjustYaw(FmdfFlags flags, double velX, double velY) =>
        (flags & FmdfFlags.NoAngle) == 0 && (velX != 0 || velY != 0);

    public static bool ShouldAdjustPitch(FmdfFlags flags) => (flags & FmdfFlags.NoPitch) == 0;

    public static double ClampYawDelta(double current, double target, double angleLimit, double offset)
    {
        if (angleLimit <= 0)
            return target + offset;

        var delta = NormalizeAngle(target - current);
        if (Math.Abs(delta) > angleLimit)
        {
            if (delta < 0)
                return current + angleLimit + offset;
            if (delta > 0)
                return current - angleLimit + offset;
        }

        return target + offset;
    }

    public static double ClampPitchDelta(double current, double target, double pitchLimit)
    {
        if (pitchLimit <= 0)
            return target;

        var pdelta = NormalizeAngle(target - current);
        if (Math.Abs(pdelta) > pitchLimit)
        {
            if (pdelta > 0)
                return current - Math.Min(pitchLimit, pdelta);
            return current + Math.Min(pitchLimit, -pdelta);
        }

        return target;
    }

    public static double PitchFromVelocity(double velX, double velY, double velZ)
    {
        var length = Math.Sqrt(velX * velX + velY * velY);
        return -Math.Atan2(velZ, length) * (180.0 / Math.PI);
    }

    public static bool CopySpriteFrameShouldFail(
        bool sameActor,
        bool hasFrom,
        bool hasTo,
        CpsfFlags flags) =>
        sameActor || !hasFrom || !hasTo || (flags & CpsfFlags.NoSprite) != 0 && (flags & CpsfFlags.NoFrame) != 0;

    public static bool ShouldCopySprite(CpsfFlags flags) => (flags & CpsfFlags.NoSprite) == 0;

    public static bool ShouldCopyFrame(CpsfFlags flags) => (flags & CpsfFlags.NoFrame) == 0;

    public static bool SetVisibleRotationShouldFail(bool hasMobj) => !hasMobj;

    public static bool ShouldAssignVisibleStartAngle(VrfFlags flags) => (flags & VrfFlags.NoAngleStart) == 0;

    public static bool ShouldAssignVisibleEndAngle(VrfFlags flags) => (flags & VrfFlags.NoAngleEnd) == 0;

    public static bool ShouldAssignVisibleStartPitch(VrfFlags flags) => (flags & VrfFlags.NoPitchStart) == 0;

    public static bool ShouldAssignVisibleEndPitch(VrfFlags flags) => (flags & VrfFlags.NoPitchEnd) == 0;

    public static bool SetTranslationRequiresName(bool hasName) => hasName;

    private static double NormalizeAngle(double degrees)
    {
        while (degrees > 180)
            degrees -= 360;
        while (degrees < -180)
            degrees += 360;
        return degrees;
    }
}
