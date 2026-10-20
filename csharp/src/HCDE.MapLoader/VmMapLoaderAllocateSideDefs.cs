namespace HCDE.MapLoader;

/// <summary>
/// <c>MapLoader::AllocateSideDefs</c> and the opening of <c>LoopSidedefs</c> from <c>maploader.cpp</c> (1888–1950).</summary>
public static class VmMapLoaderAllocateSideDefs
{
    public const int NoSide = -1;

    public readonly struct SideTempA
    {
        public SideTempA(int special, int tag, short alpha, int map)
        {
            Special = special;
            Tag = tag;
            Alpha = alpha;
            Map = map;
        }

        public int Special { get; }
        public int Tag { get; }
        public short Alpha { get; }
        public int Map { get; }
    }

    public readonly struct SideTempB
    {
        public SideTempB(int first, int next, bool lineSide)
        {
            First = first;
            Next = next;
            LineSide = lineSide;
        }

        public int First { get; }
        public int Next { get; }
        public bool LineSide { get; }
    }

    public static SideTempA CreateDefaultSideTempA() => new(0, 0, short.MinValue, NoSide);

    public static int ComputeUnusedSidedefCount(int lumpSides, int allocatedCount) =>
        lumpSides > allocatedCount ? lumpSides - allocatedCount : 0;

    public static bool ShouldReportUnusedSidedefs(int lumpSides, int allocatedCount) =>
        allocatedCount < lumpSides;

    public static int ResizeSideTempTable(int count, int vertexCount) =>
        Math.Max(count, vertexCount);

    public static void ResetVertexSideLists(Span<int> firstAtVertex, Span<int> nextAtSide, int vertexCount, int sideCount)
    {
        for (var i = 0; i < vertexCount; i++)
            firstAtVertex[i] = NoSide;
        for (var i = vertexCount; i < sideCount; i++)
            nextAtSide[i] = NoSide;
    }

    public static (int lineSide, int vertexIndex) ResolveSideLineAttachment(bool sidesOnV2, int v1, int v2) =>
        (sidesOnV2 ? 1 : 0, sidesOnV2 ? v2 : v1);

    public static int PrependSideToVertexList(int sideIndex, int vertexFirst, int sideNext) =>
        sideIndex;

    public static void LinkSideAtVertex(int[] firstAtVertex, int[] nextAtSide, int sideIndex, int vertex, int previousFirst)
    {
        nextAtSide[sideIndex] = previousFirst;
        firstAtVertex[vertex] = sideIndex;
    }

    public static void InitializeSideLoopLinks(int leftSide, int rightSide)
    {
        _ = leftSide;
        _ = rightSide;
    }

    public static bool IsSelfContainedSectorLoop(bool sameSector) => sameSector;

    public static bool ShouldWarnUnconnectedRightEdge(bool firstLoop, bool hasRightSide) =>
        firstLoop && !hasRightSide;
}
