namespace HCDE.Net.Core;

/// <summary><c>TryRunTics</c> world tick loop and client-side tail (<c>d_net.cpp</c> 7981–8268).</summary>
public static class VmDNetTryRunTicsWorldLoop
{
    public static bool ShouldUnpredictBeforeWorldLoopNative(bool runTicsPositive) => runTicsPositive;

    public static int SelectLagStateBeforeWorldNative(int clientTic, int startCommand) =>
        clientTic > startCommand ? 0 : 1;

    public static bool ShouldStabilizeTickNative(bool stabilizeFlag) => stabilizeFlag;

    public static bool ShouldBreakOnCommandsResetNative(bool commandsReset) => commandsReset;

    public static int MirrorVisualWorldStepsNative(int worldStepsUsed, bool netgame, bool invasionAuthority) =>
        !invasionAuthority && netgame && worldStepsUsed > 0 ? Math.Clamp(worldStepsUsed, 1, 32) : 1;

    public static bool ShouldRunCoopInterpolationNative(bool authority, bool netgame, bool deathmatch, int gameType) =>
        !authority && netgame && !deathmatch && gameType != 4;

    public static int ClientSideLogicIterationsNative(int totalTics) => totalTics;

    public static int SoundWorldTimerNative(int localWorldTimer, int worldTimerAtStart) =>
        localWorldTimer - Math.Min(localWorldTimer, worldTimerAtStart);
}
