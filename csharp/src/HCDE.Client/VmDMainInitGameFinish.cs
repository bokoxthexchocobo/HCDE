namespace HCDE.Client;

/// <summary><c>D_InitGame</c> tail: shaders, net, demos, title from <c>d_main.cpp</c> (4346–4499).</summary>
public static class VmDMainInitGameFinish
{
    public const int GaLoadgame = 1;
    public const int GaTitleloop = 2;
    public const int GaMapwarp = 3;

    public static bool CompileShaderWhenClient(bool norun, bool dedicated) => !norun && !dedicated;

    public static bool UpdatePaletteOnRestart(bool norun, bool dedicated, bool restart) =>
        !(norun || dedicated) && restart;

    public static bool CreateStartScreenWhenMissing(bool nostartscreen, object? startScreen) =>
        !nostartscreen && startScreen == null;

    public static bool NorunBatchEarlyExit(bool norun, bool batch) => norun || batch;

    public static int NorunExitCode() => 1337;

    public static bool RunDelayedCommandsClient(bool dedicated) => !dedicated;

    public static bool CustomizeInvulMap(bool flag) => flag;

    public static bool RecordDemoFromArg(string? recordArg) => recordArg != null;

    public static bool StartupDoneSound(bool dedicated) => !dedicated;

    public static bool FinishProgressWhenStartScreen(bool hasScreen) => hasScreen;

    public static bool CompileShadersUntilDone(bool dedicated) => !dedicated;

    public static bool InitTwoDrawer(bool dedicated) => !dedicated;

    public static bool LoadgameFatalWhenMissing(bool exists) => !exists;

    public static bool TimedemoFromArg(string? arg) => arg != null;

    public static bool AutostartWarp(bool autostart, bool netgame) => autostart || netgame;

    public static bool NoWipeUnlessWarpWipe(bool warpWipeArg) => !warpWipeArg;

    public static bool DedicatedIgnoresStoredWarp(bool dedicated) => dedicated;

    public static bool StartTitleWhenNoIntro(bool multiplayer, bool noIntros, bool noIntroArg) =>
        multiplayer || noIntros || noIntroArg;

    public static bool RestartUsesStartTitle(bool restart) => restart;

    public static bool EngineInitializeEvent(bool ok) => ok;

    public static int InitGameReturnSuccess() => 0;

    public static int StartScreenBudget(int maxProgress, int perShader) =>
        perShader > 0 ? maxProgress * 10 / 9 : maxProgress + 3;

    public static bool ShaderCompileLoopClient(bool dedicated, bool compiled) => !dedicated && !compiled;

    public static bool DeferPlayDemoWhenArg(string? arg) => arg != null;

    public static bool SkipAutostartWhenLoadgame(int gameaction, int gaLoadgame, int gaLoadgamehidecon) =>
        gameaction == gaLoadgame || gameaction == gaLoadgamehidecon;

    public static bool BeginRecordingWhenDemo(bool demorecording, bool autostart) => demorecording && autostart;

    public static bool BeginRecordingNullWhenDemo(bool demorecording) => demorecording;

    public static bool ApplyStoredWarpWhenClient(bool dedicated) => !dedicated;

    public static bool ClearStoredWarpAfterApply() => true;

    public static bool CutsceneBlocksTitle(bool started) => !started;

    public static bool NewModeAdjustOnRestart(bool restart) => restart;

    public static bool ClearSetModeNeededOnRestart() => false;

    public static bool GameStateFullConsole() => true;

    public static bool GameStateStartupAfterNet() => true;
}
