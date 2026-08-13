using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public static class ServerSnapshotApplySession
{
    public static bool TryApply(
        ServerSnapshotHeader header,
        ReadOnlySpan<byte> quitterPlayerSlots,
        IReadOnlyList<ServerSnapshotPlayerRecord> players,
        uint remoteGameTic,
        int recipientClientSlot,
        LivePeerRoutingState routing,
        LivePeerNetRegistry registry,
        LivePeerSlotTracker? peerSlots,
        IServerSnapshotCommandSink? sink,
        ulong nowMs,
        out ServerSnapshotApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (recipientClientSlot < 0 || recipientClientSlot >= registry.MaxClients)
        {
            rejectReason = "server-snapshot-recipient-invalid";
            return false;
        }

        var recipientState = registry[recipientClientSlot];
        if (!routing.IsLocalAuthority && remoteGameTic <= recipientState.LastAppliedSnapshotGameTic)
        {
            recipientState.SequenceAck = (int)header.SequenceAck;
            recipientState.ConsistencyAck = (int)header.ConsistencyAck;
            result = new ServerSnapshotApplyResult(idempotent: true, commandsApplied: 0, missingSequence: false, missingConsistency: false);
            return true;
        }

        if ((header.ControlFlags & (byte)NetCommandFlags.Quitters) != 0 && quitterPlayerSlots.Length > 0)
            peerSlots?.ApplyQuitterSlots(quitterPlayerSlots);

        if (routing.IsAuthoritySlot(recipientClientSlot))
        {
            // CommandsAhead = header.StabilityBuffer in C++; deferred to playsim.
        }
        else if (routing.IsLocalAuthority)
        {
            recipientState.StabilityBuffer = header.StabilityBuffer;
        }

        var commandsApplied = 0;
        var missingSequence = false;
        var missingConsistency = false;

        foreach (var player in players)
        {
            var playerState = registry.GetOrCreate(player.PlayerNum);
            if (!routing.IsLocalAuthority
                || routing.IsAuthoritySlot(player.PlayerNum)
                || !routing.IsAuthoritySlot(recipientClientSlot))
            {
                playerState.ConsistencyAck = (int)header.ConsistencyAck;
            }

            if (!TryApplyConsistencies(header, player, playerState, out missingConsistency))
                break;

            if (!TryApplyCommands(
                    header,
                    player,
                    playerState,
                    recipientClientSlot,
                    routing,
                    sink,
                    nowMs,
                    ref commandsApplied,
                    out missingSequence))
            {
                break;
            }
        }

        recipientState.LastAppliedSnapshotGameTic = remoteGameTic;
        recipientState.SequenceAck = (int)header.SequenceAck;
        recipientState.ConsistencyAck = (int)header.ConsistencyAck;
        result = new ServerSnapshotApplyResult(false, commandsApplied, missingSequence, missingConsistency);
        return true;
    }

    private static bool TryApplyConsistencies(
        ServerSnapshotHeader header,
        ServerSnapshotPlayerRecord player,
        LivePlayerNetState playerState,
        out bool missingConsistency)
    {
        missingConsistency = false;
        for (var i = 0; i < header.ConsistencyTics; i++)
        {
            var consistencyTic = (int)header.BaseConsistency + i;
            if (consistencyTic <= playerState.CurrentNetConsistency)
                continue;

            if (consistencyTic > playerState.CurrentNetConsistency + 1
                || i >= player.ConsistencyValues.Count
                || player.ConsistencyValues[i] == 0)
            {
                missingConsistency = true;
                return false;
            }

            playerState.CurrentNetConsistency = consistencyTic;
        }

        return true;
    }

    private static bool TryApplyCommands(
        ServerSnapshotHeader header,
        ServerSnapshotPlayerRecord player,
        LivePlayerNetState playerState,
        int recipientClientSlot,
        LivePeerRoutingState routing,
        IServerSnapshotCommandSink? sink,
        ulong nowMs,
        ref int commandsApplied,
        out bool missingSequence)
    {
        missingSequence = false;
        var clientSnapshotGatePlayer = !routing.IsLocalAuthority
            && routing.IsAuthoritySlot(recipientClientSlot)
            && player.PlayerNum == routing.AuthoritySlot;

        if (clientSnapshotGatePlayer && header.CommandTics > 0
            && header.BaseSequence > (uint)(playerState.CurrentSequence + 1))
        {
            TryResyncSnapshotGap(playerState, header, nowMs);
        }

        for (var i = 0; i < header.CommandTics; i++)
        {
            var sequence = (int)header.BaseSequence + i;
            if (sequence <= playerState.CurrentSequence)
                continue;

            var command = FindCommand(player.Commands, (byte)i);
            if (sequence > playerState.CurrentSequence + 1 || command == null)
            {
                missingSequence = true;
                return false;
            }

            if (sink != null && !sink.ApplyCommand(player.PlayerNum, sequence, command.Command, command.EventRecords))
                return false;

            commandsApplied++;
            if (!routing.IsLocalAuthority
                || routing.IsAuthoritySlot(player.PlayerNum)
                || !routing.IsAuthoritySlot(recipientClientSlot))
            {
                playerState.CurrentSequence = sequence;
                if (clientSnapshotGatePlayer)
                    playerState.SnapshotGapStallMs = -1;
            }

            if (!routing.IsLocalAuthority && !routing.IsAuthoritySlot(player.PlayerNum))
                playerState.SequenceAck = sequence;
        }

        return true;
    }

    private static void TryResyncSnapshotGap(LivePlayerNetState playerState, ServerSnapshotHeader header, ulong nowMs)
    {
        if (playerState.SnapshotGapStallMs < 0)
            playerState.SnapshotGapStallMs = (long)nowMs;

        var gapTooWide = header.BaseSequence > (uint)(playerState.CurrentSequence + LiveConstants.SnapshotGapImmediateTics);
        var stallExpired = nowMs - (ulong)playerState.SnapshotGapStallMs > LiveConstants.SnapshotGapResyncMs;
        if (!gapTooWide && !stallExpired)
            return;

        var resyncTo = (int)header.BaseSequence - 1;
        playerState.CurrentSequence = resyncTo;
        var consistencyResyncTo = (int)header.BaseConsistency - 1;
        if (playerState.CurrentNetConsistency < consistencyResyncTo)
            playerState.CurrentNetConsistency = consistencyResyncTo;
        playerState.SnapshotGapStallMs = -1;
    }

    private static ServerSnapshotCommandRecord? FindCommand(
        IReadOnlyList<ServerSnapshotCommandRecord> commands,
        byte commandOffset)
    {
        for (var i = 0; i < commands.Count; i++)
        {
            if (commands[i].CommandOffset == commandOffset)
                return commands[i];
        }

        return null;
    }
}
