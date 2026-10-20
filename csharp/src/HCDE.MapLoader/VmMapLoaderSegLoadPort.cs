namespace HCDE.MapLoader;

/// <summary>
/// Consolidated <c>LoadSegs</c> / <c>LoadSubsectors</c> port surface from <c>maploader.cpp</c> (802–1047).
/// </summary>
public static class VmMapLoaderSegLoadPort
{
    public static bool EntryShouldAbort(int numSegs) => VmMapLoaderLoadSegs.ShouldAbortEmptySegs(numSegs);

    public static void EntryValidateVertex(uint vnum, uint numVertexes, int segIndex) =>
        VmMapLoaderLoadSegs.ValidateVertexIndex(vnum, numVertexes, segIndex);

    public static int EntryCountSegs(int lumpBytes, int recordSize) =>
        VmMapLoaderLoadSegs.CountSegs(lumpBytes, recordSize);

    public static bool EntryIncompleteBsp(int subs, int maxSeg) =>
        VmMapLoaderLoadSegsPolicy.IncompleteBspTree(subs, maxSeg);

    public static bool EntryRejectZeroSubsectors(int count) =>
        VmMapLoaderLoadSegs.ShouldRejectZeroSubsectors(count);

    public static bool EntrySnapVertex(double delta) =>
        VmMapLoaderLoadSegs.ShouldSnapSplitVertex(delta);

    public static void EntryValidateLinedef(uint linedef, uint lineCount, int segIndex) =>
        VmMapLoaderLoadSegs.ValidateLinedefIndex(linedef, lineCount, segIndex);

    public static void EntryValidateSide(int side, int segIndex) =>
        VmMapLoaderLoadSegs.ValidateSideIndex(side, segIndex);

    public static bool EntrySubsectorRangeInvalid(uint first, uint count, uint max) =>
        VmMapLoaderLoadSegs.IsSubsectorSegRangeInvalid(first, count, max);

    public static string EntryBadSegMessage(VmMapLoaderLoadSegs.BadSegKind kind, int seg, int data, int max) =>
        VmMapLoaderLoadSegs.FormatBadSegMessage(kind, seg, data, max);

    public static string EntryDescribeFailure(VmMapLoaderLoadSegs.BadSegKind kind, int seg, int data, int max) =>
        VmMapLoaderLoadSegsPolicy.DescribeBspFailure(kind, seg, data, max);

    public static ushort EntryNormalizeTwoSided(ushort flags, bool hasBack) =>
        VmMapLoaderLoadSegs.NormalizeTwoSidedFlag(flags, hasBack);

    public static bool EntryClearBacksector(ushort flags, bool hasBack) =>
        VmMapLoaderLoadSegs.ShouldClearBacksector(flags, hasBack);

    public static int EntryPickVertex(uint v1, uint v2, byte c1, byte c2) =>
        VmMapLoaderLoadSegs.PickVertexToSnap(v1, v2, c1, c2);

    public static bool EntryTrySnap(
        double ptp,
        double seg,
        uint v1,
        uint v2,
        byte c1,
        byte c2,
        out int vertex) =>
        VmMapLoaderLoadSegsPolicy.TryPlanVertexSnap(ptp, seg, v1, v2, c1, c2, out vertex);

    public static (double x, double y) EntryApplySnap(
        double ax,
        double ay,
        double bx,
        double by,
        int vertex,
        double angle,
        double len) =>
        VmMapLoaderLoadSegsPolicy.ApplyVertexSnap(ax, ay, bx, by, vertex, angle, len);

    public static void EntryValidateSegVertices(uint v1, uint v2, VmMapLoaderLoadSegsPolicy.SegLoadContext ctx, int seg) =>
        VmMapLoaderLoadSegsPolicy.ValidateSegVertexPair(v1, v2, ctx, seg);

    public static void EntryValidateSegLinedef(uint line, VmMapLoaderLoadSegsPolicy.SegLoadContext ctx, int seg) =>
        VmMapLoaderLoadSegsPolicy.ValidateSegLinedef(line, ctx, seg);

    public static void EntryValidateSegSidedef(uint sideDef, VmMapLoaderLoadSegsPolicy.SegLoadContext ctx, int seg) =>
        VmMapLoaderLoadSegsPolicy.ValidateSegSidedef(sideDef, ctx, seg);

    public static bool EntryRequiresRebuild(VmMapLoaderLoadSegs.BadSegKind kind) =>
        VmMapLoaderLoadSegsPolicy.RequiresBspRebuildAfterCatch(kind);

    public static string EntryEmptySubsector(int index) =>
        VmMapLoaderLoadSegsPolicy.EmptySubsectorMessage(index);

    public static string EntryIncompleteBspMessage =>
        VmMapLoaderLoadSegsPolicy.IncompleteBspMessage;

    public static string EntryNoSegsMessage =>
        VmMapLoaderLoadSegsPolicy.MapHasNoSegsMessage;

    public static bool EntryShouldClearNodes => VmMapLoaderLoadSegsPolicy.ShouldClearLevelNodesOnFailure;

    public static bool EntryShouldClearSubsectors => VmMapLoaderLoadSegsPolicy.ShouldClearLevelSubsectorsOnFailure;

    public static bool EntryShouldClearSegs => VmMapLoaderLoadSegsPolicy.ShouldClearLevelSegsOnFailure;

    public static int EntryBuildVertexTable(int count) =>
        VmMapLoaderLoadSegsPolicy.BuildVertexChangeTable(count).Length;

    public static void EntryMarkLine(int[] table, int v1, int v2) =>
        VmMapLoaderLoadSegsPolicy.MarkLineEndpoints(table, v1, v2);

    public static bool EntryAngleMismatch(double a, double b) =>
        VmMapLoaderLoadSegsPolicy.SegAngleMismatch(a, b, 1.0);

    public static bool EntryWideSubsector(int version) =>
        VmMapLoaderLoadSegsPolicy.WideSubsectorFields(version);

    public static int EntrySubsectorNumSegsSize(int version) =>
        VmMapLoaderLoadSegsPolicy.SubsectorNumSegsSize(version);

    public static bool EntrySubsectorFirstInRange(uint first, uint count, uint max) =>
        VmMapLoaderLoadSegsPolicy.SubsectorFirstLineInRange(first, count, max);

    public static string EntryFormatSubsectorOverflow(int i, uint first, uint num, uint max) =>
        VmMapLoaderLoadSegsPolicy.FormatSubsectorOverflow(i, first, num, max);

    public static uint EntryComputeMaxSeg(int bytes, int recordSize) =>
        VmMapLoaderLoadSegsPolicy.ComputeMaxSegFromLump(bytes, recordSize);

    public static bool EntryAbortSubsector(uint num, uint first, uint max) =>
        VmMapLoaderLoadSegsPolicy.ShouldAbortSubsectorLoad(num, first, max);

    public static double EntrySegLength(double dx, double dy) =>
        VmMapLoaderLoadSegsPolicy.SegLength(dx, dy);

    public static bool EntryOppositeSide(bool[] present, int side) =>
        VmMapLoaderLoadSegsPolicy.OppositeSideExists(present, side);

    public static bool EntryAssignBacksector(ushort flags, bool opposite) =>
        VmMapLoaderLoadSegsPolicy.ShouldAssignBacksector(flags, opposite);

    public static int EntryClampSide(int side) => VmMapLoaderLoadSegsPolicy.ClampSide(side);

    public static bool EntryLineTwoSided(ushort flags, ushort bit) =>
        VmMapLoaderLoadSegsPolicy.LineHasTwoSides(flags, bit);

    public static ushort EntryStripTwoSided(ushort flags, ushort bit) =>
        VmMapLoaderLoadSegsPolicy.StripTwoSided(flags, bit);

    public static double EntryFixedToFloat(int v) => VmMapLoaderLoadSegsPolicy.FixedToFloat(v);

    public static int EntryFloatToFixed(double v) => VmMapLoaderLoadSegsPolicy.FloatToFixed(v);

    public static bool EntryValidateIterator(int i, int count) =>
        VmMapLoaderLoadSegsPolicy.ValidateIterator(i, count);

    public static int EntryBadSegPriority(VmMapLoaderLoadSegs.BadSegKind kind) =>
        VmMapLoaderLoadSegsPolicy.BadSegLogPriority(kind);

    public static bool EntryShouldLogBadSeg(VmMapLoaderLoadSegs.BadSegKind kind) =>
        VmMapLoaderLoadSegsPolicy.ShouldLogBadSeg(kind);

    public static string EntrySegSideError(int seg, int side) =>
        VmMapLoaderLoadSegsPolicy.SegSideError(seg, side);

    public static bool EntryContinueSubsector(uint i, uint count, uint first, uint num, uint max) =>
        VmMapLoaderLoadSegsPolicy.ShouldContinueSubsectorLoop(i, count, first, num, max);

    public static int EntryCountSegments(int bytes, int recordSize) =>
        VmMapLoaderLoadSegsPolicy.CountSegmentsFromLump(bytes, recordSize);

    public static bool EntryPolyobjPortal(bool wall) =>
        VmMapLoaderLoadSegsPolicy.LineUsesPolyobjPortalSample(wall);

    public static int EntryPortalGroup(int sampled, bool poly, int front) =>
        VmMapLoaderLoadSegsPolicy.PortalGroupFromSample(sampled, poly, front);

    public static double EntryPolyobjSampleX(double v1, double dx) =>
        VmMapLoaderLoadSegs.ComputePolyobjPortalSampleX(v1, dx);

    public static bool EntryRebuildOnBadSeg => VmMapLoaderLoadSegs.ShouldRebuildBspOnBadSeg;

    public static bool EntryRejectEmptySubsector(uint num) =>
        VmMapLoaderLoadSegs.ShouldRejectEmptySubsector(num);

    public static string EntryInvalidSubsectorRange(int i, uint first, uint count, uint max) =>
        VmMapLoaderLoadSegs.FormatInvalidSubsectorSegRange(i, first, count, max);

    public static string EntryBspBanner => VmMapLoaderLoadSegs.BspRebuildBanner;
}
