namespace HCDE.Client;

/// <summary><c>D_DoomMain_Internal</c> restart-loop class init from <c>d_main.cpp</c> (4658–4664).</summary>
public static class VmDDoomMainLoopIter
{
    public static bool RestartConsoleWhenRestart(bool restart) => restart;

    public static int RestartScreenWidth() => 640;

    public static int RestartScreenHeight() => 480;

    public static bool InitConsoleFullscreen(bool restart) => restart;

    public static bool LoopIterationBegin() => true;

    public static bool AfterPClassStaticInit() => true;

    public static bool AfterPTypeStaticInit() => true;

    public static bool VmDebugFromCvar(bool vmDebug) => vmDebug;

    public static bool DebugPortOverrides(bool hasArg) => hasArg;

    public static bool ShouldReinitClassStatics(bool restart) => restart || true;

    public static bool ClearTextureRenameOnRestart(bool restart) => restart;

    public static int DefaultRestartScreenWidth(bool restart) => restart ? 640 : 640;

    public static int DefaultRestartScreenHeight(bool restart) => restart ? 480 : 480;

    public static bool ReopenConsoleFullscreen(bool restart, bool wasFullscreen) =>
        restart && wasFullscreen;

    public static bool LoopBodyRunsInitGame(bool cancelled) => !cancelled;

    public static bool LoopBodyRunsDoomLoop(bool initOk) => initOk;

    public static bool LoopBodyCleanupAfterDoomLoop(bool ranLoop) => ranLoop;

    public static bool ResetGameStateAfterLoop(bool always) => always;

    public static bool KeepRestartingUntilShutdown(bool wantShutdown) => !wantShutdown;

    public static bool VmDebugLevelFromCvar(int level, int spammy) => level >= spammy;

    public static bool NetDiagSessionActive(bool netgame) => netgame;

    public static bool SpriteRenameDisabledAfterPass() => false;

    public static bool PClassStaticInitRequired() => true;

    public static bool PTypeStaticInitRequired() => true;
}
