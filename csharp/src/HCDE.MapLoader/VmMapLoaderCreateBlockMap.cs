namespace HCDE.MapLoader;

/// <summary><c>MapLoader::CreateBlockMap</c> layout policy from <c>maploader.cpp</c> (2324–2496).</summary>
public static class VmMapLoaderCreateBlockMap
{
    public const int BlockBits = 7;
    public const int BlockSize = 128;

    public static bool ShouldAbortWithNoVertices(int vertexCount) => vertexCount == 0;

    public static int ComputeBlockMapWidth(int maxX, int minX) => ((maxX - minX) >> BlockBits) + 1;

    public static int ComputeBlockMapHeight(int maxY, int minY) => ((maxY - minY) >> BlockBits) + 1;

    public static int ComputeBlockListReserve(int width, int height) => width * height * 3 + 4;

    public static int ComputeBlockIndex(int bx, int by, int bmapWidth) => bx + by * bmapWidth;

    public static (int bx, int by) ComputeBlockCoords(int x, int y, int minX, int minY) =>
        ((x - minX) >> BlockBits, (y - minY) >> BlockBits);

    public static bool IsSingleBlock(int bx, int by, int bx2, int by2) => bx == bx2 && by == by2;

    public static bool IsHorizontalSpan(int by, int by2) => by == by2;

    public static bool IsVerticalSpan(int bx, int bx2) => bx == bx2;

    public static bool ShouldSwapBlocks(ref int bx, ref int by, ref int bx2, ref int by2, bool swap)
    {
        if (!swap)
            return false;
        (bx, bx2) = (bx2, bx);
        (by, by2) = (by2, by);
        return true;
    }

    public static int ResolveXChange(int dx) => dx < 0 ? -1 : 1;

    public static int ResolveYChange(int dy) => dy < 0 ? -1 : 1;

    public static int ComputeYMove(int yChange, int bmapWidth) => yChange * bmapWidth;

    public static int AbsDelta(int delta) => Math.Abs(delta);

    public static bool IsFortyFiveDegree(int adx, int ady) => adx == ady;

    public static bool ShouldTrimAdxOn45(int xb, int yb, int dx, int dy, int blockSize)
    {
        if (dx < 0)
            xb = blockSize - xb;
        if (dy < 0)
            yb = blockSize - yb;
        return xb < yb;
    }

    public static bool IsXMajor(int adx, int ady) => adx >= ady;

    public static int ResolveHorizontalStop(int by, int y1, int minY, int yAdd, int dy, int dx, int x1, int minX) =>
        (ScaleYToBlock(by, yAdd, y1, minY, dy, dx, x1, minX));

    public static int ScaleYToBlock(int by, int yAdd, int y1, int minY, int dy, int dx, int x1, int minX)
    {
        var numerator = (by << BlockBits) + yAdd - (y1 - minY);
        return dx == 0 ? by : numerator * dx / dy + (x1 - minX) >> BlockBits;
    }

    public static int HeaderWordCount => 4;

    public static IEnumerable<int> EnumerateHeaderWords(int minX, int minY, int width, int height)
    {
        yield return minX;
        yield return minY;
        yield return width;
        yield return height;
    }
}
