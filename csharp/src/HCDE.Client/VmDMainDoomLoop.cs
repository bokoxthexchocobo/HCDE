namespace HCDE.Client;

/// <summary><c>D_DoomLoop</c> and <c>D_Render</c> from <c>d_main.cpp</c> (629–649, 1668–1761).</summary>
public static class VmDMainDoomLoop
{
    public static bool ShouldClampInterpolationUntilPlayLoop(bool noInterpolate) => noInterpolate;

    public static bool ShouldInvalidatePageOnEnter(bool pageValid) => !pageValid;

    public static bool ShouldStartFrame(int gametic, int lastTic) => gametic > lastTic;

    public static bool ShouldAbortToQuickExit(bool gameloopAbort) => gameloopAbort;

    public static bool ShouldIgnoreRestartDuringNetgame(bool wantRestart, bool netGame, bool demoPlayback) =>
        wantRestart && netGame && !demoPlayback;

    public static bool ShouldExitOnRestart(bool wantRestart, bool netGame, bool demoPlayback) =>
        wantRestart && (!netGame || demoPlayback);

    public static bool ShouldRunLevelDynamicLightPass(
        bool glLights,
        int vidRenderMode,
        bool rDynLights,
        bool levelLights,
        int lightProbeCount) =>
        (glLights && vidRenderMode == 4) || (rDynLights && vidRenderMode != 4) || lightProbeCount > 0;

    public static bool ShouldInterpolateLevel(bool interpolate) => interpolate;

    public static bool ShouldRestoreInterpolations(bool interpolate) => interpolate;

    public static bool ShouldSaveHudStateOnToggle(bool togglingOn) => togglingOn;

    public static int ResolveFullscreenHudScreenblocks() => 12;

    public static bool ShouldHideConsoleOnHudToggle(bool togglingOn) => togglingOn;

    public static bool ShouldClearMenusOnHudToggle(bool togglingOn) => togglingOn;

    public static bool ShouldCleanupOnRecoverableError(bool hasMessage) => hasMessage;

    public static bool ShouldCleanupOnFilesystemError(bool hasWhat) => hasWhat;

    public static bool ShouldCleanupOnVmAbort(bool hasStack) => hasStack;

    public static bool ShouldAllowEndgameCcmd(bool netGame) => !netGame;

    public static bool ShouldSetFullConsoleGameaction(bool allowed) => allowed;

    public static bool ShouldAdvanceDemoFlag() => true;
}
