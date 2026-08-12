namespace HCDE.Net.Core;

public sealed class LivePeerSlotTracker
{
    private readonly bool[] _connected;

    public LivePeerSlotTracker(int maxClients)
    {
        if (maxClients <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxClients));

        _connected = new bool[maxClients];
        for (var slot = 0; slot < maxClients; slot++)
            _connected[slot] = true;
    }

    public int MaxClients => _connected.Length;

    public bool IsConnected(int slot) =>
        slot >= 0 && slot < _connected.Length && _connected[slot];

    public IReadOnlyList<int> DisconnectedSlots
    {
        get
        {
            var slots = new List<int>();
            for (var slot = 0; slot < _connected.Length; slot++)
            {
                if (!_connected[slot])
                    slots.Add(slot);
            }

            return slots;
        }
    }

    public bool MarkDisconnected(int slot)
    {
        if (slot < 0 || slot >= _connected.Length)
            return false;

        if (!_connected[slot])
            return false;

        _connected[slot] = false;
        return true;
    }

    public void ApplyQuitterSlots(ReadOnlySpan<byte> quitterPlayerSlots)
    {
        for (var i = 0; i < quitterPlayerSlots.Length; i++)
            MarkDisconnected(quitterPlayerSlots[i]);
    }
}
