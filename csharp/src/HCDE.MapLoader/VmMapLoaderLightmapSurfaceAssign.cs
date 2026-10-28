namespace HCDE.MapLoader;

/// <summary>Lightmap surface slot assignment from <c>maploader.cpp</c> (3343–3571).</summary>
public static class VmMapLoaderLightmapSurfaceAssign
{
    public static void WriteMainSubsectorSlot(
        VmMapLoaderLightmap.LightmapSurfaceSlot[] pair,
        int surfaceType,
        int lightmapNum,
        int firstTexCoord)
    {
        var index = VmMapLoaderLightmap.ResolveSubsectorMainIndex(surfaceType);
        pair[index] = new VmMapLoaderLightmap.LightmapSurfaceSlot(surfaceType, lightmapNum, firstTexCoord);
    }

    public static void WriteExtraFloorSubsectorSlots(
        VmMapLoaderLightmap.LightmapSurfaceSlot[] slots,
        int surfaceType,
        int controlSectorIndex,
        int lightmapNum,
        int firstTexCoord,
        int extraFloorCount)
    {
        for (var i = 0; i < extraFloorCount; i++)
        {
            if (i + 1 >= slots.Length)
                break;
            slots[i + 1] = new VmMapLoaderLightmap.LightmapSurfaceSlot(surfaceType, lightmapNum, firstTexCoord, controlSectorIndex);
        }
    }

    public static void WriteSideWallSlot(
        Span<VmMapLoaderLightmap.LightmapSurfaceSlot> slots,
        int surfaceType,
        int lightmapNum,
        int firstTexCoord)
    {
        var slot = VmMapLoaderLightmap.ResolveSideLightmapSlot(surfaceType);
        if (slot < 0 || slot >= slots.Length)
            return;
        slots[slot] = new VmMapLoaderLightmap.LightmapSurfaceSlot(surfaceType, lightmapNum, firstTexCoord);
        if (VmMapLoaderLightmap.ShouldDuplicateMiddleWallSlots(surfaceType) && slot + 1 < slots.Length)
            slots[slot + 1] = new VmMapLoaderLightmap.LightmapSurfaceSlot(surfaceType, lightmapNum, firstTexCoord);
    }

    public static void WriteExtraFloorSideSlots(
        Span<VmMapLoaderLightmap.LightmapSurfaceSlot> slots,
        int surfaceType,
        int controlSectorIndex,
        int lightmapNum,
        int firstTexCoord,
        int extraFloorCount)
    {
        for (var i = 0; i < extraFloorCount; i++)
        {
            var idx = 4 + i;
            if (idx >= slots.Length)
                break;
            slots[idx] = new VmMapLoaderLightmap.LightmapSurfaceSlot(surfaceType, lightmapNum, firstTexCoord, controlSectorIndex);
        }
    }

    public static int LinkSideSurfaces(int sideIndex, int offset, int extraFloors) => offset + 4 + extraFloors;

    public static int LinkSubsectorSurfaces(int subsectorIndex, int offset, int extraFloors)
    {
        var count = 1 + extraFloors;
        return offset + count * 2;
    }

    public static bool ShouldMarkSectorHasLightmaps(int surfaceType) =>
        VmMapLoaderLightmap.IsFloorOrCeilingSurface(surfaceType);

    public static uint ReadSurfaceHeader(
        uint typeIndex,
        uint controlSector,
        uint lightmapNum,
        uint firstTexCoord,
        out int surfaceType)
    {
        surfaceType = (int)typeIndex;
        return controlSector == 0xffffffffu ? lightmapNum : firstTexCoord;
    }
}
