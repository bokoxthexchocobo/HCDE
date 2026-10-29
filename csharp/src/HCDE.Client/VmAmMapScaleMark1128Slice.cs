namespace HCDE.Client;

/// <summary>
/// Automap window center, scale activate/save/restore, and <c>addMark</c> from
/// <c>am_map.cpp</c> (<b>1051–1200</b>). Chunk <b>1128</b>.
/// </summary>
public static class VmAmMapScaleMark1128Slice
{
    public const int MarkPointCount = 10;
    public const string DefaultMarkFont = "AMMNUMx";

    /// <summary><c>FLT_MAX</c> seed for <c>min_x</c>/<c>min_y</c> at lines 1195–1196.</summary>
    public const double BoundMinSeed = float.MaxValue;

    /// <summary>Signed <c>FIXED_MIN</c> (<c>0x80000000</c>) seed for <c>max_x</c>/<c>max_y</c>.</summary>
    public const double BoundMaxSeed = int.MinValue;

    public sealed class MapWindow
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; }
        public double H { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public double OldW { get; set; }
        public double OldH { get; set; }
        public double ScaleMapToFrame { get; set; }
        public double ScaleFrameToMap { get; set; }
        public int FrameW { get; set; }
        public int FrameH { get; set; }
        public int MarkIndex { get; set; }
        public (double X, double Y)[] Marks { get; } = new (double X, double Y)[MarkPointCount];
    }

    public static (double X, double Y) GetPosition(MapWindow window)
    {
        return (window.X + window.W / 2, window.Y + window.H / 2);
    }

    public static void ActivateNewScale(MapWindow window)
    {
        window.X += window.W / 2;
        window.Y += window.H / 2;
        window.W = window.FrameW * window.ScaleFrameToMap;
        window.H = window.FrameH * window.ScaleFrameToMap;
        window.X -= window.W / 2;
        window.Y -= window.H / 2;
        window.X2 = window.X + window.W;
        window.Y2 = window.Y + window.H;
    }

    public static void SaveScaleAndLoc(MapWindow window)
    {
        window.OldX = window.X;
        window.OldY = window.Y;
        window.OldW = window.W;
        window.OldH = window.H;
    }

    public static void RestoreScaleAndLoc(MapWindow window, bool followPlayer, double cameraX, double cameraY)
    {
        window.W = window.OldW;
        window.H = window.OldH;
        if (!followPlayer)
        {
            window.X = window.OldX;
            window.Y = window.OldY;
        }
        else
        {
            window.X = cameraX - window.W / 2;
            window.Y = cameraY - window.H / 2;
        }

        window.X2 = window.X + window.W;
        window.Y2 = window.Y + window.H;
        window.ScaleMapToFrame = window.FrameW / window.W;
        window.ScaleFrameToMap = 1 / window.ScaleMapToFrame;
    }

    public static int TryAddMark(MapWindow window, string? markFont, bool firstMarkTextureValid)
    {
        var customFont = !string.Equals(markFont, DefaultMarkFont, StringComparison.OrdinalIgnoreCase);
        if (customFont || firstMarkTextureValid)
        {
            var slot = window.MarkIndex;
            window.Marks[slot] = (window.X + window.W / 2, window.Y + window.H / 2);
            window.MarkIndex = (slot + 1) % MarkPointCount;
            return slot;
        }

        return -1;
    }

    public static (double MinX, double MinY, double MaxX, double MaxY) SeedVertexBounds()
    {
        return (BoundMinSeed, BoundMinSeed, BoundMaxSeed, BoundMaxSeed);
    }

    public static bool VertexXIsNewMinimum(double vertexX, double minX)
    {
        return vertexX < minX;
    }
}
