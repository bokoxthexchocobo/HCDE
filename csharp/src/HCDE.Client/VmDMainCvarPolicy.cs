namespace HCDE.Client;

/// <summary>CVAR side effects from <c>d_main.cpp</c> (422–508, 594–620).</summary>
public static class VmDMainCvarPolicy
{
    public const float MinTimeScale = 0.05f;
    public const int AvgTimeLimitMinutes = 20;

    public static float ClampTimeScaleForNetGame(bool netGame, float requested) =>
        netGame ? 1.0f : requested;

    public static bool IsTimeScaleAllowed(float value) => value >= MinTimeScale;

    public static float ResolveTimeScaleApply(bool netGame, float requested, out bool rejected)
    {
        if (netGame)
        {
            rejected = requested != 1.0f;
            return 1.0f;
        }

        if (!IsTimeScaleAllowed(requested))
        {
            rejected = true;
            return requested;
        }

        rejected = false;
        return requested;
    }

    public static int ClampVidRenderMode(int value)
    {
        if (value < 0 || value > 4)
            return 4;
        if (value is 2 or 3)
            return value - 2;
        return value;
    }

    public static int ClampWipeType(int value, int wipeNone, int wipeCount, int defaultWipe) =>
        value < wipeNone || value >= wipeCount ? defaultWipe : value;

    public static (int screenblocks, bool drawSprite, bool showMessages) CaptureHudState(
        int screenblocks, bool drawSprite, bool showMessages) =>
        (screenblocks, drawSprite, showMessages);
}
