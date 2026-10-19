namespace HCDE.Net.Core;

/// <summary>Late-join sync, runtime bootstrap, and resync from <c>d_net.cpp</c>.</summary>
public sealed class LateJoinRuntimeSession
{
    public const int MaxPlayers = 8;
    public const int MaxSendTics = 3;

    private ulong _pendingMask;
    private readonly int[] _targetSequence = Enumerable.Repeat(-1, MaxPlayers).ToArray();
    private readonly int[] _targetConsistency = Enumerable.Repeat(-1, MaxPlayers).ToArray();
    private readonly int[] _startTic = Enumerable.Repeat(-1, MaxPlayers).ToArray();
    private readonly int[] _consistencyGraceUntilTic = new int[MaxPlayers];
    private readonly bool[] _playerWaiting = new bool[MaxPlayers];

    public bool IsLateJoinSyncPending(int client) =>
        client >= 0 && client < MaxPlayers && (_pendingMask & (1ul << client)) != 0;

    public void SetLateJoinSyncPending(int client, int targetSequence, int targetConsistency, int enterTic, string? reason = null)
    {
        if (client < 0 || client >= MaxPlayers)
            return;

        _pendingMask |= 1ul << client;
        _targetSequence[client] = Math.Max(targetSequence, -1);
        _targetConsistency[client] = Math.Max(targetConsistency, -1);
        _startTic[client] = enterTic;
        BaselineRepairStarted?.Invoke(client, reason ?? "late-join");
    }

    public void ClearLateJoinSyncPending(int client, int gameTic, string? reason = null)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        if ((_pendingMask & (1ul << client)) == 0)
            return;

        _pendingMask &= ~(1ul << client);
        _targetSequence[client] = -1;
        _targetConsistency[client] = -1;
        _startTic[client] = -1;
        _consistencyGraceUntilTic[client] = Math.Max(_consistencyGraceUntilTic[client], gameTic + LiveConstants.TicRate);
        BaselineRepairCompleted?.Invoke(client, reason ?? "late-join-complete");
    }

    public void ClearRuntimeClientJoinState(int clientNum, Action<int, string?>? clearBaselineRepair = null)
    {
        if (clientNum < 0 || clientNum >= MaxPlayers)
            return;

        _pendingMask &= ~(1ul << clientNum);
        _targetSequence[clientNum] = -1;
        _targetConsistency[clientNum] = -1;
        _startTic[clientNum] = -1;
        _playerWaiting[clientNum] = false;
        clearBaselineRepair?.Invoke(clientNum, "runtime-join-reset");
    }

    public void BeginRuntimeBootstrap(
        int client,
        int clientTic,
        int ticDup,
        int currentConsistency,
        int enterTic,
        LivePeerNetRegistry registry,
        string? reason = null)
    {
        if (client < 0 || client >= MaxPlayers)
            return;

        var state = registry[client];
        var currentSequence = Math.Max(clientTic / Math.Max(ticDup, 1), 0);
        var consistency = Math.Max(currentConsistency, 0);
        var replayWindow = Math.Max(MaxSendTics / 2, 1);
        state.ResendSequenceFrom = Math.Min(
            state.ResendSequenceFrom >= 0 ? state.ResendSequenceFrom : currentSequence,
            Math.Max(currentSequence - replayWindow, 0));
        state.ResendConsistencyFrom = Math.Min(
            state.ResendConsistencyFrom >= 0 ? state.ResendConsistencyFrom : consistency,
            Math.Max(consistency - replayWindow, 0));

        SetLateJoinSyncPending(
            client,
            Math.Max(currentSequence - 1, 0),
            Math.Max(consistency - 1, 0),
            enterTic,
            reason ?? "runtime-bootstrap");
    }

    public void RequestRuntimeResync(
        int client,
        int clientTic,
        int ticDup,
        int currentConsistency,
        int enterTic,
        LivePeerNetRegistry registry,
        bool inGame,
        Action? resetAuthorityWaitWatchdog = null,
        string? reason = null)
    {
        if (client < 0 || client >= MaxPlayers || !inGame)
            return;

        var state = registry[client];
        var currentSequence = Math.Max(clientTic / Math.Max(ticDup, 1), 0);
        var consistency = Math.Max(currentConsistency, 0);
        state.ResendSequenceFrom = Math.Max(state.SequenceAck + 1, 0);
        state.ResendConsistencyFrom = Math.Max(state.ConsistencyAck + 1, 0);
        SetLateJoinSyncPending(
            client,
            Math.Max(currentSequence - 1, 0),
            Math.Max(consistency - 1, 0),
            enterTic,
            reason ?? "runtime-resync");
        _playerWaiting[client] = true;
        resetAuthorityWaitWatchdog?.Invoke();
    }

    public bool IsPlayerWaiting(int client) =>
        client >= 0 && client < MaxPlayers && _playerWaiting[client];

    public int TargetSequence(int client) =>
        client >= 0 && client < MaxPlayers ? _targetSequence[client] : -1;

    public event Action<int, string?>? BaselineRepairStarted;
    public event Action<int, string?>? BaselineRepairCompleted;
}
