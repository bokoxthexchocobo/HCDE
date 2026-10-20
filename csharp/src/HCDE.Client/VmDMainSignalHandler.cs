namespace HCDE.Client;

/// <summary><c>SignalHandler</c> from <c>d_main.cpp</c> (4795–4807).</summary>
public static class VmDMainSignalHandler
{
    public static bool ExitImmediatelyWhenAlreadyAborting(bool gameloopAbort) => gameloopAbort;

    public static int ImmediateExitCode() => 0;

    public static bool RequestShutdownWhenRunning(bool gameloopAbort) => !gameloopAbort;

    public static bool SetGameloopAbort() => true;
}
