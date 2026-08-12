using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class EchoStringCodec
{
    public static bool TryRead(ReadOnlySpan<byte> buffer, ref int cursor, out ReadOnlySpan<byte> stringBytes)
    {
        stringBytes = default;
        if (buffer.Length - cursor < 2)
            return false;

        var length = BinaryPrimitives.ReadUInt16BigEndian(buffer[cursor..]);
        cursor += 2;
        if (length > 255 || buffer.Length - cursor < length)
            return false;

        stringBytes = buffer.Slice(cursor, length);
        cursor += length;
        return true;
    }

    public static int Write(Span<byte> buffer, ref int cursor, ReadOnlySpan<byte> stringBytes)
    {
        if (stringBytes.Length > 255 || buffer.Length - cursor < 2 + stringBytes.Length)
            return 0;

        BinaryPrimitives.WriteUInt16BigEndian(buffer[cursor..], (ushort)stringBytes.Length);
        cursor += 2;
        stringBytes.CopyTo(buffer[cursor..]);
        cursor += stringBytes.Length;
        return 2 + stringBytes.Length;
    }
}

public readonly struct PresentationEchoHeader
{
    public PresentationEchoHeader(byte playerCount, byte inventoryPlayerSlot = LiveConstants.PresentationEchoInvalidInventorySlot)
    {
        PlayerCount = playerCount;
        InventoryPlayerSlot = inventoryPlayerSlot;
    }

    public byte PlayerCount { get; }
    public byte InventoryPlayerSlot { get; }
}

public static class PresentationEchoCodec
{
    public static int WriteMinimal(Span<byte> chunk)
    {
        if (chunk.Length < LiveConstants.PresentationEchoMinHeaderSize)
            return 0;

        LiveConstants.PresentationEchoMagic.CopyTo(chunk);
        chunk[4] = LiveConstants.PresentationEchoProtocolVersion;
        chunk[5] = 0;
        chunk[6] = LiveConstants.PresentationEchoInvalidInventorySlot;
        return LiveConstants.PresentationEchoMinHeaderSize;
    }

    public static bool TryReadAndSkip(
        ReadOnlySpan<byte> chunk,
        out PresentationEchoHeader header,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        bytesConsumed = 0;
        rejectReason = null;

        if (chunk.Length < LiveConstants.PresentationEchoMinHeaderSize)
        {
            rejectReason = "presentation-echo-truncated";
            return false;
        }

        if (!chunk[..4].SequenceEqual(LiveConstants.PresentationEchoMagic))
        {
            rejectReason = "presentation-echo-magic-mismatch";
            return false;
        }

        var version = chunk[4];
        if (version != LiveConstants.PresentationEchoProtocolVersion)
        {
            rejectReason = "presentation-echo-version-mismatch";
            return false;
        }

        var playerCount = chunk[5];
        var cursor = 6;
        var inventoryPlayer = chunk[cursor++];
        header = new PresentationEchoHeader(playerCount, inventoryPlayer);

        if (inventoryPlayer != LiveConstants.PresentationEchoInvalidInventorySlot)
        {
            if (chunk.Length - cursor < 2)
            {
                rejectReason = "presentation-echo-inventory-truncated";
                return false;
            }

            var itemCount = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
            cursor += 2;
            for (var i = 0; i < itemCount; i++)
            {
                if (chunk.Length - cursor < 7)
                {
                    rejectReason = "presentation-echo-item-truncated";
                    return false;
                }

                var flags = chunk[cursor++];
                cursor += 4;
                if (!EchoStringCodec.TryRead(chunk, ref cursor, out _))
                {
                    rejectReason = "presentation-echo-item-string-truncated";
                    return false;
                }

                if ((flags & 0x02) != 0)
                {
                    if (chunk.Length - cursor < 10)
                    {
                        rejectReason = "presentation-echo-armor-slots-truncated";
                        return false;
                    }

                    cursor += 10;
                }
            }
        }

        for (var p = 0; p < playerCount; p++)
        {
            if (chunk.Length - cursor < 25)
            {
                rejectReason = "presentation-echo-player-truncated";
                return false;
            }

            cursor += 25;
            if (!EchoStringCodec.TryRead(chunk, ref cursor, out _)
                || !EchoStringCodec.TryRead(chunk, ref cursor, out _))
            {
                rejectReason = "presentation-echo-player-string-truncated";
                return false;
            }

            if (chunk.Length - cursor < 1)
            {
                rejectReason = "presentation-echo-player-flags-truncated";
                return false;
            }

            cursor += 1;
        }

        bytesConsumed = cursor;
        return true;
    }
}
