using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public sealed class ServerSnapshotCommandRecord
{
    public byte CommandOffset { get; init; }
    public ReadOnlyMemory<byte> EventRecords { get; init; } = EventRecordsCodec.CreateEmptyBlock();
    public UserCmd Command { get; init; } = UserCmd.Zero;
}

public sealed class ServerSnapshotPlayerRecord
{
    public byte PlayerNum { get; init; }
    public ushort AverageLatency { get; init; }
    public IReadOnlyList<ushort> ConsistencyValues { get; init; } = Array.Empty<ushort>();
    public IReadOnlyList<ServerSnapshotCommandRecord> Commands { get; init; } = Array.Empty<ServerSnapshotCommandRecord>();
}

/// <summary>
/// Encodes/decodes the HCSR player-record section of a server snapshot body.
/// Trailing chunks (HCDW, actor delta, invasion, echo) are appended separately in C++.
/// </summary>
public static class ServerSnapshotBodyCodec
{
    public static int WritePlayerRecords(Span<byte> body, IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        if (body.Length < LiveConstants.ServerSnapshotRecordsHeaderSize)
            return 0;

        if (ServerSnapshotRecordsHeader.Write(body, new ServerSnapshotRecordsHeader((byte)players.Count)) == 0)
            return 0;

        var cursor = LiveConstants.ServerSnapshotRecordsHeaderSize;
        foreach (var player in players)
        {
            if (cursor + 5 > body.Length)
                return 0;

            body[cursor++] = player.PlayerNum;
            BinaryPrimitives.WriteUInt16BigEndian(body[cursor..], player.AverageLatency);
            cursor += 2;
            body[cursor++] = (byte)player.ConsistencyValues.Count;
            body[cursor++] = (byte)player.Commands.Count;

            for (var r = 0; r < player.ConsistencyValues.Count; r++)
            {
                if (cursor + 3 > body.Length)
                    return 0;
                body[cursor++] = (byte)r;
                BinaryPrimitives.WriteUInt16BigEndian(body[cursor..], player.ConsistencyValues[r]);
                cursor += 2;
            }

            foreach (var command in player.Commands)
            {
                if (cursor + 1 + command.EventRecords.Length + LiveConstants.ExplicitUserCmdBytes > body.Length)
                    return 0;

                body[cursor++] = command.CommandOffset;
                command.EventRecords.Span.CopyTo(body[cursor..]);
                cursor += command.EventRecords.Length;
                if (UserCmdCodec.Write(body, ref cursor, command.Command) == 0)
                    return 0;
            }
        }

        return cursor;
    }

    public static bool TryReadPlayerRecords(
        ReadOnlySpan<byte> body,
        byte expectedConsistencyTics,
        byte expectedCommandTics,
        out IReadOnlyList<ServerSnapshotPlayerRecord> players,
        out int bytesConsumed,
        out string? rejectReason)
    {
        players = Array.Empty<ServerSnapshotPlayerRecord>();
        bytesConsumed = 0;
        rejectReason = null;

        if (!ServerSnapshotRecordsHeader.TryRead(body, out var recordsHeader))
        {
            rejectReason = "missing-records";
            return false;
        }

        var cursor = LiveConstants.ServerSnapshotRecordsHeaderSize;
        var parsed = new List<ServerSnapshotPlayerRecord>(recordsHeader.PlayerCount);
        var playersSeen = 0UL;

        for (var p = 0; p < recordsHeader.PlayerCount; p++)
        {
            if (body.Length - cursor < 5)
            {
                rejectReason = "server-snapshot-record-truncated";
                return false;
            }

            var playerNum = body[cursor++];
            if (playerNum >= NetConstants.MaxPlayers)
            {
                rejectReason = "server-snapshot-invalid-player-record";
                return false;
            }

            var averageLatency = BinaryPrimitives.ReadUInt16BigEndian(body[cursor..]);
            cursor += 2;
            var consistencyCount = body[cursor++];
            var commandCount = body[cursor++];
            if (consistencyCount != expectedConsistencyTics || commandCount != expectedCommandTics)
            {
                rejectReason = "server-snapshot-invalid-player-record";
                return false;
            }

            var playerMask = 1UL << playerNum;
            if ((playersSeen & playerMask) != 0)
            {
                rejectReason = "server-snapshot-duplicate-player-record";
                return false;
            }

            playersSeen |= playerMask;

            var consistencies = new List<ushort>(consistencyCount);
            var consistencyOffsets = 0UL;
            for (var r = 0; r < consistencyCount; r++)
            {
                if (body.Length - cursor < 3 || body[cursor] >= consistencyCount)
                {
                    rejectReason = "server-snapshot-consistency-truncated";
                    return false;
                }

                var offsetMask = 1UL << body[cursor];
                if ((consistencyOffsets & offsetMask) != 0)
                {
                    rejectReason = "server-snapshot-duplicate-consistency-offset";
                    return false;
                }

                consistencyOffsets |= offsetMask;
                cursor += 1;
                consistencies.Add(BinaryPrimitives.ReadUInt16BigEndian(body[cursor..]));
                cursor += 2;
            }

            var commands = new List<ServerSnapshotCommandRecord>(commandCount);
            var commandOffsets = 0UL;
            for (var t = 0; t < commandCount; t++)
            {
                if (body.Length - cursor < 3 + LiveConstants.ExplicitUserCmdBytes)
                {
                    rejectReason = "server-snapshot-command-truncated";
                    return false;
                }

                var commandOffset = body[cursor++];
                if (commandOffset >= commandCount)
                {
                    rejectReason = "server-snapshot-command-offset-out-of-range";
                    return false;
                }

                var commandMask = 1UL << commandOffset;
                if ((commandOffsets & commandMask) != 0)
                {
                    rejectReason = "server-snapshot-duplicate-command-offset";
                    return false;
                }

                commandOffsets |= commandMask;
                var eventCursor = cursor;
                if (!EventRecordsCodec.TryRead(body, ref eventCursor, out _, out rejectReason))
                    return false;

                var eventBytes = body[cursor..eventCursor].ToArray();
                cursor = eventCursor;
                if (!UserCmdCodec.TryRead(body, ref cursor, out var userCmd))
                {
                    rejectReason = "server-snapshot-usercmd-invalid";
                    return false;
                }

                commands.Add(new ServerSnapshotCommandRecord
                {
                    CommandOffset = commandOffset,
                    EventRecords = eventBytes,
                    Command = userCmd,
                });
            }

            parsed.Add(new ServerSnapshotPlayerRecord
            {
                PlayerNum = playerNum,
                AverageLatency = averageLatency,
                ConsistencyValues = consistencies,
                Commands = commands,
            });
        }

        bytesConsumed = cursor;
        players = parsed;
        return true;
    }
}
