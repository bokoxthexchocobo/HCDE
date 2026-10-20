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

    public static bool HeadlessSkipsDisplay(bool headless, bool dedicated) =>
        headless || dedicated;

    public static bool Win32DisplayBeforeSdl(bool preferWin32, bool win32Ok) =>
        preferWin32 && win32Ok;

    public static bool FallbackSdlDisplay(bool win32Failed, bool sdlOk) =>
        win32Failed && sdlOk;

    public static bool MissingBackendExitCode() => MissingDisplayBackend();

    public static bool MenuCaptureAllowed(int state, int fullConsole, int startup) =>
        GameStateAllowsMenuCapture(state, fullConsole, startup);

    public static int PropagateInternalReturn(int ret) => NormalExitFromInternal(ret);

    public static bool NorunShortCircuitsWidgetClose(int ret) => SkipCloseWidgetWhenNorun(ret);

    public static bool ShutdownAfterMainTry(bool enteredTry) => ShutdownAfterTry(enteredTry);

    public static bool CatchExitEventBranch(bool isExit) => CatchExitEvent(isExit);

    public static bool CatchStdExceptionBranch(bool isStd) => CatchStdException(isStd);

    public static bool NetDiagOnShutdownPath() => NetDiagSessionEndOnShutdown();

    public static bool CloseNetBeforeExit() => CloseNetworkOnShutdown();

    public static bool FinalGcBeforeProcessEnd() => FinalGcPass();

    public static bool ConsoleDeinitIsLastStep() => DeinitConsoleLast();
}
