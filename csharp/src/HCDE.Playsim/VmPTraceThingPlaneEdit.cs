namespace HCDE.Playsim;

/// <summary><c>ThingCheck</c>, <c>CheckPlane</c>, <c>EditTraceResult</c> from <c>p_trace.cpp</c>.</summary>
public static class VmPTraceThingPlaneEdit
{
    public const int TraceHitActor = 1;
    public const int TraceHitNone = 0;
    public const int TraceHasHitSky = 2;

    public static bool ThingCheckPassesTopEntry(double hitZ, double thingTop, double vecZ) =>
        hitZ <= thingTop || vecZ < 0;

    public static bool CheckPlaneHits(double den, double num, double maxDist, double vecComponent) =>
        den != 0 && num / den >= 0 && num / den <= maxDist * Math.Abs(vecComponent);

    public static bool EditTraceResult(uint flags, int hitType, int hitTexture, int skyFlat, bool upperWallSkyPair)
    {
        if ((flags & 1) != 0 && (hitType == 2 || hitType == 3) && hitTexture == skyFlat)
        {
            return true;
        }
        if ((flags & 2) != 0 && (hitType == 2 || hitType == 3) && hitTexture == skyFlat)
            return false;
        if ((flags & 2) != 0 && hitType == 4 && upperWallSkyPair)
            return false;
        return true;
    }
}
