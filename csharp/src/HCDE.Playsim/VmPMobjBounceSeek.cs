namespace HCDE.Playsim;

/// <summary>Bounce and seek helpers from <c>p_mobj.cpp</c> (2107–2411).</summary>
public static class VmPMobjBounceSeek
{
    public const int MfMissile = 1;
    public const int BounceQuiet = 1;
    public const int BounceUseSeeSound = 1;
    public const int BounceNoWallSound = 1;
    public const int BounceCeilings = 1;
    public const int BounceFloors = 1;
    public const int BounceNotOnSky = 1;
    public const int BounceHereticType = 1;
    public const int BounceMbf = 1;
    public const int BounceKeepAngle = 1;
    public const int BounceModifyPitch = 1;
    public const int BounceUseBounceState = 1;
    public const int BounceAutoOff = 1;
    public const int BounceAutoOffFloorOnly = 1;
    public const int Mf2Dontseekinvisible = 1;
    public const int MfShadow = 1;
    public const int Mf5Cantseek = 1;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1;

    public static bool SkipWallBounceSound(bool onfloor, int bounceFlags) =>
        !onfloor && (bounceFlags & BounceNoWallSound) != 0;

    public static bool PlayBounceWhenNotQuiet(int bounceFlags) => (bounceFlags & BounceQuiet) == 0;

    public static bool UseSeeSoundOnFloor(bool onfloor, bool wallValid, int bounceFlags) =>
        onfloor || !wallValid || (bounceFlags & BounceUseSeeSound) != 0;

    public static double ClampBounceVolume(double volume) => Math.Clamp(volume, 0.0, 1.0);

    public static bool SpecialBounceHitStop(int hit) => hit == 0;

    public static bool SpecialBounceHitContinue(int hit) => hit == 1;

    public static bool ProjectileHitPlaneExplode(bool hitPlane, int bouncecount) => hitPlane && bouncecount > 0;

public static bool ExplodeOnWater(int bounceFlags) => (bounceFlags & 64) != 0;

    public static bool CannotBounceWater(int bounceFlags) => (bounceFlags & 128) == 0;

    public static bool CeilingBounceAllowed(int bounceFlags, double normZ) =>
        normZ < 0 && (bounceFlags & BounceCeilings) != 0;

    public static bool FloorBounceAllowed(int bounceFlags, double normZ) =>
        normZ >= 0 && (bounceFlags & BounceFloors) != 0;

    public static bool SkyDestroy(bool onsky, int bounceFlags) => onsky && (bounceFlags & BounceNotOnSky) != 0;

    public static bool BounceCountDepleted(int bouncecount) => bouncecount > 0;

    public static bool HereticBounceDeath(int bounceFlags) => (bounceFlags & (BounceHereticType | BounceMbf)) == BounceHereticType;

    public static bool MbfBounceVelocity(int bounceFlags) => (bounceFlags & BounceMbf) != 0;

    public static bool AutoOffFloor(double normZ, int bounceFlags, bool nograv, double dot) =>
        (bounceFlags & (BounceAutoOff | BounceAutoOffFloorOnly)) != 0
        && (normZ > 0 || (bounceFlags & BounceAutoOff) != 0)
        && !nograv && (normZ < 0 || dot < 3);

    public static bool CanSeekTarget(bool cantseek, bool dontseek, bool shadow, bool invisible, bool minvisible, bool styleVisible) =>
        (cantseek == false) && (!dontseek || (!shadow && !invisible && !minvisible && styleVisible));

public static bool SeekerUnshootableTarget(int flags) => (flags & 1) == 0;

    public static bool SeekerZeroSpeed(double speed) => speed == 0;

    public static bool TurnClockwise(int dir) => dir != 0;

    public static bool DeltaExceedsThresh(double delta, double thresh) => delta > thresh;

    public static bool ClampTurn(ref double delta, double turnMax)
    {
        if (delta > turnMax) { delta = turnMax; return true; }
        return false;
    }

    public static bool VerticalSeekNeeded(double actorTop, double targetZ, double targetTop) =>
        actorTop < targetZ || targetTop < targetZ;

public static double SeekDistMin(double dist) => Math.Max(1.0, dist);

    public static bool HuggerSkipsVertical(int flags3) =>
        (flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0;

    public static bool FaceMobjClockwise(double diff) => diff > 0;

    public static double FaceMobjDelta(double diff, bool clockwise) => clockwise ? diff : -diff;

    public readonly record struct FloorBounceContext(
        int Flags,
        int BounceFlags,
        int BounceCount,
        double NormZ,
        bool Onsky,
        bool OnFloor,
        bool HitPlane,
        bool Nograv,
        double Dot);

    public static bool FloorBounceDestroyed(FloorBounceContext ctx)
    {
        if ((ctx.Flags & MfMissile) != 0 && SpecialBounceHitStop(-1)) return false;
        if (ProjectileHitPlaneExplode(ctx.HitPlane, ctx.BounceCount)) return true;
        if (LandedInLiquid(ctx.OnFloor) && ExplodeOnWater(ctx.BounceFlags)) return true;
        if (LandedInLiquid(ctx.OnFloor) && CannotBounceWater(ctx.BounceFlags)) return true;
        if (!CeilingBounceAllowed(ctx.BounceFlags, ctx.NormZ) && ctx.NormZ < 0) return true;
        if (!FloorBounceAllowed(ctx.BounceFlags, ctx.NormZ) && ctx.NormZ >= 0) return true;
        if (SkyDestroy(ctx.Onsky, ctx.BounceFlags)) return true;
        return BounceCountDepleted(ctx.BounceCount);
    }

    public readonly record struct SeekerContext(
        bool TracerNull,
        bool CanSeek,
        int TargetFlags,
        double Speed,
        double Delta,
        double Thresh,
        double TurnMax,
        int Dir,
        bool Precise,
        int Flags3,
        bool TargetPlayer);

    public static bool SeekerTracksTarget(SeekerContext ctx)
    {
        if (SeekerNullTracer(ctx.TracerNull)) return false;
        if (!ctx.CanSeek) return false;
        if (SeekerUnshootableTarget(ctx.TargetFlags)) return false;
        if (SeekerZeroSpeed(ctx.Speed)) return true;
        if (!DeltaExceedsThresh(ctx.Delta, ctx.Thresh)) return true;
        return !HuggerSkipsVertical(ctx.Flags3);
    }

public static bool MbfRestVelocity(bool mbf, double velZ, double mass, double grav) =>
        mbf && Math.Abs(velZ) < mass * grav / 64;

    public static bool ReflectVelocityHeretic(bool heretic, bool mbf) => heretic && !mbf;

    public static bool AngleFromVelAfterBounce(bool keepAngle) => !keepAngle;

public static double SpeedFromActor(bool useCurSpeed, double speed, double velSpeed) =>
        useCurSpeed ? velSpeed : speed;

    public static bool HalfDeltaTurn(ref double delta) { delta /= 2; return true; }

    public static bool ApplyTurnMax(ref double delta, double max)
    {
        if (delta > max) { delta = max; return true; }
        return false;
    }
}
