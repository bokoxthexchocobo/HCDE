namespace HCDE.Playsim;

/// <summary>
/// <c>P_RunClientSideLogic</c>, <c>P_CheckTickerPaused</c>, and <c>P_ClearLevelInterpolation</c> head from <c>p_tick.cpp</c> (**1–150**).
/// </summary>
public static class VmPTicker978Slice
{
    public const int GsLevel = 4;
    public const int GsTitleLevel = 9;
    public const int GsCutscene = 10;
    public const int GsIntro = 11;
    public const int MenuOff = 0;
    public const int MenuOnNoPause = 2;
    public const int ConsoleDown = 1;
    public const int ConsoleFalling = 2;
    public const int Level2PauseMusicInMenus = 1 << 0;

    public static bool DedicatedServerSkipsClientSideLogic(bool hcdeDedicated, bool interfaceDedicated) =>
        hcdeDedicated || interfaceDedicated;

    public static bool GameStateRunsLevelClientSideTick(int gameState) =>
        gameState == GsLevel || gameState == GsTitleLevel;

    public static bool ShouldDecrementPlayerInventoryTics(bool worldPaused, bool playerInGame, int inventoryTics) =>
        !worldPaused && playerInGame && inventoryTics > 0;

    public static int DecrementInventoryTics(int inventoryTics) => inventoryTics - 1;

    public static bool ShouldClearClientSideActorInterpolation(object? actor) => actor is not null;

    public static bool ShouldTickAutomapOnLevel(int gameState) => gameState == GsLevel;

    public static bool ShouldRunCutsceneTicker(int gameState) =>
        gameState == GsCutscene || gameState == GsIntro;

    public static bool MenuActivePausesTicker(int menuActive) =>
        menuActive != MenuOff && menuActive != MenuOnNoPause;

    public static bool ConsoleStatePausesTicker(int consoleState) =>
        consoleState == ConsoleDown || consoleState == ConsoleFalling;

    public static bool CheckTickerPausedGate(
        bool netGame,
        int gameState,
        bool menuPauses,
        bool consolePauses,
        bool demoPlayback,
        bool demoRecording,
        bool viewInitialized,
        bool wipeMatchesGameState)
    {
        if (netGame || gameState == GsTitleLevel)
            return false;
        if (!menuPauses && !consolePauses)
            return false;
        if (demoPlayback || demoRecording)
            return false;
        if (!viewInitialized || !wipeMatchesGameState)
            return false;
        return true;
    }

    public static bool PauseMusicWhenMenusPauseSound(int levelFlags2) =>
        (levelFlags2 & Level2PauseMusicInMenus) != 0;

    public static bool ShouldUpdateLevelInterpolatorBeforeActorClear(int levelCount) => levelCount > 0;

    public static bool TryDecrementInventoryTicsForPlayer(
        bool worldPaused,
        bool playerInGame,
        int inventoryTics,
        out int nextInventoryTics)
    {
        nextInventoryTics = inventoryTics;
        if (!ShouldDecrementPlayerInventoryTics(worldPaused, playerInGame, inventoryTics))
            return false;

        nextInventoryTics = DecrementInventoryTics(inventoryTics);
        return true;
    }
}
