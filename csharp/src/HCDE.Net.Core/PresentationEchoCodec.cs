using System.Buffers.Binary;
using System.Text;

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
        => Write(chunk, new PresentationEchoBlock(null, Array.Empty<PresentationEchoInventoryItem>(), Array.Empty<PresentationEchoPlayerRecord>()));

    public static int Write(Span<byte> chunk, PresentationEchoBlock block)
    {
        if (block.Players.Length > byte.MaxValue)
            return 0;

        var required = LiveConstants.PresentationEchoMinHeaderSize;
        if (block.InventoryPlayerSlot is byte)
        {
            required += 2;
            foreach (var item in block.InventoryItems)
            {
                required += 1 + 4 + 2 + item.ClassName.Length;
                if (item.IsArmor)
                    required += 10;
            }
        }

        foreach (var player in block.Players)
        {
            required += LiveConstants.PresentationEchoPlayerFixedPrefixSize
                + 2 + player.PspriteOwnerName.Length
                + 2 + player.ReadyWeaponName.Length
                + 1;
        }

        if (chunk.Length < required)
            return 0;

        var cursor = 0;
        LiveConstants.PresentationEchoMagic.CopyTo(chunk);
        chunk[4] = LiveConstants.PresentationEchoProtocolVersion;
        chunk[5] = (byte)block.Players.Length;
        cursor = 6;
        if (block.InventoryPlayerSlot is byte invSlot)
        {
            chunk[cursor++] = invSlot;
            BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], (ushort)block.InventoryItems.Length);
            cursor += 2;
            foreach (var item in block.InventoryItems)
            {
                chunk[cursor++] = item.Flags;
                BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], item.Amount);
                cursor += 4;
                if (EchoStringCodec.Write(chunk, ref cursor, item.ClassName) == 0)
                    return 0;

                if (item.IsArmor)
                {
                    if (item.HexenSlots.Length != 5)
                        return 0;

                    for (var slot = 0; slot < 5; slot++)
                    {
                        BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], item.HexenSlots[slot]);
                        cursor += 2;
                    }
                }
            }
        }
        else
        {
            chunk[cursor++] = LiveConstants.PresentationEchoInvalidInventorySlot;
        }

        foreach (var player in block.Players)
        {
            chunk[cursor++] = player.PlayerNum;
            BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], player.ReadyWeaponNameIndex);
            cursor += 4;
            BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], player.PendingWeaponNameIndex);
            cursor += 4;
            BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], player.PspriteStateNameIndex);
            cursor += 4;
            BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], unchecked((ushort)player.PspriteTics));
            cursor += 2;
            BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], player.WeaponState);
            cursor += 2;
            chunk[cursor++] = player.PlayerState;
            BinaryPrimitives.WriteUInt16BigEndian(chunk[cursor..], unchecked((ushort)player.ViewHeight));
            cursor += 2;
            BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], player.PspriteStateOffset);
            cursor += 4;
            if (EchoStringCodec.Write(chunk, ref cursor, player.PspriteOwnerName) == 0
                || EchoStringCodec.Write(chunk, ref cursor, player.ReadyWeaponName) == 0)
            {
                return 0;
            }

            chunk[cursor++] = player.WeaponChangeFlags;
        }

        return cursor;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out PresentationEchoBlock block,
        out int bytesConsumed,
        out string? rejectReason)
    {
        block = default;
        bytesConsumed = 0;
        rejectReason = null;

        if (!TryReadHeaderAndBody(chunk, out var header, out block, out bytesConsumed, out rejectReason))
            return false;

        _ = header;
        return true;
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
        return TryReadHeaderAndBody(chunk, out header, out _, out bytesConsumed, out rejectReason);
    }

    private static bool TryReadHeaderAndBody(
        ReadOnlySpan<byte> chunk,
        out PresentationEchoHeader header,
        out PresentationEchoBlock block,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        block = default;
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

        PresentationEchoInventoryItem[] inventoryItems = Array.Empty<PresentationEchoInventoryItem>();
        byte? inventorySlot = inventoryPlayer == LiveConstants.PresentationEchoInvalidInventorySlot
            ? null
            : inventoryPlayer;

        if (inventorySlot is byte invSlot)
        {
            if (chunk.Length - cursor < 2)
            {
                rejectReason = "presentation-echo-inventory-truncated";
                return false;
            }

            var itemCount = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
            cursor += 2;
            const int minBytesPerItem = 7;
            if (chunk.Length - cursor < itemCount * minBytesPerItem)
            {
                rejectReason = "presentation-echo-inventory-truncated";
                return false;
            }

            inventoryItems = new PresentationEchoInventoryItem[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                if (chunk.Length - cursor < 7)
                {
                    rejectReason = "presentation-echo-item-truncated";
                    return false;
                }

                var flags = chunk[cursor++];
                var amount = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
                cursor += 4;
                if (!EchoStringCodec.TryRead(chunk, ref cursor, out var className))
                {
                    rejectReason = "presentation-echo-item-string-truncated";
                    return false;
                }

                ushort[] hexenSlots = Array.Empty<ushort>();
                if ((flags & LiveConstants.PresentationEchoInventoryFlagArmor) != 0)
                {
                    if (chunk.Length - cursor < 10)
                    {
                        rejectReason = "presentation-echo-armor-slots-truncated";
                        return false;
                    }

                    hexenSlots = new ushort[5];
                    for (var slot = 0; slot < 5; slot++)
                    {
                        hexenSlots[slot] = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
                        cursor += 2;
                    }
                }

                inventoryItems[i] = new PresentationEchoInventoryItem(flags, amount, className, hexenSlots);
            }

            inventorySlot = invSlot;
        }

        var players = new PresentationEchoPlayerRecord[playerCount];
        for (var p = 0; p < playerCount; p++)
        {
            if (chunk.Length - cursor < LiveConstants.PresentationEchoPlayerFixedPrefixSize)
            {
                rejectReason = "presentation-echo-player-truncated";
                return false;
            }

            var playerNum = chunk[cursor++];
            var readyWeaponNameIndex = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            var pendingWeaponNameIndex = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            var pspriteStateNameIndex = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            var pspriteTics = unchecked((short)BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]));
            cursor += 2;
            var weaponState = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
            cursor += 2;
            var playerState = chunk[cursor++];
            var viewHeight = unchecked((short)BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]));
            cursor += 2;
            var pspriteStateOffset = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            if (!EchoStringCodec.TryRead(chunk, ref cursor, out var pspriteOwnerName)
                || !EchoStringCodec.TryRead(chunk, ref cursor, out var readyWeaponName))
            {
                rejectReason = "presentation-echo-player-string-truncated";
                return false;
            }

            if (chunk.Length - cursor < 1)
            {
                rejectReason = "presentation-echo-player-flags-truncated";
                return false;
            }

            var weaponChangeFlags = chunk[cursor++];
            players[p] = new PresentationEchoPlayerRecord(
                playerNum,
                readyWeaponNameIndex,
                pendingWeaponNameIndex,
                pspriteStateNameIndex,
                pspriteTics,
                weaponState,
                playerState,
                viewHeight,
                pspriteStateOffset,
                pspriteOwnerName,
                readyWeaponName,
                weaponChangeFlags);
        }

        block = new PresentationEchoBlock(inventorySlot, inventoryItems, players);
        bytesConsumed = cursor;
        return true;
    }

    public static PresentationEchoBlock CreateExampleBlock()
    {
        var inventory = new[]
        {
            new PresentationEchoInventoryItem(
                LiveConstants.PresentationEchoInventoryFlagWeapon,
                amount: 1,
                Encoding.UTF8.GetBytes("Pistol")),
            new PresentationEchoInventoryItem(
                LiveConstants.PresentationEchoInventoryFlagArmor,
                amount: 100,
                Encoding.UTF8.GetBytes("GreenArmor"),
                new ushort[] { 10, 20, 30, 40, 50 }),
        };
        var players = new[]
        {
            new PresentationEchoPlayerRecord(
                playerNum: 0,
                readyWeaponNameIndex: 42,
                pendingWeaponNameIndex: 0xFFFFFFFF,
                pspriteStateNameIndex: 7,
                pspriteTics: 4,
                weaponState: 1,
                playerState: 1,
                viewHeight: 41 * 256,
                pspriteStateOffset: 12,
                Encoding.UTF8.GetBytes("Pistol"),
                Encoding.UTF8.GetBytes("Pistol"),
                LiveConstants.WeaponChangeReadyClass),
        };
        return new PresentationEchoBlock(inventoryPlayerSlot: 0, inventory, players);
    }
}
