namespace HCDE.Net.Core;

/// <summary>Native <c>HCDEActorPriorityQueues</c> cleared on client slot reset in <c>Net_ResetClientState</c>.</summary>
public sealed class NetClientActorPriorityQueue
{
    private readonly Queue<int> _actorIds = new();

    public int Count => _actorIds.Count;

    public void Enqueue(int actorId) => _actorIds.Enqueue(actorId);

    public bool TryDequeue(out int actorId)
    {
        if (_actorIds.Count == 0)
        {
            actorId = 0;
            return false;
        }

        actorId = _actorIds.Dequeue();
        return true;
    }

    public void Clear() => _actorIds.Clear();
}

public sealed class NetClientActorReplicationQueues
{
    private readonly NetClientActorPriorityQueue[] _queues;

    public NetClientActorReplicationQueues(int maxClients = NetClientDisconnectSession.MaxPlayers)
    {
        _queues = new NetClientActorPriorityQueue[maxClients];
        for (var i = 0; i < _queues.Length; i++)
            _queues[i] = new NetClientActorPriorityQueue();
    }

    public NetClientActorPriorityQueue this[int client] => _queues[client];

    public void ResetClient(int clientNum)
    {
        if (clientNum < 0 || clientNum >= _queues.Length)
            return;
        _queues[clientNum].Clear();
    }
}
