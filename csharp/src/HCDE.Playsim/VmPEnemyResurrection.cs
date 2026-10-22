namespace HCDE.Playsim;

/// <summary><c>P_CanResurrect</c> and <c>P_CheckForResurrection</c> from <c>p_enemy.cpp</c> (2777–2972).</summary>
public static class VmPEnemyResurrection
{
    public const int DiNodir = 8;
    public const int MfSolid = 1;
    public const int MfFriendly = 1;
    public const int CompatVileGhosts = 1;
    public const int TranslationBlood = 1;
    public const int StyleNormal = 0;
    public const int StyleTranslucent = 1;

    public static bool RaiserNull(bool hasRaiser) => !hasRaiser;

    public static bool SkipWhenNoMoveDir(int moveDir) => moveDir == DiNodir;

    public static double ProbeOffset(double absSpeed, double axisSpeed) => absSpeed * axisSpeed;

    public static bool CorpseMissingRaiseState(bool hasRaiseState) => !hasRaiseState;

    public static bool TouchDistanceBlocks(double corpseX, double corpseY, double probeX, double probeY, double maxDist) =>
        Math.Abs(corpseX - probeX) > maxDist || Math.Abs(corpseY - probeY) > maxDist;

    public static bool DifferentPortalNeedsSight(int corpsePortal, int raiserPortal, bool sightOk) =>
        corpsePortal != raiserPortal && !sightOk;

    public static bool Pick3dFloorTestSector(bool vileHas3d, bool sectorsDiffer, bool corpseHas3d) =>
        vileHas3d || (sectorsDiffer && corpseHas3d);

    public static bool Mismatched3dFloorBlocks(
        bool foundCorpseFloor,
        bool foundRaiserFloor,
        bool sameSector,
        double zdistCorpse,
        double raiserZ,
        double raiserHeight) =>
        foundCorpseFloor != foundRaiserFloor
        && (sameSector || Math.Abs(zdistCorpse - raiserZ) > raiserHeight);

    public static bool PositionOrResurrectBlocks(bool positionOk, bool canResurrect) =>
        !positionOk || !canResurrect;

    public static bool ActiveVirtualBlocks(bool hasVirtual, bool retval) => hasVirtual && !retval;

    public static bool PassiveSelfCheckSkipsSecond(object? raiser, object? corpse) =>
        corpse == null || ReferenceEquals(corpse, raiser);

    public static bool EvaluateCanResurrect(CanResurrectContext ctx)
    {
        if (RaiserNull(ctx.HasRaiser)) return false;
        if (ActiveVirtualBlocks(ctx.RaiserHasVirtual, ctx.RaiserVirtualOk)) return false;
        if (PassiveSelfCheckSkipsSecond(ctx.Raiser, ctx.Corpse)) return true;
        return !ActiveVirtualBlocks(ctx.CorpseHasVirtual, ctx.CorpseVirtualOk);
    }

    public static bool FriendlyClearsLastEnemy(bool friendly, bool lastEnemyMatches) =>
        friendly && lastEnemyMatches;

    public static bool UseArchvileFallback(bool useVileStates, bool hasHealState) =>
        useVileStates && !hasHealState;

    public static bool VileGhostsEnabled(int compatFlags) => (compatFlags & CompatVileGhosts) != 0;

    public static double GhostHeightScale(double height) => height * 4;

    public static bool GhostStyleWhenZeroHeight(double height) => height == 0;

    public static bool HalveAlphaWhenGhost(double alpha) => alpha > 0.5;

    public static double HalvedAlpha(double alpha) => alpha / 2;

    public static bool PromoteTranslucentStyle(int renderStyle, int normalStyle) =>
        renderStyle == normalStyle;

    public readonly record struct CanResurrectContext(
        bool HasRaiser,
        object? Raiser,
        object? Corpse,
        bool RaiserHasVirtual,
        bool RaiserVirtualOk,
        bool CorpseHasVirtual,
        bool CorpseVirtualOk);

    public readonly record struct ResurrectionCandidateContext(
        int MoveDir,
        bool HasRaiseState,
        double CorpseX,
        double CorpseY,
        double ProbeX,
        double ProbeY,
        double CorpseRadius,
        double RaiserRadius,
        int CorpsePortal,
        int RaiserPortal,
        bool SightOk,
        bool VileSector3d,
        bool CorpseSector3d,
        bool SameSector,
        bool FoundCorpse3dFloor,
        bool FoundRaiser3dFloor,
        double Corpse3dZ,
        double RaiserZ,
        double RaiserHeight,
        bool PositionOk,
        bool CanResurrect);

    public static bool EvaluateResurrectionCandidate(ResurrectionCandidateContext ctx)
    {
        if (SkipWhenNoMoveDir(ctx.MoveDir)) return false;
        if (CorpseMissingRaiseState(ctx.HasRaiseState)) return false;
        var maxDist = ctx.CorpseRadius + ctx.RaiserRadius;
        if (TouchDistanceBlocks(ctx.CorpseX, ctx.CorpseY, ctx.ProbeX, ctx.ProbeY, maxDist)) return false;
        if (DifferentPortalNeedsSight(ctx.CorpsePortal, ctx.RaiserPortal, ctx.SightOk)) return false;
        if (Pick3dFloorTestSector(ctx.VileSector3d, !ctx.SameSector, ctx.CorpseSector3d)
            && Mismatched3dFloorBlocks(
                ctx.FoundCorpse3dFloor,
                ctx.FoundRaiser3dFloor,
                ctx.SameSector,
                ctx.Corpse3dZ,
                ctx.RaiserZ,
                ctx.RaiserHeight))
            return false;
        return !PositionOrResurrectBlocks(ctx.PositionOk, ctx.CanResurrect);
    }

    public readonly record struct FriendlyTargetCleanupContext(
        bool Friendly,
        bool LastEnemyWasTemp,
        bool LastEnemyWasCorpse,
        bool TempEqualsTarget);

    public static bool EvaluateFriendlyTargetCleanup(FriendlyTargetCleanupContext ctx) =>
        FriendlyClearsLastEnemy(ctx.Friendly, ctx.LastEnemyWasTemp)
        || FriendlyClearsLastEnemy(ctx.Friendly, ctx.LastEnemyWasCorpse)
        || (ctx.Friendly && ctx.TempEqualsTarget);

    public readonly record struct HealStatePickContext(
        bool HasExplicit,
        bool HasNamedHeal,
        bool UseVileStates,
        bool HasArchvileHeal);

    public static bool EvaluateHealStateEntry(HealStatePickContext ctx)
    {
        if (ctx.HasExplicit) return true;
        if (ctx.HasNamedHeal) return true;
        return UseArchvileFallback(ctx.UseVileStates, ctx.HasNamedHeal) && ctx.HasArchvileHeal;
    }

    public readonly record struct CorpseDimensionContext(
        int CompatFlags,
        double Height,
        double DefaultHeight,
        double DefaultRadius,
        double Alpha,
        int RenderStyle);

    public static CorpseDimensionResult EvaluateCorpseDimensions(CorpseDimensionContext ctx)
    {
        if (!VileGhostsEnabled(ctx.CompatFlags))
            return new CorpseDimensionResult(ctx.DefaultHeight, ctx.DefaultRadius, ctx.Alpha, ctx.RenderStyle, false);
        var height = GhostHeightScale(ctx.Height);
        if (!GhostStyleWhenZeroHeight(height))
            return new CorpseDimensionResult(height, ctx.DefaultRadius, ctx.Alpha, ctx.RenderStyle, true);
        var alpha = HalveAlphaWhenGhost(ctx.Alpha) ? HalvedAlpha(ctx.Alpha) : ctx.Alpha;
        var style = PromoteTranslucentStyle(ctx.RenderStyle, StyleNormal)
            ? StyleTranslucent
            : ctx.RenderStyle;
        return new CorpseDimensionResult(height, ctx.DefaultRadius, alpha, style, true);
    }

    public readonly record struct CorpseDimensionResult(
        double Height,
        double Radius,
        double Alpha,
        int RenderStyle,
        bool GhostCompatApplied);
}
