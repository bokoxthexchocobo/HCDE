namespace HCDE.Client;

/// <summary>Native <c>palette.cpp</c> <c>BestColor</c>.</summary>
public static class VmPaletteBestColor
{
    public static int BestColor(ReadOnlySpan<uint> palette, int r, int g, int b, int first, int num, ReadOnlySpan<byte> indexMap = default)
    {
        var bestColor = first;
        var bestDist = 257L * 257 * 3;
        for (var color = first; color < num; color++)
        {
            var co = indexMap.Length > 0 ? indexMap[color] : color;
            var entry = palette[co];
            var pr = (entry >> 0) & 0xFF;
            var pg = (entry >> 8) & 0xFF;
            var pb = (entry >> 16) & 0xFF;
            var x = r - pr;
            var y = g - pg;
            var z = b - pb;
            var dist = x * x + y * y + z * z;
            if (dist < bestDist)
            {
                if (dist == 0) return co;
                bestDist = dist;
                bestColor = co;
            }
        }

        return bestColor;
    }
}
