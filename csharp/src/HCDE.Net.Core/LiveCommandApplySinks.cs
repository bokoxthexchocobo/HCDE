namespace HCDE.Net.Core;

public interface IServerSnapshotCommandSink
{
    bool ApplyCommand(byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords);
}

public interface IClientInputCommandSink
{
    bool ApplyCommand(int clientSlot, byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords);
}

public readonly struct ServerSnapshotApplyResult
{
    public ServerSnapshotApplyResult(
        bool idempotent,
        int commandsApplied,
        bool missingSequence,
        bool missingConsistency)
    {
        Idempotent = idempotent;
        CommandsApplied = commandsApplied;
        MissingSequence = missingSequence;
        MissingConsistency = missingConsistency;
    }

    public bool Idempotent { get; }
    public int CommandsApplied { get; }
    public bool MissingSequence { get; }
    public bool MissingConsistency { get; }
}

public readonly struct ClientInputApplyResult
{
    public ClientInputApplyResult(int commandsApplied, bool missingSequence, bool missingConsistency)
    {
        CommandsApplied = commandsApplied;
        MissingSequence = missingSequence;
        MissingConsistency = missingConsistency;
    }

    public int CommandsApplied { get; }
    public bool MissingSequence { get; }
    public bool MissingConsistency { get; }
}
