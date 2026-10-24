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
        if (OneSidedBlocks(ctx.TwoSided, ctx.Backsector) && !false) return false;
        if (OpeningTooLow(ctx.OpenRange, ctx.Height)) return false;
        if (TooHigh(ctx.OpenTop, ctx.ActorTop)) return false;
        if (TooLow(ctx.OpenBottom, ctx.ActorZ)) return false;
        return true;
    }

    public readonly record struct BounceWallContext(
        int BounceFlags,
        bool Missile,
        int SpecialHit,
        bool Horizon,
        bool HitSky,
        int BounceCount,
        bool KeepAngleFlag,
        double MoveLen,
        bool UseBounceState);

    public static bool EvaluateBounceWallDestroy(BounceWallContext ctx)
    {
        if (!BounceWallsEnabled(ctx.BounceFlags)) return false;
        if (ctx.Missile && MissileSpecialHitStop(ctx.SpecialHit)) return true;
        if (ctx.Missile && MissileSpecialHitBlock(ctx.SpecialHit)) return false;
        if (HorizonDestroys(ctx.Horizon, (ctx.BounceFlags & BounceNotOnSky) != 0, ctx.HitSky)) return true;
        if (BounceCountDepleted(ctx.BounceCount)) return true;
        return false;
    }

    public static bool EvaluateBounceWallReflect(BounceWallContext ctx)
    {
        if (ctx.MoveLen < 1) return true;
        return UseBounceState(ctx.BounceFlags);
    }

    public static bool ProjectileHitLinedef(bool hit, int bouncecount) => hit && bouncecount > 0;

    public static bool ExplodeMissileOnHit(bool missile, bool hit) => missile && hit;

    public static double ReflectAngle(double lineAngle, double moveAngle, int side) =>
        side == 1 ? (lineAngle + 180) * 2 - moveAngle : lineAngle * 2 - moveAngle;

    public static bool BoxOnWrongSide(int boxSide) => boxSide == -1;

    public static double MinMoveLen(double len) => len < 1 ? 2 : len;

    public readonly record struct BounceVelocityContext(
        bool MissileHitLinedef,
        int BounceCount,
        bool Missile,
        int SpecialHit,
        bool Horizon,
        bool SkyHit,
        int BounceFlags,
        double MoveLen,
        int Side,
        double LineAngle,
        double MoveAngle,
        int BoxSide);

    public static bool EvaluateBounceWallComplete(BounceVelocityContext ctx)
    {
        if (EvaluateBounceWallDestroy(new BounceWallContext(
            ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, ctx.Horizon, ctx.SkyHit, ctx.BounceCount, false, ctx.MoveLen, false)))
            return true;
        if (ProjectileHitLinedef(ctx.MissileHitLinedef, ctx.BounceCount)) return true;
        ReflectAngle(ctx.LineAngle, ctx.MoveAngle, ctx.Side);
        if (BoxOnWrongSide(ctx.BoxSide)) { }
        MinMoveLen(ctx.MoveLen);
        return EvaluateBounceWallReflect(new BounceWallContext(ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, false, false, ctx.BounceCount, false, ctx.MoveLen, true));
    }
}
