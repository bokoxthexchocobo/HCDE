using System.Buffers.Binary;
using System.Text;

namespace HCDE.Net.Core;

public static class AuthorityEventsCodec
{
    public static bool TryPeek(ReadOnlySpan<byte> chunk) =>
        chunk.Length >= LiveConstants.AuthorityEventsHeaderSize
        && chunk[..4].SequenceEqual(LiveConstants.AuthorityEventsMagic);

    public static int Write(Span<byte> chunk, ReadOnlySpan<AuthorityEventRecord> events)
    {
        if (events.Length == 0 || events.Length > byte.MaxValue)
            return 0;

        var required = LiveConstants.AuthorityEventsHeaderSize;
        foreach (var record in events)
        {
            if (!record.IsValid(out _))
                return 0;

            required += AuthorityEventRecord.MinRecordSize(record.ClassName);
        }

        if (chunk.Length < required)
            return 0;

        if (AuthorityEventsHeader.Write(chunk, new AuthorityEventsHeader(flags: 0, (byte)events.Length)) == 0)
            return 0;

        var cursor = LiveConstants.AuthorityEventsHeaderSize;
        foreach (var record in events)
        {
            var written = WriteRecord(chunk[cursor..], record);
            if (written == 0)
                return 0;

            cursor += written;
        }

        return cursor;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out AuthorityEventsHeader header,
        out AuthorityEventRecord[] records,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        records = Array.Empty<AuthorityEventRecord>();
        bytesConsumed = 0;
        rejectReason = null;

        if (!AuthorityEventsHeader.TryRead(chunk, out header))
        {
            rejectReason = "missing-authority-events-header";
            return false;
        }

        if (header.ProtocolVersion != LiveConstants.AuthorityEventsProtocolVersion)
        {
            rejectReason = "authority-events-version-mismatch";
            return false;
        }

        var cursor = LiveConstants.AuthorityEventsHeaderSize;
        records = new AuthorityEventRecord[header.EventCount];
        for (var i = 0; i < header.EventCount; i++)
        {
            if (!TryReadRecord(chunk[cursor..], out records[i], out var recordBytes, out rejectReason))
                return false;

            cursor += recordBytes;
        }

        bytesConsumed = cursor;
        return true;
    }

    public static bool TryReadAndSkip(ReadOnlySpan<byte> chunk, ref int cursor, out string? rejectReason)
    {
        rejectReason = null;
        if (!TryRead(chunk[cursor..], out _, out _, out var bytesConsumed, out rejectReason))
            return false;

        cursor += bytesConsumed;
        return true;
    }

    private static int WriteRecord(Span<byte> chunk, AuthorityEventRecord record)
    {
        var required = AuthorityEventRecord.MinRecordSize(record.ClassName);
        if (chunk.Length < required)
            return 0;

        var cursor = 0;
        chunk[cursor++] = (byte)record.EventType;
        chunk[cursor++] = (byte)record.Source;
        chunk[cursor++] = (byte)record.Category;
        chunk[cursor++] = record.ActorFlags;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], record.ActorId);
        cursor += 4;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], record.EventTic);
        cursor += 4;
        BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], record.ClassId);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], unchecked((ushort)record.Health));
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], record.Wave);
        cursor += 2;
        chunk[cursor++] = (byte)record.ClassName.Length;
        record.ClassName.CopyTo(chunk[cursor..]);
        cursor += record.ClassName.Length;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.PosX);
        cursor += 8;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.PosY);
        cursor += 8;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.PosZ);
        cursor += 8;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.VelX);
        cursor += 8;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.VelY);
        cursor += 8;
        BinaryPrimitives.WriteDoubleLittleEndian(chunk[cursor..], record.VelZ);
        cursor += 8;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], record.Yaw);
        cursor += 4;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], record.Pitch);
        cursor += 4;
        return cursor;
    }

    private static bool TryReadRecord(
        ReadOnlySpan<byte> chunk,
        out AuthorityEventRecord record,
        out int bytesConsumed,
        out string? rejectReason)
    {
        record = default;
        bytesConsumed = 0;
        rejectReason = null;

        if (chunk.Length < LiveConstants.AuthorityEventRecordPrefixSize)
        {
            rejectReason = "authority-event-truncated";
            return false;
        }

        var cursor = 0;
        var eventType = (AuthorityEventType)chunk[cursor++];
        var source = (ReplicatedActorSource)chunk[cursor++];
        var category = (ReplicatedActorCategory)chunk[cursor++];
        var actorFlags = chunk[cursor++];
        var actorId = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var eventTic = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var classId = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
        cursor += 2;
        var health = unchecked((short)BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]));
        cursor += 2;
        var wave = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
        cursor += 2;
        var classNameLen = chunk[cursor++];
        if (chunk.Length - cursor < classNameLen + LiveConstants.AuthorityEventRecordSuffixSize)
        {
            rejectReason = "authority-event-payload-truncated";
            return false;
        }

        var className = chunk.Slice(cursor, classNameLen).ToArray();
        cursor += classNameLen;
        var posX = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var posY = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var posZ = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var velX = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var velY = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var velZ = BinaryPrimitives.ReadDoubleLittleEndian(chunk[cursor..]);
        cursor += 8;
        var yaw = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var pitch = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;

        record = new AuthorityEventRecord(
            eventType,
            source,
            category,
            actorFlags,
            actorId,
            eventTic,
            classId,
            health,
            wave,
            className,
            posX,
            posY,
            posZ,
            velX,
            velY,
            velZ,
            yaw,
            pitch);

        if (!record.IsValid(out rejectReason))
            return false;

        bytesConsumed = cursor;
        return true;
    }

    public static AuthorityEventRecord CreateSpawnExample(string className, uint actorId = 42)
    {
        var classBytes = Encoding.UTF8.GetBytes(className);
        return new AuthorityEventRecord(
            AuthorityEventType.Spawn,
            ReplicatedActorSource.Invasion,
            ReplicatedActorCategory.Monster,
            LiveConstants.ActorDeltaFlagLive,
            actorId,
            eventTic: 100,
            classId: 7,
            health: 80,
            wave: 2,
            classBytes,
            posX: 64.0,
            posY: -32.0,
            posZ: 16.0,
            velX: 0.0,
            velY: 0.0,
            velZ: 0.0,
            yaw: 0x40000000,
            pitch: 0);
    }
}
