using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

/// <summary>
/// Ports <c>MakeSessionToken</c> from <c>i_net.cpp</c>.
/// </summary>
public static class SessionToken
{
    public static uint Mint(NetworkEndpoint address, int clientSlot, ReadOnlySpan<byte> gameId, ulong timeMilliseconds)
    {
        var token = Crc32.Calc(gameId);
        token = Crc32.Add(token, address.Address.GetAddressBytes());

        Span<byte> portBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(portBytes, (ushort)address.Port);
        token = Crc32.Add(token, portBytes);

        Span<byte> clientBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(clientBytes, clientSlot);
        token = Crc32.Add(token, clientBytes);

        token ^= (uint)(timeMilliseconds & 0xFFFFFFFF);
        return token == 0 ? 1u : token;
    }
}
