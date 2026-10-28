namespace HCDE.MapLoader;

/// <summary><c>SetSubsectorLightmap</c>, <c>SetSideLightmap</c>, and <c>LoadLightmap</c> from <c>maploader.cpp</c> (3343–3589).</summary>
public static class VmMapLoaderLightmap
{
    public const int SurfaceCeiling = 1;
    public const int SurfaceFloor = 0;
    public const int SurfaceUpperWall = 2;
    public const int SurfaceMiddleWall = 3;
    public const int SurfaceLowerWall = 4;

    public static int ResolveSubsectorMainIndex(int surfaceType) =>
        surfaceType == SurfaceCeiling ? 1 : 0;

    public static int ResolveExtraFloorIndex(int surfaceType) =>
        surfaceType == SurfaceCeiling ? 1 : 0;

    public static bool ShouldAssignMainSubsectorLightmap(bool hasControlSector) => !hasControlSector;

    public static int ResolveSideLightmapSlot(int surfaceType) =>
        surfaceType switch
        {
            SurfaceUpperWall => 0,
            SurfaceMiddleWall => 1,
            SurfaceLowerWall => 3,
            _ => -1
        };

    public static bool ShouldDuplicateMiddleWallSlots(int surfaceType) => surfaceType == SurfaceMiddleWall;

    public static bool ShouldSkipLoad(bool noLightmapsArg, int lumpSize) =>
        noLightmapsArg || lumpSize == 0;

    public static bool IsSupportedVersion(int version) => version == 0;

    public static bool ShouldAbortEmptyPayload(uint numSurfaces, uint numTexCoords, uint numTexBytes) =>
        numSurfaces == 0 || numTexCoords == 0 || numTexBytes == 0;

    public static uint ComputeTextureByteCount(ushort numTextures, ushort textureSize) =>
        (uint)numTextures * textureSize * textureSize * 3 * 2;

    public static int CompareLightProbesByCell(LightProbe a, LightProbe b, double rcpCellSize)
    {
        var cellYa = (int)Math.Floor(a.Y * rcpCellSize);
        var cellYb = (int)Math.Floor(b.Y * rcpCellSize);
        if (cellYa != cellYb)
            return cellYa.CompareTo(cellYb);
        var cellXa = (int)Math.Floor(a.X * rcpCellSize);
        var cellXb = (int)Math.Floor(b.X * rcpCellSize);
        return cellXa.CompareTo(cellXb);
    }

    public static (int minX, int minY, int width, int height) ComputeProbeGridBounds(
        IReadOnlyList<LightProbe> probes,
        double cellSize)
    {
        var rcp = 1.0 / cellSize;
        var minXf = probes[0].X;
        var maxXf = probes[0].X;
        var minYf = probes[0].Y;
        var maxYf = probes[0].Y;
        foreach (var p in probes)
        {
            minXf = Math.Min(minXf, p.X);
            maxXf = Math.Max(maxXf, p.X);
            minYf = Math.Min(minYf, p.Y);
            maxYf = Math.Max(maxYf, p.Y);
        }

        var minX = (int)Math.Floor(minXf * rcp);
        var minY = (int)Math.Floor(minYf * rcp);
        var width = (int)Math.Floor(maxXf * rcp) + 1 - minX;
        var height = (int)Math.Floor(maxYf * rcp) + 1 - minY;
        return (minX, minY, width, height);
    }

    public static bool TryPlaceProbeInCell(
        LightProbe probe,
        int minX,
        int minY,
        int width,
        int height,
        double cellSize,
        out int gridIndex)
    {
        var rcp = 1.0 / cellSize;
        var gridX = (int)Math.Floor(probe.X * rcp) - minX;
        var gridY = (int)Math.Floor(probe.Y * rcp) - minY;
        if (gridX < 0 || gridY < 0 || gridX >= width || gridY >= height)
        {
            gridIndex = -1;
            return false;
        }

        gridIndex = gridX + gridY * width;
        return true;
    }

    public static int CountSideSurfaces(int sideCount, Func<int, int> extraFloorCount) =>
        sideCount == 0 ? 0 : Enumerable.Range(0, sideCount).Sum(i => 4 + extraFloorCount(i));

    public static int CountSubsectorSurfaces(int subsectorCount, Func<int, int> extraFloorCount) =>
        subsectorCount == 0 ? 0 : Enumerable.Range(0, subsectorCount).Sum(i => 2 + extraFloorCount(i) * 2);

    public static bool IsFloorOrCeilingSurface(int type) => type == SurfaceFloor || type == SurfaceCeiling;

    public static bool ShouldApplySubsectorSurface(int type) => IsFloorOrCeilingSurface(type);

    public static bool ShouldApplySideSurface(int type) => type != 0 && !IsFloorOrCeilingSurface(type);

    public readonly record struct LightProbe(float X, float Y);

    public static int ComputePackedTextureWords(uint numTexBytes) => (int)((numTexBytes + 1) / 2);

    public readonly record struct LightmapSurfaceSlot(
        int SurfaceType,
        int LightmapNum,
        int FirstTexCoord,
        int ControlSectorIndex = -1);
}
