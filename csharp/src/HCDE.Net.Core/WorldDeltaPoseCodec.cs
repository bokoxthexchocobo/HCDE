using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct PlayerPoseWorldDelta
{
    public PlayerPoseWorldDelta(
        byte playerNum,
        byte flags,
        short health,
        short armor,
        float posX,
        float posY,
        float posZ,
        float velX,
        float velY,
        float velZ,
        uint yawBams,
        uint pitchBams)
    {
        PlayerNum = playerNum;
        Flags = flags;
        Health = health;
        Armor = armor;
        PosX = posX;
        PosY = posY;
        PosZ = posZ;
        VelX = velX;
        VelY = velY;
        VelZ = velZ;
        YawBams = yawBams;
        PitchBams = pitchBams;
    }

    public byte PlayerNum { get; }
    public byte Flags { get; }
    public short Health { get; }
    public short Armor { get; }
    public float PosX { get; }
    public float PosY { get; }
    public float PosZ { get; }
    public float VelX { get; }
    public float VelY { get; }
    public float VelZ { get; }
    public uint YawBams { get; }
    public uint PitchBams { get; }
}

public readonly struct SectorWorldDelta
{
    public SectorWorldDelta(ushort sectorIndex, byte flags, float floor, float ceiling)
    {
        SectorIndex = sectorIndex;
        Flags = flags;
        Floor = floor;
        Ceiling = ceiling;
    }

    public ushort SectorIndex { get; }
    public byte Flags { get; }
    public float Floor { get; }
    public float Ceiling { get; }
}

public static class WorldDeltaPoseCodec
{
    public static int Write(Span<byte> buffer, ref int cursor, PlayerPoseWorldDelta pose)
    {
        if (buffer.Length - cursor < LiveConstants.ServerWorldDeltaPoseRecordV4Size)
            return 0;

        buffer[cursor++] = pose.PlayerNum;
        buffer[cursor++] = pose.Flags;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], (ushort)pose.Health);
        cursor += 2;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], (ushort)pose.Armor);
        cursor += 2;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.PosX);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.PosY);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.PosZ);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.VelX);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.VelY);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], pose.VelZ);
        cursor += 4;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[cursor..], pose.YawBams);
        cursor += 4;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[cursor..], pose.PitchBams);
        cursor += 4;
        return LiveConstants.ServerWorldDeltaPoseRecordV4Size;
    }

    public static bool TryRead(ReadOnlySpan<byte> buffer, ref int cursor, out PlayerPoseWorldDelta pose)
    {
        pose = default;
        if (buffer.Length - cursor < LiveConstants.ServerWorldDeltaPoseRecordV4Size)
            return false;

        var playerNum = buffer[cursor++];
        var flags = buffer[cursor++];
        var health = (short)BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        var armor = (short)BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        var posX = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var posY = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var posZ = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var velX = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var velY = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var velZ = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var yawBams = BinaryPrimitives.ReadUInt32BigEndian(buffer[cursor..]);
        cursor += 4;
        var pitchBams = BinaryPrimitives.ReadUInt32BigEndian(buffer[cursor..]);
        cursor += 4;

        pose = new PlayerPoseWorldDelta(playerNum, flags, health, armor, posX, posY, posZ, velX, velY, velZ, yawBams, pitchBams);
        return true;
    }

    public static int WriteSector(Span<byte> buffer, ref int cursor, SectorWorldDelta sector)
    {
        if (buffer.Length - cursor < LiveConstants.ServerWorldDeltaSectorRecordSize)
            return 0;

        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], sector.SectorIndex);
        cursor += 2;
        buffer[cursor++] = sector.Flags;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], sector.Floor);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], sector.Ceiling);
        cursor += 4;
        return LiveConstants.ServerWorldDeltaSectorRecordSize;
    }

    public static bool TryReadSector(ReadOnlySpan<byte> buffer, ref int cursor, out SectorWorldDelta sector)
    {
        sector = default;
        if (buffer.Length - cursor < LiveConstants.ServerWorldDeltaSectorRecordSize)
            return false;

        var sectorIndex = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        var flags = buffer[cursor++];
        var floor = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var ceiling = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        sector = new SectorWorldDelta(sectorIndex, flags, floor, ceiling);
        return true;
    }
}
