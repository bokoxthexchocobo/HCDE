namespace HCDE.Net.Core;

public sealed class LiveSequenceTracker
{
    private uint _rxSequence;
    private uint _rxControlSequence;
    private uint _rxClientCommandSequence;
    private uint _rxServerSnapshotSequence;
    private uint _duplicateCount;

    public uint RxSequence => _rxSequence;
    public uint DuplicateCount => _duplicateCount;

    public bool IsFresh(LiveMessageType messageType, uint sequence)
    {
        if (sequence == 0)
            return false;

        var lastForType = GetReceiveSequence(messageType);
        if (sequence <= lastForType)
        {
            _duplicateCount++;
            return false;
        }

        return true;
    }

    public void Accept(LiveMessageType messageType, uint sequence)
    {
        ref var typed = ref GetReceiveSequenceRef(messageType);
        typed = sequence;
        if (sequence > _rxSequence)
            _rxSequence = sequence;
    }

    public uint GetReceiveSequence(LiveMessageType messageType) =>
        messageType switch
        {
            LiveMessageType.Control => _rxControlSequence,
            LiveMessageType.ClientCommands => _rxClientCommandSequence,
            LiveMessageType.ServerSnapshot => _rxServerSnapshotSequence,
            _ => _rxSequence,
        };

    private ref uint GetReceiveSequenceRef(LiveMessageType messageType)
    {
        switch (messageType)
        {
            case LiveMessageType.Control:
                return ref _rxControlSequence;
            case LiveMessageType.ClientCommands:
                return ref _rxClientCommandSequence;
            case LiveMessageType.ServerSnapshot:
                return ref _rxServerSnapshotSequence;
            default:
                return ref _rxSequence;
        }
    }
}
