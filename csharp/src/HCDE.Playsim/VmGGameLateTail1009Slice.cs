namespace HCDE.Playsim;

/// <summary>
/// `G_CheckDemoStatus` continuation, demo record finish, slideshow/autosave hooks, and late globals from <c>g_game.cpp</c> (**3650–3799**).
/// </summary>
public static class VmGGameLateTail1009Slice
{
    public const int MaxPlayers = 8;
    public const int ConsolePlayerIndex = 0;
    public const int GaNothing = 0;
    public const int GaAutosave = 10;
    public const int GaFullConsole = 17;
    public const int GaResumeConversation = 18;
    public const int GsLevel = 0;
    public const int ZLibOk = 0;
    public const int TicRate = 35;

    public enum DemoPlaybackEndPolicy
    {
        AdvanceDemoLoop,
        ExitToFullConsole,
    }

    public enum ScriptScreenshotPolicy
    {
        Skip,
        Capture,
    }

    public static int ComputeTimingDemoElapsedTicks(int currentTime, int startTime) =>
        currentTime - startTime;

    public static bool ShouldClearPlayerInGameSlot(int playerIndex) =>
        playerIndex >= 1 && playerIndex < MaxPlayers;

    public static bool ShouldReattachStatusBarToConsole(bool hasStatusBar, int consolePlayer) =>
        hasStatusBar && consolePlayer == ConsolePlayerIndex;

    public static DemoPlaybackEndPolicy ResolveDemoPlaybackEndPolicy(bool singleDemo, bool timingDemo) =>
        singleDemo || timingDemo
            ? DemoPlaybackEndPolicy.ExitToFullConsole
            : DemoPlaybackEndPolicy.AdvanceDemoLoop;

    public static float ComputeTimedDemoFps(int gameTics, int realTics, int ticRate) =>
        realTics == 0 ? 0f : (float)gameTics / realTics * ticRate;

    public static bool ShouldAdvanceDemoLoopAfterPlayback(
        bool demoPlayback,
        bool singleDemo,
        bool timingDemo) =>
        demoPlayback && !singleDemo && !timingDemo;

    public static int EstimateDemoBodyCompressCapacity(int bodyLength) =>
        bodyLength + bodyLength / 100 + 12;

    public static bool ShouldReplaceDemoBodyWithCompressed(
        int zlibResult,
        long compressedLength,
        long bodyLength) =>
        zlibResult == ZLibOk && compressedLength < bodyLength;

    public static bool DemoRecordingFileWriteSucceeded(bool writerOpened, bool writeMatchedSize) =>
        writerOpened && writeMatchedSize;

    public static bool ShouldRemoveDemoFileAfterFailedWrite(bool writerOpened, bool saved) =>
        writerOpened && !saved;

    public static string ResolveSlideshowSelection(string? whichOne, string levelDefaultSlideshow) =>
        string.IsNullOrEmpty(whichOne) ? levelDefaultSlideshow : whichOne;

    public static ScriptScreenshotPolicy ResolveScriptScreenshotPolicy(bool enableScriptScreenshot) =>
        enableScriptScreenshot ? ScriptScreenshotPolicy.Capture : ScriptScreenshotPolicy.Skip;

    public static bool ShouldTraceAutosaveGameAction(int gameAction) =>
        gameAction == GaNothing;
}
