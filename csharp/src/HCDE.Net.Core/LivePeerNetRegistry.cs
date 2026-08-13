namespace HCDE.Net.Core;

public sealed class LivePlayerNetState
{
    public int CurrentSequence { get; set; }
    public int SequenceAck { get; set; }
    public int CurrentNetConsistency { get; set; }
    public int ConsistencyAck { get; set; }
    public byte StabilityBuffer { get; set; }
    public uint LastAppliedSnapshotGameTic { get; set; }
    public long SnapshotGapStallMs { get; set; } = -1;
    public int InputGapStallTic { get; set; } = -1;
}

public sealed class LivePeerNetRegistry
{
    private readonly LivePlayerNetState[] _states;

    public LivePeerNetRegistry(int maxClients)
    {
        if (maxClients <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxClients));

        _states = new LivePlayerNetState[maxClients];
        for (var slot = 0; slot < maxClients; slot++)
            _states[slot] = new LivePlayerNetState();
    }

    public int MaxClients => _states.Length;

    public LivePlayerNetState this[int slot] => _states[slot];

    public LivePlayerNetState GetOrCreate(int slot)
    {
        if (slot < 0 || slot >= _states.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));

        return _states[slot];
    }

    public void ResetClient(int slot)
    {
        if (slot < 0 || slot >= _states.Length)
            return;

        _states[slot] = new LivePlayerNetState();
    }
}
