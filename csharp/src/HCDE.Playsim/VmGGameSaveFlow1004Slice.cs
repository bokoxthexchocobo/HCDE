namespace HCDE.Playsim;

/// <summary>
/// `G_DoQuickSave`, save comment/pic helpers, and `G_DoSaveGame` head from <c>g_game.cpp</c> (**2900–3049**).
/// </summary>
public static class VmGGameSaveFlow1004Slice
{
    public const int GsLevel = 0;
    public const int CurrentSaveVersion = 4560;

    public enum QuicksaveSkipReason
    {
        None,
        NetGame,
    }

    public readonly struct SaveSnapshotFailureCleanup
    {
        public bool ClearInSave { get; init; }
        public bool UnfreezeTime { get; init; }
    }

    public readonly struct SaveCommentElapsed
    {
        public int Hours { get; init; }
        public int Minutes { get; init; }
        public int Seconds { get; init; }
    }

    public static QuicksaveSkipReason ResolveQuicksaveSkipReason(bool netGame) =>
        netGame ? QuicksaveSkipReason.NetGame : QuicksaveSkipReason.None;

    public static int ResolveQuicksaveRotationCount(int quicksaveRotationCount) =>
        quicksaveRotationCount != 0 ? quicksaveRotationCount : 1;

    public static int ResolveLastQuicksaveSlot(int quicksaveNum, int slotCount) =>
        quicksaveNum < 0 ? 0 : (quicksaveNum + 1) % slotCount;

    public static string FormatQuicksaveFileStem(int slotIndex) => $"quick{slotIndex:D2}";

    public static string FormatSaveCommentMapHeader(string mapName, string levelName) =>
        $"{mapName} - {levelName}\n";

    public static bool ShouldAbortSaveWithoutLevel(int lineCount, int sectorCount, int gameState) =>
        lineCount == 0 || sectorCount == 0 || gameState != GsLevel;

    public static string ResolveSaveFilenameForContext(
        bool demoPlayback,
        string requestedFilename,
        string demoSaveLeafName) =>
        demoPlayback ? demoSaveLeafName : requestedFilename;

    public static SaveSnapshotFailureCleanup ResolveSnapshotFailureCleanup(bool clWaitForSave) =>
        new() { ClearInSave = true, UnfreezeTime = clWaitForSave };

    public static bool IsInvalidSavePicDimensions(int width, int height) =>
        width <= 0 || height <= 0;

    public static bool ShouldWriteDummySavePic(int width, int height, bool storeSavePic) =>
        IsInvalidSavePicDimensions(width, height) || !storeSavePic;

    public static int LevelTimeWholeSeconds(int levelTimeTics, int ticRate) =>
        ticRate == 0 ? 0 : levelTimeTics / ticRate;

    public static SaveCommentElapsed FormatElapsedLevelTime(int levelTimeTics, int ticRate)
    {
        var levelTime = LevelTimeWholeSeconds(levelTimeTics, ticRate);
        return new SaveCommentElapsed
        {
            Hours = levelTime / 3600,
            Minutes = levelTime % 3600 / 60,
            Seconds = levelTime % 60,
        };
    }

    public static bool UsesCurrentSaveVersionConstant(int versionWritten, int saveVerConstant) =>
        versionWritten == saveVerConstant;
}
