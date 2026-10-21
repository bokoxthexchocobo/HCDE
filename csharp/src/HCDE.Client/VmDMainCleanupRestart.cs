namespace HCDE.Client;

/// <summary><c>D_Cleanup</c> guards and <c>debug_restart</c> from <c>d_main.cpp</c> (4888–5019).</summary>
public static class VmDMainCleanupRestart
{
    public static bool NetgameCleanupWarn(bool netgame, bool demoplayback) => netgame && !demoplayback;

    public static bool ErrorCleanupShutdown(string tag) => tag.Length > 0;

    public static bool IncrementRestartCounter(int restart) => restart >= 0;

    public static bool IgnoreDebugRestartDuringNetgame(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool StripRestartArgs(bool norun) => !norun;

    public static bool ReappendArgvWhenPresent(int argc) => argc > 1;

    public static bool ShutdownTagNonEmpty(string tag) => !string.IsNullOrEmpty(tag);
}
