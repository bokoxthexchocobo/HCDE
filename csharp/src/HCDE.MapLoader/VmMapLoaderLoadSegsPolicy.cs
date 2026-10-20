namespace HCDE.MapLoader;

/// <summary>Additional <c>LoadSegs</c> / <c>LoadSubsectors</c> policy from <c>maploader.cpp</c>.</summary>
public static class VmMapLoaderLoadSegsPolicy
{
    public static double AngleDeltaDegrees(double pointAngle, double segAngle) =>
        Math.Abs(pointAngle - segAngle);

    public static bool ShouldAdjustSplitVertex(double deltaDegrees, byte vertChanged) =>
        deltaDegrees >= 1.0 && vertChanged == 0;

    public static (double x, double y) SnapVertexAlongSeg(
        double anchorX,
        double anchorY,
        double dirX,
        double dirY,
        double length)
    {
        var mag = Math.Sqrt(dirX * dirX + dirY * dirY);
        if (mag == 0)
            return (anchorX, anchorY);
        var scale = length / mag;
        return (anchorX + dirX * scale, anchorY + dirY * scale);
    }

    public static bool HasValidSidePair(int side, uint sidedefIndex, uint sidesCount) =>
        (side == 0 || side == 1) && sidedefIndex < sidesCount;

    public static bool ShouldAssignBacksector(ushort lineFlags, bool oppositeSidePresent) =>
        VmMapLoaderZNodeLoad.IsTwoSided(lineFlags, oppositeSidePresent);

    public static int MapBadSegKindToLogLevel(VmMapLoaderLoadSegs.BadSegKind kind) =>
        kind == VmMapLoaderLoadSegs.BadSegKind.SideIndex ? 1 : 0;

    public static string DescribeBspFailure(VmMapLoaderLoadSegs.BadSegKind kind, int seg, int data, int max) =>
        $"{VmMapLoaderLoadSegs.FormatBadSegMessage(kind, seg, data, max)} {VmMapLoaderLoadSegs.BspRebuildBanner}";

    public static bool ShouldAbortSubsectorLoad(uint numSegs, uint firstSeg, uint maxSeg) =>
        numSegs == 0 || firstSeg >= maxSeg || firstSeg + numSegs > maxSeg;

    public static uint ComputeMaxSegFromLump(int lumpBytes, int recordSize) =>
        recordSize > 0 ? (uint)(lumpBytes / recordSize) : 0u;

    public static bool IncompleteBspTree(int subsectors, int maxSeg) =>
        subsectors == 0 || maxSeg == 0;

    public static int SelectVertexToAdjust(uint v1, uint v2, byte changed1, byte changed2)
    {
        if (v2 > v1 && changed2 == 0)
            return 2;
        if (changed1 == 0)
            return 1;
        return 0;
    }

    public static double SegLength(double dx, double dy) => Math.Sqrt(dx * dx + dy * dy);

    public static bool LineUsesPolyobjPortalSample(bool polyobjWall) => polyobjWall;

    public static int PortalGroupFromSample(int sampledGroup, bool polyobjWall, int frontSectorGroup) =>
        polyobjWall ? sampledGroup : frontSectorGroup;

    public static bool ShouldMarkVertexUsed(Span<byte> table, int index) =>
        index >= 0 && index < table.Length && table[index] == 0;

    public static void MarkVertexUsed(Span<byte> table, int index)
    {
        if (index >= 0 && index < table.Length)
            table[index] = 1;
    }

    public static bool ValidateSegIteratorBounds(int index, int count) => index >= 0 && index < count;

    public static string EmptySubsectorMessage(int index) => $"Subsector {index} is empty.";

    public static string IncompleteBspMessage => "This map has an incomplete BSP tree.";

    public static bool ShouldZeroSegMemory => true;

    public static bool ShouldRelinkSubsectorFirstLines => true;

    public static int SubsectorRecordSize(bool extended) => extended ? 8 : 4;

    public static bool UseWideSubsectorFields(bool extended) => extended;

    public static uint MaskSegIndex(ushort value) => value == 0xffff ? 0xffffffffu : value;

    public static bool SegReferencesLine(uint lineIndex) => lineIndex != 0xffffffffu;

    public static int SideFromSegByte(byte side) => side;

    public static bool OppositeSideExists(bool[] sidesPresent, int side) =>
        side >= 0 && side < sidesPresent.Length && sidesPresent[side ^ 1];

    public readonly struct SegLoadContext
    {
        public SegLoadContext(uint vertexCount, uint lineCount, uint sideCount, int recordSize)
        {
            VertexCount = vertexCount;
            LineCount = lineCount;
            SideCount = sideCount;
            RecordSize = recordSize;
        }

        public uint VertexCount { get; }
        public uint LineCount { get; }
        public uint SideCount { get; }
        public int RecordSize { get; }
    }

    public static void ValidateSegVertexPair(uint v1, uint v2, in SegLoadContext ctx, int segIndex)
    {
        if (v1 >= ctx.VertexCount || v2 >= ctx.VertexCount)
            throw new InvalidOperationException(
                VmMapLoaderLoadSegs.FormatBadSegMessage(
                    VmMapLoaderLoadSegs.BadSegKind.Vertex,
                    segIndex,
                    (int)Math.Max(v1, v2),
                    (int)ctx.VertexCount));
    }

    public static void ValidateSegLinedef(uint linedef, in SegLoadContext ctx, int segIndex)
    {
        if (linedef >= ctx.LineCount)
            throw new InvalidOperationException(
                VmMapLoaderLoadSegs.FormatBadSegMessage(
                    VmMapLoaderLoadSegs.BadSegKind.Linedef,
                    segIndex,
                    (int)linedef,
                    (int)ctx.LineCount));
    }

    public static void ValidateSegSidedef(uint sidedefIndex, in SegLoadContext ctx, int segIndex)
    {
        if (sidedefIndex >= ctx.SideCount)
            throw new InvalidOperationException(
                VmMapLoaderLoadSegs.FormatBadSegMessage(
                    VmMapLoaderLoadSegs.BadSegKind.Sidedef,
                    segIndex,
                    (int)sidedefIndex,
                    (int)ctx.SideCount));
    }

    public static void ValidateSegSideByte(int side, int segIndex) =>
        VmMapLoaderLoadSegs.ValidateSideIndex(side, segIndex);

    public static bool TryPlanVertexSnap(
        double ptpAngle,
        double segAngle,
        uint v1,
        uint v2,
        byte changed1,
        byte changed2,
        out int vertexToMove)
    {
        vertexToMove = 0;
        var delta = AngleDeltaDegrees(ptpAngle, segAngle);
        if (!ShouldAdjustSplitVertex(delta, 0))
            return false;
        vertexToMove = SelectVertexToAdjust(v1, v2, changed1, changed2);
        return vertexToMove != 0;
    }

    public static (double x, double y) ApplyVertexSnap(
        double ax,
        double ay,
        double bx,
        double by,
        int vertexToMove,
        double segAngleRadians,
        double length)
    {
        var (dx, dy) = (Math.Cos(segAngleRadians), Math.Sin(segAngleRadians));
        return vertexToMove == 2
            ? SnapVertexAlongSeg(ax, ay, dx, dy, length)
            : SnapVertexAlongSeg(bx, by, -dx, -dy, length);
    }

    public static bool ShouldContinueSubsectorLoop(uint index, uint count, uint first, uint num, uint maxSeg) =>
        index < count && !ShouldAbortSubsectorLoad(num, first, maxSeg);

    public static string FormatSubsectorOverflow(int i, uint first, uint num, uint maxSeg) =>
        VmMapLoaderLoadSegs.FormatInvalidSubsectorSegRange(i, first, num, maxSeg);

    public static int CountSegmentsFromLump(int bytes, int recordSize) =>
        recordSize > 0 ? bytes / recordSize : 0;

    public static bool RequiresBspRebuildAfterCatch(VmMapLoaderLoadSegs.BadSegKind kind) =>
        kind is VmMapLoaderLoadSegs.BadSegKind.Vertex
            or VmMapLoaderLoadSegs.BadSegKind.Linedef
            or VmMapLoaderLoadSegs.BadSegKind.Sidedef
            or VmMapLoaderLoadSegs.BadSegKind.SideIndex;

    public static int[] BuildVertexChangeTable(int vertexCount)
    {
        var table = new int[vertexCount];
        for (var i = 0; i < table.Length; i++)
            table[i] = 0;
        return table;
    }

    public static void MarkLineEndpoints(int[] table, int v1, int v2)
    {
        if (v1 >= 0 && v1 < table.Length)
            table[v1] = 1;
        if (v2 >= 0 && v2 < table.Length)
            table[v2] = 1;
    }

    public static bool IsVertexEligibleForSnap(int[] table, int index) =>
        index >= 0 && index < table.Length && table[index] == 0;

    public static double NormalizeAngle(double radians) =>
        radians % (Math.PI * 2);

    public static bool SegAngleMismatch(double a, double b, double toleranceDegrees) =>
        AngleDeltaDegrees(a, b) >= toleranceDegrees;

    public static uint ReadUInt16SegIndex(ushort value) => value;

    public static uint ReadUInt32SegIndex(uint value) => value;

    public static bool WideSubsectorFields(int formatVersion) => formatVersion >= 2;

    public static int SubsectorNumSegsSize(int formatVersion) => WideSubsectorFields(formatVersion) ? 4 : 2;

    public static bool ShouldClearLevelNodesOnFailure => true;

    public static bool ShouldClearLevelSubsectorsOnFailure => true;

    public static bool ShouldClearLevelSegsOnFailure => true;

    public static string MapHasNoSegsMessage => "This map has no segs.";

    public static bool ValidateIterator(int i, int count) => i >= 0 && i < count;

    public static int ClampSide(int side) => side & 1;

    public static bool LineHasTwoSides(ushort flags, ushort twoSided) =>
        (flags & twoSided) != 0;

    public static ushort StripTwoSided(ushort flags, ushort twoSided) =>
        (ushort)(flags & ~twoSided);

    public static double FixedToFloat(int value, int fracBits = 16) =>
        value / (double)(1 << fracBits);

    public static int FloatToFixed(double value, int fracBits = 16) =>
        (int)Math.Round(value * (1 << fracBits));

    public static bool SubsectorFirstLineInRange(uint first, uint count, uint max) =>
        first < max && first + count <= max;

    public static string SegSideError(int seg, int side) =>
        $"Sidedef reference in seg {seg} is {side} (must be 0 or 1).";

    public static bool ShouldLogBadSeg(VmMapLoaderLoadSegs.BadSegKind kind) => true;

    public static int BadSegLogPriority(VmMapLoaderLoadSegs.BadSegKind kind) =>
        (int)kind;
}
