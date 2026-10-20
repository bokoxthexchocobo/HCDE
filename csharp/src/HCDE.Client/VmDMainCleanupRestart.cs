namespace HCDE.Client;

/// <summary><c>D_Cleanup</c> body and <c>debug_restart</c> from <c>d_main.cpp</c> (4888–5019).</summary>
public static class VmDMainCleanupRestart
{
    public static bool NetgameCleanupWarn(bool netgame, bool demoplayback) => netgame && !demoplayback;

    public static bool StopDebugServer(bool hasServer) => hasServer;

    public static bool CheckDemoOnCleanup(bool demorecording) => demorecording;

    public static bool StopMusicFirst() => true;

    public static bool ClearMapinfoOnCleanup() => true;

    public static bool ErrorCleanupShutdown(string tag) => tag.Length > 0;

    public static bool SaveDefaultsBeforeRestart() => true;

    public static bool ResetGameinfoPlacement() => true;

    public static bool ClearStartupInfoStrings() => true;

    public static bool FullGcBeforeRestart() => true;

    public static bool NullVmActionPointers() => true;

    public static bool ClearPendingWeapons() => true;

    public static bool StaticShutdownClasses() => true;

    public static bool AssertGcRootNull(bool rootNull) => rootNull;

    public static bool IncrementRestartCounter(int restart) => restart >= 0;

    public static bool ClearVmOperationalFlags() => false;

    public static bool IgnoreDebugRestartDuringNetgame(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool StripRestartArgs(bool norun) => !norun;

    public static bool SetWantToRestart() => true;
}
