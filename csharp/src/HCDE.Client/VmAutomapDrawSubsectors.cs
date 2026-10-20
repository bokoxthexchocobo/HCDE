namespace HCDE.Client;

/// <summary><c>DAutomap::drawSubsectors</c> policy from <c>am_map.cpp</c> (2062–2307).</summary>
public static class VmAutomapDrawSubsectors
{
    public const int SubsectorPolyOrg = 1;
    public const int SubsectorDrawn = 2;
    public const int SubsectorHole = 4;
    public const int SectorHidden = 8;

    public static bool ShouldSkipSubsector(
        uint subFlags,
        uint renderSectorMoreFlags,
        int amCheat,
        bool portalOverlay,
        int subPortalGroup,
        int mapPortalGroup)
    {
        if ((subFlags & SubsectorPolyOrg) != 0)
            return true;

        var notDrawn = (subFlags & SubsectorDrawn) == 0;
        var isHole = (subFlags & SubsectorHole) != 0;
        var hidden = (renderSectorMoreFlags & SectorHidden) != 0;
        if ((notDrawn || isHole || hidden) && amCheat == 0)
            return true;

        if (amCheat >= 4 && hidden)
            return true;

        if (portalOverlay && subPortalGroup != mapPortalGroup && subPortalGroup != 0)
            return true;

        return false;
    }

    public static bool ShouldUseTexturedFill(bool amTextured, bool viewActive) =>
        amTextured && !viewActive;

    public static bool ShouldSkipSkyFlat(bool textureIsSky) => textureIsSky;

    public static bool NeedsHoleTriangulation(uint subFlags, int lineCount) =>
        (subFlags & SubsectorHole) != 0 && lineCount > 3;

    public static (byte r, byte g, byte b, int desat) ApplyUndrawnCheatTint(
        byte r,
        byte g,
        byte b,
        int desaturation,
        bool subDrawn,
        int amCheat)
    {
        if (subDrawn || amCheat >= 4)
            return (r, g, b, desaturation);

        return (
            (byte)((r + 255) / 2),
            (byte)((g + 200) / 2),
            (byte)((b + 160) / 2),
            255 - (255 - desaturation) / 4);
    }

    public static uint FadeTableAutomapColor => 0x80808080;

    public static double ComputeSoftFadeLevel(int floorLight, int numColorMaps = 32)
    {
        var map = numColorMaps * 2.0 - (floorLight + 12) * (numColorMaps / 128.0);
        return Math.Clamp((map - 12) / numColorMaps, 0.0, 1.0);
    }

    public static double ComputeLinearFadeLevel(int floorLight) =>
        1.0 - Math.Clamp(floorLight, 0, 255) / 255.0;

    public static double ResolveFadeLevel(int floorLight, bool softLightRamp, int numColorMaps = 32) =>
        softLightRamp
            ? ComputeSoftFadeLevel(floorLight, numColorMaps)
            : ComputeLinearFadeLevel(floorLight);

    public static (float x, float y) MapSubsectorPointToScreen(
        double mapX,
        double mapY,
        int frameX,
        int frameY,
        int frameHeight,
        double mapOriginX,
        double mapOriginY,
        double scale)
    {
        var sx = (float)(frameX + (mapX - mapOriginX) * scale);
        var sy = (float)(frameY + (frameHeight - (mapY - mapOriginY) * scale));
        return (sx, sy);
    }

    [Flags]
    public enum FFloorFlags
    {
        Exists = 1,
        Fog = 2,
        ThisInside = 4,
        RenderPlanes = 8,
        InvertPlanes = 16,
    }

    public static bool ShouldSkipFFloor(FFloorFlags flags, int alpha) =>
        (flags & FFloorFlags.Exists) == 0
        || (flags & (FFloorFlags.Fog | FFloorFlags.ThisInside)) != 0
        || (flags & FFloorFlags.RenderPlanes) == 0
        || alpha == 0;

    public static bool IsRoverBelowCeiling(double roverZ, double secCeilingZ) => roverZ > secCeilingZ;

    public static bool IsRoverBelowFloor(double roverZ, double secFloorZ) => roverZ < secFloorZ;

    public static bool ShouldPickRoverTop(double roverZ, double cmpZ, double secFloorZ, double secCeilingZ) =>
        !IsRoverBelowCeiling(roverZ, secCeilingZ) && !IsRoverBelowFloor(roverZ, secFloorZ) && roverZ < cmpZ;

    public static int SelectPlaneSelector(FFloorFlags flags) =>
        (flags & FFloorFlags.InvertPlanes) != 0 ? 0 : 1;

    public static bool ShouldUseSectorFillColor(bool sectorFillValid, bool texturedMode) =>
        sectorFillValid && !texturedMode;

    public static double ResolveFillAlpha(double fillAlpha) => fillAlpha;

    public static bool ShouldApplyFadeTableColormap(bool levelHasFadeTable, bool texturedMode) =>
        levelHasFadeTable && texturedMode;
}
