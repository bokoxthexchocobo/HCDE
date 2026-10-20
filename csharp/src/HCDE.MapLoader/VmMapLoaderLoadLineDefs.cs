namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadLineDefs</c> and <c>LoadLineDefs2</c> preprocessing from <c>maploader.cpp</c> (1676–1879).</summary>
public static class VmMapLoaderLoadLineDefs
{
    public const int NoIndex = 0xffff;

    public static bool IsVertexIndexInvalid(uint v1, uint v2, uint vertexCount) =>
        v1 >= vertexCount || v2 >= vertexCount;

    public static bool IsZeroLengthLine(uint v1, uint v2, double v1x, double v1y, double v2x, double v2y) =>
        v1 == v2 || (v1x == v2x && v1y == v2y);

    public static int CountSidesForLine(bool hasSecondSide) => hasSecondSide ? 2 : 1;

    public static void ValidateSidedefIndex(ref ushort sideIndex, int numsides, int lineIndex, int side)
    {
        if (sideIndex != NoIndex && sideIndex >= numsides)
        {
            sideIndex = 0;
        }
    }

    public static void PatchMissingFirstSide(ref ushort frontSide, int lineIndex)
    {
        if (frontSide == NoIndex)
            frontSide = 0;
    }

    public static int MapLineIndexAfterSkips(int logicalIndex, int skipped) => logicalIndex + skipped;

    public static bool ShouldForceNodeBuildOnSkip(bool skippedAny) => skippedAny;

    public static ushort ReadLittleShort(ushort value) => value;

    public static int ResolveActivationFromFlags(int lineFlags, int spacMask, int spacAnyCross, int spacImpact, int spacPcross)
    {
        var activation = 1 << GetSpac(lineFlags, spacMask);
        if (activation == spacAnyCross)
            return spacImpact | spacPcross;
        if (activation == spacImpact)
            return spacImpact | spacPcross;
        return activation;
    }

    public static int GetSpac(int flags, int spacMask) => (flags & spacMask) >> 0;

    public static int StripSpacMask(int flags, int spacMask) => flags & ~spacMask;

    public static bool ShouldApplyClipMidTex(bool levelClipMidTex) => levelClipMidTex;

    public static bool ShouldApplyWrapMidTex(bool levelWrapMidTex) => levelWrapMidTex;

    public static bool ShouldApplyCheckSwitchRange(bool levelCheckSwitch) => levelCheckSwitch;

    public static int ApplyLevelLineFlags(int lineFlags, bool clipMidTex, bool wrapMidTex, bool checkSwitch)
    {
        if (clipMidTex)
            lineFlags |= 1 << 0;
        if (wrapMidTex)
            lineFlags |= 1 << 1;
        if (checkSwitch)
            lineFlags |= 1 << 2;
        return lineFlags;
    }

    public static bool ShouldTagLineForId(int special, int staticInit, int initEdLine, int initEdSector, int tag) =>
        special != staticInit || (tag != initEdLine && tag != initEdSector);

    public readonly struct DoomLineRecord
    {
        public DoomLineRecord(ushort v1, ushort v2, ushort side0, ushort side1)
        {
            V1 = v1;
            V2 = v2;
            Side0 = side0;
            Side1 = side1;
        }

        public ushort V1 { get; }
        public ushort V2 { get; }
        public ushort Side0 { get; }
        public ushort Side1 { get; }
    }

    public static (List<DoomLineRecord> kept, int skipped, int sideCount) CompactZeroLengthDoomLines(
        IReadOnlyList<DoomLineRecord> lines,
        Func<ushort, (double x, double y)> vertexPos,
        uint vertexCount)
    {
        var kept = new List<DoomLineRecord>(lines.Count);
        var skipped = 0;
        var sideCount = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var v1 = line.V1;
            var v2 = line.V2;
            if (IsVertexIndexInvalid(v1, v2, vertexCount))
                throw new InvalidOperationException($"Line {i + skipped} has invalid vertices {v1}/{v2} (max {vertexCount}).");

            var (x1, y1) = vertexPos(v1);
            var (x2, y2) = vertexPos(v2);
            if (IsZeroLengthLine(v1, v2, x1, y1, x2, y2))
            {
                skipped++;
                continue;
            }

            kept.Add(line);
            sideCount += CountSidesForLine(line.Side1 != NoIndex);
        }

        return (kept, skipped, sideCount);
    }

    public static (List<DoomLineRecord> kept, int skipped, int sideCount) CompactZeroLengthHexenLines(
        IReadOnlyList<DoomLineRecord> lines,
        Func<ushort, (double x, double y)> vertexPos)
    {
        var kept = new List<DoomLineRecord>(lines.Count);
        var skipped = 0;
        var sideCount = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var side0 = line.Side0;
            PatchMissingFirstSide(ref side0, i + skipped);
            var (x1, y1) = vertexPos(line.V1);
            var (x2, y2) = vertexPos(line.V2);
            if (IsZeroLengthLine(line.V1, line.V2, x1, y1, x2, y2))
            {
                skipped++;
                continue;
            }

            kept.Add(new DoomLineRecord(line.V1, line.V2, side0, line.Side1));
            sideCount += CountSidesForLine(line.Side1 != NoIndex);
        }

        return (kept, skipped, sideCount);
    }

    public static int ResolveActivationFromFlags(int lineFlags, int spacMask, int spacAnyCross, int spacImpact, int spacPcross)
    {
        var spac = (lineFlags & spacMask) >> 10;
        var activation = 1 << spac;
        if (activation == spacAnyCross)
            return spacImpact | spacPcross;
        if (activation == spacImpact)
            return spacImpact | spacPcross;
        return activation;
    }
}
