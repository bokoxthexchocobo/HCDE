namespace HCDE.Net.Core;

/// <summary>Runtime client admission from <c>d_net.cpp</c> (5202–5347, 5395–5401).</summary>
public static class VmDNetRuntimeClientConnect
{
    public static bool ClientConnectingShouldReturnEarly(
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

    public static bool ShouldKeepRetransmitOnly(bool lateJoinPending, int sequenceAck) =>
        lateJoinPending && sequenceAck >= 0;

    /// <summary>Mirrors late-join retransmit/resend seeding in <c>ClientConnecting</c> (5233–5248).</summary>
    public static (bool keepRetransmitOnly, int resendSequenceFrom, int resendConsistencyFrom, bool clearJoinStateFirst) PrepareLateJoinRetransmit(
        bool lateJoinPending,
        int sequenceAck,
        int currentSequence,
        int currentConsistency,
        int existingResendSequence,
        int existingResendConsistency,
        int replayWindow)
    {
        if (ShouldKeepRetransmitOnly(lateJoinPending, sequenceAck))
            return (true, existingResendSequence, existingResendConsistency, false);

        var clearJoin = lateJoinPending;
        var (seqFrom, conFrom) = ResendFromTargets(
            currentSequence,
            currentConsistency,
            existingResendSequence,
            existingResendConsistency,
            replayWindow);
        return (false, seqFrom, conFrom, clearJoin);
    }

    public static (int seqFrom, int conFrom) ResendFromTargets(
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

    public static bool ShouldSetPlayerEnter(bool reserved, int playerState, bool hasMobj) =>
        !reserved && (playerState is 0 or 2 || !hasMobj);

    public static int AppliedSequenceAfterSeed(int currentSequence, int appliedSequence) =>
        appliedSequence < 0 ? Math.Max(currentSequence - 1, 0) : appliedSequence;

    public static void ClearRuntimeJoinStateNative(
        ulong lateJoinPendingMask,
        int clientNum,
        Action clearPendingMaskAndTargets,
        Action clearBaselineRepair)
    {
        if (((lateJoinPendingMask >> clientNum) & 1ul) != 0ul)
            clearPendingMaskAndTargets();
        clearBaselineRepair();
    }

    public static bool EnsureRuntimeSlotAddsClient(bool wasKnown, int client, List<int> networkClients)
    {
        if (wasKnown)
            return false;
        networkClients.Add(client);
        return true;
    }

    public static bool ShouldEnterOnSlotActivation(bool reserved, bool wasKnown, int playerState, bool hasMobj) =>
        !reserved && !wasKnown && ShouldSetPlayerEnter(reserved, playerState, hasMobj);

    public static bool HGetPacketRejectsLengthMismatch(ulong netBufferLength, ulong expectedSize) =>
        netBufferLength != expectedSize;

}
