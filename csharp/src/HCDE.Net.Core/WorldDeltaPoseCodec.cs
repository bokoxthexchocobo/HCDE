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
    public SectorWorldDelta(
        ushort sectorIndex,
        byte flags,
        float floor,
        float ceiling,
        short lightLevel = 0,
        short special = 0)
    {
        SectorIndex = sectorIndex;
        Flags = flags;
        Floor = floor;
        Ceiling = ceiling;
        LightLevel = lightLevel;
        Special = special;
    }

    public ushort SectorIndex { get; }
    public byte Flags { get; }
    public float Floor { get; }
    public float Ceiling { get; }
    public short LightLevel { get; }
    public short Special { get; }

    public static int GetWireSize(byte flags) =>
        LiveConstants.ServerWorldDeltaSectorRecordSize
        + ((flags & LiveConstants.ServerWorldDeltaSectorHasLight) != 0 ? 2 : 0)
        + ((flags & LiveConstants.ServerWorldDeltaSectorHasSpecial) != 0 ? 2 : 0);
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
        var required = SectorWorldDelta.GetWireSize(sector.Flags);
        if (buffer.Length - cursor < required)
            return 0;

        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], sector.SectorIndex);
        cursor += 2;
        buffer[cursor++] = sector.Flags;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], sector.Floor);
        cursor += 4;
        BinaryPrimitives.WriteSingleBigEndian(buffer[cursor..], sector.Ceiling);
        cursor += 4;

        if ((sector.Flags & LiveConstants.ServerWorldDeltaSectorHasLight) != 0)
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer[cursor..], sector.LightLevel);
            cursor += 2;
        }

        if ((sector.Flags & LiveConstants.ServerWorldDeltaSectorHasSpecial) != 0)
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer[cursor..], sector.Special);
            cursor += 2;
        }

        return required;
    }

    public static bool TryReadSector(ReadOnlySpan<byte> buffer, ref int cursor, out SectorWorldDelta sector)
    {
        sector = default;
        if (buffer.Length - cursor < LiveConstants.ServerWorldDeltaSectorRecordSize)
            return false;

        var sectorIndex = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        var flags = buffer[cursor++];
        if ((flags & ~LiveConstants.ServerWorldDeltaSectorKnownFlags) != 0)
            return false;

        var floor = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;
        var ceiling = BinaryPrimitives.ReadSingleBigEndian(buffer[cursor..]);
        cursor += 4;

        short lightLevel = 0;
        if ((flags & LiveConstants.ServerWorldDeltaSectorHasLight) != 0)
        {
            if (buffer.Length - cursor < 2)
                return false;

            lightLevel = BinaryPrimitives.ReadInt16BigEndian(buffer[cursor..]);
            cursor += 2;
        }

        short special = 0;
        if ((flags & LiveConstants.ServerWorldDeltaSectorHasSpecial) != 0)
        {
            if (buffer.Length - cursor < 2)
                return false;

            special = BinaryPrimitives.ReadInt16BigEndian(buffer[cursor..]);
            cursor += 2;
        }

        sector = new SectorWorldDelta(sectorIndex, flags, floor, ceiling, lightLevel, special);
        return true;
    }
}
