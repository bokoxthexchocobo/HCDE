using System.Collections.Generic;

namespace HCDE.Net.Core;

public static class VmDNetLateJoinCutscene
{
    public const int InvalidClient = -1;

    public static bool ClientInRange(int client, int maxPlayers) =>
        client >= 0 && client < maxPlayers;

    public static bool SetLateJoinSyncPendingNative(
        int client,
        int maxPlayers,
        int targetSequence,
        int targetConsistency,
        int enterTic,
        out int clampedSequence,
        out int clampedConsistency,
        out bool shouldSetBitmask)
    {
        clampedSequence = -1;
        clampedConsistency = -1;
        shouldSetBitmask = false;
        if (!ClientInRange(client, maxPlayers))
            return false;
        clampedSequence = targetSequence < -1 ? -1 : targetSequence;
        clampedConsistency = targetConsistency < -1 ? -1 : targetConsistency;
        shouldSetBitmask = true;
        _ = enterTic;
        return true;
    }

    public static bool ClearLateJoinSyncPendingNative(
        int client,
        int maxPlayers,
        ulong pendingMask,
        int gametic,
        int ticRate,
        int priorConsistencyGraceUntilTic,
        out bool shouldClearBitmask,
        out int consistencyGraceUntilTic)
    {
        shouldClearBitmask = false;
        consistencyGraceUntilTic = priorConsistencyGraceUntilTic;
        if (!ClientInRange(client, maxPlayers))
            return false;
        if ((pendingMask & (1ul << client)) == 0)
            return false;
        shouldClearBitmask = true;
        var graceFloor = gametic + ticRate;
        consistencyGraceUntilTic = int.Max(priorConsistencyGraceUntilTic, graceFloor);
        return true;
    }

    public static bool BeginRuntimeBootstrapNative(
        bool localAuthority,
        int client,
        int maxPlayers,
        bool isAuthoritySlot,
        int currentSequence,
        int currentConsistency,
        int replayWindow,
        int resendSequenceFrom,
        int resendConsistencyFrom,
        out int newResendSequenceFrom,
        out int newResendConsistencyFrom,
        out bool shouldSetLateJoinPending)
    {
        newResendSequenceFrom = resendSequenceFrom;
        newResendConsistencyFrom = resendConsistencyFrom;
        shouldSetLateJoinPending = false;
        if (!localAuthority || !ClientInRange(client, maxPlayers) || isAuthoritySlot)
            return false;
        newResendSequenceFrom = int.Min(resendSequenceFrom >= 0 ? resendSequenceFrom : currentSequence, int.Max(currentSequence - replayWindow, 0));
        newResendConsistencyFrom = int.Min(resendConsistencyFrom >= 0 ? resendConsistencyFrom : currentConsistency, int.Max(currentConsistency - replayWindow, 0));
        shouldSetLateJoinPending = true;
        return true;
    }

    public static bool RequestRuntimeResyncNative(
        bool localAuthority,
        int client,
        int maxPlayers,
        bool isAuthoritySlot,
        bool inGame,
        int sequenceAck,
        int consistencyAck,
        out int resendSequenceFrom,
        out int resendConsistencyFrom,
        out bool setPlayerWaiting,
        out bool shouldSetLateJoinPending)
    {
        resendSequenceFrom = 0;
        resendConsistencyFrom = 0;
        setPlayerWaiting = false;
        shouldSetLateJoinPending = false;
        if (!localAuthority || !ClientInRange(client, maxPlayers) || isAuthoritySlot || !inGame)
            return false;
        resendSequenceFrom = int.Max(sequenceAck + 1, 0);
        resendConsistencyFrom = int.Max(consistencyAck + 1, 0);
        setPlayerWaiting = true;
        shouldSetLateJoinPending = true;
        return true;
    }

    public static int GetCutsceneReadyHostNative(int authoritySlot, bool authorityReady, IEnumerable<int> networkClients, Func<int, bool> isReadyParticipant)
    {
        if (authorityReady && isReadyParticipant(authoritySlot))
            return authoritySlot;
        foreach (var client in networkClients)
        {
            if (isReadyParticipant(client))
                return client;
        }

        return InvalidClient;
    }

    public static bool IsPlayerReadyNative(
        bool demoPlayback,
        int cutsceneReadyTypeVote,
        bool isReadyParticipant,
        bool hasCutsceneRunner,
        int skipTypeUnskippable,
        int skipTypeVote,
        bool hasBot,
        ulong cutsceneReadyMask,
        int player) =>
        !demoPlayback && cutsceneReadyTypeVote == skipTypeVote && isReadyParticipant &&
        !(hasCutsceneRunner && skipTypeUnskippable == skipTypeVote) &&
        (hasBot || (cutsceneReadyMask & (1ul << player)) != 0);

    public static int CountCutsceneReadyHumanParticipantsNative(IEnumerable<int> clients, Func<int, bool> isParticipant, Func<int, bool> hasBot)
    {
        var count = 0;
        foreach (var client in clients)
        {
            if (!isParticipant(client))
                continue;
            if (!hasBot(client))
                count++;
        }

        return count;
    }

    public static bool PlayerReadiedUpToggleNative(
        bool netgame,
        bool demoPlayback,
        int player,
        int maxPlayers,
        int gametic,
        int lastToggleTic,
        int debounceTics,
        int humanParticipants,
        bool currentlyReady,
        out bool forceSetReady,
        out bool toggleOff,
        out bool toggleOn)
    {
        forceSetReady = false;
        toggleOff = false;
        toggleOn = false;
        if (!netgame || demoPlayback || !ClientInRange(player, maxPlayers))
            return false;
        if (gametic - lastToggleTic < debounceTics)
            return false;
        if (humanParticipants <= 1)
        {
            forceSetReady = true;
            return true;
        }

        if (currentlyReady)
            toggleOff = true;
        else
            toggleOn = true;
        return true;
    }
}
