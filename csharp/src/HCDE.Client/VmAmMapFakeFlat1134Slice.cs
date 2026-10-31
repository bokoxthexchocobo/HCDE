namespace HCDE.Client;

/// <summary>
/// <c>AM_FakeFlat</c> floor transfer and the <c>drawSubsectors</c> skip gates from
/// <c>am_map.cpp</c> (<b>1951–2100</b>). Chunk <b>1134</b>.
/// </summary>
public static class VmAmMapFakeFlat1134Slice
{
    public const int FakeFloorOnly = 2;
    public const int ClipFakePlanes = 4;
    public const int NoFakeLight = 8;
    public const int SectorHidden = 256;
    public const int SubsectorDrawn = 2;
    public const int SubsectorPolyOrg = 4;
    public const int SubsectorHole = 8;

    public const int TextureUnset = 0;
    public const int TextureControlFloor = 1;
    public const int TextureTargetFloor = 2;
    public const int TextureControlCeiling = 3;

    public static double ViewerZ(bool hasPlayer, double viewZ, double actorZ, double cameraHeight)
    {
        if (hasPlayer)
            return viewZ;
        return actorZ + cameraHeight;
    }

    public static int ViewerArea(bool viewerHasHeightSector, double z, double floorZ, double ceilingZ, int viewerHeightFlags)
    {
        if (!viewerHasHeightSector)
            return 0;
        if (z <= floorZ)
            return -1;
        if (z > ceilingZ && (viewerHeightFlags & FakeFloorOnly) == 0)
            return 1;
        return 0;
    }

    public sealed class FlatResult
    {
        public bool UseDestination { get; set; }
        public bool CopiedControlFloorPlane { get; set; }
        public bool RestoredTargetFloor { get; set; }
        public bool UsedCeilingAsFloor { get; set; }
        public bool CopiedColormap { get; set; }
        public bool CopiedLight { get; set; }
        public int FloorTextureSource { get; set; }
    }

    public static FlatResult Apply(
        bool viewerHasHeightSector,
        double viewerZ,
        double viewerFloorZ,
        double viewerCeilingZ,
        int viewerHeightFlags,
        int controlFlags,
        bool planeCopyValid,
        bool controlFloorIsSky)
    {
        var area = ViewerArea(viewerHasHeightSector, viewerZ, viewerFloorZ, viewerCeilingZ, viewerHeightFlags);
        var diffTex = (controlFlags & ClipFakePlanes) != 0;
        var noLight = (controlFlags & NoFakeLight) != 0;
        var result = new FlatResult { UseDestination = true };
        if (diffTex)
        {
            if (planeCopyValid)
            {
                result.CopiedControlFloorPlane = true;
                result.FloorTextureSource = TextureControlFloor;
            }
            else if ((controlFlags & FakeFloorOnly) != 0)
            {
                if (area == -1)
                {
                    result.CopiedColormap = true;
                    if (!noLight)
                        result.CopiedLight = true;
                    return result;
                }

                result.UseDestination = false;
                return result;
            }
        }
        else
        {
            result.CopiedControlFloorPlane = true;
        }

        if (area == -1)
        {
            result.CopiedColormap = true;
            result.RestoredTargetFloor = true;
            result.FloorTextureSource = diffTex ? TextureTargetFloor : TextureControlFloor;
            if (!noLight)
                result.CopiedLight = true;
        }
        else if (area == 1)
        {
            result.CopiedColormap = true;
            result.UsedCeilingAsFloor = true;
            result.FloorTextureSource = controlFloorIsSky ? TextureControlCeiling : TextureControlFloor;
            if (!noLight)
                result.CopiedLight = true;
        }

        return result;
    }

    public static bool SkipSubsector(
        int subsectorFlags, int sectorFlags, int cheat, bool portalOverlay, int portalGroup, int mapPortalGroup)
    {
        if ((subsectorFlags & SubsectorPolyOrg) != 0)
            return true;
        var undrawn = (subsectorFlags & SubsectorDrawn) == 0
            || (subsectorFlags & SubsectorHole) != 0
            || (sectorFlags & SectorHidden) != 0;
        if (cheat == 0 && undrawn)
            return true;
        if (cheat >= 4 && (sectorFlags & SectorHidden) != 0)
            return true;
        if (portalOverlay && portalGroup != mapPortalGroup && portalGroup != 0)
            return true;
        return false;
    }
}
