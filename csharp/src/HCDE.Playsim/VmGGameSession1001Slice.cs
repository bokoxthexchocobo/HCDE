namespace HCDE.Playsim;

/// <summary>
/// Player disconnect, screenshot queue, load-game WAD checks, and save CVAR filter from <c>g_game.cpp</c> (**2449–2598**).
/// </summary>
public static class VmGGameSession1001Slice
{
    public const int GameActionNothing = 0;
    public const int GameActionLoadGame = 1;
    public const int GameActionLoadGameHideConsole = 2;

    public readonly struct LoadGameRequest
    {
        public string SaveName { get; init; }
        public int GameAction { get; init; }
    }

    public static bool ShouldRevertSpyCameraToSelf(
        bool viewerInGame,
        object? viewerCamera,
        object? departingPlayerMob) =>
        viewerInGame && ReferenceEquals(viewerCamera, departingPlayerMob);

    public static bool ShouldAttachConsoleStatusBar(int viewerIndex, int consolePlayerIndex) =>
        viewerIndex == consolePlayerIndex;

    public static bool ShouldDestroyMorphedReplacement(
        object? replacementMob,
        object? destroyedMob,
        bool markedForEuthanize) =>
        replacementMob != null
        && !ReferenceEquals(replacementMob, destroyedMob)
        && !markedForEuthanize;

    public static bool ShouldQueueScreenshot(int gameAction, string? filename) =>
        gameAction == GameActionNothing && !string.IsNullOrEmpty(filename);

    public static LoadGameRequest? TryCreateLoadGameRequest(string? saveName, bool hideConsole)
    {
        if (saveName == null)
            return null;
        return new LoadGameRequest
        {
            SaveName = saveName,
            GameAction = hideConsole ? GameActionLoadGameHideConsole : GameActionLoadGame,
        };
    }

    public static bool IsResourceWadLoaded(int resourceCheckResult) => resourceCheckResult >= 0;

    public static bool ShouldSkipMapWadValidation(string? gameWad, string? mapWad) =>
        gameWad == null
        || mapWad == null
        || string.Equals(gameWad, mapWad, StringComparison.OrdinalIgnoreCase);

    public static bool MergePrintRequiresOnMissingWad(
        bool currentPrintRequires,
        bool printWarnings,
        bool wadMissing) =>
        wadMissing && printWarnings ? true : currentPrintRequires;

    public static bool ResolveSaveGameWadsComplete(
        bool gameWadLoaded,
        bool mapWadLoadedOrSkipped) =>
        gameWadLoaded && mapWadLoadedOrSkipped;

    public static bool ShouldSerializeCvarForSave(
        uint cvarFlags,
        uint filterMask,
        uint noSaveFlag,
        uint ignoreFlag,
        uint configOnlyFlag)
    {
        if ((cvarFlags & filterMask) == 0)
            return false;
        if ((cvarFlags & (noSaveFlag | ignoreFlag | configOnlyFlag)) != 0)
            return false;
        return true;
    }
}
