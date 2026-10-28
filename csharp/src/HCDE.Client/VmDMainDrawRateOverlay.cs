namespace HCDE.Client;

/// <summary><c>DrawRateStuff</c> overlay policy from <c>d_main.cpp</c> (1068–1111).</summary>
public static class VmDMainDrawRateOverlay
{
    public static bool ShouldDrawFpsOverlay(bool vidFps) => vidFps;

    public static int ComputeFpsTextRight(int screenWidth, int textScale, int stringWidth) =>
        screenWidth / textScale - stringWidth;

    public static int ComputeFpsClearHeight(int fontHeight, int textScale) => fontHeight * textScale;

    public static bool ShouldDrawTicDots(bool tickerEnabled) => tickerEnabled;

    public static long ClampTicDelta(long tics) => tics > 20 ? 20 : tics;

    public static int ComputeActiveTicDots(long tics) => (int)(tics * 2);

    public static IEnumerable<int> EnumerateTicDotColumns(int activeDots)
    {
        for (var i = 0; i < activeDots; i += 2)
            yield return i;
    }

    public static IEnumerable<int> EnumerateClearedTicDotColumns(int activeDots, int maxDots)
    {
        for (var i = activeDots; i < maxDots; i += 2)
            yield return i;
    }

    public static bool ShouldDrawPaletteDebug(int vidShowPalette) => vidShowPalette != 0;
}
