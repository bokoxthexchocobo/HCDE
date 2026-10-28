namespace HCDE.MapLoader;

/// <summary><c>MapLoader::CreateBlockMap</c> line-span walks from <c>maploader.cpp</c> (2398–2496).</summary>
public static class VmMapLoaderCreateBlockMapSpan
{
    public static void PushHorizontalSpan(List<int>[] blockLists, int bmapWidth, int bx, int by, int bx2, int line)
    {
        var blockIndex = VmMapLoaderCreateBlockMap.ComputeBlockIndex(bx, by, bmapWidth);
        var endIndex = VmMapLoaderCreateBlockMap.ComputeBlockIndex(bx2, by, bmapWidth);
        if (bx > bx2)
            (blockIndex, endIndex) = (endIndex, blockIndex);
        var block = blockIndex;
        while (block <= endIndex)
        {
            blockLists[block].Add(line);
            block += 1;
        }
    }

    public static void PushVerticalSpan(List<int>[] blockLists, int bmapWidth, int bx, int by, int by2, int line)
    {
        var blockIndex = VmMapLoaderCreateBlockMap.ComputeBlockIndex(bx, by, bmapWidth);
        var endIndex = VmMapLoaderCreateBlockMap.ComputeBlockIndex(bx, by2, bmapWidth);
        if (by > by2)
            (blockIndex, endIndex) = (endIndex, blockIndex);
        var block = blockIndex;
        while (block <= endIndex)
        {
            blockLists[block].Add(line);
            block += bmapWidth;
        }
    }

    public static int Compute45TrimmedAdx(int adx, int xb, int yb, int dx, int dy)
    {
        if (!VmMapLoaderCreateBlockMap.IsFortyFiveDegree(adx, VmMapLoaderCreateBlockMap.AbsDelta(dy)))
            return adx;
        return VmMapLoaderCreateBlockMap.ShouldTrimAdxOn45(
            xb, yb, dx, dy, VmMapLoaderCreateBlockMap.BlockSize)
            ? adx - 1
            : adx;
    }

    public static int ComputeBlockLocalX(int x1, int minX) => (x1 - minX) & (VmMapLoaderCreateBlockMap.BlockSize - 1);

    public static int ComputeBlockLocalY(int y1, int minY) => (y1 - minY) & (VmMapLoaderCreateBlockMap.BlockSize - 1);

    public static int ResolveYAddForXMajor(int dy) => dy < 0 ? -1 : VmMapLoaderCreateBlockMap.BlockSize;

    public static int ResolveXAddForYMajor(int dx) => dx < 0 ? -1 : VmMapLoaderCreateBlockMap.BlockSize;

    public static void CopyBlockMapLump(int[] blockMap, int[] destination)
    {
        for (var i = 0; i < blockMap.Length && i < destination.Length; i++)
            destination[i] = blockMap[i];
    }

    public static bool ShouldInvokePackedBuilder(int bmapWidth, int bmapHeight) =>
        bmapWidth > 0 && bmapHeight > 0;
}
