namespace HCDE.Net.Core;

public interface IServerSnapshotCommandSink
{
    bool ApplyCommand(byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords);
}

public interface IClientInputCommandSink
{
    bool ApplyCommand(int clientSlot, byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords);
}

public interface IClientInputViewTicSink
{
    bool TryApplyClientViewTic(int clientSlot, uint sequenceAck);
}

public readonly struct ServerSnapshotApplyResult
{
    public ServerSnapshotApplyResult(
        bool idempotent,
        int commandsApplied,
        bool missingSequence,
        bool missingConsistency,
        bool snapshotGapResynced = false)
    {
        Idempotent = idempotent;
        CommandsApplied = commandsApplied;
        MissingSequence = missingSequence;
        MissingConsistency = missingConsistency;
        SnapshotGapResynced = snapshotGapResynced;
    }

    public bool Idempotent { get; }
    public int CommandsApplied { get; }
    public bool MissingSequence { get; }
    public bool MissingConsistency { get; }
    public bool SnapshotGapResynced { get; }
}

public readonly struct ClientInputApplyResult
{
    public ClientInputApplyResult(
        int commandsApplied,
        bool missingSequence,
        bool missingConsistency,
        bool inputGapResynced = false,
        bool viewTicRecorded = false)
    {
        CommandsApplied = commandsApplied;
        MissingSequence = missingSequence;
        MissingConsistency = missingConsistency;
        InputGapResynced = inputGapResynced;
        ViewTicRecorded = viewTicRecorded;
    }

    public int CommandsApplied { get; }
    public bool MissingSequence { get; }
    public bool MissingConsistency { get; }
    public bool InputGapResynced { get; }
    public bool ViewTicRecorded { get; }
}
