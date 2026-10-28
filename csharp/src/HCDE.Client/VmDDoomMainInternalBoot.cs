namespace HCDE.Client;

/// <summary><c>D_DoomMain_Internal</c> entry: buttons, callbacks, batch flags from <c>d_main.cpp</c> (4507–4656).</summary>
public static class VmDDoomMainInternalBoot
{
    public const int Maxplayers = 32;

    public static int NetIdStart() => Maxplayers + 1;

    public static bool ForceSwNodesFromArg(bool hasFlag) => hasFlag;

    public static bool UseBatchErrorLog(string? batchout) => batchout != null && batchout.Length > 0;

    public static bool BatchRunFromErrorLog(bool hasBatchout) => hasBatchout;

    public static bool NosoundWhenBatch(bool batchrun) => batchrun;

    public static bool DedicatedProcessTag(bool dedicatedExe) => dedicatedExe;

    public static bool FatalWhenBasewadMissing(bool found) => !found;

    public static bool WantsVersion(bool v, bool version) => v || version;

    public static bool WantsHelp(bool help, bool h, bool helpAll, bool doshelp) => help || h || helpAll || doshelp;

    public static bool EarlyExitVersionHelp(bool wantsVersion, bool wantsHelp) => wantsVersion || wantsHelp;

    public static int VersionHelpExitCode() => 0;

    public static bool ApplyLogfileArg(bool notEmpty) => notEmpty;

    public static bool ExecLogfileBatch(bool batchoutPresent) => batchoutPresent;

    public static bool PrintCommandLineWhenBatch(bool batchrun) => batchrun;

    public static bool ConfirmSendStatsAfterLogging() => true;

    public static bool CreateIwadManager(bool hasBasewad) => hasBasewad;

    public static bool DoAutoloadSetup(bool hasManager) => hasManager;

    public static bool EnableVmDebugFromArg(string? debugPort) => debugPort != null;

    public static bool ShouldDebug(bool vmDebug, bool debugArg) => vmDebug || debugArg;

    public static bool MainLoopBegin() => true;

    public static bool StaticInitPClass() => true;

    public static bool StaticInitPType() => true;

    public static bool ReinitConsoleOnRestart(bool restart) => restart;

    public static int ConsoleWidthRestart() => 640;

    public static int ConsoleHeightRestart() => 480;

    public static bool MlookReleaseLock() => true;

    public static bool KlookReleaseLock() => true;

    public static bool SysCallbacksPopulate() => true;

    public static bool CheckSkipGameOptionBlock() => true;

    public static bool OkForLocalization() => true;

    public static bool RemapUserTranslation() => true;

    public static bool EnterMainDoLoop() => true;

    public static bool RestartConsoleDimensions(bool restart) => restart;

    public static bool LoopMarkBegin() => true;

    public static bool LoopMarkAfterPClass() => true;

    public static bool LoopMarkAfterPType() => true;

    public static bool LoadHexFontEarly(bool hasWad) => hasWad;

    public static bool InitWidgetResources(bool hasWad) => hasWad;

    public static bool InitConsole80x25() => true;

    public static bool PrintVersionBanner() => true;

    public static bool CheckHelpAll(bool helpAll) => helpAll;

    public static bool TakeLogfileValue(bool notEmpty) => notEmpty;

    public static bool MarkStartupLogging() => true;

    public static bool MarkIwadManagerInit() => true;

    public static bool MarkAutoloadDone() => true;

    public static bool StartupPreVideoStatus(string _) => true;

    public static bool OptionalWadSearch() => true;

    public static string BasewadFromSearch(string path) => path;

    public static bool NewFailureHandler() => true;

    public static bool DebugTraceInitSession() => true;

    public static bool NetDiagSessionBegin() => true;

    public static bool PrintDebugTracePath() => true;
}
