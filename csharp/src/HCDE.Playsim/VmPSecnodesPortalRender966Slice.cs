namespace HCDE.Playsim;

/// <summary>
/// Portal secnode unlink/add and actor render-portal list maintenance from
/// <c>p_secnodes.cpp</c> (**284–433**). Exclusive to chunk **966**; reuses chunk **810** freelist via <see cref="VmPSecnodes"/>.
/// </summary>
public static class VmPSecnodesPortalRender966Slice
{
    public const int MfNosector = 1 << 3;
    public const int PorttVisual = 1;
    public const double SpriteSpace = 64.0;

    public static VmPSecnodes.MsecNode? DelPortalnode(
        VmPSecnodes.MsecNode? node,
        ref VmPSecnodes.MsecNode? portalThingHead)
    {
        if (node == null)
            return null;

        var tp = node.ThingPrev;
        var tn = node.ThingNext;
        if (tp != null)
            tp.ThingNext = tn;
        if (tn != null)
            tn.ThingPrev = tp;

        var sp = node.SectorPrev;
        var sn = node.SectorNext;
        if (sp != null)
            sp.SectorNext = sn;
        else
            portalThingHead = sn;
        if (sn != null)
            sn.SectorPrev = sp;

        VmPSecnodes.PutSecnode(node);
        return tn;
    }

    public static VmPSecnodes.MsecNode AddPortalnode(
        object portal,
        object thing,
        VmPSecnodes.MsecNode? nextOnThing,
        ref VmPSecnodes.MsecNode? portalThingHead)
    {
        if (portal == null!)
            throw new InvalidOperationException("AddPortalnode of null portal");

        var node = VmPSecnodes.GetSecnode();
        node.Visited = 0;
        node.Sector = portal;
        node.Thing = thing;
        node.ThingPrev = null;
        node.ThingNext = nextOnThing;
        if (nextOnThing != null)
            nextOnThing.ThingPrev = node;
        node.SectorPrev = null;
        node.SectorNext = portalThingHead;
        if (portalThingHead != null)
            portalThingHead.SectorPrev = node;
        portalThingHead = node;
        return node;
    }

    public static bool ShouldRefreshRenderLists(bool noSectorFlag, bool positionChanged) =>
        !noSectorFlag && positionChanged;

    public static bool ShouldScanBlockForLinePortals(bool mapContainsLinePortals, bool blockValid, bool neighborHasLines) =>
        mapContainsLinePortals && blockValid && neighborHasLines;

    public static bool ShouldSkipVisualPortal(int portalType, int visualType) => portalType == visualType;

    public static bool ShouldLinkLinePortal(bool inRange, bool onCorrectSide) => inRange && onCorrectSide;

    public static double RenderBoundingRadius(double actorRadius) => Math.Min(actorRadius * 1.5, 128.0);

    public static bool CeilingPortalLoopContinues(bool blocksMovement, double planeZ, double lastHeight, double actorTop) =>
        !blocksMovement && planeZ > lastHeight && actorTop + SpriteSpace >= planeZ;

    public static bool FloorPortalLoopContinues(bool blocksMovement, double planeZ, double lastHeight, double actorZ) =>
        !blocksMovement && planeZ < lastHeight && actorZ - SpriteSpace <= planeZ;

    public static void ClearRenderSectorPortalList(ref VmPSecnodes.MsecNode? touchingList, ref VmPSecnodes.MsecNode? sectorPortalHead)
    {
        VmPSecnodes.DelSeclist(touchingList, ref sectorPortalHead);
        touchingList = null;
    }

    public static void ClearRenderLinePortalList(ref VmPSecnodes.MsecNode? touchingList, ref VmPSecnodes.MsecNode? linePortalHead)
    {
        VmPSecnodes.DelSeclist(touchingList, ref linePortalHead);
        touchingList = null;
    }
}
