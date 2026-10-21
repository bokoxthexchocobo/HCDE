namespace HCDE.Net.Core;

/// <summary>Runtime client admission from <c>d_net.cpp</c> (5202–5401).</summary>
public static class VmDNetRuntimeClientConnect
{
    public static bool ShouldIgnoreClientConnecting(
        bool localAuthority,
        int client,
        int maxPlayers,
        bool inGame,
        bool authoritySlot,
        bool setupInProgress) =>
        !localAuthority
        || client < 0
        || client >= maxPlayers
        || !inGame
        || authoritySlot
        || setupInProgress;

    public static bool ShouldKeepLateJoinReplayPressure(bool lateJoinPending, int sequenceAck) =>
        lateJoinPending && sequenceAck >= 0;

    public static (int resendSequenceFrom, int resendConsistencyFrom) LateJoinReplayWindow(
        int currentSequence,
        int currentConsistency,
        int existingResendSequence,
        int existingResendConsistency,
        int replayWindow)
    {
        var seqFrom = Math.Min(existingResendSequence >= 0 ? existingResendSequence : currentSequence,
            Math.Max(currentSequence - replayWindow, 0));
        var conFrom = Math.Min(existingResendConsistency >= 0 ? existingResendConsistency : currentConsistency,
            Math.Max(currentConsistency - replayWindow, 0));
        return (seqFrom, conFrom);
    }

    public static bool ShouldMarkPlayerEnter(bool reserved, int playerState, bool hasMobj) =>
        !reserved && (playerState is 0 or 2 || !hasMobj);

    public static int SeedAppliedSequence(int currentSequence, int appliedSequence) =>
        appliedSequence < 0 ? Math.Max(currentSequence - 1, 0) : appliedSequence;

    public static bool ShouldSetWaiting(bool currentlyWaiting) => !currentlyWaiting;

    public static void ClientConnectingNative(
        bool ignore,
        bool keepReplayPressure,
        bool lateJoinPending,
        int sequenceAck,
        Action clearJoinState,
        Action armRetransmit,
        Action setLateJoinPending,
        Action setWaiting,
        Action resetWatchdog)
    {
        if (ignore)
            return;
        if (keepReplayPressure)
        {
            armRetransmit();
            return;
        }

        if (lateJoinPending)
            clearJoinState();

        armRetransmit();
        setLateJoinPending();
        if (ShouldSetWaiting(false))
        {
            setWaiting();
            resetWatchdog();
        }
    }

    public static void ClearRuntimeJoinStateNative(
        ulong lateJoinPendingMask,
        int clientNum,
        Action clearLateJoinFields,
        Action clearBaselineRepair)
    {
        if (((lateJoinPendingMask >> clientNum) & 1ul) != 0ul)
            clearLateJoinFields();
        clearBaselineRepair();
    }

    public static bool EnsureRuntimeSlotAddsClient(bool wasKnown, int client, List<int> networkClients)
    {
        if (wasKnown)
            return false;
        networkClients.Add(client);
        return true;
    }

    public static void ResetClientStateNative(
        Action clearJoin,
        Action resetLatency,
        Action resetSequences,
        Action resetTics,
        Action resetPeers,
        Action resetPresentation,
        Action resetInterp,
        Action resetDeltaCursors)
    {
        clearJoin();
        resetLatency();
        resetSequences();
        resetTics();
        resetPeers();
        resetPresentation();
        resetInterp();
        resetDeltaCursors();
    }
}
