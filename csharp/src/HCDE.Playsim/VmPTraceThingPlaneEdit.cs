namespace HCDE.Playsim;

/// <summary>Faithful ports of <c>ThingCheck</c>, <c>CheckPlane</c>, and <c>EditTraceResult</c> (<c>p_trace.cpp</c>).</summary>
public static class VmPTraceThingPlaneEdit
{
    public const int TraceHitNone = 0;
    public const int TraceHitFloor = 1;
    public const int TraceHitCeiling = 2;
    public const int TraceHitWall = 3;
    public const int TraceHitActor = 4;
    public const int TraceHasHitSky = 5;

    public const uint TraceHitSky = 1;
    public const uint TraceNoSky = 2;

    public const int TraceContinue = 0;
    public const int TraceContinueOutOfBounds = 1;
    public const int TraceStop = 2;
    public const int TraceAbort = 3;
    public const int TraceSkip = 4;

    public sealed class ThingCheckState
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double VecX { get; set; }
        public double VecY { get; set; }
        public double VecZ { get; set; }
        public double MaxDist { get; set; }
        public double HitX { get; set; }
        public double HitY { get; set; }
        public double HitZ { get; set; }
        public double ThingX { get; set; }
        public double ThingY { get; set; }
        public double ThingZ { get; set; }
        public double ThingTop { get; set; }
        public double ThingRadius { get; set; }
        public double Frac { get; set; }
        public double Dist { get; set; }
        public int FfFloorCount { get; set; }
        public double FfFloorZ { get; set; }
        public double FfCeilingZ { get; set; }
        public bool PortalBlocksCeiling { get; set; }
        public bool PortalBlocksFloor { get; set; }
        public int CeilingTexture { get; set; }
        public int FloorTexture { get; set; }
        public int ResultHitType { get; set; }
        public int ResultTexture { get; set; }
        public object? ResultActor { get; set; }
        public double ResultDistance { get; set; }
        public double ResultFraction { get; set; }
        public double SrcAngle { get; set; }
        public Func<bool, bool>? CheckSectorPlane { get; init; }
        public Func<int>? TraceCallback { get; init; }
        public bool HasTraceCallback { get; set; }
    }

    /// <summary>Returns true to continue traverse, false to stop.</summary>
    public static bool ThingCheck(ThingCheckState s)
    {
        var hitX = s.HitX;
        var hitY = s.HitY;
        var hitZ = s.HitZ;
        var dist = s.Dist;
        var frac = s.Frac;

        if (hitZ > s.ThingTop)
        {
            if (s.VecZ >= 0)
                return true;
            dist = (s.ThingTop - s.StartZ) / s.VecZ;
            if (dist > s.MaxDist)
                return true;
            frac = dist / s.MaxDist;
            hitX = s.StartX + s.VecX * dist;
            hitY = s.StartY + s.VecY * dist;
            hitZ = s.StartZ + s.VecZ * dist;
            if (Math.Abs(hitX - s.ThingX) > s.ThingRadius || Math.Abs(hitY - s.ThingY) > s.ThingRadius)
                return true;
        }
        else if (hitZ < s.ThingZ)
        {
            if (s.VecZ <= 0)
                return true;
            dist = (s.ThingZ - s.StartZ) / s.VecZ;
            if (dist > s.MaxDist)
                return true;
            frac = dist / s.MaxDist;
            hitX = s.StartX + s.VecX * dist;
            hitY = s.StartY + s.VecY * dist;
            hitZ = s.StartZ + s.VecZ * dist;
            if (Math.Abs(hitX - s.ThingX) > s.ThingRadius || Math.Abs(hitY - s.ThingY) > s.ThingRadius)
                return true;
        }

        if (s.FfFloorCount > 0)
        {
            if (hitZ > s.FfCeilingZ && s.PortalBlocksCeiling)
            {
                s.ResultHitType = TraceHitCeiling;
                s.ResultTexture = s.CeilingTexture;
            }
            else if (hitZ < s.FfFloorZ && s.PortalBlocksFloor)
            {
                s.ResultHitType = TraceHitFloor;
                s.ResultTexture = s.FloorTexture;
            }
            else
                goto ActorHit;

            if (s.CheckSectorPlane?.Invoke(s.ResultHitType == TraceHitFloor) == false)
                s.ResultHitType = TraceHitNone;

            if (s.HasTraceCallback)
            {
                switch (s.TraceCallback?.Invoke() ?? TraceStop)
                {
                    case TraceContinue:
                    case TraceContinueOutOfBounds:
                        return true;
                    case TraceStop:
                        return false;
                    case TraceAbort:
                        s.ResultHitType = TraceHitNone;
                        return false;
                    case TraceSkip:
                        s.ResultHitType = TraceHitNone;
                        return true;
                }
            }
            return false;
        }

ActorHit:
        s.ResultHitType = TraceHitActor;
        s.HitX = hitX;
        s.HitY = hitY;
        s.HitZ = hitZ;
        s.ResultDistance = dist;
        s.ResultFraction = frac;

        if (s.HasTraceCallback)
        {
            switch (s.TraceCallback?.Invoke() ?? TraceStop)
            {
                case TraceStop:
                    return false;
                case TraceAbort:
                    s.ResultHitType = TraceHitNone;
                    return false;
                case TraceSkip:
                    s.ResultHitType = TraceHitNone;
                    return true;
                default:
                    return true;
            }
        }
        return false;
    }

    public sealed class CheckPlaneState
    {
        public double Den { get; set; }
        public double Num { get; set; }
        public double EnterDist { get; set; }
        public double MaxDist { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double VecX { get; set; }
        public double VecY { get; set; }
        public double VecZ { get; set; }
        public double HitX { get; set; }
        public double HitY { get; set; }
        public double HitZ { get; set; }
        public double ResultDistance { get; set; }
        public double ResultFraction { get; set; }
    }

    public static bool CheckPlane(CheckPlaneState s)
    {
        if (s.Den == 0)
            return false;
        var hitdist = -s.Num / s.Den;
        if (hitdist > s.EnterDist && hitdist < s.MaxDist)
        {
            s.HitX = s.StartX + s.VecX * hitdist;
            s.HitY = s.StartY + s.VecY * hitdist;
            s.HitZ = s.StartZ + s.VecZ * hitdist;
            s.ResultDistance = hitdist;
            s.ResultFraction = hitdist / s.MaxDist;
            return true;
        }
        return false;
    }

    public sealed class EditTraceResultState
    {
        public uint Flags { get; set; }
        public int HitType { get; set; }
        public int HitTexture { get; set; }
        public int SkyFlat { get; set; }
        public int TierUpper { get; set; }
        public int FrontCeilingTexture { get; set; }
        public int BackCeilingTexture { get; set; }
    }

    public static bool EditTraceResult(EditTraceResultState s)
    {
        if ((s.Flags & TraceHitSky) != 0)
        {
            if (s.HitType == TraceHitFloor || s.HitType == TraceHitCeiling)
            {
                if (s.HitTexture == s.SkyFlat)
                {
                    s.HitType = TraceHasHitSky;
                    return true;
                }
            }
            else if (s.HitType == TraceHitWall)
            {
                if (s.TierUpper == 1 &&
                    s.FrontCeilingTexture == s.SkyFlat &&
                    s.BackCeilingTexture == s.SkyFlat)
                {
                    s.HitType = TraceHasHitSky;
                    return true;
                }
            }
        }
        if ((s.Flags & TraceNoSky) != 0)
        {
            if (s.HitType == TraceHitFloor || s.HitType == TraceHitCeiling)
            {
                if (s.HitTexture == s.SkyFlat)
                {
                    s.HitType = TraceHitNone;
                    return false;
                }
            }
            else if (s.HitType == TraceHitWall)
            {
                if (s.TierUpper == 1 &&
                    s.FrontCeilingTexture == s.SkyFlat &&
                    s.BackCeilingTexture == s.SkyFlat)
                {
                    s.HitType = TraceHitNone;
                    return false;
                }
            }
        }
        return true;
    }
}
