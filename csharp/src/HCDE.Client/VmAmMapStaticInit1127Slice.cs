namespace HCDE.Client;

/// <summary>
/// Rest of <c>AM_StaticInit</c> plus automap scale helpers <c>FTOM</c>/<c>MTOF</c>/<c>CXMTOF</c>/<c>CYMTOF</c>
/// from <c>am_map.cpp</c> (<b>901–1050</b>). Chunk <b>1127</b>.
/// </summary>
public static class VmAmMapStaticInit1127Slice
{
    public const int MarkPointCount = 10;
    public const int TicRate = 35;
    public const int PanPixelsPerSecond = 140;
    public const double DefaultScaleMapToFrame = 0.2;

    public sealed class WindowDefaults
    {
        public double ScaleMapToFrame { get; init; } = DefaultScaleMapToFrame;
        public int MarkPointNumber { get; init; }
        public double MapYStart { get; init; }
        public double MapXStart { get; init; }
        public int PanIncrement { get; init; }
    }

    public static WindowDefaults CreateWindowDefaults()
    {
        return new WindowDefaults
        {
            ScaleMapToFrame = DefaultScaleMapToFrame,
            MarkPointNumber = 0,
            MapYStart = 0,
            MapXStart = 0,
            PanIncrement = PanIncrementPerTic()
        };
    }

    public static int PanIncrementPerTic()
    {
        return PanPixelsPerSecond / TicRate;
    }

    public static IReadOnlyList<string> ArrowLumpsToLoad(string? mapArrow, string? cheatMapArrow, string cheatKey, string easyKey)
    {
        var lumps = new List<string>();
        if (!string.IsNullOrEmpty(mapArrow))
            lumps.Add(mapArrow);
        if (!string.IsNullOrEmpty(cheatMapArrow))
            lumps.Add(cheatMapArrow);
        lumps.Add(cheatKey);
        lumps.Add(easyKey);
        return lumps;
    }

    public static bool MissingMapArrowIsFatal(int parsedMapArrowLines)
    {
        if (parsedMapArrowLines == 0)
            return true;
        return false;
    }

    public static string[] MarkTextureNames()
    {
        var names = new string[MarkPointCount];
        for (var i = 0; i < MarkPointCount; i++)
            names[i] = $"AMMNUM{i}";
        return names;
    }

    public static double FrameToMap(double frameDistance, double scaleFrameToMap)
    {
        return frameDistance * scaleFrameToMap;
    }

    public static double MapToFrame(double mapDistance, double scaleMapToFrame)
    {
        return mapDistance * scaleMapToFrame;
    }

    public static int MapXToFrame(double mapX, double windowMapX, double scaleMapToFrame)
    {
        return (int)MapToFrame(mapX - windowMapX, scaleMapToFrame);
    }

    public static int MapYToFrame(double mapY, double windowMapY, double scaleMapToFrame, int frameHeight)
    {
        return (int)(frameHeight - MapToFrame(mapY - windowMapY, scaleMapToFrame));
    }
}
