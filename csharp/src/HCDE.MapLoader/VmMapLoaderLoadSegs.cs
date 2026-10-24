namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadSegs</c> and <c>LoadSubsectors</c> entry policy from <c>maploader.cpp</c> (802–860, 985–1054).</summary>
public static class VmMapLoaderLoadSegs
{
    public static bool ShouldAbortEmptySegs(int numSegs)
    {
        if (numSegs == 0)
            return true;
        return false;
    }

    public static void ValidateVertexIndex(uint vnum, uint numVertexes, int segIndex)
    {
        if (vnum >= numVertexes)
            throw new InvalidOperationException($"seg {segIndex} references invalid vertex {vnum}");
    }

    public static int CountSegs(int lumpBytes, int recordSize) =>
        recordSize > 0 ? lumpBytes / recordSize : 0;

    public static bool ShouldRebuildBspAfterEmptySegs => true;

    public static int CountSubsectors(int lumpBytes, int recordSize) =>
        recordSize > 0 ? lumpBytes / recordSize : 0;

    public static bool ShouldRejectZeroSubsectors(int numSubsectors) => numSubsectors == 0;

    public static uint ResolveSegAngle(ushort rawAngle) => rawAngle;

    public static void MarkLineVertices(Span<byte> vertChanged, int v1, int v2)
    {
        if (v1 >= 0 && v1 < vertChanged.Length)
            vertChanged[v1] = 1;
        if (v2 >= 0 && v2 < vertChanged.Length)
            vertChanged[v2] = 1;
    }

    public static bool ShouldSnapSplitVertex(double deltaDegrees) => deltaDegrees >= 1.0;

    public static int PickVertexToSnap(uint v1, uint v2, byte vertChangedV1, byte vertChangedV2) =>
        v2 > v1 && vertChangedV2 == 0 ? 2 : vertChangedV1 == 0 ? 1 : 0;

    public static void ValidateLinedefIndex(uint linedef, uint lineCount, int segIndex)
    {
        if (linedef >= lineCount)
            throw new InvalidOperationException($"seg {segIndex} references invalid linedef {linedef}");
    }

    public static void ValidateSideIndex(int side, int segIndex)
    {
        if (side != 0 && side != 1)
            throw new InvalidOperationException($"seg {segIndex} has invalid side {side}");
    }

    public static bool HasIncompleteBsp(int numSubsectors, int maxSeg) =>
        numSubsectors == 0 || maxSeg == 0;

    public static bool IsSubsectorSegRangeInvalid(uint firstSeg, uint segCount, uint maxSeg)
    {
        if (firstSeg >= maxSeg)
            return true;
        return firstSeg + segCount > maxSeg;
    }

    public enum BadSegKind
    {
        Vertex = 0,
        Linedef = 1,
        Sidedef = 2,
        SideIndex = 3,
    }

    public static string FormatBadSegMessage(BadSegKind kind, int segIndex, int badData, int maxValue) =>
        kind switch
        {
            BadSegKind.Vertex =>
                $"Seg {segIndex} references a nonexistant vertex {badData} (max {maxValue}).",
            BadSegKind.Linedef =>
                $"Seg {segIndex} references a nonexistant linedef {badData} (max {maxValue}).",
            BadSegKind.Sidedef =>
                $"The linedef for seg {segIndex} references a nonexistant sidedef {badData} (max {maxValue}).",
            BadSegKind.SideIndex =>
                $"Sidedef reference in seg {segIndex} is {badData} (must be 0 or 1).",
            _ => $"Seg {segIndex} has invalid data {badData}.",
        };

    public static bool ShouldClearBacksector(ushort lineFlags, bool hasOppositeSide) =>
        !VmMapLoaderZNodeLoad.IsTwoSided(lineFlags, hasOppositeSide);

    public static ushort NormalizeTwoSidedFlag(ushort lineFlags, bool hasOppositeSide) =>
        VmMapLoaderZNodeLoad.ClearTwoSidedIfNoBack(lineFlags, hasOppositeSide);

    public static double ComputePolyobjPortalSampleX(double v1x, double deltaX) =>
        v1x + deltaX * 0.5;

    public static bool ShouldRebuildBspOnBadSeg => true;

    public static bool ShouldRejectEmptySubsector(uint numSegs) => numSegs == 0;

    public static string FormatInvalidSubsectorSegRange(int subsectorIndex, uint first, uint count, uint maxSeg) =>
        $"Subsector {subsectorIndex} contains invalid segs {first}-{first + count - 1} (max {maxSeg})";

    public static int ReadSubsectorSegCount(bool wideFields, ushort narrow, uint wide) =>
        wideFields ? (int)wide : narrow;

    public static int ReadSubsectorFirstSeg(bool wideFields, ushort narrow, uint wide) =>
        wideFields ? (int)wide : narrow;

    public static bool ShouldClearNodesOnIncompleteBsp => true;

    public static bool ShouldClearSubsectorsOnIncompleteBsp => true;

    public static string BspRebuildBanner => "The BSP will be rebuilt.";

    public static string DescribeBspFailure(BadSegKind kind, int segIndex, int badData, int maxValue) =>
        $"{FormatBadSegMessage(kind, segIndex, badData, maxValue)} {BspRebuildBanner}";
}
