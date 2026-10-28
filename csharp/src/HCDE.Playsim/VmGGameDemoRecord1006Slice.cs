namespace HCDE.Playsim;

/// <summary>
/// Demo read default dispatch seam, `CCMD (stop)`, `G_WriteDemoTiccmd`, `G_RecordDemo`, and `G_BeginRecording` from <c>g_game.cpp</c> (**3200–3349**).
/// </summary>
public static class VmGGameDemoRecord1006Slice
{
    public const int MaxPlayers = 8;
    public const int InitialMaxDemoSize = 0x20000;
    public const int DemoBufferGrowthBytes = 0x20000;
    public const int DemoBufferSafetyMargin = 64;
    public const string DefaultDemoExtension = ".lmp";

    public enum DemoRecordingWriteGate
    {
        ProceedWithTicWrite,
        FinalizeRecording,
    }

    public readonly struct DemoBufferExpansion
    {
        public int NewMaxDemoSize { get; init; }
        public int RemainingWriteCapacity { get; init; }
    }

    public static bool RoutesUnhandledDemoCommandToNet(int commandId, int demoUserCmd, int demoEmptyUserCmd, int demoStop, int demoDropPlayer) =>
        commandId != demoUserCmd
        && commandId != demoEmptyUserCmd
        && commandId != demoStop
        && commandId != demoDropPlayer;

    public static DemoRecordingWriteGate ResolveDemoRecordingWriteGate(
        bool stopRecording,
        bool demoRecordingActive) =>
        stopRecording && demoRecordingActive
            ? DemoRecordingWriteGate.FinalizeRecording
            : DemoRecordingWriteGate.ProceedWithTicWrite;

    public static bool ShouldTraceFullConsoleAfterDemoStop(bool stopRecording, bool netGame) =>
        stopRecording && !netGame;

    public static bool ShouldWriteSpecialDemoTics(bool hasSpecData, int gameTic, int ticDup) =>
        hasSpecData && ticDup != 0 && gameTic % ticDup == 0;

    public static bool RequiresDemoBufferExpansion(long writeOffset, long bufferSize, int safetyMargin) =>
        writeOffset > bufferSize - safetyMargin;

    public static DemoBufferExpansion ResolveDemoBufferExpansion(int currentMaxSize, long writeOffset, long bufferSize) =>
        new()
        {
            NewMaxDemoSize = currentMaxSize + DemoBufferGrowthBytes,
            RemainingWriteCapacity = (int)Math.Max(0, bufferSize - writeOffset),
        };

    public static string AppendDefaultDemoExtensionIfMissing(string path, string extension) =>
        path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

    public static string ResolveRecordingStartMap(string? startMap, string primaryMapName) =>
        string.IsNullOrEmpty(startMap) ? primaryMapName : startMap;

    public static bool ShouldWritePlayerInfoDemoChunk(int playerIndex, bool playerInGame) =>
        playerIndex < MaxPlayers && playerInGame;

    public static bool ShouldWriteNetdemoHeaderChunk(bool multiplayer, int activePlayerCount) =>
        multiplayer && activePlayerCount >= 1;

    public static int DemoHeaderMinVersionField() => 2;

    public static int DemoHeaderFormatSubField() => 3;

    public static long RelinkDemoBufferPointer(long pointerOffset, long newBase, long oldBase) =>
        newBase + (pointerOffset - oldBase);

    public enum UserGameModeForDemo
    {
        UserGame,
        DemoSession,
    }

    public static UserGameModeForDemo ResolveUserGameModeForDemoRecord(bool recordDemo) =>
        recordDemo ? UserGameModeForDemo.DemoSession : UserGameModeForDemo.UserGame;

    public static int ResolveInitialDemoBufferSize() => InitialMaxDemoSize;
}
