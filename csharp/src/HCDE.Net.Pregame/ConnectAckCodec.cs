using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public readonly struct ConnectAckPacket
{
    public ConnectAckPacket(
        byte clientSlot,
        byte connectedPlayers,
        byte maxClients,
        uint sessionToken,
        PreConnectAckFlags flags,
        byte connectVersion,
        HcdeConnectFlags connectFlags)
    {
        ClientSlot = clientSlot;
        ConnectedPlayers = connectedPlayers;
        MaxClients = maxClients;
        SessionToken = sessionToken;
        Flags = flags;
        ConnectVersion = connectVersion;
        ConnectFlags = connectFlags;
    }

    public byte ClientSlot { get; }
    public byte ConnectedPlayers { get; }
    public byte MaxClients { get; }
    public uint SessionToken { get; }
    public PreConnectAckFlags Flags { get; }
    public byte ConnectVersion { get; }
    public HcdeConnectFlags ConnectFlags { get; }

    public const int MinimumSize = 10;
    public const int WithConnectInfoSize = MinimumSize + HcdeConnectInfo.EncodedSize;

    public static int Write(
        Span<byte> netBuffer,
        byte clientSlot,
        byte connectedPlayers,
        byte maxClients,
        uint sessionToken,
        PreConnectAckFlags flags,
        byte connectVersion,
        HcdeConnectFlags connectFlags)
    {
        if (netBuffer.Length < WithConnectInfoSize)
            return 0;

        netBuffer[PregameConstants.SetupCommandOffset] = (byte)NetCommandFlags.Setup;
        netBuffer[PregameConstants.SetupTypeOffset] = (byte)PregameSetupType.ConnectAck;
        netBuffer[2] = clientSlot;
        netBuffer[3] = connectedPlayers;
        netBuffer[4] = maxClients;
        BinaryPrimitives.WriteUInt32BigEndian(netBuffer[PregameConstants.ConnectAckSessionTokenOffset..], sessionToken);
        netBuffer[PregameConstants.ConnectAckFlagsOffset] = (byte)flags;
        var length = MinimumSize;
        length += HcdeConnectInfo.Write(netBuffer[length..], connectVersion, connectFlags);
        return length;
    }

    public static bool TryRead(ReadOnlySpan<byte> netBuffer, out ConnectAckPacket packet)
    {
        packet = default;
        if (netBuffer.Length < MinimumSize)
            return false;
        if (netBuffer[PregameConstants.SetupCommandOffset] != (byte)NetCommandFlags.Setup)
            return false;
        if (netBuffer[PregameConstants.SetupTypeOffset] != (byte)PregameSetupType.ConnectAck)
            return false;

        var flags = (PreConnectAckFlags)netBuffer[PregameConstants.ConnectAckFlagsOffset];
        byte connectVersion = 0;
        HcdeConnectFlags connectFlags = HcdeConnectFlags.None;
        if (netBuffer.Length >= WithConnectInfoSize
            && HcdeConnectInfo.TryRead(netBuffer[MinimumSize..], out connectVersion, out connectFlags))
        {
            // HCD3 block present.
        }

        packet = new ConnectAckPacket(
            netBuffer[2],
            netBuffer[3],
            netBuffer[4],
            BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ConnectAckSessionTokenOffset..]),
            flags,
            connectVersion,
            connectFlags);
        return true;
    }
}
