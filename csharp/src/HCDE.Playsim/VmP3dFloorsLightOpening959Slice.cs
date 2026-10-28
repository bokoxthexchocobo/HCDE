namespace HCDE.Playsim;

/// <summary>
/// Light-list refresh, plane light lookup, and <c>P_LineOpening_XFloors</c> head from <c>p_3dfloors.cpp</c> (**601–750**).
/// Exclusive to chunk **959**; does not re-credit retired chunk **781** / <c>VmP3DFloorSupport</c> tally rows.
/// </summary>
public static class VmP3dFloorsLightOpening959Slice
{
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;
    public const double EqualEpsilon = 1.0 / 65536.0;

    public static void RefreshLightListEntry(
        object? lightSource,
        object? sectorColormap,
        Action<object?, object?> updateColormap,
        Func<object?, int> getBlend,
        Action<object?> setExtraColormap,
        Action<int> setBlend)
    {
        if (lightSource is not null)
        {
            updateColormap(lightSource, sectorColormap);
            setBlend(getBlend(lightSource));
        }
        else
        {
            setExtraColormap(sectorColormap);
            setBlend(0);
        }
    }

    public static void RecalculateAttachedSectorLights(
        int attachedCount,
        Action<int> recalculateAttachedAt,
        Action recalculateSelf)
    {
        for (var i = 0; i < attachedCount; i++)
            recalculateAttachedAt(i);
        recalculateSelf();
    }

    public static double PlaneHeightAtCenter(Func<double, double, double> zAtPoint, double centerX, double centerY, bool underside)
    {
        var height = zAtPoint(centerX, centerY);
        return underside ? height - EqualEpsilon : height;
    }

    public static int SelectPlaneLightIndex(
        int lightListCount,
        Func<int, double> tierZAtCenter,
        double planeHeight)
    {
        for (var i = 1; i < lightListCount; i++)
        {
            if (tierZAtCenter(i) <= planeHeight)
                return i - 1;
        }

        return lightListCount - 1;
    }

    public static bool SectorHas3dFloors(int frontCount, int backCount) => frontCount > 0 || backCount > 0;

    public static bool Solid3dFloorActive(int flags) => (flags & FfExists) != 0 && (flags & FfSolid) != 0;

    public static (double DeltaToBottom, double DeltaToTop) MidpointDistanceDeltas(
        double thingBottom,
        double thingTop,
        double ffBottom,
        double ffTop)
    {
        var mid = (ffBottom + ffTop) / 2;
        return (Math.Abs(thingBottom - mid), Math.Abs(thingTop - mid));
    }

    public static bool ShouldPickLowestCeiling(
        double ffBottom,
        double lowestCeiling,
        double deltaToBottom,
        double deltaToTop) =>
        ffBottom < lowestCeiling && deltaToBottom > deltaToTop;

    public static bool FloorCandidateDominatesThing(
        double deltaToBottom,
        double deltaToTop,
        bool restrict,
        double thingZ,
        double ffTop) =>
        deltaToBottom <= deltaToTop && (!restrict || thingZ >= ffTop);

    public static bool ShouldRaiseHighestFloor(double ffTop, double highestFloor) => ffTop > highestFloor;

    public static bool ShouldAdoptHigherFloorPlane(double ffTop, double sectorFloorZ) => ffTop > sectorFloorZ;

    public static bool ShouldTrackSteppableLowestFloor(
        double ffTop,
        double lowestFloorOnSide,
        double thingZ,
        double maxStepHeight) =>
        ffTop > lowestFloorOnSide && ffTop <= thingZ + maxStepHeight;

    public static bool ShouldRaiseOpeningBottom(double highestFloor, double openBottom) => highestFloor > openBottom;

    public static bool FloorPlaneDiffersFromOpening(object? trackedPlane, object? openingPlane) =>
        !ReferenceEquals(trackedPlane, openingPlane);

    public static bool SecPlaneNeedsFlip(double planeCoeffC) => planeCoeffC < 0;
}
