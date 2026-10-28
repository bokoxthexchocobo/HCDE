namespace HCDE.Playsim;

/// <summary>
/// `C_SerializeCVars` deserialize path and `G_DoLoadGame` validation head from <c>g_game.cpp</c> (**2600–2749**).
/// </summary>
public static class VmGGameLoadFlow1002Slice
{
    public const int GaAutoloadGame = 8;
    public const int GaLoadGameHideConsole = 6;
    public const int GsFullConsole = 4;
    public const int GsHideConsole = 5;
    public const int MinSaveVersion = 4556;
    public const int CurrentSaveVersion = 4560;

    public enum EngineCompatibilityReject
    {
        None,
        LegacyEmptyEngine,
        ForeignEngine,
    }

    public static bool ShouldDeserializeCvarFromSave(
        uint cvarFlags,
        uint filterMask,
        uint noSaveFlag,
        uint ignoreFlag,
        uint configOnlyFlag) =>
        VmGGameSession1001Slice.ShouldSerializeCvarForSave(
            cvarFlags,
            filterMask,
            noSaveFlag,
            ignoreFlag,
            configOnlyFlag);

    public static bool ShouldApplyLoadedCvarString(string? loadedValue) => loadedValue != null;

    public static bool ShouldStopDemoPlaybackForLoad(int gameAction) =>
        gameAction != GaAutoloadGame;

    public static bool ShouldHideConsoleAfterLoadMenu(int gameAction) =>
        gameAction == GaLoadGameHideConsole;

    public static bool IsSaveArchiveEntryMissing(int entryIndex) => entryIndex < 0;

    public static bool IsEngineSignatureAllowed(
        string engineSignature,
        string gameSignature,
        IReadOnlyList<string> allowLoadIn,
        bool gzdoomCompatOk = false)
    {
        if (gzdoomCompatOk)
            return true;
        if (string.Equals(engineSignature, gameSignature, StringComparison.OrdinalIgnoreCase))
            return true;
        foreach (var allowed in allowLoadIn)
        {
            if (string.Equals(allowed, gameSignature, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static EngineCompatibilityReject ResolveEngineCompatibilityReject(
        string engineSignature,
        bool engineAllowed)
    {
        if (engineAllowed)
            return EngineCompatibilityReject.None;
        return string.IsNullOrEmpty(engineSignature)
            ? EngineCompatibilityReject.LegacyEmptyEngine
            : EngineCompatibilityReject.ForeignEngine;
    }

    public static bool IsSaveVersionSupported(int saveVersion, int minVersion, int maxVersion) =>
        saveVersion >= minVersion && saveVersion <= maxVersion;

    public static bool IsSaveVersionTooOld(int saveVersion, int minVersion) =>
        saveVersion < minVersion;

    public static bool IsSaveVersionTooNew(int saveVersion, int maxVersion) =>
        saveVersion > maxVersion;

    public static bool ShouldRejectEmptySaveMap(string? mapName) =>
        string.IsNullOrEmpty(mapName);

    public static int ResolveGameStateAfterLoadHideRequest(
        bool hideConsole,
        int currentGameState,
        int gsFullConsole,
        int gsHideConsole) =>
        hideConsole && currentGameState == gsFullConsole ? gsHideConsole : currentGameState;
}
