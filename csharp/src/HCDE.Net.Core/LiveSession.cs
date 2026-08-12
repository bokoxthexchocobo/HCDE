using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public sealed class LiveGuestSession
{
    private readonly LiveControlEndpoint _control;
    private readonly LiveGameplayEndpoint _gameplay;
    private readonly NetworkEndpoint _authorityEndpoint;
    private readonly LivePeerRoutingState _routing;
    private byte _roomId;
    private uint _gameTic;

    public LiveGuestSession(
        UdpTransport transport,
        ReadOnlySpan<byte> gameId,
        NetworkEndpoint authorityEndpoint,
        int guestPlayerSlot,
        int authoritySlot,
        int maxClients)
    {
        _control = new LiveControlEndpoint(transport, gameId);
        _gameplay = new LiveGameplayEndpoint(transport, gameId);
        _authorityEndpoint = authorityEndpoint;
        _routing = new LivePeerRoutingState(
            consolePlayer: guestPlayerSlot,
            maxClients: maxClients,
            authoritySlot: authoritySlot,
            isLocalAuthority: false,
            usesHcdeService: true);
    }

    public void Pump(ulong nowMs, byte roomId = 0)
    {
        _roomId = roomId;
        _gameTic++;

        if (_routing.ShouldSendControlTo(_routing.AuthoritySlot))
        {
            _control.TrySendScheduledControl(
                nowMs,
                _authorityEndpoint,
                new LiveControlBasePayload(_gameTic, (byte)_routing.ConsolePlayer, (byte)_routing.MaxClients));
        }

        if (_routing.ShouldSendClientInputTo(_routing.AuthoritySlot))
            _gameplay.TrySendClientInput(_authorityEndpoint, _roomId, _gameTic, (byte)_routing.ConsolePlayer);
    }

    public bool TryReceiveAuthorityControl(out LiveControlBasePayload basePayload) =>
        _control.TryReceiveControl(_authorityEndpoint, out basePayload, out _, out _);

    public bool TryReceiveServerSnapshot(
        out ServerSnapshotHeader header,
        out IReadOnlyList<ServerSnapshotPlayerRecord> players,
        out ServerSnapshotTailSections? tailSections)
    {
        header = default;
        players = Array.Empty<ServerSnapshotPlayerRecord>();
        tailSections = null;
        if (!_gameplay.TryReceiveGameplay(
                _authorityEndpoint,
                GameplayPayloadKind.ServerSnapshot,
                _roomId,
                out _,
                out _,
                out var nativePayload))
        {
            return false;
        }

        if (!ServerSnapshotHeader.TryRead(nativePayload.Span, out header))
            return false;

        if (!ServerSnapshotBodyCodec.TryReadPlayerRecords(
                nativePayload.Span[LiveConstants.ServerSnapshotHeaderSize..],
                header.ConsistencyTics,
                header.CommandTics,
                out players,
                out var hcsrBytes,
                out _))
        {
            return false;
        }

        var tail = nativePayload.Span[(LiveConstants.ServerSnapshotHeaderSize + hcsrBytes)..];
        if (tail.Length == 0)
            return true;

        if (!ServerSnapshotTailWalker.TryWalk(tail, out var sections, out _, out _))
            return false;

        tailSections = sections;
        return true;
    }

    public bool TryReceiveServerSnapshot(out ServerSnapshotHeader header, out IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        var ok = TryReceiveServerSnapshot(out header, out players, out _);
        return ok;
    }
}

public sealed class LiveAuthoritySession
{
    private readonly LiveControlEndpoint _control;
    private readonly LiveGameplayEndpoint _gameplay;
    private readonly LivePeerRoutingState _routing;
    private byte _roomId;
    private uint _gameTic;

    public LiveAuthoritySession(
        UdpTransport transport,
        ReadOnlySpan<byte> gameId,
        int authoritySlot,
        int maxClients)
    {
        _control = new LiveControlEndpoint(transport, gameId);
        _gameplay = new LiveGameplayEndpoint(transport, gameId);
        _routing = new LivePeerRoutingState(
            consolePlayer: authoritySlot,
            maxClients: maxClients,
            authoritySlot: authoritySlot,
            isLocalAuthority: true,
            usesHcdeService: true);
    }

    public void PumpClient(ulong nowMs, NetworkEndpoint clientEndpoint, int clientSlot, byte roomId = 0)
    {
        _roomId = roomId;
        _gameTic++;

        if (_routing.ShouldSendControlTo(clientSlot))
        {
            _control.TrySendScheduledControl(
                nowMs,
                clientEndpoint,
                new LiveControlBasePayload(_gameTic, (byte)_routing.AuthoritySlot, (byte)_routing.MaxClients),
                new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities));
        }

        if (_routing.ShouldSendServerSnapshotTo(clientSlot))
            _gameplay.TrySendServerSnapshot(clientEndpoint, _roomId, _gameTic, playerNum: (byte)clientSlot);
    }

    public bool TryReceiveClientInput(
        NetworkEndpoint clientEndpoint,
        out ClientInputHeader header,
        out IReadOnlyList<ClientInputPlayerRecord> players,
        int maxAttempts = 8)
    {
        header = default;
        players = Array.Empty<ClientInputPlayerRecord>();
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!_gameplay.TryReceiveGameplay(
                    clientEndpoint,
                    GameplayPayloadKind.ClientInputs,
                    _roomId,
                    out var liveHeader,
                    out _,
                    out var nativePayload))
            {
                continue;
            }

            if (liveHeader.MessageType != LiveMessageType.ClientCommands)
                continue;

            if (!ClientInputHeader.TryRead(nativePayload.Span, out header))
                continue;

            if (ClientInputBodyCodec.TryRead(
                    nativePayload.Span[LiveConstants.ClientInputHeaderSize..],
                    header.ConsistencyTics,
                    header.CommandTics,
                    out players,
                    out _))
            {
                return true;
            }
        }

        return false;
    }
}
