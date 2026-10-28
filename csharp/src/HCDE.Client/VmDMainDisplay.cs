namespace HCDE.Client;

/// <summary><c>D_Display</c> early gates and wipe branch policy from <c>d_main.cpp</c> (1192–1607).</summary>
public static class VmDMainDisplay
{
    public static bool ShouldSkipDisplay(bool noDrawers, bool screenNull, bool dedicatedServer) =>
        noDrawers || screenNull || dedicatedServer;

    public static bool ShouldForceStartupFocus(bool firstFrame) => firstFrame;

    public static bool ShouldPromoteAppActive(bool sawAppActive, bool appActive) =>
        !sawAppActive && !appActive;

    public static bool IsWaitingForFirstActivation(bool sawAppActive) => !sawAppActive;

    public static bool IsStartupTraceWindow(int startupRenderTraceFrame, int ticRate) =>
        startupRenderTraceFrame < ticRate * 10;

    public static bool ShouldLogStartupTraceEveryFiveFrames(int startupRenderTraceFrame) =>
        startupRenderTraceFrame % 5 == 0;

    public static bool ShouldLogWaitingActivationTrace(int startupRenderTraceFrame) =>
        startupRenderTraceFrame % 10 == 0;

    public static int AdvanceStartupFrameWhileWaitingActivation(int frame, int ticRate, bool sawAppActive)
    {
        if (!sawAppActive && frame < ticRate * 6)
            return frame + 1;
        return frame;
    }

    public static int AdvanceStartupFrameInTraceWindow(int frame, bool traceWindow) =>
        traceWindow ? frame + 1 : frame;

    public static bool ShouldMarkSawAppActive(bool appActive) => appActive;

    public static bool ShouldBlockInactiveRender(
        bool appActive,
        bool setModeNeeded,
        bool activeInBackground,
        bool userGame,
        int gameState,
        int gsStartup,
        int gsTitleLevel,
        int gsDemoScreen,
        bool waitingForFirstActivation) =>
        !appActive
        && !setModeNeeded
        && !activeInBackground
        && userGame
        && gameState != gsStartup
        && gameState != gsTitleLevel
        && gameState != gsDemoScreen
        && !waitingForFirstActivation;

    public static bool ShouldEmitBlockedInactiveTrace(int traceCount, int maxTraces) => traceCount < maxTraces;

    public static int IncrementBlockedInactiveTrace(int traceCount) => traceCount + 1;

    public static bool ShouldDisableWipes(bool netGame, int noWipe, bool canWipe)
    {
        if (netGame || !canWipe)
            return true;
        return noWipe > 0;
    }

    public static int DecayNoWipeCounter(int noWipe) => noWipe > 0 ? noWipe - 1 : noWipe;

    public static bool ShouldStartWipe(
        int gameState,
        int wipeGameState,
        int gsFullConsole,
        int gsTitleLevel,
        int vrMode,
        int vidRenderMode) =>
        gameState != wipeGameState
        && gameState != gsFullConsole
        && gameState != gsTitleLevel
        && (vrMode == 0 || vidRenderMode != 4);

    public static bool ShouldCaptureWipeStartScreen(bool shouldStartWipe) => shouldStartWipe;

    public static int ResolveWipeType(int wipeGameState, int defaultWipeType, int gsForceFade, int gsForceBurn, int gsForceMelt, int wipeFade, int wipeBurn, int wipeMelt)
    {
        return wipeGameState switch
        {
            var s when s == gsForceFade => wipeFade,
            var s when s == gsForceBurn => wipeBurn,
            var s when s == gsForceMelt => wipeMelt,
            _ => defaultWipeType,
        };
    }

    public static bool ShouldClearWipeStartWhenDisabled(bool wipesDisabled) => wipesDisabled;

    public static bool ShouldEnterLevelRenderBranch(int gameState, int gsLevel, int gsTitleLevel, int gametic) =>
        (gameState == gsLevel || gameState == gsTitleLevel) && gametic != 0;

    public static bool ShouldLogLevelBranchTrace(int startupRenderTraceFrame) =>
        startupRenderTraceFrame % 10 == 0;

    public static bool ShouldDrawAutomapInDisplay(bool hudToggled, bool automapActive) =>
        !hudToggled && automapActive;

    public static int ResolveAutomapDrawerBottom(bool altHud, int viewHeight, int screenHeight, int statusBarTop) =>
        altHud && viewHeight == screenHeight ? viewHeight : statusBarTop;

    public static bool ShouldRefreshViewBorder(bool automapActive, bool viewActive) =>
        !automapActive || viewActive;

    public static bool ShouldPresentDirect(bool wipeStartPresent, int noWipe, int wipeTypeNone, bool hudToggled) =>
        wipeStartPresent == false || noWipe < 0 || wipeTypeNone == 0 || hudToggled;

    public static bool ShouldLogDirectPresentTrace(int startupRenderTraceFrame) =>
        startupRenderTraceFrame % 10 == 0;

    public static bool ShouldLogWipePresentTrace(int startupRenderTraceFrame) =>
        startupRenderTraceFrame % 10 == 0;
}
