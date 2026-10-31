namespace HCDE.Client;

/// <summary>
/// <c>drawMline</c> thickness and antialias strokes, grid spacing, and the
/// <c>AM_FakeFlat</c> no-height-sector return from <c>am_map.cpp</c> (<b>1801–1950</b>).
/// Chunk <b>1133</b>.
/// </summary>
public static class VmAmMapDrawLine1133Slice
{
    public const int MapBlockUnits = 128;

    public static (int X, int Y) ScreenEnd(int frameOriginX, int frameOriginY, int clippedX, int clippedY)
    {
        return (frameOriginX + clippedX, frameOriginY + clippedY);
    }

    public static int DrawThickness(int requested, int scaledFallback)
    {
        if (requested > 0)
            return requested;
        return scaledFallback;
    }

    public static int AntialiasDivisor(int thickness)
    {
        if (thickness >= 3)
            return 3;
        return 2;
    }

    public static int AntialiasThickness(int thickness)
    {
        var reduced = thickness - 2;
        if (reduced < 1)
            return 1;
        return reduced;
    }

    public static int AlphaByte(double lineAlpha, int divisor)
    {
        return (int)(lineAlpha * 255 / divisor);
    }

    public static List<(int Dx, int Dy)> AntialiasOffsets(int thickness)
    {
        var offsets = new List<(int Dx, int Dy)>();
        if (AntialiasThickness(thickness) >= 2)
        {
            offsets.Add((-1, -1));
            offsets.Add((1, -1));
            offsets.Add((0, -1));
            offsets.Add((-1, 0));
            offsets.Add((1, 0));
            offsets.Add((0, 0));
            offsets.Add((-1, 1));
            offsets.Add((1, 1));
            offsets.Add((0, -1));
            return offsets;
        }

        if (thickness >= 3)
        {
            offsets.Add((-1, -1));
            offsets.Add((1, -1));
        }

        offsets.Add((0, -1));
        offsets.Add((-1, 0));
        offsets.Add((1, 0));
        offsets.Add((0, 0));
        if (thickness >= 3)
        {
            offsets.Add((-1, 1));
            offsets.Add((1, 1));
        }

        offsets.Add((0, -1));
        return offsets;
    }

    public static bool UseThickLine(bool antialias, int thickness)
    {
        if (antialias)
            return AntialiasThickness(thickness) >= 2;
        return thickness >= 2;
    }

    public static double CoverLength(double windowW, double windowH)
    {
        return Math.Sqrt(windowW * windowW + windowH * windowH);
    }

    public static double GridExtension(double coverLength, double span)
    {
        return (coverLength - span) / 2;
    }

    public static double GridStart(double edge, double extension, double blockOrigin)
    {
        var start = edge - extension;
        return Math.Ceiling((start - blockOrigin) / MapBlockUnits) * MapBlockUnits + blockOrigin;
    }

    public static double GridEnd(double edge, double coverLength, double extension)
    {
        return edge + coverLength - extension;
    }

    public static bool ShouldRotateGrid(int rotateMode, bool viewActive)
    {
        if (rotateMode == 1)
            return true;
        if (rotateMode == 2 && viewActive)
            return true;
        return false;
    }

    public static List<(double Ax, double Ay, double Bx, double By)> VerticalGridLines(
        double minX, double minY, double windowW, double windowH, double blockOriginX)
    {
        var cover = CoverLength(windowW, windowH);
        var extX = GridExtension(cover, windowW);
        var extY = GridExtension(cover, windowH);
        var start = GridStart(minX, extX, blockOriginX);
        var end = GridEnd(minX, cover, extX);
        var lines = new List<(double Ax, double Ay, double Bx, double By)>();
        for (var x = start; x < end; x += MapBlockUnits)
            lines.Add((x, minY - extY, x, minY - extY + cover));
        return lines;
    }

    public static uint PaletteColor(int index, IReadOnlyList<uint> colors)
    {
        return colors[index];
    }

    public static bool KeepOriginalSector(bool hasHeightSector)
    {
        if (hasHeightSector)
            return false;
        return true;
    }
}
