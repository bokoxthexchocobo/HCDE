namespace HCDE.Net.Core;

/// <summary>Managed slice of <c>d_net.cpp</c> <c>Net_ResetCommands</c> room/sequence re-anchor.</summary>
public sealed class NetRoomCommandResetSession
{
    public const int TicDup = 1;

    public byte CurrentRoomId { get; private set; }
    public int PendingAdoptRoomId { get; set; } = -1;
    public bool AuthorityWaiting { get; set; }
    public IReadOnlyList<bool> PlayerWaiting => _playerWaiting;

    private readonly bool[] _playerWaiting;
    private readonly LivePeerNetRegistry _registry;

    public NetRoomCommandResetSession(LivePeerNetRegistry registry, byte initialRoomId = 0)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _playerWaiting = new bool[registry.MaxClients];
        CurrentRoomId = initialRoomId;
    }

    public void ResetCommands(bool midTic, int gameTic, IReadOnlyList<int> activeClientSlots)
    {
        CurrentRoomId++;
        if (PendingAdoptRoomId >= 0)
        {
            CurrentRoomId = (byte)PendingAdoptRoomId;
            PendingAdoptRoomId = -1;
        }

        ClearStaleWaitingFlags();
        var tic = gameTic / TicDup;
        if (!midTic)
            tic--;

        foreach (var slot in activeClientSlots)
        {
            if (slot < 0 || slot >= _registry.MaxClients)
                continue;
            var state = _registry[slot];
            state.StabilityBuffer = 0;
            state.ResendId = CurrentRoomId;
            state.CurrentSequence = tic;
            state.SequenceAck = tic;
            state.AppliedSequence = tic;
            state.ResendSequenceFrom = -1;
            state.ResendConsistencyFrom = -1;
            state.InputGapStallTic = -1;
            state.SnapshotGapStallMs = -1;
        }
    }

    public void ClearStaleWaitingFlags()
    {
        AuthorityWaiting = false;
        Array.Clear(_playerWaiting, 0, _playerWaiting.Length);
    }

    public void SetPlayerWaiting(int slot, bool waiting)
    {
        if (slot >= 0 && slot < _playerWaiting.Length)
            _playerWaiting[slot] = waiting;
    }
}
