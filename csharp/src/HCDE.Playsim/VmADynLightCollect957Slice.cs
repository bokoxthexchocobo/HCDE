namespace HCDE.Playsim;

/// <summary>
/// <c>AddLightNode</c>, <c>DistToSeg</c>, and <c>CollectWithinRadius</c> head from <c>a_dynlight.cpp</c> (**451–600**).
/// Exclusive to chunk **957**; segment/front/portal predicates delegate to chunk **956** helpers.
/// </summary>
public static class VmADynLightCollect957Slice
{
    public const int PortTypeLinked = 3;

    public static bool AddLightNodeTargetsWall(object? section, object? sidedef) =>
        section is null && sidedef is not null;

    public static bool LightNodeListNeedsResize(int listSize, int elementIndex) =>
        listSize <= elementIndex;

    public static int LightNodeListSizeAfterResize(int elementIndex) => elementIndex + 1;

    public static (bool NeedsResize, int NewSize) EvaluateLightListCapacity(int listSize, int elementIndex) =>
        listSize <= elementIndex ? (true, elementIndex + 1) : (false, listSize);

    public static bool ShouldSkipCollectWithinRadius(object? section) => section is null;

    public static double DistToSegSquared(
        double posX,
        double posY,
        double startX,
        double startY,
        double endX,
        double endY)
    {
        var segDx = endX - startX;
        var segDy = endY - startY;
        var segLengthSq = segDx * segDx + segDy * segDy;
        if (segLengthSq == 0)
            return 0;
        var u = (((posX - startX) * segDx) + ((posY - startY) * segDy)) / segLengthSq;
        u = VmADynLightLink956Slice.DistToSegClampParameter(u);
        var px = startX + (u * segDx) - posX;
        var py = startY + (u * segDy) - posY;
        return px * px + py * py;
    }

    public static bool SegmentWithinLightRadius(
        double posX,
        double posY,
        double startX,
        double startY,
        double endX,
        double endY,
        float radiusSquared) =>
        VmADynLightLink956Slice.SegmentDistanceWithinRadius(
            DistToSegSquared(posX, posY, startX, startY, endX, endY),
            radiusSquared);

    public static bool ProcessSideWhenLineFresh(
        object? linedef,
        int lineValidCount,
        int globalValidCount) =>
        linedef is not null
        && VmADynLightLink956Slice.LineNeedsLightNode(lineValidCount, globalValidCount);

    public static bool ShouldAddWallLightNode(
        double posX,
        double posY,
        double v1X,
        double v1Y,
        double v2X,
        double v2Y) =>
        VmADynLightLink956Slice.LightInFrontOfSeg(posX, posY, v1X, v1Y, v2X, v2Y);

    public static bool RecordOneSidedBackface(
        bool isFrontSide,
        object? backSide) =>
        VmADynLightLink956Slice.OneSidedBackfaceHit(isFrontSide, backSide);

    public static bool LinkedPortalEligible(int? portalType) =>
        portalType == PortTypeLinked;

    public static bool ShouldTraverseLinkedPortal(int lineValidCount, int globalValidCount) =>
        VmADynLightLink956Slice.LinkedPortalLineNeedsVisit(lineValidCount, globalValidCount);

    public static bool PartnerSectionEligible(int partnerValidCount, int dlValidCount) =>
        VmADynLightLink956Slice.PartnerSectionNeedsCollection(partnerValidCount, dlValidCount);

    public static bool OtherSectionEligible(int otherValidCount, int globalValidCount) =>
        VmADynLightLink956Slice.LinkedPortalLineNeedsVisit(otherValidCount, globalValidCount);
}
