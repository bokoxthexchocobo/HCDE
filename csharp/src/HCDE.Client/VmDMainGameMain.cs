namespace HCDE.Client;

/// <summary><c>GameMain</c> from <c>d_main.cpp</c> (4809–4880).</summary>
public static class VmDMainGameMain
{
    public const int NorunExit = 1337;

    public static bool CreateDisplayWhenInteractive(bool headless, bool dedicated) => !headless && !dedicated;

    public static bool TryWin32ThenSdl(bool win32Ok, bool sdlOk) => win32Ok || sdlOk;

    public static int MissingDisplayBackend() => -1;

    public static int DefaultTicRate() => 35;

    public static bool ConsoleNotifyAfterCvarInit() => true;

    public static bool GameStateAllowsMenuCapture(int gamestate, int gsFullConsole, int gsStartup) =>
        gamestate != gsFullConsole && gamestate != gsStartup;

    public static int NormalExitFromInternal(int ret) => ret;

    public static int ExitReasonFromEvent(int reason) => reason;

    public static int FatalFromStdException() => -1;

    public static bool ShutdownAfterTry(bool always) => always;

    public static bool SkipCloseWidgetWhenNorun(int ret) => ret == NorunExit;

    public static bool ClearArgsPointerAfterDelete() => true;

    public static bool InitTimeBeforeMain() => true;

    public static bool InstallConsoleHandlers(bool installed) => installed;

    public static bool RunDoomMainInternal() => true;

    public static bool CatchExitEvent(bool isExitEvent) => isExitEvent;

    public static bool CatchStdException(bool isStd) => isStd;

    public static bool NetDiagSessionEndOnShutdown() => true;

    public static bool CloseNetworkOnShutdown() => true;

    public static bool FinalGcPass() => true;

    public static bool DeinitConsoleLast() => true;
}
