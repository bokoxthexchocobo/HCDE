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
}
