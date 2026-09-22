namespace HCDE.MapLoader;

/// <summary>
/// Reads linedef indexes out of a decoded Doom BLOCKMAP. Blocks are 128 map units.
/// An offset in the lump is a short index from the start of the lump; the cell array begins after the 4-short header.
/// </summary>
public static class BlockmapLines
{
    public const int BlockSize = 128;

    public static IEnumerable<int> Near(MapBlockmapRecord map, double x, double y, double radius)
    {
        var header = map.Header;
        var minX = BlockOf(x - radius, header.OriginX);
        var maxX = BlockOf(x + radius, header.OriginX);
        var minY = BlockOf(y - radius, header.OriginY);
        var maxY = BlockOf(y + radius, header.OriginY);
        var seen = new HashSet<int>();
        for (var blockY = minY; blockY <= maxY; blockY++)
        {
            for (var blockX = minX; blockX <= maxX; blockX++)
            {
                foreach (var line in LinesInBlock(map, blockX, blockY))
                {
                    if (seen.Add(line))
                        yield return line;
                }
            }
        }
    }

    public static IEnumerable<int> LinesInBlock(MapBlockmapRecord map, int blockX, int blockY)
    {
        var header = map.Header;
        if ((uint)blockX >= header.Width || (uint)blockY >= header.Height)
            yield break;

        var slot = blockY * header.Width + blockX;
        if (slot >= map.Cells.Length)
            yield break;

        var cursor = map.Cells[slot] - 4;
        if ((uint)cursor >= (uint)map.Cells.Length)
            yield break;

        if (map.Cells[cursor] == 0)
            cursor++;

        for (; cursor < map.Cells.Length; cursor++)
        {
            var value = map.Cells[cursor];
            if (value == MapBlockmapCodec.Terminator)
                yield break;
            yield return value;
        }
    }

    public static int BlockOf(double coordinate, int origin) =>
        (int)Math.Floor((coordinate - origin) / BlockSize);
}
