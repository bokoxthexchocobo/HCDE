namespace HCDE.MapLoader;

/// <summary><c>MapLoader::CreateBlockMap</c> layout policy from <c>maploader.cpp</c> (2324–2496).</summary>
public static class VmMapLoaderCreateBlockMap
{
    public const int BlockBits = 7;
    public const int BlockSize = 128;

    public static bool ShouldAbortWithNoVertices(int vertexCount) => vertexCount == 0;

    public static int ComputeBlockMapWidth(int maxX, int minX) => ((maxX - minX) >> BlockBits) + 1;

    public static int ComputeBlockMapHeight(int maxY, int minY) => ((maxY - minY) >> BlockBits) + 1;

    public static int ComputeBlockIndex(int bx, int by, int bmapWidth) => bx + by * bmapWidth;

    public static (int bx, int by) ComputeBlockCoords(int x, int y, int minX, int minY) =>
        ((x - minX) >> BlockBits, (y - minY) >> BlockBits);

    public static bool IsSingleBlock(int bx, int by, int bx2, int by2) => bx == bx2 && by == by2;

    public static bool IsHorizontalSpan(int by, int by2) => by == by2;

    public static bool IsVerticalSpan(int bx, int bx2) => bx == bx2;

    public static int ResolveXChange(int dx) => dx < 0 ? -1 : 1;

    public static int ResolveYChange(int dy) => dy < 0 ? -1 : 1;

    public static int ComputeYMove(int yChange, int bmapWidth) => yChange * bmapWidth;

    public static bool IsFortyFiveDegree(int adx, int ady) => adx == ady;

    public static bool IsXMajor(int adx, int ady) => adx >= ady;

    public static int HeaderWordCount => 4;
}
