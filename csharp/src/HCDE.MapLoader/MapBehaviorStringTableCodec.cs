using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader;

/// <summary>Reads ACS BEHAVIOR string tables for old lumps and enhanced <c>STRL</c> chunks.</summary>
public static class MapBehaviorStringTableCodec
{
    private const uint StrlChunkId = 0x4C525453; // "STRL"
    private const uint StreChunkId = 0x45545253; // "STRE"

    public static bool TryRead(
        ReadOnlySpan<byte> data,
        MapBehaviorFormat format,
        uint directoryOffset,
        out string[] strings,
        out string? error)
    {
        strings = Array.Empty<string>();
        error = null;
        if (format == MapBehaviorFormat.AcsOld)
            return TryReadOld(data, directoryOffset, out strings, out error);

        if (format is MapBehaviorFormat.AcsEnhanced or MapBehaviorFormat.AcsLittleEnhanced)
            return TryReadEnhanced(data, directoryOffset, out strings, out error);

        error = "behavior-format-unknown";
        return false;
    }

    private static bool TryReadOld(ReadOnlySpan<byte> data, uint directoryOffset, out string[] strings, out string? error)
    {
        strings = Array.Empty<string>();
        error = null;
        if ((ulong)directoryOffset + 4 > (ulong)data.Length)
            return Reject("behavior-directory-out-of-range", out strings, out error);

        var scriptCount = BinaryPrimitives.ReadInt32LittleEndian(data[(int)directoryOffset..]);
        if (scriptCount < 0)
            return Reject("behavior-script-count-negative", out strings, out error);

        var tableStart = (long)directoryOffset + 4 + scriptCount * 12L;
        if (tableStart > data.Length)
            return true;

        if (!TryMinimumScriptAddress(data, directoryOffset, scriptCount, out var minScriptAddress, out error))
            return false;

        if (tableStart >= minScriptAddress)
            return true;

        return TryReadUnpaddedTable(data, (int)tableStart, dataBase: 0, (int)minScriptAddress, out strings, out error);
    }

    private static bool TryMinimumScriptAddress(
        ReadOnlySpan<byte> data,
        uint directoryOffset,
        int scriptCount,
        out int minScriptAddress,
        out string? error)
    {
        minScriptAddress = data.Length;
        error = null;
        var cursor = (int)directoryOffset + 4;
        for (var i = 0; i < scriptCount; i++)
        {
            if (cursor + 12 > data.Length)
                return Reject("behavior-script-directory-truncated", out minScriptAddress, out error);
            var address = BinaryPrimitives.ReadUInt32LittleEndian(data[(cursor + 4)..]);
            if (address < (uint)minScriptAddress)
                minScriptAddress = (int)address;
            cursor += 12;
        }

        return true;
    }

    private static bool TryReadEnhanced(ReadOnlySpan<byte> data, uint directoryOffset, out string[] strings, out string? error)
    {
        strings = Array.Empty<string>();
        error = null;
        var cursor = (int)directoryOffset;
        while (cursor + 8 <= data.Length)
        {
            var id = BinaryPrimitives.ReadUInt32LittleEndian(data[cursor..]);
            var size = BinaryPrimitives.ReadInt32LittleEndian(data[(cursor + 4)..]);
            if (size < 0 || cursor + 8 + size > data.Length)
                return Reject("behavior-chunk-truncated", out strings, out error);

            if (id == StreChunkId)
            {
                var payload = data.Slice(cursor + 8, size).ToArray();
                if (!TryDecryptStre(payload, out error))
                    return Reject(error!, out strings, out error);
                if (!TryReadPaddedTable(payload, out strings, out error))
                    return false;
                return true;
            }

            if (id == StrlChunkId)
            {
                var payload = data.Slice(cursor + 8, size);
                return TryReadPaddedTable(payload, out strings, out error);
            }

            cursor += 8 + size;
        }

        return true;
    }

    private static bool TryReadPaddedTable(ReadOnlySpan<byte> table, out string[] strings, out string? error)
    {
        strings = Array.Empty<string>();
        error = null;
        if (table.IsEmpty)
            return true;
        if (table.Length < 12)
            return Reject("behavior-string-table-truncated", out strings, out error);

        var count = BinaryPrimitives.ReadInt32LittleEndian(table[4..]);
        if (count < 0)
            return Reject("behavior-string-count-negative", out strings, out error);
        if (count == 0)
            return true;

        var headerBytes = 12 + count * 4;
        if (headerBytes > table.Length)
            return Reject("behavior-string-table-truncated", out strings, out error);

        return ReadStrings(table, table, dataBase: 0, count, offsetIndex: 3, table.Length, out strings, out error);
    }

    private static bool TryReadUnpaddedTable(
        ReadOnlySpan<byte> data,
        int tableStart,
        int dataBase,
        int maxExclusive,
        out string[] strings,
        out string? error)
    {
        strings = Array.Empty<string>();
        error = null;
        if (tableStart + 4 > data.Length || tableStart + 4 > maxExclusive)
            return true;

        var count = BinaryPrimitives.ReadInt32LittleEndian(data[tableStart..]);
        if (count < 0)
            return Reject("behavior-string-count-negative", out strings, out error);
        if (count == 0)
            return true;

        var headerBytes = 4 + count * 4;
        if (tableStart + headerBytes > data.Length || tableStart + headerBytes > maxExclusive)
            return Reject("behavior-string-table-truncated", out strings, out error);

        return ReadStrings(data, data.Slice(tableStart), dataBase, count, offsetIndex: 1, maxExclusive, out strings, out error);
    }

    private static bool ReadStrings(
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> header,
        int dataBase,
        int count,
        int offsetIndex,
        int maxExclusive,
        out string[] strings,
        out string? error)
    {
        strings = new string[count];
        for (var i = 0; i < count; i++)
        {
            var offset = BinaryPrimitives.ReadInt32LittleEndian(header.Slice((offsetIndex + i) * 4, 4));
            var absolute = dataBase + offset;
            if (absolute < 0 || absolute >= data.Length || absolute >= maxExclusive)
                return Reject("behavior-string-offset-out-of-range", out strings, out error);
            if (!TryReadCString(data, absolute, out var raw, out error))
                return false;
            strings[i] = AcsStringEscapes.DecodeStrBin(raw);
        }

        error = null;
        return true;
    }

    private static bool TryReadCString(ReadOnlySpan<byte> data, int offset, out ReadOnlySpan<byte> raw, out string? error)
    {
        error = null;
        var end = offset;
        while (end < data.Length && data[end] != 0)
            end++;
        if (end >= data.Length)
        {
            raw = ReadOnlySpan<byte>.Empty;
            return Reject("behavior-string-unterminated", out raw, out error);
        }

        raw = data.Slice(offset, end - offset);
        return true;
    }

    private static bool TryDecryptStre(Span<byte> payload, out string? error)
    {
        error = null;
        if (payload.Length < 12)
            return Reject("behavior-stre-truncated", out error);

        // Native FBehavior::UnencryptStrings: count at chunk[3], offsets at chunk[5+strnum], ciphertext at chunk+ofs+8 (payload base + ofs).
        var count = BinaryPrimitives.ReadInt32LittleEndian(payload[4..]);
        if (count < 0)
            return Reject("behavior-string-count-negative", out error);

        var headerBytes = 12 + count * 4;
        if (headerBytes > payload.Length)
            return Reject("behavior-stre-truncated", out error);

        for (var strnum = 0; strnum < count; strnum++)
        {
            var ofs = BinaryPrimitives.ReadInt32LittleEndian(payload[(12 + strnum * 4)..]);
            var dataIndex = ofs;
            if (dataIndex < 0 || dataIndex >= payload.Length)
                return Reject("behavior-stre-offset-out-of-range", out error);

            var p = (byte)(ofs * 157135);
            var i = 0;
            var terminated = false;
            while (dataIndex + i < payload.Length)
            {
                payload[dataIndex + i] ^= (byte)(p + (i >> 1));
                i++;
                if (payload[dataIndex + i - 1] == 0)
                {
                    terminated = true;
                    break;
                }
            }

            if (!terminated)
                return Reject("behavior-stre-unterminated", out error);
        }

        return true;
    }

    private static bool Reject(string reason, out string[] strings, out string? error)
    {
        strings = Array.Empty<string>();
        error = reason;
        return false;
    }

    private static bool Reject(string reason, out int minScriptAddress, out string? error)
    {
        minScriptAddress = 0;
        error = reason;
        return false;
    }

    private static bool Reject(string reason, out ReadOnlySpan<byte> raw, out string? error)
    {
        raw = ReadOnlySpan<byte>.Empty;
        error = reason;
        return false;
    }

    private static bool Reject(string reason, out string? error)
    {
        error = reason;
        return false;
    }
}

internal static class AcsStringEscapes
{
    public static string DecodeStrBin(ReadOnlySpan<byte> raw)
    {
        if (raw.IsEmpty)
            return string.Empty;

        var builder = new StringBuilder(raw.Length);
        for (var i = 0; i < raw.Length; i++)
        {
            var c = raw[i];
            if (c != (byte)'\\')
            {
                builder.Append((char)c);
                continue;
            }

            if (i + 1 >= raw.Length)
                break;

            i++;
            switch (raw[i])
            {
                case (byte)'a': builder.Append('\a'); break;
                case (byte)'b': builder.Append('\b'); break;
                case (byte)'c': builder.Append('\u001c'); break;
                case (byte)'f': builder.Append('\f'); break;
                case (byte)'n': builder.Append('\n'); break;
                case (byte)'t': builder.Append('\t'); break;
                case (byte)'r': builder.Append('\r'); break;
                case (byte)'v': builder.Append('\v'); break;
                case (byte)'?': builder.Append('?'); break;
                case (byte)'\n': break;
                case (byte)'x':
                case (byte)'X':
                    builder.Append((char)ReadHexByte(raw, ref i));
                    break;
                case >= (byte)'0' and <= (byte)'7':
                    builder.Append((char)ReadOctalByte(raw, ref i));
                    break;
                default:
                    builder.Append((char)raw[i]);
                    break;
            }
        }

        return builder.ToString();
    }

    private static int ReadHexByte(ReadOnlySpan<byte> raw, ref int index)
    {
        var value = 0;
        for (var digit = 0; digit < 2 && index + 1 < raw.Length; digit++)
        {
            var next = raw[index + 1];
            int? nibble = next switch
            {
                >= (byte)'0' and <= (byte)'9' => next - '0',
                >= (byte)'a' and <= (byte)'f' => next - 'a' + 10,
                >= (byte)'A' and <= (byte)'F' => next - 'A' + 10,
                _ => null,
            };
            if (nibble is null)
                break;
            value = (value << 4) + nibble.Value;
            index++;
        }

        return value;
    }

    private static int ReadOctalByte(ReadOnlySpan<byte> raw, ref int index)
    {
        var value = raw[index] - (byte)'0';
        for (var digit = 0; digit < 2 && index + 1 < raw.Length; digit++)
        {
            var next = raw[index + 1];
            if (next < (byte)'0' || next > (byte)'7')
                break;
            value = (value << 3) + (next - '0');
            index++;
        }

        return value;
    }
}
