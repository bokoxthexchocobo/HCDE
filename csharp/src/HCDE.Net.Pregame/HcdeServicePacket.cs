using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public readonly struct HcdeServicePacket
{
    public HcdeServicePacket(
        PregameServiceType service,
        uint sessionToken,
        uint sequence,
        uint acknowledgement,
        ReadOnlyMemory<byte> payload)
    {
        Service = service;
        SessionToken = sessionToken;
        Sequence = sequence;
        Acknowledgement = acknowledgement;
        Payload = payload;
    }

    public PregameServiceType Service { get; }
    public uint SessionToken { get; }
    public uint Sequence { get; }
    public uint Acknowledgement { get; }
    public ReadOnlyMemory<byte> Payload { get; }

    public static bool TryRead(ReadOnlySpan<byte> netBuffer, out HcdeServicePacket packet)
    {
        packet = default;
        if (netBuffer.Length < PregameConstants.ServiceHeaderSize)
            return false;
        if (netBuffer[PregameConstants.SetupCommandOffset] != (byte)NetCommandFlags.Setup)
            return false;
        if (netBuffer[PregameConstants.SetupTypeOffset] != (byte)PregameSetupType.HcdeService)
            return false;

        var service = (PregameServiceType)netBuffer[PregameConstants.ServiceTypeOffset];
        var token = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.SessionTokenOffset..]);
        var sequence = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ServiceSequenceOffset..]);
        var acknowledgement = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ServiceAckOffset..]);
        var payload = netBuffer[PregameConstants.ServiceHeaderSize..].ToArray();
        packet = new HcdeServicePacket(service, token, sequence, acknowledgement, payload);
        return true;
    }

    public static int WriteHeader(
        Span<byte> netBuffer,
        PregameServiceType service,
        uint sessionToken,
        uint sequence,
        uint acknowledgement)
    {
        if (netBuffer.Length < PregameConstants.ServiceHeaderSize)
            return 0;

        netBuffer[PregameConstants.SetupCommandOffset] = (byte)NetCommandFlags.Setup;
        netBuffer[PregameConstants.SetupTypeOffset] = (byte)PregameSetupType.HcdeService;
        netBuffer[PregameConstants.ServiceTypeOffset] = (byte)service;
        BinaryPrimitives.WriteUInt32BigEndian(netBuffer[PregameConstants.SessionTokenOffset..], sessionToken);
        BinaryPrimitives.WriteUInt32BigEndian(netBuffer[PregameConstants.ServiceSequenceOffset..], sequence);
        BinaryPrimitives.WriteUInt32BigEndian(netBuffer[PregameConstants.ServiceAckOffset..], acknowledgement);
        return PregameConstants.ServiceHeaderSize;
    }

    public static int Write(
        Span<byte> netBuffer,
        PregameServiceType service,
        uint sessionToken,
        uint sequence,
        uint acknowledgement,
        ReadOnlySpan<byte> payload)
    {
        var headerSize = WriteHeader(netBuffer, service, sessionToken, sequence, acknowledgement);
        if (headerSize == 0 || netBuffer.Length < headerSize + payload.Length)
            return 0;

        payload.CopyTo(netBuffer[headerSize..]);
        return headerSize + payload.Length;
    }
}
