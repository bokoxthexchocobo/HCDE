namespace HCDE.Playsim;

/// <summary>Play bounce sound and seeker missile helpers from <c>p_mobj.cpp</c> (2107–2411).</summary>
public static class VmPMobjSeekBounceSound
{
    public const int BounceQuiet = 1;
    public const int BounceUseSeeSound = 1 << 7;
    public const int BounceNoWallSound = 1 << 2;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;

    public static bool SkipWallBounceSound(bool onfloor, int bounceFlags) =>
        !onfloor && (bounceFlags & BounceNoWallSound) != 0;

    public static bool PlayBounceWhenNotQuiet(int bounceFlags) => (bounceFlags & BounceQuiet) == 0;

    public static double ClampBounceVolume(double volume) => Math.Clamp(volume, 0.0, 1.0);

    public static bool RunPlayBounceSound(bool onfloor, int bounceFlags, bool wallSoundValid, Action<double>? playSee, Action<double>? playBounce, Action<double>? playWall, double volume)
    {
        if (SkipWallBounceSound(onfloor, bounceFlags))
            return false;
        if (!PlayBounceWhenNotQuiet(bounceFlags))
            return false;
        volume = ClampBounceVolume(volume);
        if ((bounceFlags & BounceUseSeeSound) != 0)
            playSee?.Invoke(volume);
        else if (onfloor || !wallSoundValid)
            playBounce?.Invoke(volume);
        else
            playWall?.Invoke(volume);
        return true;
    }

    public static (int Dir, double Delta) FaceMobjDir(double sourceYaw, double angleToTarget)
    {
        var diff = angleToTarget - sourceYaw;
        if (diff > 0)
            return (1, diff);
        return (0, -diff);
    }

    public static bool CanSeek(bool targetCantSeek, bool dontSeekInvisible, bool shadow, bool invisible, bool minVisible, bool styleVisible) =>
        !targetCantSeek && (!dontSeekInvisible || (!shadow && !invisible && !minVisible && styleVisible));

    public static bool SeekerUnshootableTarget(int flags) => (flags & 1) == 0;

    public static bool SeekerZeroSpeed(double speed) => speed == 0;

    public static bool DeltaExceedsThresh(double delta, double thresh) => delta > thresh;

    public static bool ClampTurn(ref double delta, double turnMax)
    {
        if (delta > turnMax) { delta = turnMax; return true; }
        return false;
    }

    public static bool VerticalSeekNeeded(double actorTop, double targetZ, double targetTop) =>
        actorTop < targetZ || targetTop < targetZ;

    public static bool HuggerSkipsVertical(int flags3) =>
        (flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0;

    public readonly record struct SeekerMissileState(
        object? Tracer,
        bool CanSeekTarget,
        int TargetFlags,
        double Speed,
        double SourceYaw,
        double AngleToTarget,
        double Thresh,
        double TurnMax,
        bool Precise,
        int Flags3,
        double ActorTop,
        double TargetZ,
        double TargetTop);

    public static bool SeekerMissile(ref SeekerMissileState st, ref double yaw, Action? clearTracer)
    {
        if (st.Tracer == null || !st.CanSeekTarget)
            return false;
        if (SeekerUnshootableTarget(st.TargetFlags))
        {
            clearTracer?.Invoke();
            return false;
        }
        if (SeekerZeroSpeed(st.Speed))
            return true;

        var (dir, delta) = FaceMobjDir(st.SourceYaw, st.AngleToTarget);
        if (DeltaExceedsThresh(delta, st.Thresh))
        {
            delta /= 2;
            ClampTurn(ref delta, st.TurnMax);
        }

        yaw = dir != 0 ? st.SourceYaw + delta : st.SourceYaw - delta;
        return true;
    }
}
