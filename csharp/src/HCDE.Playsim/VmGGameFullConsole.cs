namespace HCDE.Playsim;

/// <summary><c>G_FullConsole</c> from <c>g_game.cpp</c> (1392–1412).</summary>
public static class VmGGameFullConsole
{
    public const int GsStartup = 0;

    public static bool ShouldToggleHudBeforeQuit(bool hudToggled) => hudToggled;

    public static bool ShouldCheckDemoOnQuit(bool demoplayback) => demoplayback;

    public static bool ShouldStopMusicAfterQuit(int oldGameState) => oldGameState != GsStartup;

    public static bool ShouldFreeLevelDataAfterQuit(int oldGameState) => oldGameState != GsStartup;
}
