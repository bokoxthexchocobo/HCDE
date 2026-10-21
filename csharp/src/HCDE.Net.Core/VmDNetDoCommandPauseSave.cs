namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> pause and savegame cases (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandPauseSave
{
    public static bool ShouldTogglePauseOnLevelNative(int gameState, int levelState) => gameState == levelState;

    public static bool PauseWasActiveNative(int paused) => paused != 0;

    public static bool ShouldReadSavegamePathsNative(int gameState, int levelState) => gameState == levelState;

    public static bool ShouldRewriteSavePathForRemotePlayerNative(int player, int consolePlayer) =>
        player != consolePlayer;

    public static int SetPausedForPlayerNative(int player) => player + 1;

    public static int ResumePausedStateNative(int oldPaused) => oldPaused != 0 ? 0 : oldPaused;

    public static bool ShouldEmitPauseDebugTraceNative(int netEventDebugLevel) => netEventDebugLevel >= 2;

    public static bool ShouldTraceAutosaveRequestNative(int gameState, int levelState) => gameState == levelState;
}
