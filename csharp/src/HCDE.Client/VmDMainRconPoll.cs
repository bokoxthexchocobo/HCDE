namespace HCDE.Client;

/// <summary><c>HCDERconPollListener</c> from <c>d_net_rcon.cpp</c> (423–436).</summary>
public static class VmDMainRconPoll
{
    public const int StateListening = 1;

    public static bool ShouldRestartListenerWhenPortChanges(
        int state,
        bool shouldAcceptCommands,
        int effectivePort,
        int boundPort) =>
        state == StateListening && (!shouldAcceptCommands || effectivePort != boundPort);

    public static bool ShouldStartListenerWhenNotListening(int state) => state != StateListening;

    public static bool ShouldSkipDrainWhenNotListening(int state) => state != StateListening;

    public static bool ShouldRejectClientWhenNoSlots(bool foundSlot) => !foundSlot;

    public static bool ShouldDisconnectOnZeroRead(int received) => received == 0;

    public static bool ShouldIgnoreNegativeRead(int received) => received < 0;
}
