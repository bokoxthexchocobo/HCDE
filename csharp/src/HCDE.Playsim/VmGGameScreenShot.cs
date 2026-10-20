namespace HCDE.Playsim;

/// <summary><c>G_ScreenShot</c> from <c>g_game.cpp</c> (2501–2509).</summary>
public static class VmGGameScreenShot
{
    public const int GaNothing = 0;

    public static bool ShouldQueueScreenshot(int gameAction, string? filename) =>
        gameAction == GaNothing && !string.IsNullOrEmpty(filename);
}
