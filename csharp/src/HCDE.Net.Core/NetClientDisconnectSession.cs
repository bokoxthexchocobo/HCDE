namespace HCDE.Net.Core;

/// <summary>
/// Idempotent client disconnect and <c>Net_ResetClientState</c> parity for live peer bookkeeping.
/// </summary>
public sealed class NetClientDisconnectSession
{
    public const int MaxPlayers = 8;

    private readonly HashSet<int> _inGameClients = new();
    private ulong _mutedClients;
    private ulong _cutsceneReady;
    private ulong _levelStartAck;
    private readonly bool[] _playingInGame = new bool[MaxPlayers];
    private readonly LateJoinRuntimeSession _lateJoin = new();
    private readonly LivePeerNetRegistry _registry;

    public NetClientDisconnectSession(LivePeerNetRegistry registry) =>
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));

    public LateJoinRuntimeSession LateJoin => _lateJoin;

    public void MarkInGame(int client)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        _inGameClients.Add(client);
        _playingInGame[client] = true;
    }

    public bool IsInGame(int client) => _inGameClients.Contains(client);

    public bool TryDisconnect(int clientNum, Action<int>? clearPresentationEcho = null)
    {
        if (clientNum < 0 || clientNum >= MaxPlayers)
            return false;

        if (!_inGameClients.Contains(clientNum))
            return false;

        _inGameClients.Remove(clientNum);
        _lateJoin.ClearRuntimeClientJoinState(clientNum);
        var mask = ~(1ul << clientNum);
        _mutedClients &= mask;
        _cutsceneReady &= mask;
        _levelStartAck &= mask;
        _playingInGame[clientNum] = false;
        clearPresentationEcho?.Invoke(clientNum);
        ResetClientState(clientNum);
        return true;
    }

    public void ResetClientState(int clientNum)
    {
        if (clientNum < 0 || clientNum >= MaxPlayers)
            return;

        _lateJoin.ClearRuntimeClientJoinState(clientNum);
        _registry.ResetClient(clientNum);
        ActorDeltaSendCursor[clientNum] = 0;
        InvasionActorDeltaSendCursor[clientNum] = 0;
        CoopRepairCooldownUntilTic[clientNum] = 0;
    }

    public uint[] ActorDeltaSendCursor { get; } = new uint[MaxPlayers];
    public uint[] InvasionActorDeltaSendCursor { get; } = new uint[MaxPlayers];
    public int[] CoopRepairCooldownUntilTic { get; } = new int[MaxPlayers];

    public IReadOnlyList<CutsceneParticipant> BuildCutsceneParticipants(int authoritySlot, bool authorityReserved)
    {
        var list = new List<CutsceneParticipant>(MaxPlayers);
        for (var slot = 0; slot < MaxPlayers; slot++)
        {
            var reserved = slot == authoritySlot && authorityReserved;
            list.Add(new CutsceneParticipant(
                slot,
                _playingInGame[slot],
                reserved,
                _lateJoin.IsLateJoinSyncPending(slot),
                false));
        }

        return list;
    }
}
