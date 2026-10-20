namespace HCDE.Playsim;

/// <summary><c>FSlide::BounceTraverse</c> and <c>FSlide::BounceWall</c> head from <c>p_map.cpp</c> (3511–3707).</summary>
public static class VmPMapBounceTraverse
{
    public const int MlBlockeverything = 1;
    public const int MlTwosided = 1;
    public const int BounceWalls = 1;
    public const int BounceNotOnSky = 1;
    public const int MfMissile = 1;

    public static bool BlockEverythingLine(int flags) => (flags & MlBlockeverything) != 0;

    public static bool OneSidedBlocks(bool twoSided, bool backsector) => !twoSided || !backsector;

    public static bool BackSidePass(bool onBackSide) => onBackSide;

    public static bool OpeningTooLow(double range, double height) => range < height;

    public static bool TooHigh(double top, double actorTop) => top < actorTop;

    public static bool TooLow(double bottom, double actorZ) => bottom > actorZ;

    public static bool CloserSlideFrac(double frac, double best) => frac < best;

    public static bool BounceWallsEnabled(int bounceFlags) => (bounceFlags & BounceWalls) != 0;

    public static bool MissileSpecialHitStop(int hit) => hit == 1;

    public static bool MissileSpecialHitBlock(int hit) => hit == 0;

    public static bool HorizonDestroys(bool horizon, bool notOnSky, bool hitSky) =>
        horizon || (notOnSky && hitSky);

    public static bool BounceCountDepleted(int count) => count > 0 && count - 1 == 0;

    public static bool KeepAngle(int bounceFlags) => (bounceFlags & 64) != 0;

    public static bool UseBounceState(int bounceFlags) => (bounceFlags & 128) != 0;

    public readonly record struct BounceTraverseContext(
        bool IsLine,
        int LineFlags,
        bool TwoSided,
        bool Backsector,
        double OpenRange,
        double Height,
        double OpenTop,
        double ActorTop,
        double OpenBottom,
        double ActorZ,
        double Frac,
        double BestFrac);

    public static bool EvaluateTraverseContinue(BounceTraverseContext ctx)
    {
        if (!ctx.IsLine) return true;
        if (BlockEverythingLine(ctx.LineFlags)) return false;
        if (OneSidedBlocks(ctx.TwoSided, ctx.Backsector) && !BackSidePass(false)) return false;
        if (OpeningTooLow(ctx.OpenRange, ctx.Height)) return false;
        if (TooHigh(ctx.OpenTop, ctx.ActorTop)) return false;
        if (TooLow(ctx.OpenBottom, ctx.ActorZ)) return false;
        return true;
    }
}
