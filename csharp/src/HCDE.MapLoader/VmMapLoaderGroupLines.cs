namespace HCDE.MapLoader;

/// <summary><c>MapLoader::GroupLines</c> from <c>maploader.cpp</c> (2659–2786).</summary>
public static class VmMapLoaderGroupLines
{
    public static bool ShouldFlagMissingFrontSector(bool frontSectorNull, bool alreadyFlagged) =>
        frontSectorNull && !alreadyFlagged;

    public static bool ShouldCountBackSectorLine(bool hasBackSector, bool sameAsFront) =>
        hasBackSector && !sameAsFront;

    public static int IncrementLineCount(int total) => total + 1;

    public static int AddFrontSectorLine(int sectorLineCount) => sectorLineCount + 1;

    public static int AddBackSectorLine(int sectorLineCount) => sectorLineCount + 1;

    public static bool ShouldAbortOnMissingFrontSectors(bool flaggedNoFronts) => flaggedNoFronts;

    public static bool ShouldStripSectorTags(int sectorLineCount) => sectorLineCount == 0;

    public static bool IsLineAssignmentMiscount(int linesDone, int sectorLineCount) =>
        linesDone != sectorLineCount;

    public static bool UseBoundingBoxCenter(int sectorLineCount) => sectorLineCount > 3;

    public static bool UseVertexAverageCenter(int sectorLineCount) =>
        sectorLineCount > 0 && sectorLineCount <= 3;

    public static (double x, double y) ComputeBoundingBoxCenter(
        IEnumerable<(double x, double y)> vertices)
    {
        double left = double.PositiveInfinity;
        double right = double.NegativeInfinity;
        double bottom = double.PositiveInfinity;
        double top = double.NegativeInfinity;
        foreach (var (x, y) in vertices)
        {
            if (x < left) left = x;
            if (x > right) right = x;
            if (y < bottom) bottom = y;
            if (y > top) top = y;
        }

        return ((right + left) / 2, (top + bottom) / 2);
    }

    public static (double x, double y) ComputeSmallSectorCenter(
        IReadOnlyList<(double v1x, double v1y, double v2x, double v2y)> lineEndpoints)
    {
        double posX = 0;
        double posY = 0;
        foreach (var (v1x, v1y, v2x, v2y) in lineEndpoints)
        {
            posX += v1x + v2x;
            posY += v1y + v2y;
        }

        var divisor = 2 * lineEndpoints.Count;
        return (posX / divisor, posY / divisor);
    }

    public static int CountTotalLineSlots(IEnumerable<(bool hasFront, bool hasDistinctBack)> lines)
    {
        var total = 0;
        foreach (var (hasFront, hasDistinctBack) in lines)
        {
            if (hasFront)
                total++;
            if (hasDistinctBack)
                total++;
        }

        return total;
    }

    public static int[] AssignLinesToSectors(
        int sectorCount,
        IReadOnlyList<int> sectorLineCounts,
        IReadOnlyList<(int frontSector, int backSector, bool hasFront, bool hasDistinctBack)> lineSectors)
    {
        var done = new int[sectorCount];
        foreach (var (frontSector, backSector, hasFront, hasDistinctBack) in lineSectors)
        {
            if (hasFront)
                done[frontSector]++;
            if (hasDistinctBack)
                done[backSector]++;
        }

        return done;
    }

    public static bool AllSectorsBalanced(int[] linesDone, IReadOnlyList<int> sectorLineCounts)
    {
        for (var i = 0; i < linesDone.Length; i++)
        {
            if (IsLineAssignmentMiscount(linesDone[i], sectorLineCounts[i]))
                return false;
        }

        return true;
    }

    public static bool ShouldInvokeSetSlopes(bool buildMap) => !buildMap;

    public static int ResolveSubsectorSectorIndex(int firstSideSectorIndex) => firstSideSectorIndex;
}
