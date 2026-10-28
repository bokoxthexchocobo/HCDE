namespace HCDE.MapLoader;

/// <summary>Right-edge and angle selection policy from the tail of <c>MapLoader::LoopSidedefs</c> (1996–2041).</summary>
public static class VmMapLoaderLoopSidedefsRightEdge
{
    public const double FullCircleDegrees = 360.0;

    public static bool ShouldSearchMultipleCandidates(int rightNext) => rightNext != VmMapLoaderAllocateSideDefs.NoSide;

    public static bool IsCandidateSide(int leftSide) => leftSide == VmMapLoaderAllocateSideDefs.NoSide;

    public static bool ShouldCompareAngles(bool frontSectorDiffersFromBack) => frontSectorDiffersFromBack;

    public static double NormalizeAngleDegrees(double degrees) =>
        degrees < 0 ? degrees + FullCircleDegrees : degrees;

    public static bool IsBetterAngle(double candidate, double best) =>
        candidate != 0 && candidate <= best;

    public static bool ShouldLinkRightToLeft(int sideIndex, int rightIndex, int sideCount) =>
        (uint)sideIndex < (uint)sideCount && (uint)rightIndex < (uint)sideCount;
}
