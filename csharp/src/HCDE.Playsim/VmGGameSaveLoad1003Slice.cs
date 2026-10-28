namespace HCDE.Playsim;

/// <summary>
/// Load-game globals tail, `G_SaveGame` gates, and `G_DoAutoSave` rotation from <c>g_game.cpp</c> (**2750–2899**).
/// </summary>
public static class VmGGameSaveLoad1003Slice
{
    public const int GaSaveGame = 9;
    public const int GaQuicksave = 24;
    public const int GaNothing = 0;
    public const int GsLevel = 0;
    public const uint Level2NoAutosaveHint = 0x40000000;

    public enum SaveGameRejectReason
    {
        None,
        PendingSave,
        NotUserGame,
        NotInLevel,
        SinglePlayerDead,
        NetLimitedSaves,
    }

    public enum AutosaveSkipReason
    {
        None,
        NetGame,
    }

    public readonly struct SaveGameCommandEffect
    {
        public bool SendSave { get; init; }
        public int? TraceGameAction { get; init; }
    }

    public static bool ShouldLoadImportantCvarsFromBlob(string? importantCvars) =>
        !string.IsNullOrEmpty(importantCvars);

    public static int ScaleLevelTimeFromSaveTicks(int levelTime, int engineTicRate, int saveTicRate) =>
        saveTicRate == 0
            ? levelTime
            : (int)((long)levelTime * engineTicRate / saveTicRate);

    public static SaveGameRejectReason ResolveSaveGameRejectReason(
        bool sendSave,
        int gameAction,
        bool userGame,
        int gameState,
        int health,
        bool multiplayer,
        bool netGame,
        bool netLimitSaves,
        bool localCanControlSettings)
    {
        if (sendSave || gameAction == GaSaveGame || gameAction == GaQuicksave)
            return SaveGameRejectReason.PendingSave;
        if (!userGame)
            return SaveGameRejectReason.NotUserGame;
        if (gameState != GsLevel)
            return SaveGameRejectReason.NotInLevel;
        if (health <= 0 && !multiplayer)
            return SaveGameRejectReason.SinglePlayerDead;
        if (netGame && netLimitSaves && !localCanControlSettings)
            return SaveGameRejectReason.NetLimitedSaves;
        return SaveGameRejectReason.None;
    }

    public static SaveGameCommandEffect ResolveSaveGameCommandEffect(
        bool quickSave,
        int currentGameAction)
    {
        if (quickSave)
        {
            return new SaveGameCommandEffect
            {
                SendSave = false,
                TraceGameAction = GaQuicksave,
            };
        }

        return new SaveGameCommandEffect
        {
            SendSave = true,
            TraceGameAction = currentGameAction == GaQuicksave ? GaNothing : null,
        };
    }

    public static AutosaveSkipReason ResolveAutosaveSkipReason(bool netGame) =>
        netGame ? AutosaveSkipReason.NetGame : AutosaveSkipReason.None;

    public static int ResolveAutosaveSlotCount(int autosaveCount) =>
        autosaveCount != 0 ? autosaveCount : 1;

    public static int ComputeInitialNextAutosaveIndex(int autosaveNum, int slotCount) =>
        (autosaveNum + 1) % slotCount;

    public static int AdvanceAutosaveSlotIndex(int nextAutosave, int slotCount) =>
        (nextAutosave + 1) % slotCount;

    public static bool ShouldRotateAutosaveHint(uint levelFlags2) =>
        (levelFlags2 & Level2NoAutosaveHint) == 0;

    public static uint ClearLevelNoAutosaveHint(uint levelFlags2) =>
        levelFlags2 & ~Level2NoAutosaveHint;
}
