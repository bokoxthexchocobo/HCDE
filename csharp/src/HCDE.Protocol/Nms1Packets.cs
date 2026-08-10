using System.Buffers.Binary;
using System.Text;

namespace HCDE.Protocol;

public static class Nms1Packets
{
    public static bool IsValidProtocolFamily(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MasterProtocol.Nms1MaxProtocolFamilyBytes)
            return false;

        foreach (var c in value)
        {
            var ok = c is >= 'a' and <= 'z'
                or >= '0' and <= '9'
                or '.'
                or '_'
                or '-';
            if (!ok)
                return false;
        }

        return true;
    }

    public static bool TryWriteChallengeRequest(uint requestId, Nms1ChallengePurpose purpose, Span<byte> buffer, out int length)
    {
        length = 0;
        var writer = new PacketWriter(buffer, Nms1MessageType.ChallengeRequest, requestId);
        return writer.WriteU8(Nms1FieldType.Purpose, (byte)purpose) && writer.Finish(buffer, out length);
    }

    public static bool TryWriteRegisterRequest(uint requestId, Nms1RegisterRequest request, Span<byte> buffer, out int length)
    {
        length = 0;
        if (!IsValidProtocolFamily(request.ProtocolFamily)
            || request.GamePort == 0
            || request.QueryPort == 0
            || !TextFits(request.BuildLabel, MasterProtocol.Nms1MaxBuildLabelBytes)
            || !TextFits(request.DisplayName, MasterProtocol.Nms1MaxDisplayNameBytes)
            || !TextFits(request.GameName, MasterProtocol.Nms1MaxGameNameBytes)
            || !TextFits(request.MapName, MasterProtocol.Nms1MaxMapNameBytes))
        {
            return false;
        }

        var writer = new PacketWriter(buffer, Nms1MessageType.Register, requestId);
        if (!writer.WriteU32(Nms1FieldType.ChallengeIssuedUnix, request.Challenge.IssuedUnix)
            || !writer.WriteBytes(Nms1FieldType.ChallengeToken, request.Challenge.Token)
            || !writer.WriteText(Nms1FieldType.ProtocolFamily, request.ProtocolFamily, MasterProtocol.Nms1MaxProtocolFamilyBytes)
            || !writer.WriteU16(Nms1FieldType.GamePort, request.GamePort)
            || !writer.WriteU16(Nms1FieldType.QueryPort, request.QueryPort)
            || !writer.WriteU16(Nms1FieldType.CurrentPlayers, request.CurrentPlayers)
            || !writer.WriteU16(Nms1FieldType.MaxPlayers, request.MaxPlayers)
            || !writer.WriteU32(Nms1FieldType.ServerFlags, request.ServerFlags))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.BuildLabel)
            && !writer.WriteText(Nms1FieldType.BuildLabel, request.BuildLabel, MasterProtocol.Nms1MaxBuildLabelBytes))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.DisplayName)
            && !writer.WriteText(Nms1FieldType.DisplayName, request.DisplayName, MasterProtocol.Nms1MaxDisplayNameBytes))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.GameName)
            && !writer.WriteText(Nms1FieldType.GameName, request.GameName, MasterProtocol.Nms1MaxGameNameBytes))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.MapName)
            && !writer.WriteText(Nms1FieldType.MapName, request.MapName, MasterProtocol.Nms1MaxMapNameBytes))
        {
            return false;
        }

        return writer.Finish(buffer, out length);
    }

    public static bool TryWriteHeartbeatRequest(uint requestId, Nms1HeartbeatRequest request, Span<byte> buffer, out int length)
    {
        length = 0;
        if (!IsValidProtocolFamily(request.ProtocolFamily) || request.GamePort == 0)
            return false;

        var writer = new PacketWriter(buffer, Nms1MessageType.Heartbeat, requestId);
        return writer.WriteText(Nms1FieldType.ProtocolFamily, request.ProtocolFamily, MasterProtocol.Nms1MaxProtocolFamilyBytes)
            && writer.WriteU16(Nms1FieldType.GamePort, request.GamePort)
            && writer.WriteBytes(Nms1FieldType.EntryToken, request.Entry.Token)
            && writer.WriteU16(Nms1FieldType.CurrentPlayers, request.CurrentPlayers)
            && writer.WriteU16(Nms1FieldType.MaxPlayers, request.MaxPlayers)
            && writer.WriteU32(Nms1FieldType.ServerFlags, request.ServerFlags)
            && writer.Finish(buffer, out length);
    }

    public static bool TryWriteUnregisterRequest(uint requestId, Nms1UnregisterRequest request, Span<byte> buffer, out int length)
    {
        length = 0;
        if (!IsValidProtocolFamily(request.ProtocolFamily) || request.GamePort == 0)
            return false;

        var writer = new PacketWriter(buffer, Nms1MessageType.Unregister, requestId);
        return writer.WriteText(Nms1FieldType.ProtocolFamily, request.ProtocolFamily, MasterProtocol.Nms1MaxProtocolFamilyBytes)
            && writer.WriteU16(Nms1FieldType.GamePort, request.GamePort)
            && writer.WriteBytes(Nms1FieldType.EntryToken, request.Entry.Token)
            && writer.Finish(buffer, out length);
    }

    public static bool TryWriteChallengeResponse(uint requestId, Nms1ChallengeToken challenge, ushort ttlSeconds, Span<byte> buffer, out int length)
    {
        length = 0;
        var writer = new PacketWriter(buffer, Nms1MessageType.ChallengeResponse, requestId);
        return writer.WriteU32(Nms1FieldType.ChallengeIssuedUnix, challenge.IssuedUnix)
            && writer.WriteBytes(Nms1FieldType.ChallengeToken, challenge.Token)
            && writer.WriteU16(Nms1FieldType.TtlSeconds, ttlSeconds)
            && writer.Finish(buffer, out length);
    }

    public static bool TryWriteRegisterAck(uint requestId, Nms1EntryToken entry, ushort ttlSeconds, Span<byte> buffer, out int length)
    {
        length = 0;
        var writer = new PacketWriter(buffer, Nms1MessageType.RegisterAck, requestId);
        return writer.WriteBytes(Nms1FieldType.EntryToken, entry.Token)
            && writer.WriteU16(Nms1FieldType.TtlSeconds, ttlSeconds)
            && writer.Finish(buffer, out length);
    }

    public static bool TryWriteHeartbeatAck(uint requestId, ushort ttlSeconds, Span<byte> buffer, out int length)
    {
        length = 0;
        var writer = new PacketWriter(buffer, Nms1MessageType.HeartbeatAck, requestId);
        return writer.WriteU16(Nms1FieldType.TtlSeconds, ttlSeconds) && writer.Finish(buffer, out length);
    }

    public static bool TryWriteUnregisterAck(uint requestId, Span<byte> buffer, out int length)
    {
        length = 0;
        var writer = new PacketWriter(buffer, Nms1MessageType.UnregisterAck, requestId);
        return writer.Finish(buffer, out length);
    }

    public static bool TryWriteErrorResponse(uint requestId, ushort errorCode, string errorText, Span<byte> buffer, out int length)
    {
        var writer = new PacketWriter(buffer, Nms1MessageType.Error, requestId);
        if (!writer.WriteU16(Nms1FieldType.ErrorCode, errorCode))
        {
            length = 0;
            return false;
        }

        if (!string.IsNullOrEmpty(errorText)
            && !writer.WriteText(Nms1FieldType.ErrorText, errorText, 96))
        {
            length = 0;
            return false;
        }

        return writer.Finish(buffer, out length);
    }

    public static Nms1ParseResult TryReadErrorResponse(
        ReadOnlySpan<byte> data,
        uint requestId,
        Nms1ErrorResponse error)
    {
        if (!TryReadHeader(data, out var packet))
            return Nms1ParseResult.Malformed;

        if (packet.RequestId != requestId)
            return Nms1ParseResult.NotForRequest;

        if (packet.Type != Nms1MessageType.Error)
            return Nms1ParseResult.Malformed;

        return TryReadErrorResponse(packet, error) ? Nms1ParseResult.ErrorResponse : Nms1ParseResult.Malformed;
    }

    public static Nms1ParseResult TryReadChallengeResponse(
        ReadOnlySpan<byte> data,
        uint requestId,
        Nms1ChallengeToken challenge,
        out ushort ttlSeconds,
        Nms1ErrorResponse? error)
    {
        ttlSeconds = 0;
        if (!TryReadExpectedPacket(data, requestId, Nms1MessageType.ChallengeResponse, out var packet, error, out var result))
            return result;

        if (!TryReadU32Field(packet, Nms1FieldType.ChallengeIssuedUnix, out var issued)
            || !TryReadBytesField(packet, Nms1FieldType.ChallengeToken, challenge.Token)
            || !TryReadU16Field(packet, Nms1FieldType.TtlSeconds, out ttlSeconds))
        {
            return Nms1ParseResult.Malformed;
        }

        challenge.IssuedUnix = issued;
        return Nms1ParseResult.Ok;
    }

    public static Nms1ParseResult TryReadRegisterAck(
        ReadOnlySpan<byte> data,
        uint requestId,
        Nms1EntryToken entry,
        out ushort ttlSeconds,
        Nms1ErrorResponse? error)
    {
        ttlSeconds = 0;
        if (!TryReadExpectedPacket(data, requestId, Nms1MessageType.RegisterAck, out var packet, error, out var result))
            return result;

        if (!TryReadBytesField(packet, Nms1FieldType.EntryToken, entry.Token)
            || !TryReadU16Field(packet, Nms1FieldType.TtlSeconds, out ttlSeconds))
        {
            return Nms1ParseResult.Malformed;
        }

        return Nms1ParseResult.Ok;
    }

    public static Nms1ParseResult TryReadHeartbeatAck(
        ReadOnlySpan<byte> data,
        uint requestId,
        out ushort ttlSeconds,
        Nms1ErrorResponse? error)
    {
        ttlSeconds = 0;
        if (!TryReadExpectedPacket(data, requestId, Nms1MessageType.HeartbeatAck, out var packet, error, out var result))
            return result;

        return TryReadU16Field(packet, Nms1FieldType.TtlSeconds, out ttlSeconds)
            ? Nms1ParseResult.Ok
            : Nms1ParseResult.Malformed;
    }

    public static Nms1ParseResult TryReadUnregisterAck(
        ReadOnlySpan<byte> data,
        uint requestId,
        Nms1ErrorResponse? error)
    {
        if (!TryReadExpectedPacket(data, requestId, Nms1MessageType.UnregisterAck, out var packet, error, out var result))
            return result;

        return packet.PayloadLength == 0 ? Nms1ParseResult.Ok : Nms1ParseResult.Malformed;
    }

    private static bool TextFits(string value, int maxBytes)
    {
        if (value.Length > maxBytes)
            return false;

        foreach (var c in value)
        {
            if (c == '\0' || (c < 0x20 && c != '\t'))
                return false;
        }

        return true;
    }

    private ref struct PacketView
    {
        public PacketView(Nms1MessageType type, uint requestId, ReadOnlySpan<byte> payload)
        {
            Type = type;
            RequestId = requestId;
            Payload = payload;
        }

        public Nms1MessageType Type { get; }
        public uint RequestId { get; }
        public ReadOnlySpan<byte> Payload { get; }
        public int PayloadLength => Payload.Length;
    }

    private ref struct FieldView
    {
        public FieldView(ReadOnlySpan<byte> data, bool found, bool duplicate)
        {
            Data = data;
            Found = found;
            Duplicate = duplicate;
        }

        public ReadOnlySpan<byte> Data { get; }
        public bool Found { get; }
        public bool Duplicate { get; }
    }

    private static bool TryReadExpectedPacket(
        ReadOnlySpan<byte> data,
        uint requestId,
        Nms1MessageType expectedType,
        out PacketView packet,
        Nms1ErrorResponse? error,
        out Nms1ParseResult result)
    {
        packet = default;
        result = Nms1ParseResult.Malformed;

        if (!TryReadHeader(data, out packet))
            return false;

        if (packet.RequestId != requestId)
        {
            result = Nms1ParseResult.NotForRequest;
            return false;
        }

        if (packet.Type == Nms1MessageType.Error)
        {
            result = TryReadErrorResponse(packet, error) ? Nms1ParseResult.ErrorResponse : Nms1ParseResult.Malformed;
            return false;
        }

        if (packet.Type != expectedType)
            return false;

        result = Nms1ParseResult.Ok;
        return true;
    }

    private static bool TryReadHeader(ReadOnlySpan<byte> data, out PacketView packet)
    {
        packet = default;
        if (data.Length < MasterProtocol.Nms1HeaderSize || data.Length > MasterProtocol.Nms1MaxPacketSize)
            return false;

        if (!data.StartsWith(Encoding.ASCII.GetBytes(MasterProtocol.Nms1Magic)))
            return false;

        if (data[4] != MasterProtocol.Nms1Version)
            return false;

        var rawType = data[5];
        if (rawType < (byte)Nms1MessageType.ChallengeRequest || rawType > (byte)Nms1MessageType.Error)
            return false;

        if (BinaryPrimitives.ReadUInt16BigEndian(data[6..]) != 0 || BinaryPrimitives.ReadUInt16BigEndian(data[14..]) != 0)
            return false;

        var payloadLength = BinaryPrimitives.ReadUInt16BigEndian(data[12..]);
        if (payloadLength != data.Length - MasterProtocol.Nms1HeaderSize)
            return false;

        packet = new PacketView(
            (Nms1MessageType)rawType,
            BinaryPrimitives.ReadUInt32BigEndian(data[8..]),
            data[MasterProtocol.Nms1HeaderSize..]);
        return true;
    }

    private static bool TryFindField(PacketView packet, Nms1FieldType type, out FieldView field)
    {
        field = default;
        var duplicate = false;
        var found = false;
        ReadOnlySpan<byte> data = default;
        var offset = 0;

        while (offset < packet.PayloadLength)
        {
            if (packet.PayloadLength - offset < 4)
                return false;

            var rawType = BinaryPrimitives.ReadUInt16BigEndian(packet.Payload[offset..]);
            var fieldLength = BinaryPrimitives.ReadUInt16BigEndian(packet.Payload[(offset + 2)..]);
            offset += 4;
            if (fieldLength > packet.PayloadLength - offset)
                return false;

            if (rawType == (ushort)type)
            {
                if (found)
                {
                    duplicate = true;
                    return false;
                }

                data = packet.Payload.Slice(offset, fieldLength);
                found = true;
            }

            offset += fieldLength;
        }

        field = new FieldView(data, found, duplicate);
        return true;
    }

    private static bool TryReadU16Field(PacketView packet, Nms1FieldType type, out ushort value)
    {
        value = 0;
        if (!TryFindField(packet, type, out var field) || !field.Found || field.Data.Length != 2)
            return false;

        value = BinaryPrimitives.ReadUInt16BigEndian(field.Data);
        return true;
    }

    private static bool TryReadU32Field(PacketView packet, Nms1FieldType type, out uint value)
    {
        value = 0;
        if (!TryFindField(packet, type, out var field) || !field.Found || field.Data.Length != 4)
            return false;

        value = BinaryPrimitives.ReadUInt32BigEndian(field.Data);
        return true;
    }

    private static bool TryReadBytesField(PacketView packet, Nms1FieldType type, Span<byte> destination)
    {
        if (!TryFindField(packet, type, out var field) || !field.Found || field.Data.Length != destination.Length)
            return false;

        field.Data.CopyTo(destination);
        return true;
    }

    private static bool TryReadOptionalTextField(PacketView packet, Nms1FieldType type, int maxBytes, out string value)
    {
        value = string.Empty;
        if (!TryFindField(packet, type, out var field))
            return false;

        if (!field.Found)
            return true;

        if (field.Data.Length > maxBytes)
            return false;

        foreach (var b in field.Data)
        {
            if (b == 0 || (b < 0x20 && b != (byte)'\t'))
                return false;
        }

        value = Encoding.UTF8.GetString(field.Data);
        return true;
    }

    private static bool TryReadErrorResponse(PacketView packet, Nms1ErrorResponse? error)
    {
        if (error is null)
            return true;

        error.Code = 0;
        error.Text = string.Empty;

        if (!TryReadU16Field(packet, Nms1FieldType.ErrorCode, out var code))
            return false;

        if (!TryReadOptionalTextField(packet, Nms1FieldType.ErrorText, 96, out var text))
            return false;

        error.Code = code;
        error.Text = text;
        return true;
    }

    private sealed class PacketWriter
    {
        private readonly byte[] _buffer;
        private int _size;

        public PacketWriter(Span<byte> buffer, Nms1MessageType type, uint requestId)
        {
            if (buffer.Length < MasterProtocol.Nms1MaxPacketSize)
                throw new ArgumentException("NMS1 buffer is too small", nameof(buffer));

            _buffer = buffer.ToArray();
            _size = MasterProtocol.Nms1HeaderSize;
            Array.Clear(_buffer);
            Encoding.ASCII.GetBytes(MasterProtocol.Nms1Magic, _buffer);
            _buffer[4] = MasterProtocol.Nms1Version;
            _buffer[5] = (byte)type;
            BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(6), 0);
            BinaryPrimitives.WriteUInt32BigEndian(_buffer.AsSpan(8), requestId);
            BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(12), 0);
            BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(14), 0);
        }

        public bool WriteU8(Nms1FieldType type, byte value)
        {
            Span<byte> bytes = stackalloc byte[1];
            bytes[0] = value;
            return WriteField(type, bytes);
        }

        public bool WriteU16(Nms1FieldType type, ushort value)
        {
            Span<byte> bytes = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            return WriteField(type, bytes);
        }

        public bool WriteU32(Nms1FieldType type, uint value)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            return WriteField(type, bytes);
        }

        public bool WriteBytes(Nms1FieldType type, ReadOnlySpan<byte> data) => WriteField(type, data);

        public bool WriteText(Nms1FieldType type, string value, int maxBytes)
        {
            if (!TextFits(value, maxBytes))
                return false;

            return WriteField(type, Encoding.UTF8.GetBytes(value));
        }

        public bool Finish(Span<byte> destination, out int length)
        {
            length = 0;
            if (_size < MasterProtocol.Nms1HeaderSize || _size > MasterProtocol.Nms1MaxPacketSize)
                return false;

            var payloadLength = _size - MasterProtocol.Nms1HeaderSize;
            if (payloadLength > ushort.MaxValue)
                return false;

            BinaryPrimitives.WriteUInt16BigEndian(_buffer.AsSpan(12), (ushort)payloadLength);
            if (destination.Length < _size)
                return false;

            _buffer.AsSpan(0, _size).CopyTo(destination);
            length = _size;
            return true;
        }

        private bool WriteField(Nms1FieldType type, ReadOnlySpan<byte> data)
        {
            if (data.Length > ushort.MaxValue || _size + 4 + data.Length > MasterProtocol.Nms1MaxPacketSize)
                return false;

            var fieldType = (ushort)type;
            _buffer[_size] = (byte)(fieldType >> 8);
            _buffer[_size + 1] = (byte)fieldType;
            _buffer[_size + 2] = (byte)(data.Length >> 8);
            _buffer[_size + 3] = (byte)data.Length;
            _size += 4;
            if (!data.IsEmpty)
            {
                data.CopyTo(_buffer.AsSpan(_size));
                _size += data.Length;
            }

            return true;
        }
    }
}
