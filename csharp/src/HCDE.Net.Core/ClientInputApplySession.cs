namespace HCDE.Net.Core;

public static class ClientInputApplySession
{
    public static bool TryApply(
        ClientInputHeader header,
        IReadOnlyList<ClientInputPlayerRecord> players,
        int clientSlot,
        LivePeerRoutingState routing,
        LivePeerNetRegistry registry,
        IClientInputCommandSink? sink,
        int gameTic,
        out ClientInputApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (!routing.IsLocalAuthority)
        {
            rejectReason = "client-input-apply-not-authority";
            return false;
        }

        if (clientSlot < 0 || clientSlot >= registry.MaxClients)
        {
            rejectReason = "client-input-client-slot-invalid";
            return false;
        }

        var clientState = registry[clientSlot];
        clientState.StabilityBuffer = header.StabilityBuffer;

        var commandsApplied = 0;
        var missingSequence = false;
        var missingConsistency = false;
        var inputGapResynced = false;

        foreach (var player in players)
        {
            if (!IsAuthorizedInputRecord(clientSlot, player.PlayerNum, header.PlayerCount))
            {
                rejectReason = "client-input-unauthorized-player-record";
                return false;
            }

            var playerState = registry.GetOrCreate(player.PlayerNum);
            if (!routing.IsAuthoritySlot(player.PlayerNum) || !routing.IsAuthoritySlot(clientSlot))
                playerState.ConsistencyAck = (int)header.ConsistencyAck;

            if (!TryApplyConsistencies(header, player, playerState, out missingConsistency))
                break;

            if (!TryApplyCommands(
                    header,
                    player,
                    playerState,
                    clientSlot,
                    routing,
                    sink,
                    gameTic,
                    ref commandsApplied,
                    ref inputGapResynced,
                    out missingSequence))
            {
                break;
            }
        }

        clientState.SequenceAck = (int)header.SequenceAck;
        clientState.ConsistencyAck = (int)header.ConsistencyAck;
        result = new ClientInputApplyResult(commandsApplied, missingSequence, missingConsistency, inputGapResynced);
        return true;
    }

    private static bool IsAuthorizedInputRecord(int clientSlot, byte playerNum, byte playerCount) =>
        playerCount <= 1 && playerNum == clientSlot;

    private static bool TryApplyConsistencies(
        ClientInputHeader header,
        ClientInputPlayerRecord player,
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
        ClientInputHeader header,
        ClientInputPlayerRecord player,
        LivePlayerNetState playerState,
        int clientSlot,
        LivePeerRoutingState routing,
        IClientInputCommandSink? sink,
        int gameTic,
        ref int commandsApplied,
        ref bool inputGapResynced,
        out bool missingSequence)
    {
        missingSequence = false;
        var authorityOwnClientInput = routing.IsLocalAuthority
            && !routing.IsAuthoritySlot(clientSlot)
            && player.PlayerNum == clientSlot;

        if (authorityOwnClientInput && header.CommandTics > 0
            && header.BaseSequence > (uint)(playerState.CurrentSequence + 1))
        {
            if (TryResyncInputGap(playerState, header, gameTic))
                inputGapResynced = true;
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

            if (sink != null && !sink.ApplyCommand(clientSlot, player.PlayerNum, sequence, command.Command, command.EventRecords))
                return false;

            commandsApplied++;
            playerState.CurrentSequence = sequence;
            if (authorityOwnClientInput)
                playerState.InputGapStallTic = -1;
        }

        return true;
    }

    private static bool TryResyncInputGap(LivePlayerNetState playerState, ClientInputHeader header, int gameTic)
    {
        if (playerState.InputGapStallTic < 0)
            playerState.InputGapStallTic = gameTic;

        if (gameTic - playerState.InputGapStallTic <= LiveConstants.InputGapResyncTics)
            return false;

        playerState.CurrentSequence = (int)header.BaseSequence - 1;
        playerState.InputGapStallTic = -1;
        return true;
    }

    private static ClientInputCommandRecord? FindCommand(
        IReadOnlyList<ClientInputCommandRecord> commands,
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
