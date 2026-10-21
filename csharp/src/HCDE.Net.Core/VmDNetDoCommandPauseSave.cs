namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> pause and savegame cases (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandPauseSave
{
    public static bool ShouldTogglePauseOnLevelNative(int gameState, int levelState) => gameState == levelState;

    public static bool PauseWasActiveNative(int paused) => paused != 0;

    public static bool ShouldReadSavegamePathsNative(int gameState, int levelState) => gameState == levelState;

    public static bool ShouldRewriteSavePathForRemotePlayerNative(int player, int consolePlayer) =>
        player != consolePlayer;

}
