using System.Buffers.Binary;

namespace HCDE.Net.Core;

public sealed class ActorDeltaRecord
{
    public uint ActorId { get; init; }
    public ushort ClassId { get; init; }
    public ushort FieldMask { get; init; }
    public byte Category { get; init; }
    public byte Flags { get; init; }
    public byte ActionState { get; init; }
    public short Health { get; init; }
    public double PosX { get; init; }
    public double PosY { get; init; }
    public double PosZ { get; init; }
    public double VelX { get; init; }
    public double VelY { get; init; }
    public double VelZ { get; init; }
    public uint YawBams { get; init; }
    public uint PitchBams { get; init; }
    public uint CoopSpawnIndex { get; init; }

    public static int MinRecordSize(ushort fieldMask)
    {
        var size = 8;
        if ((fieldMask & LiveConstants.ActorDeltaFieldCategory) != 0) size += 1;
        if ((fieldMask & LiveConstants.ActorDeltaFieldFlags) != 0) size += 1;
        if ((fieldMask & LiveConstants.ActorDeltaFieldAction) != 0) size += 1;
        if ((fieldMask & LiveConstants.ActorDeltaFieldHealth) != 0) size += 2;
        if ((fieldMask & LiveConstants.ActorDeltaFieldPos) != 0) size += 12;
        if ((fieldMask & LiveConstants.ActorDeltaFieldVel) != 0) size += 6;
        if ((fieldMask & LiveConstants.ActorDeltaFieldAngles) != 0) size += 4;
        if ((fieldMask & LiveConstants.ActorDeltaFieldCoopSpawnIndex) != 0) size += 4;
        return size;
    }
}

public static class ActorDeltaRecordCodec
{
    public static int Write(Span<byte> buffer, ref int cursor, ActorDeltaRecord record)
    {
        if (record.FieldMask == 0 || record.ActorId == 0)
            return 0;

        var required = ActorDeltaRecord.MinRecordSize(record.FieldMask);
        if (buffer.Length - cursor < required)
            return 0;

        var start = cursor;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[cursor..], record.ActorId);
        cursor += 4;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], record.ClassId);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], record.FieldMask);
        cursor += 2;

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldCategory) != 0)
            buffer[cursor++] = record.Category;
        if ((record.FieldMask & LiveConstants.ActorDeltaFieldFlags) != 0)
            buffer[cursor++] = record.Flags;
        if ((record.FieldMask & LiveConstants.ActorDeltaFieldAction) != 0)
            buffer[cursor++] = record.ActionState;
        if ((record.FieldMask & LiveConstants.ActorDeltaFieldHealth) != 0)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], (ushort)record.Health);
            cursor += 2;
        }

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldPos) != 0)
        {
            if (ActorDeltaQuantization.WriteQuantizedPos(buffer, ref cursor, record.PosX) == 0
                || ActorDeltaQuantization.WriteQuantizedPos(buffer, ref cursor, record.PosY) == 0
                || ActorDeltaQuantization.WriteQuantizedPos(buffer, ref cursor, record.PosZ) == 0)
                return 0;
        }

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldVel) != 0)
        {
            if (ActorDeltaQuantization.WriteQuantizedVel(buffer, ref cursor, record.VelX) == 0
                || ActorDeltaQuantization.WriteQuantizedVel(buffer, ref cursor, record.VelY) == 0
                || ActorDeltaQuantization.WriteQuantizedVel(buffer, ref cursor, record.VelZ) == 0)
                return 0;
        }

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldAngles) != 0)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], ActorDeltaQuantization.CompactAngle(record.YawBams));
            cursor += 2;
            BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], ActorDeltaQuantization.CompactAngle(record.PitchBams));
            cursor += 2;
        }

        if ((record.FieldMask & LiveConstants.ActorDeltaFieldCoopSpawnIndex) != 0)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer[cursor..], record.CoopSpawnIndex);
            cursor += 4;
        }

        return cursor - start;
    }

    public static bool TryRead(ReadOnlySpan<byte> buffer, ref int cursor, out ActorDeltaRecord record)
    {
        record = new ActorDeltaRecord();
        if (buffer.Length - cursor < 8)
            return false;

        var actorId = BinaryPrimitives.ReadUInt32BigEndian(buffer[cursor..]);
        cursor += 4;
        var classId = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        var fieldMask = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        if (actorId == 0 || fieldMask == 0 || buffer.Length - cursor < ActorDeltaRecord.MinRecordSize(fieldMask) - 8)
            return false;

        byte category = 0;
        byte flags = 0;
        byte actionState = 0;
        short health = 0;
        double posX = 0;
        double posY = 0;
        double posZ = 0;
        double velX = 0;
        double velY = 0;
        double velZ = 0;
        uint yawBams = 0;
        uint pitchBams = 0;
        uint coopSpawnIndex = 0;

        if ((fieldMask & LiveConstants.ActorDeltaFieldCategory) != 0)
            category = buffer[cursor++];
        if ((fieldMask & LiveConstants.ActorDeltaFieldFlags) != 0)
            flags = buffer[cursor++];
        if ((fieldMask & LiveConstants.ActorDeltaFieldAction) != 0)
            actionState = buffer[cursor++];
        if ((fieldMask & LiveConstants.ActorDeltaFieldHealth) != 0)
        {
            health = (short)BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
            cursor += 2;
        }

        if ((fieldMask & LiveConstants.ActorDeltaFieldPos) != 0)
        {
            if (!ActorDeltaQuantization.TryReadQuantizedPos(buffer, ref cursor, out posX)
                || !ActorDeltaQuantization.TryReadQuantizedPos(buffer, ref cursor, out posY)
                || !ActorDeltaQuantization.TryReadQuantizedPos(buffer, ref cursor, out posZ))
                return false;
        }

        if ((fieldMask & LiveConstants.ActorDeltaFieldVel) != 0)
        {
            if (!ActorDeltaQuantization.TryReadQuantizedVel(buffer, ref cursor, out velX)
                || !ActorDeltaQuantization.TryReadQuantizedVel(buffer, ref cursor, out velY)
                || !ActorDeltaQuantization.TryReadQuantizedVel(buffer, ref cursor, out velZ))
                return false;
        }

        if ((fieldMask & LiveConstants.ActorDeltaFieldAngles) != 0)
        {
            yawBams = ActorDeltaQuantization.ExpandCompactAngle(BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]));
            cursor += 2;
            pitchBams = ActorDeltaQuantization.ExpandCompactAngle(BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]));
            cursor += 2;
        }

        if ((fieldMask & LiveConstants.ActorDeltaFieldCoopSpawnIndex) != 0)
        {
            coopSpawnIndex = BinaryPrimitives.ReadUInt32BigEndian(buffer[cursor..]);
            cursor += 4;
        }

        record = new ActorDeltaRecord
        {
            ActorId = actorId,
            ClassId = classId,
            FieldMask = fieldMask,
            Category = category,
            Flags = flags,
            ActionState = actionState,
            Health = health,
            PosX = posX,
            PosY = posY,
            PosZ = posZ,
            VelX = velX,
            VelY = velY,
            VelZ = velZ,
            YawBams = yawBams,
            PitchBams = pitchBams,
            CoopSpawnIndex = coopSpawnIndex,
        };
        return true;
    }
}
