namespace HCDE.Playsim;

/// <summary>
/// <c>R_OffsetView</c>, <c>P_TalkFacing</c>, <c>P_UsePuzzleItem</c>, and splash thrust scale from <c>p_map.cpp</c> (5692–6118).
/// </summary>
public static class VmPMapOffsetTalkPuzzle
{
    public const int AlfForcenosmart = 1;
    public const int AlfCheckconversation = 1 << 1;
    public const int AlfPortalrestrict = 1 << 2;
    public const int UsePuzzleItemSpecial = 1;
    public const int PtAddlines = 1;
    public const int PtAddthings = 1 << 1;
    public const int PtDelta = 1 << 2;

    public static bool AllowOutOfBandOffset(bool allowedOob, bool hardwareRenderer) =>
        allowedOob && hardwareRenderer;

    public static double ClampedTracePullback(double distance) => distance < 5.0 ? distance : 5.0;

    public static bool TraceSucceeded(bool traceHit) => traceHit;

    public static bool WalkFloorPortalDown(double dirZ, bool blocksMovement, double posZ, double portalPlaneZ) =>
        dirZ < 0 && !blocksMovement && posZ < portalPlaneZ;

    public static bool WalkCeilingPortalUp(double dirZ, bool blocksMovement, double posZ, double portalPlaneZ) =>
        dirZ > 0 && !blocksMovement && posZ > portalPlaneZ;

    public static bool ShouldPortalWalk(bool allowedOob, bool hardwareRenderer) =>
        !allowedOob || !hardwareRenderer;

    public readonly record struct TalkFacingAngle(double Degrees);

    public static TalkFacingAngle[] TalkFacingAngles() =>
    [
        new(0),
        new(90.0 / 16),
        new(-90.0 / 16),
    ];

    public static int AimLineAttackFlags() =>
        AlfForcenosmart | AlfCheckconversation | AlfPortalrestrict;

    public static bool TalkHit(bool hasLineTarget) => hasLineTarget;

    public static bool StartConversationOnHit(bool hasLineTarget) => hasLineTarget;

    public static double UseRangeForPlayer(bool isPlayer, double playerUseRange, double defaultUseRange) =>
        isPlayer ? playerUseRange : defaultUseRange;

    public static int PathTraverseFlags() => PtDelta | PtAddlines | PtAddthings;

    public static bool LineIsPuzzleSpecial(int special) => special == UsePuzzleItemSpecial;

    public static bool BlockedLineOpening(double range) => range <= 0;

    public static bool WrongPuzzleSide(int side) => side == 1;

    public static bool PuzzleTypeMatches(int itemType, int expected) => itemType == expected;

    public static bool ThingIsPuzzleSpecial(int special) => special == UsePuzzleItemSpecial;

    public static bool ClearSpecialAfterUse(bool used) => used;

    public readonly record struct PuzzleLineUseContext(
        int LineSpecial,
        double OpeningRange,
        int Side,
        int ItemType,
        int LineArg0);

    public static bool TryUsePuzzleOnLine(PuzzleLineUseContext ctx)
    {
        if (!LineIsPuzzleSpecial(ctx.LineSpecial))
            return BlockedLineOpening(ctx.OpeningRange);
        if (WrongPuzzleSide(ctx.Side))
            return false;
        if (!PuzzleTypeMatches(ctx.ItemType, ctx.LineArg0))
            return false;
        return true;
    }

    public readonly record struct PuzzleThingUseContext(int Special, int ItemType, int ThingArg0);

    public static bool TryUsePuzzleOnThing(PuzzleThingUseContext ctx) =>
        ThingIsPuzzleSpecial(ctx.Special) && PuzzleTypeMatches(ctx.ItemType, ctx.ThingArg0);

    public static float NormalizeSplashFactor(float value) => value <= 0f ? 1f : value;

    public static float SelfThrustScaleFromSplash(float splashFactor) =>
        1f / NormalizeSplashFactor(splashFactor);
}
