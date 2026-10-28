namespace HCDE.Client;

/// <summary><c>System_OnScreenSizeChanged</c>, scene rect, and related UI helpers from <c>d_main.cpp</c> (3478–3584).</summary>
public static class VmDMainScreenUi
{
    public const int ConsoleFalling = 2;
    public const int HudToggled = 1;

    public static bool ShouldNotifyStatusBar(object? statusBar) => statusBar != null;

    public static bool ShouldReloadCrosshair() => true;

    public static bool ShouldRefreshAutomap(bool hasLevel, bool hasAutomap) => hasLevel && hasAutomap;

    public static bool SceneUsesFullScreen(int screenBlocks) => screenBlocks >= 10;

    public static int SceneHeight(int screenBlocks, int screenHeight) =>
        SceneUsesFullScreen(screenBlocks) ? screenHeight : (screenBlocks * screenHeight / 10) & ~7;

    public static int SceneWidth(int screenBlocks, int screenWidth) =>
        SceneUsesFullScreen(screenBlocks) ? screenWidth : screenBlocks * screenWidth / 10;

    public static bool ConsoleToggleFallsHud(int consoleState, int hudToggled) =>
        consoleState == ConsoleFalling && hudToggled != 0;

    public static bool ShouldRefreshLevelNames(bool hasInfo) => hasInfo;

    public static bool AppendCrashTextNoOp(char[]? buffer, char[]? end) => buffer == null || end == null;

    public static int AppendCrashTextRemaining(char[] buffer, char[] end) => end.Length - buffer.Length;

    public static string LocationDescriptionForLevel(bool hasLevel) => hasLevel ? "map" : string.Empty;

    public static bool CrashInfoRejectBuffer(object? buffer, int buffLen) => buffer == null || buffLen == 0;

    public static bool CrashInfoRejectShortBuffer(int buffLen) => buffLen < 3;

    public static string CrashLineFeed(string? lfstr) => lfstr ?? "\n";

    public static bool CrashNotInLevel(int gameState, int gsLevel, int gsTitle) =>
        gameState != gsLevel && gameState != gsTitle;

    public static bool CrashViewInactive(bool viewActive) => !viewActive;
}
