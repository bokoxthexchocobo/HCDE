namespace HCDE.Client;

/// <summary><c>HCDERconPollListener</c> and <c>HCDERconDrainIngress</c> from <c>d_net_rcon.cpp</c>.</summary>
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

    public static bool ShouldSendTooManyClientsError(bool foundSlot) => !foundSlot;

    public static bool ShouldSetNonBlockingOnAccept(bool accepted) => accepted;

    public static bool ShouldResetClientAuthOnAccept(bool accepted) => accepted;

    public static bool ShouldClearClientBufferOnAccept(bool accepted) => accepted;

    public static bool ShouldFormatHelloWithNonce(bool accepted) => accepted;
}
