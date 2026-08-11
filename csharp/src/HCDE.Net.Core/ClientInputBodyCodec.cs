using System.Buffers.Binary;

namespace HCDE.Net.Core;

public sealed class ClientInputCommandRecord
{
    public byte CommandOffset { get; init; }
    public ReadOnlyMemory<byte> EventRecords { get; init; } = EventRecordsCodec.CreateEmptyBlock();
    public UserCmd Command { get; init; } = UserCmd.Zero;
}

public sealed class ClientInputPlayerRecord
{
    public byte PlayerNum { get; init; }
    public IReadOnlyList<ushort> ConsistencyValues { get; init; } = Array.Empty<ushort>();
    public IReadOnlyList<ClientInputCommandRecord> Commands { get; init; } = Array.Empty<ClientInputCommandRecord>();
}

public static class ClientInputBodyCodec
{
    public static int Write(Span<byte> body, IReadOnlyList<ClientInputPlayerRecord> players)
    {
        if (body.Length < LiveConstants.ClientInputRecordsHeaderSize)
            return 0;

        var cursor = LiveConstants.ClientInputRecordsHeaderSize;
        if (ClientInputRecordsHeader.Write(body, new ClientInputRecordsHeader((byte)players.Count)) == 0)
            return 0;

        foreach (var player in players)
        {
            if (cursor + 3 > body.Length)
                return 0;

            body[cursor++] = player.PlayerNum;
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

    public static bool TryRead(
        ReadOnlySpan<byte> body,
        byte expectedConsistencyTics,
        byte expectedCommandTics,
        out IReadOnlyList<ClientInputPlayerRecord> players,
        out string? rejectReason)
    {
        players = Array.Empty<ClientInputPlayerRecord>();
        rejectReason = null;

        if (!ClientInputRecordsHeader.TryRead(body, out var recordsHeader))
        {
            rejectReason = "missing-records";
            return false;
        }

        var cursor = LiveConstants.ClientInputRecordsHeaderSize;
        var parsed = new List<ClientInputPlayerRecord>(recordsHeader.PlayerCount);
        var playersSeen = 0UL;

        for (var p = 0; p < recordsHeader.PlayerCount; p++)
        {
            if (body.Length - cursor < 3)
            {
                rejectReason = "client-input-record-truncated";
                return false;
            }

            var playerNum = body[cursor++];
            var consistencyCount = body[cursor++];
            var commandCount = body[cursor++];
            if (consistencyCount != expectedConsistencyTics || commandCount != expectedCommandTics)
            {
                rejectReason = "client-input-invalid-player-record";
                return false;
            }

            var playerMask = 1UL << playerNum;
            if ((playersSeen & playerMask) != 0)
            {
                rejectReason = "client-input-duplicate-player-record";
                return false;
            }

            playersSeen |= playerMask;

            var consistencies = new List<ushort>(consistencyCount);
            var consistencyOffsets = 0UL;
            for (var r = 0; r < consistencyCount; r++)
            {
                if (body.Length - cursor < 3 || body[cursor] >= consistencyCount)
                {
                    rejectReason = "client-input-consistency-truncated";
                    return false;
                }

                var offsetMask = 1UL << body[cursor];
                if ((consistencyOffsets & offsetMask) != 0)
                {
                    rejectReason = "client-input-duplicate-consistency-offset";
                    return false;
                }

                consistencyOffsets |= offsetMask;
                cursor += 1;
                consistencies.Add(BinaryPrimitives.ReadUInt16BigEndian(body[cursor..]));
                cursor += 2;
            }

            var commands = new List<ClientInputCommandRecord>(commandCount);
            var commandOffsets = 0UL;
            for (var t = 0; t < commandCount; t++)
            {
                if (body.Length - cursor < 3 + LiveConstants.ExplicitUserCmdBytes)
                {
                    rejectReason = "client-input-command-truncated";
                    return false;
                }

                var commandOffset = body[cursor++];
                if (commandOffset >= commandCount)
                {
                    rejectReason = "client-input-command-offset-out-of-range";
                    return false;
                }

                var commandMask = 1UL << commandOffset;
                if ((commandOffsets & commandMask) != 0)
                {
                    rejectReason = "client-input-duplicate-command-offset";
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
                    rejectReason = "client-input-usercmd-invalid";
                    return false;
                }

                commands.Add(new ClientInputCommandRecord
                {
                    CommandOffset = commandOffset,
                    EventRecords = eventBytes,
                    Command = userCmd,
                });
            }

            parsed.Add(new ClientInputPlayerRecord
            {
                PlayerNum = playerNum,
                ConsistencyValues = consistencies,
                Commands = commands,
            });
        }

        if (cursor != body.Length)
        {
            rejectReason = "client-input-body-trailing-bytes";
            return false;
        }

        players = parsed;
        return true;
    }
}
