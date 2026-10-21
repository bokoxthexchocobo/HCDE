namespace HCDE.Playsim;

/// <summary><c>A_Weave</c> bob indexing and XY/Z motion guards from <c>p_actionfunctions.cpp</c> (3484–3524).</summary>
public static class VmActionWeave
{
    public const int Mf5Nointeraction = 1 << 0;
    public const int WeaveMask = 63;

    public static int WeaveIndexXY(int weaveIndexXy) => weaveIndexXy & WeaveMask;

    public static int WeaveIndexZ(int weaveIndexZ) => weaveIndexZ & WeaveMask;

    public static bool ShouldWeaveXY(double xyDist, int xySpeed) => xyDist != 0 && xySpeed != 0;

    public static bool ShouldWeaveZ(double zDist, int zSpeed) => zDist != 0 && zSpeed != 0;

    public static int AdvanceWeaveIndex(int weaveIndex, int speed) => (weaveIndex + speed) & WeaveMask;

    public static bool UseTryMoveWhenInteractive(int flags5) => (flags5 & Mf5Nointeraction) == 0;

    public static bool ShouldSkipWeaveZ(double zDist, int zSpeed) => !ShouldWeaveZ(zDist, zSpeed);
}
