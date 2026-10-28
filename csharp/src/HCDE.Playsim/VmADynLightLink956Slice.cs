namespace HCDE.Playsim;

/// <summary>
/// <c>CollectWithinRadius</c> portal tail, <c>LinkLight</c>, <c>UnlinkLight</c>, <c>AttachLight</c>, and
/// <c>SetDynamicLights</c> head from <c>a_dynlight.cpp</c> (**601–750**). Exclusive to chunk **956**.
/// </summary>
public static class VmADynLightLink956Slice
{
    public const int SectorCeilingPlane = 0;
    public const int SectorFloorPlane = 1;

    public static bool PortalSightOpen(bool blocksSight) => !blocksSight;

    public static bool CeilingPortalAffectsLight(double portalZ, double lightZ, float radius) =>
        portalZ < lightZ + radius;

    public static bool FloorPortalAffectsLight(double portalZ, double lightZ, float radius) =>
        portalZ > lightZ - radius;

    public static bool ShouldCollectPortalSection(int sectionValidCount, int dlValidCount) =>
        sectionValidCount != dlValidCount;

    public static bool ShadowMappedAfterCollection(bool hitOneSidedBack, bool dontShadowmap) =>
        hitOneSidedBack && !dontShadowmap;

    public static bool ShouldLinkDynamicLight(float radius) => radius > 0;

    public static float LinkLightRadiusSquared(float radius) => radius * radius;

    public static bool UnlinkSkipsNullSide(object? sidedef) => sidedef is null;

    public static bool UnlinkListIndexInRange(int index, int listSize) => listSize > index;

    public static bool ReuseAttachedLightSlot(int count, int attachedCount) =>
        VmADynLightAttach955Slice.HasAttachedLightSlot(count, attachedCount);

    public static bool AbortAttachLightWhenEuthanizing(int objectFlags) =>
        VmADynLightAttach955Slice.AbortLightAttachWhenEuthanizing(objectFlags);

    public static bool ShouldAbortSetDynamicLights(object? state) => state is null;

    public static bool AssociationTriggersAttach(
        int sprite,
        int frame,
        int associationSprite,
        int associationFrame) =>
        VmADynLightAttach955Slice.LightAssociationMatches(sprite, frame, associationSprite, associationFrame);

    public static bool ShouldIterateStateLights(int attachedCount, int stateLightIndex) =>
        VmADynLightAttach955Slice.ShouldFallbackToStateLights(attachedCount, stateLightIndex);

    public static bool StateLightEntryAttachable(bool isSentinelMinusOne) =>
        VmADynLightAttach955Slice.ShouldAttachStateLightEntry(isSentinelMinusOne);

    public static double DistToSegClampParameter(double u) => u < 0.0 ? 0.0 : u > 1.0 ? 1.0 : u;

    public static bool LightInFrontOfSeg(
        double posX,
        double posY,
        double v1X,
        double v1Y,
        double v2X,
        double v2Y) =>
        (posY - v1Y) * (v2X - v1X) + (v1X - posX) * (v2Y - v1Y) <= 0.0;

    public static bool OneSidedBackfaceHit(bool isFrontSide, object? backSide) =>
        isFrontSide && backSide is null;

    public static bool SegmentDistanceWithinRadius(double distanceSquared, float radiusSquared) =>
        distanceSquared <= radiusSquared;
}
