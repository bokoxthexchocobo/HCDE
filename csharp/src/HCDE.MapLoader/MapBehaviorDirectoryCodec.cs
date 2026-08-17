using System.Buffers.Binary;

namespace HCDE.MapLoader;

public readonly struct MapBehaviorScriptEntry
{
    public MapBehaviorScriptEntry(int number, byte type, byte argCount, uint address)
    {
        Number = number;
        Type = type;
        ArgCount = argCount;
        Address = address;
    }

    public int Number { get; }
    public byte Type { get; }
    public byte ArgCount { get; }
    public uint Address { get; }
}

public static class MapBehaviorDirectoryCodec
{
    private const uint AcsOldMagic = 0x00005343; // "ACS\0"
    private const uint AcsEnhancedTag = 0x45534341; // "ACSE"
    private const uint AcsLittleEnhancedTag = 0x65534341; // "ACSe"
    private const uint ScriptPointerChunkId = 0x52545053; // "SPTR"

    public static bool TryReadScripts(
        ReadOnlySpan<byte> data,
        MapBehaviorFormat format,
        uint directoryOffset,
        out IReadOnlyList<MapBehaviorScriptEntry> scripts,
        out string? rejectReason)
    {
        scripts = Array.Empty<MapBehaviorScriptEntry>();
        rejectReason = null;

        if (data.Length < MapBehaviorRecord.MinLumpSize)
        {
            rejectReason = "behavior-lump-too-small";
            return false;
        }

        var effectiveFormat = ResolveEffectiveFormat(data, format, directoryOffset);
        return effectiveFormat switch
        {
            MapBehaviorFormat.AcsOld => TryReadOldScripts(data, directoryOffset, out scripts, out rejectReason),
            MapBehaviorFormat.AcsEnhanced or MapBehaviorFormat.AcsLittleEnhanced
                => TryReadEnhancedScripts(data, directoryOffset, out scripts, out rejectReason),
            _ => Reject("behavior-format-unknown", out scripts, out rejectReason),
        };
    }

    private static MapBehaviorFormat ResolveEffectiveFormat(
        ReadOnlySpan<byte> data,
        MapBehaviorFormat format,
        uint directoryOffset)
    {
        if (format != MapBehaviorFormat.AcsOld || directoryOffset < 24 || directoryOffset + 4 > data.Length)
            return format;

        var pretag = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice((int)directoryOffset - 4, 4));
        if (pretag == AcsEnhancedTag)
            return MapBehaviorFormat.AcsEnhanced;
        if (pretag == AcsLittleEnhancedTag)
            return MapBehaviorFormat.AcsLittleEnhanced;

        return format;
    }

    private static bool TryReadOldScripts(
        ReadOnlySpan<byte> data,
        uint directoryOffset,
        out IReadOnlyList<MapBehaviorScriptEntry> scripts,
        out string? rejectReason)
    {
        scripts = Array.Empty<MapBehaviorScriptEntry>();
        rejectReason = null;

        if (directoryOffset + 4 > data.Length)
            return Reject("behavior-directory-out-of-range", out scripts, out rejectReason);

        var scriptCount = BinaryPrimitives.ReadInt32LittleEndian(data[(int)directoryOffset..]);
        if (scriptCount < 0)
            return Reject("behavior-script-count-negative", out scripts, out rejectReason);

        var cursor = (int)directoryOffset + 4;
        var required = cursor + scriptCount * 12;
        if (required > data.Length)
            return Reject("behavior-script-directory-truncated", out scripts, out rejectReason);

        if (scriptCount == 0)
            return true;

        var entries = new MapBehaviorScriptEntry[scriptCount];
        for (var i = 0; i < scriptCount; i++)
        {
            var packedNumber = BinaryPrimitives.ReadInt32LittleEndian(data[cursor..]);
            entries[i] = new MapBehaviorScriptEntry(
                packedNumber % 1000,
                (byte)(packedNumber / 1000),
                (byte)BinaryPrimitives.ReadInt32LittleEndian(data[(cursor + 8)..]),
                BinaryPrimitives.ReadUInt32LittleEndian(data[(cursor + 4)..]));
            cursor += 12;
        }

        scripts = entries;
        return true;
    }

    private static bool TryReadEnhancedScripts(
        ReadOnlySpan<byte> data,
        uint directoryOffset,
        out IReadOnlyList<MapBehaviorScriptEntry> scripts,
        out string? rejectReason)
    {
        scripts = Array.Empty<MapBehaviorScriptEntry>();
        rejectReason = null;

        if (directoryOffset >= data.Length)
            return Reject("behavior-chunk-table-out-of-range", out scripts, out rejectReason);

        if (!TryFindChunk(data, directoryOffset, ScriptPointerChunkId, out var chunk, out rejectReason))
            return false;

        if (chunk.Length < 8)
            return Reject("behavior-sptr-chunk-too-small", out scripts, out rejectReason);

        var payloadSize = BinaryPrimitives.ReadInt32LittleEndian(chunk[4..]);
        if (payloadSize < 0 || 8 + payloadSize > chunk.Length)
            return Reject("behavior-sptr-chunk-truncated", out scripts, out rejectReason);

        var payload = chunk[8..(8 + payloadSize)];
        var usesCompactScriptPointers = BinaryPrimitives.ReadUInt32LittleEndian(data) == AcsOldMagic;
        var entrySize = usesCompactScriptPointers ? 8 : 12;
        if (payload.Length % entrySize != 0)
            return Reject("behavior-sptr-payload-size-mismatch", out scripts, out rejectReason);

        var scriptCount = payload.Length / entrySize;
        if (scriptCount == 0)
            return true;

        var entries = new MapBehaviorScriptEntry[scriptCount];
        var cursor = 0;
        for (var i = 0; i < scriptCount; i++)
        {
            if (usesCompactScriptPointers)
            {
                entries[i] = new MapBehaviorScriptEntry(
                    BinaryPrimitives.ReadInt16LittleEndian(payload[cursor..]),
                    payload[cursor + 2],
                    payload[cursor + 3],
                    BinaryPrimitives.ReadUInt32LittleEndian(payload[(cursor + 4)..]));
            }
            else
            {
                entries[i] = new MapBehaviorScriptEntry(
                    BinaryPrimitives.ReadInt16LittleEndian(payload[cursor..]),
                    (byte)BinaryPrimitives.ReadUInt16LittleEndian(payload[(cursor + 2)..]),
                    (byte)BinaryPrimitives.ReadInt32LittleEndian(payload[(cursor + 8)..]),
                    BinaryPrimitives.ReadUInt32LittleEndian(payload[(cursor + 4)..]));
            }

            cursor += entrySize;
        }

        scripts = entries;
        return true;
    }

    private static bool TryFindChunk(
        ReadOnlySpan<byte> data,
        uint chunkOffset,
        uint chunkId,
        out ReadOnlySpan<byte> chunk,
        out string? rejectReason)
    {
        chunk = ReadOnlySpan<byte>.Empty;
        rejectReason = null;
        var cursor = (int)chunkOffset;

        while (cursor + 8 <= data.Length)
        {
            var id = BinaryPrimitives.ReadUInt32LittleEndian(data[cursor..]);
            var size = BinaryPrimitives.ReadInt32LittleEndian(data[(cursor + 4)..]);
            if (size < 0 || cursor + 8 + size > data.Length)
            {
                rejectReason = "behavior-chunk-truncated";
                return false;
            }

            if (id == chunkId)
            {
                chunk = data.Slice(cursor, 8 + size);
                return true;
            }

            cursor += 8 + size;
        }

        rejectReason = "behavior-sptr-chunk-missing";
        return false;
    }

    private static bool Reject(
        string reason,
        out IReadOnlyList<MapBehaviorScriptEntry> scripts,
        out string? rejectReason)
    {
        scripts = Array.Empty<MapBehaviorScriptEntry>();
        rejectReason = reason;
        return false;
    }
}
