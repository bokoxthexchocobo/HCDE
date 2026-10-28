namespace HCDE.Client;

/// <summary>Automap constants and CVAR side effects from <c>am_map.cpp</c> (66–183).</summary>
public static class VmAutomapCvarModel
{
    public const int MarkPointCount = 10;
    public const double PlayerRadius = 16;
    public const double ZoomInPerSecond = 2;
    public const double ZoomOutPerSecond = 0.2;

    public static int ClampEmptySpaceMargin(int value)
    {
        if (value < 0) return 0;
        if (value > 90) return 90;
        return value;
    }

    public static bool ShouldBlockAutomapCheat(bool netGame, bool svCheats, int requestedCheat) =>
        netGame && !svCheats && requestedCheat != 0;
}
