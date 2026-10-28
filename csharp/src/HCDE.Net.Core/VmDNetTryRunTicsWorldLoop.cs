namespace HCDE.Net.Core;

/// <summary><c>TryRunTics</c> positive <c>runTics</c> world loop and frame tail (<c>d_net.cpp</c>).</summary>
public static class VmDNetTryRunTicsWorldLoop
{
    public static int SelectLagStateBeforeWorldNative(int clientTic, int startCommand) =>
        clientTic > startCommand ? 0 : 1;

    public static void ClearClientWaitingFlagsNative(Action<int> setWaiting, IEnumerable<int> clients)
    {
        foreach (var client in clients)
            setWaiting(client);
    }

    public static bool ShouldUpdateLastGameUpdateNative(bool runTicsPositive, int enterTic, int lastGameUpdate) =>
        runTicsPositive && enterTic != lastGameUpdate;

    public static bool ShouldRunWhileLoopNative(int runTicsRemaining) => runTicsRemaining > 0;

    public static int DecrementRunTicsNative(int runTics) => runTics - 1;

    public static int WorldStepsUsedIncrementNative(int used) => used + 1;

    public static int MirrorVisualWorldStepsNative(int worldStepsUsed, bool netgame, bool invasionAuthority) =>
        !invasionAuthority && netgame && worldStepsUsed > 0 ? Math.Clamp(worldStepsUsed, 1, 32) : 1;

    public static bool ShouldRunCoopInterpolationNative(bool authority, bool netgame, bool deathmatch, int gameType) =>
        !authority && netgame && !deathmatch && gameType != 4;

    public static int MinWorldTimerNative(int local, int start) => Math.Min(local, start);

    public static bool ShouldRunClientSideLogicLoopNative(int totalTics) => totalTics > 0;

    public static bool ShouldUpdateSoundsAfterLoopNative(int totalTics) => totalTics > 0;

    public static bool ShouldRunInvasionMirrorVisualFrameNative(bool invasionAuthority) => !invasionAuthority;

    public static int InvasionMirrorVisualStepsNative(int worldStepsUsed, bool netgame, bool invasionAuthority) =>
        !invasionAuthority && netgame && worldStepsUsed > 0 ? Math.Clamp(worldStepsUsed, 1, 32) : 1;

    public static int SoundUpdateWorldTimerNative(int localWorldTimer, int worldTimerAtStart) =>
        localWorldTimer - Math.Min(localWorldTimer, worldTimerAtStart);

    public static bool ShouldVerifyPredictedEntitiesAfterLoopNative(int totalTics) => totalTics > 0;
}
