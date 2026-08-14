using System.Buffers.Binary;

namespace HCDE.Net.Pregame;

public readonly struct BootstrapControlPayload
{
    public BootstrapControlPayload(byte roomId, uint gameTic, uint clientTic, uint consistency)
    {
        RoomId = roomId;
        GameTic = gameTic;
        ClientTic = clientTic;
        Consistency = consistency;
    }

    public byte RoomId { get; }
    public uint GameTic { get; }
    public uint ClientTic { get; }
    public uint Consistency { get; }
}

public sealed class MapLoadInfo
{
    public string MapName { get; init; } = "MAP01";
    public int RngSeed { get; init; } = 1;
    public string? LoadGamePath { get; init; }
}

public sealed class GameInfoPayload
{
    public byte TicDup { get; init; } = 1;
    public byte[] GameId { get; init; } = new byte[8];
    public byte[] ServerInfo { get; init; } = Array.Empty<byte>();
}

public sealed class RosterEntry
{
    public byte ClientSlot { get; init; }
    public byte[]? Address { get; init; }
    public string UserInfo { get; init; } = "";
}

public static class PregameServicePayloads
{
    public const int SockAddrInSize = 16;
    public const int BootstrapControlPayloadSize = 13;

    public static int WriteBootstrapControl(Span<byte> buffer, BootstrapControlPayload payload)
    {
        if (buffer.Length < BootstrapControlPayloadSize)
            return 0;

        buffer[0] = payload.RoomId;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[1..], payload.GameTic);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[5..], payload.ClientTic);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[9..], payload.Consistency);
        return BootstrapControlPayloadSize;
    }

    public static bool TryReadBootstrapControl(ReadOnlySpan<byte> payload, out BootstrapControlPayload control)
    {
        control = default;
        if (payload.Length < BootstrapControlPayloadSize)
            return false;

        control = new BootstrapControlPayload(
            payload[0],
            BinaryPrimitives.ReadUInt32BigEndian(payload[1..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[5..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[9..]));
        return true;
    }

    public static int WriteMapLoadInfo(Span<byte> buffer, MapLoadInfo info)
    {
        var offset = 0;
        offset += ProtocolStreamCodec.WriteNullTerminatedString(buffer[offset..], info.MapName);
        if (offset == 0)
            return 0;
        offset += ProtocolStreamCodec.WriteInt32(buffer[offset..], info.RngSeed);
        if (info.LoadGamePath is { Length: > 0 } loadGame)
        {
            offset += ProtocolStreamCodec.WriteInt8(buffer[offset..], 1);
            offset += ProtocolStreamCodec.WriteNullTerminatedString(buffer[offset..], loadGame);
        }
        else
        {
            offset += ProtocolStreamCodec.WriteInt8(buffer[offset..], 0);
        }

        return offset;
    }

    public static bool TryReadMapLoadInfo(ReadOnlySpan<byte> payload, out MapLoadInfo info)
    {
        info = new MapLoadInfo();
        var offset = 0;
        if (!ProtocolStreamCodec.TryReadNullTerminatedString(payload, ref offset, out var mapName))
            return false;
        if (!ProtocolStreamCodec.TryReadInt32(payload, ref offset, out var seed))
            return false;
        if (!ProtocolStreamCodec.TryReadInt8(payload, ref offset, out var hasLoad))
            return false;

        string? loadGame = null;
        if (hasLoad != 0 && !ProtocolStreamCodec.TryReadNullTerminatedString(payload, ref offset, out loadGame!))
            return false;

        info = new MapLoadInfo { MapName = mapName, RngSeed = seed, LoadGamePath = loadGame };
        return true;
    }

    public static int WriteGameInfo(Span<byte> buffer, GameInfoPayload info)
    {
        if (buffer.Length < 9 + info.ServerInfo.Length)
            return 0;

        buffer[0] = info.TicDup;
        info.GameId.AsSpan(0, Math.Min(8, info.GameId.Length)).CopyTo(buffer[1..9]);
        info.ServerInfo.CopyTo(buffer[9..]);
        return 9 + info.ServerInfo.Length;
    }

    public static bool TryReadGameInfo(ReadOnlySpan<byte> payload, out GameInfoPayload info)
    {
        info = new GameInfoPayload();
        if (payload.Length < 9)
            return false;

        var gameId = payload[1..9].ToArray();
        info = new GameInfoPayload
        {
            TicDup = payload[0],
            GameId = gameId,
            ServerInfo = payload[9..].ToArray(),
        };
        return true;
    }

    public static int WriteRoster(Span<byte> buffer, IReadOnlyList<RosterEntry> entries)
    {
        if (buffer.Length < 1)
            return 0;

        buffer[0] = (byte)entries.Count;
        var offset = 1;
        foreach (var entry in entries)
        {
            if (offset + 1 > buffer.Length)
                return 0;
            buffer[offset++] = entry.ClientSlot;

            if (entry.ClientSlot > 0)
            {
                var address = entry.Address ?? new byte[SockAddrInSize];
                if (offset + address.Length > buffer.Length)
                    return 0;
                address.AsSpan(0, Math.Min(SockAddrInSize, address.Length)).CopyTo(buffer[offset..]);
                offset += SockAddrInSize;
            }

            var userInfoBytes = System.Text.Encoding.ASCII.GetBytes(entry.UserInfo);
            if (offset + 2 + userInfoBytes.Length > buffer.Length)
                return 0;
            ProtocolStreamCodec.WriteUInt16BigEndian(buffer[offset..], (ushort)userInfoBytes.Length);
            offset += 2;
            userInfoBytes.CopyTo(buffer[offset..]);
            offset += userInfoBytes.Length;
        }

        return offset;
    }

    public static bool TryReadRoster(ReadOnlySpan<byte> payload, out IReadOnlyList<RosterEntry> entries)
    {
        entries = Array.Empty<RosterEntry>();
        if (payload.Length < 1)
            return false;

        var count = payload[0];
        var list = new List<RosterEntry>(count);
        var offset = 1;
        for (var i = 0; i < count; i++)
        {
            if (offset >= payload.Length)
                return false;
            var slot = payload[offset++];
            byte[]? address = null;
            if (slot > 0)
            {
                if (offset + SockAddrInSize > payload.Length)
                    return false;
                address = payload.Slice(offset, SockAddrInSize).ToArray();
                offset += SockAddrInSize;
            }

            if (!ProtocolStreamCodec.TryReadUInt16BigEndian(payload, ref offset, out var infoSize))
                return false;
            if (offset + infoSize > payload.Length)
                return false;
            var userInfo = System.Text.Encoding.ASCII.GetString(payload.Slice(offset, infoSize));
            offset += infoSize;
            list.Add(new RosterEntry { ClientSlot = slot, Address = address, UserInfo = userInfo });
        }

        entries = list;
        return true;
    }
}
