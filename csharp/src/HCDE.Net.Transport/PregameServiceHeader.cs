using System.Buffers.Binary;

namespace HCDE.Net.Transport;

public readonly struct PregameServiceHeader
{
    public PregameServiceHeader(uint crc, byte commandByte, uint sequence, uint acknowledgement)
    {
        Crc = crc;
        CommandByte = commandByte;
        Sequence = sequence;
        Acknowledgement = acknowledgement;
    }

    public uint Crc { get; }
    public byte CommandByte { get; }
    public uint Sequence { get; }
    public uint Acknowledgement { get; }

    public static bool TryRead(ReadOnlySpan<byte> data, out PregameServiceHeader header)
    {
        header = default;
        if (data.Length < PregameConstants.ServiceHeaderSize)
            return false;

        header = new PregameServiceHeader(
            BinaryPrimitives.ReadUInt32BigEndian(data),
            data[4],
            BinaryPrimitives.ReadUInt32BigEndian(data[PregameConstants.ServiceSequenceOffset..]),
            BinaryPrimitives.ReadUInt32BigEndian(data[PregameConstants.ServiceAckOffset..]));
        return true;
    }

    public int Write(Span<byte> buffer)
    {
        if (buffer.Length < PregameConstants.ServiceHeaderSize)
            return 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, Crc);
        buffer[4] = CommandByte;
        buffer[5] = 0;
        buffer[6] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[PregameConstants.ServiceSequenceOffset..], Sequence);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[PregameConstants.ServiceAckOffset..], Acknowledgement);
        return PregameConstants.ServiceHeaderSize;
    }
}

public static class HcdeConnectInfo
{
    public const int EncodedSize = 6;

    public static bool TryRead(ReadOnlySpan<byte> data, out byte version, out HcdeConnectFlags flags)
    {
        version = 0;
        flags = HcdeConnectFlags.None;
        if (data.Length < EncodedSize)
            return false;

        if (!data[..4].SequenceEqual(PregameConstants.ConnectMagic))
            return false;

        version = data[4];
        flags = (HcdeConnectFlags)data[5];
        return true;
    }

    public static int Write(Span<byte> buffer, byte version, HcdeConnectFlags flags)
    {
        if (buffer.Length < EncodedSize)
            return 0;

        PregameConstants.ConnectMagic.CopyTo(buffer);
        buffer[4] = version;
        buffer[5] = (byte)flags;
        return EncodedSize;
    }
}
