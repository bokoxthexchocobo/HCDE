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
}
