namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadBlockMap</c> and <c>FBlockmap::VerifyBlockMap</c> policy from <c>maploader.cpp</c>.</summary>
public static class VmMapLoaderLoadBlockMap
{
    public const int BlockmapEntryLimit = 0x10000;
    public const int BlockmapTerminator = -1;

    public static bool ShouldGenerateBlockMap(
        bool forceNodeBuild,
        bool genBlockMap,
        int lumpHalfWords,
        bool blockMapArg,
        int lineCount) =>
        forceNodeBuild
        || genBlockMap
        || lumpHalfWords / 2 >= BlockmapEntryLimit
        || lumpHalfWords == 0
        || blockMapArg;

    public static int ExpandWadOffset(short wadValue) =>
        wadValue == -1 ? unchecked((int)0xffffffff) : wadValue & 0xffff;

    public static bool ShouldRegenerateAfterVerify(bool verifyOk) => !verifyOk;

    public static int ComputeBlockLinkCount(int bmapWidth, int bmapHeight) => bmapWidth * bmapHeight;

    public static bool IsBlockOffsetOutOfRange(int blockOffsetIndex, int lumpCount) =>
        blockOffsetIndex >= lumpCount;

    public static bool IsListOffsetOutOfRange(int offset, int count) =>
        offset < 4 || offset >= count;

    public static bool IsOpenBlockList(int listIndex, int maxIndex) => listIndex >= maxIndex;

    public static bool RequiresLeadingZeroEntry(int firstEntry) => firstEntry != 0;

    public static bool IsLineIndexOutOfRange(int lineIndex, int numLines) =>
        (uint)lineIndex >= (uint)numLines;
}
