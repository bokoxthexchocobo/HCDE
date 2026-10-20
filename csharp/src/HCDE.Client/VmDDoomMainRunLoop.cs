namespace HCDE.Client;

/// <summary>Post-<c>D_InitGame</c> debug listen and <c>D_DoomLoop</c> restart cycle from <c>d_main.cpp</c> (4763–4792).</summary>
public static class VmDDoomMainRunLoop
{
    public const int MaxDebugPort = 65535;

    public static bool ShouldListenDebug(bool shouldDebug) => shouldDebug;

    public static bool DebugPortFromArg(string? debugPortArg, int cvarPort, out int port)
    {
        port = debugPortArg != null ? int.Parse(debugPortArg) : cvarPort;
        return true;
    }

    public static bool DebugPortInRange(int port) => port >= 0 && port <= MaxDebugPort;

    public static bool RunAnonStatsAfterInit() => true;

    public static bool UpdateTitleAfterInit() => true;

    public static bool EnterDoomLoop() => true;

    public static bool CleanupAfterLoopExit() => true;

    public static int GameStateAfterCleanup() => 2; // GS_STARTUP

    public static bool InfiniteRestartLoop() => true;
}
