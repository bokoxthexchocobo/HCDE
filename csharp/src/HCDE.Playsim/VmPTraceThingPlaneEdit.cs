namespace HCDE.Playsim;

/// <summary><c>ThingCheck</c>, <c>CheckPlane</c>, <c>EditTraceResult</c> from <c>p_trace.cpp</c>.</summary>
public static class VmPTraceThingPlaneEdit
{
    public const int TraceHitActor = 1;
    public const int TraceHitNone = 0;
    public const int TraceHitFloor = 2;
    public const int TraceHitCeiling = 3;
    public const int TraceHitWall = 4;

    public static bool ThingCheckPassesTopEntry(double hitZ, double thingTop, double vecZ) =>
        hitZ <= thingTop || vecZ < 0;

    public static bool ThingCheckPassesBottomEntry(double hitZ, double thingBottom, double vecZ) =>
        hitZ >= thingBottom || vecZ > 0;

    public static bool ThingWithinRadius(double hitX, double hitY, double thingX, double thingY, double radius) =>
        Math.Abs(hitX - thingX) <= radius && Math.Abs(hitY - thingY) <= radius;

    public static bool CheckPlaneHits(double den, double num, double maxDist, double vecComponent) =>
        den != 0 && num / den >= 0 && num / den <= maxDist * Math.Abs(vecComponent);

    public static bool EditTraceResult(uint flags, int hitType, int hitTexture, int skyFlat, bool upperWallSkyPair)
    {
        const uint hitSky = 1;
        const uint noSky = 2;
        if ((flags & hitSky) != 0 && (hitType == TraceHitFloor || hitType == TraceHitCeiling) && hitTexture == skyFlat)
            return true;
        if ((flags & noSky) != 0 && (hitType == TraceHitFloor || hitType == TraceHitCeiling) && hitTexture == skyFlat)
            return false;
        if ((flags & noSky) != 0 && hitType == TraceHitWall && upperWallSkyPair)
            return false;
        return true;
    }

    public sealed class ThingCheckContext
    {
        public double HitZ { get; set; }
        public double ThingTop { get; set; }
        public double ThingBottom { get; set; }
        public double VecZ { get; set; }
        public int ResultHitType { get; set; }
        public int HitTypeNone { get; set; }
        public Func<int>? TraceCallbackResult { get; init; }
    }

    public static bool ThingCheckContinue(ThingCheckContext ctx)
    {
        if (!ThingCheckPassesTopEntry(ctx.HitZ, ctx.ThingTop, ctx.VecZ))
            return false;
        if (!ThingCheckPassesBottomEntry(ctx.HitZ, ctx.ThingBottom, ctx.VecZ))
            return false;
        ctx.ResultHitType = TraceHitActor;
        return ctx.TraceCallbackResult?.Invoke() is not (2 or 3);
    }
}
