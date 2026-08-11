using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct EventRecord
{
    public EventRecord(byte eventType, ReadOnlyMemory<byte> payload)
    {
        EventType = eventType;
        Payload = payload;
    }

    public byte EventType { get; }
    public ReadOnlyMemory<byte> Payload { get; }
}

public static class EventRecordsCodec
{
    public const int EmptyBlockSize = 2;

    public static int WriteEmpty(Span<byte> buffer)
    {
        if (buffer.Length < EmptyBlockSize)
            return 0;

        BinaryPrimitives.WriteUInt16BigEndian(buffer, 0);
        return EmptyBlockSize;
    }

    public static byte[] CreateEmptyBlock()
    {
        var buffer = new byte[EmptyBlockSize];
        WriteEmpty(buffer);
        return buffer;
    }

    public static int Write(Span<byte> buffer, ReadOnlySpan<EventRecord> events)
    {
        if (buffer.Length < EmptyBlockSize)
            return 0;

        var cursor = EmptyBlockSize;
        ushort count = 0;
        foreach (var record in events)
        {
            if (count == ushort.MaxValue || buffer.Length - cursor < 3 + record.Payload.Length)
                return 0;

            buffer[cursor++] = record.EventType;
            BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], (ushort)record.Payload.Length);
            cursor += 2;
            record.Payload.Span.CopyTo(buffer[cursor..]);
            cursor += record.Payload.Length;
            count++;
        }

        BinaryPrimitives.WriteUInt16BigEndian(buffer, count);
        return cursor;
    }

    public static bool TryRead(ReadOnlySpan<byte> buffer, ref int cursor, out ushort eventCount, out string? rejectReason)
    {
        eventCount = 0;
        rejectReason = null;
        if (cursor < 0 || buffer.Length - cursor < EmptyBlockSize)
        {
            rejectReason = "event-records-truncated";
            return false;
        }

        eventCount = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += EmptyBlockSize;
        for (var i = 0; i < eventCount; i++)
        {
            if (buffer.Length - cursor < 3)
            {
                rejectReason = "event-record-header-truncated";
                return false;
            }

            cursor += 1; // event type
            var payloadSize = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
            cursor += 2;
            if (buffer.Length - cursor < payloadSize)
            {
                rejectReason = "event-record-payload-truncated";
                return false;
            }

            cursor += payloadSize;
        }

        return true;
    }
}
