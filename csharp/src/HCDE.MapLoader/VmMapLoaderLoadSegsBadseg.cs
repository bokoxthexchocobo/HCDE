namespace HCDE.MapLoader;

/// <summary><c>badseg</c> exception type and <c>LoadSegs</c> preamble from <c>maploader.cpp</c> (793–813).</summary>
public static class VmMapLoaderLoadSegsBadseg
{
    public readonly record struct BadSeg(int BadType, int BadSegNum, int BadData);

    public static int ComputeSegCount(int lumpLen, int recordSize) =>
        recordSize == 0 ? 0 : lumpLen / recordSize;

    public static bool ShouldAbortWhenZeroSegs(int segCount) => segCount == 0;

    public static bool IsVertexIndexOutOfRange(uint v1, uint v2, uint vertexCount) =>
        v1 >= vertexCount || v2 >= vertexCount;

    public static int SelectHigherVertexForSnap(uint v1, uint v2) => v2 > v1 ? (int)v2 : (int)v1;

    public static string FormatBadSegMessage(BadSeg bad, uint lineCount, int sideCount)
    {
        return bad.BadType switch
        {
            0 => $"Seg {bad.BadSegNum} references a nonexistant vertex {bad.BadData} (max {sideCount}).\n",
            1 => $"Seg {bad.BadSegNum} references a nonexistant linedef {bad.BadData} (max {lineCount}).\n",
            2 => $"The linedef for seg {bad.BadSegNum} references a nonexistant sidedef {bad.BadData} (max {sideCount}).\n",
            3 => $"Sidedef reference in seg {bad.BadSegNum} is {bad.BadData} (must be 0 or 1).\n",
            _ => string.Empty
        };
    }

    public static string DescribeBspRebuildBanner() => "The BSP will be rebuilt.\n";

    public static void MarkLineVertices(Span<byte> vertChanged, int v1, int v2)
    {
        if ((uint)v1 < (uint)vertChanged.Length)
            vertChanged[v1] = 1;
        if ((uint)v2 < (uint)vertChanged.Length)
            vertChanged[v2] = 1;
    }

    public static bool IsSideIndexValid(int side) => side == 0 || side == 1;

    public static int CombineTwosidedBacksector(int side, bool twosided, bool oppositeSidePresent) =>
        twosided && oppositeSidePresent ? side ^ 1 : -1;

    public static bool ShouldSnapSplitVertex(double deltaAngleDegrees) => deltaAngleDegrees >= 1.0;

    public static bool ShouldSnapHigherVertex(uint v1, uint v2, int vertChangedV2) =>
        v2 > v1 && vertChangedV2 == 0;

    public static bool ShouldSnapLowerVertex(int vertChangedV1) => vertChangedV1 == 0;

    public static int NormalizeSegAngle(ushort rawAngle) => rawAngle << 16;
}
