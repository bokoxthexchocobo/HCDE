using System.Buffers.Binary;
using System.Net;

namespace HCDE.Protocol;

public readonly record struct MasterServerEntry(IPAddress Address, ushort Port);

public static class MasterPackets
{
    public static bool TryReadServerHeartbeat(ReadOnlySpan<byte> data, out ushort gamePort)
    {
        gamePort = 0;
        if (data.Length < MasterProtocol.ServerHeartbeatPacketSize)
            return false;

        var marker = BinaryPrimitives.ReadUInt32LittleEndian(data);
        if (marker != MasterProtocol.ServerHeartbeatMarker)
            return false;

        gamePort = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);
        return gamePort != 0;
    }

    public static byte[] CreateServerHeartbeat(ushort gamePort)
    {
        var packet = new byte[MasterProtocol.ServerHeartbeatPacketSize];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, MasterProtocol.ServerHeartbeatMarker);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(4), gamePort);
        return packet;
    }

    public static byte[] CreateLauncherListQuery()
    {
        var packet = new byte[MasterProtocol.LauncherListQueryPacketSize];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, MasterProtocol.LauncherListQueryMarker);
        return packet;
    }

    public static bool TryReadLauncherListQuery(ReadOnlySpan<byte> data)
    {
        if (data.Length < MasterProtocol.LauncherListQueryPacketSize)
            return false;

        return BinaryPrimitives.ReadUInt32LittleEndian(data) == MasterProtocol.LauncherListQueryMarker;
    }

    public static byte[] CreateMasterListResponse(IReadOnlyList<MasterServerEntry> servers)
    {
        var maxCount = Math.Min(
            servers.Count,
            (65507 - MasterProtocol.MasterListResponseHeaderSize) / MasterProtocol.MasterListResponseEntrySize);

        var packet = new byte[MasterProtocol.MasterListResponseHeaderSize + maxCount * MasterProtocol.MasterListResponseEntrySize];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, MasterProtocol.MasterListResponseMarker);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(4), (ushort)maxCount);

        var offset = MasterProtocol.MasterListResponseHeaderSize;
        for (var i = 0; i < maxCount; i++)
        {
            var entry = servers[i];
            if (!entry.Address.TryWriteBytes(packet.AsSpan(offset, 4), out var written) || written != 4)
                throw new InvalidOperationException("master list entries require IPv4 addresses");

            BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(offset + 4), entry.Port);
            offset += MasterProtocol.MasterListResponseEntrySize;
        }

        return packet;
    }

    public static bool TryReadMasterListResponse(ReadOnlySpan<byte> data, out IReadOnlyList<MasterServerEntry> servers)
    {
        servers = Array.Empty<MasterServerEntry>();
        if (data.Length < MasterProtocol.MasterListResponseHeaderSize)
            return false;

        var marker = BinaryPrimitives.ReadUInt32LittleEndian(data);
        if (marker != MasterProtocol.MasterListResponseMarker)
            return false;

        var count = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);
        var expectedSize = MasterProtocol.MasterListResponseHeaderSize + count * MasterProtocol.MasterListResponseEntrySize;
        if (data.Length < expectedSize)
            return false;

        var entries = new List<MasterServerEntry>(count);
        var offset = MasterProtocol.MasterListResponseHeaderSize;
        for (var i = 0; i < count; i++)
        {
            var address = new IPAddress(data.Slice(offset, 4));
            var port = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 4)..]);
            entries.Add(new MasterServerEntry(address, port));
            offset += MasterProtocol.MasterListResponseEntrySize;
        }

        servers = entries;
        return true;
    }
}
