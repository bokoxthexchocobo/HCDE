using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace HCDE.MapLoader;

public sealed record MapBehaviorLocalLayout(int LocalVariableCount, IReadOnlyList<int> ArraySizes);

/// <summary>Decodes SVCT/SARY from an already-delimited enhanced ACS chunk region.</summary>
public static class MapBehaviorLocalLayoutCodec
{
    public static bool TryReadChunks(ReadOnlySpan<byte> chunks,
        out IReadOnlyDictionary<int, MapBehaviorLocalLayout>? layouts, out string? error)
    {
        layouts = null; error = null;
        var counts = new Dictionary<int, int>();
        var arrays = new List<(int Script, int[] Sizes)>();
        var foundCounts = false;
        while (!chunks.IsEmpty)
        {
            if (chunks.Length < 8) { error = "behavior-local-chunk-header-truncated"; return false; }
            var size = BinaryPrimitives.ReadInt32LittleEndian(chunks[4..]);
            if (size < 0 || size > chunks.Length - 8) { error = "behavior-local-chunk-truncated"; return false; }
            var payload = chunks.Slice(8, size);
            if (chunks[..4].SequenceEqual("SVCT"u8) && !foundCounts)
            {
                foundCounts = true;
                if (size % 4 != 0) { error = "behavior-svct-size-mismatch"; return false; }
                for (var i = 0; i < size; i += 4)
                    counts[BinaryPrimitives.ReadInt16LittleEndian(payload[i..])] = BinaryPrimitives.ReadUInt16LittleEndian(payload[(i + 2)..]);
            }
            else if (chunks[..4].SequenceEqual("SARY"u8))
            {
                if (size < 6 || (size - 2) % 4 != 0) { error = "behavior-sary-size-mismatch"; return false; }
                var sizes = new int[(size - 2) / 4];
                for (var i = 0; i < sizes.Length; i++)
                {
                    sizes[i] = BinaryPrimitives.ReadInt32LittleEndian(payload[(2 + i * 4)..]);
                    if (sizes[i] < 0) { error = "behavior-local-array-size-unsupported"; return false; }
                }
                arrays.Add((BinaryPrimitives.ReadInt16LittleEndian(payload), sizes));
            }
            chunks = chunks[(8 + size)..];
        }
        var result = counts.ToDictionary(p => p.Key, p => new MapBehaviorLocalLayout(p.Value, Array.Empty<int>()));
        foreach (var entry in arrays)
        {
            // Native SVCT is applied before all SARY chunks. Repeated SARY appends
            // to the prior total storage but replaces the visible array descriptors.
            long offset = 20;
            if (result.TryGetValue(entry.Script, out var previous))
                offset = previous.LocalVariableCount + previous.ArraySizes.Sum(size => (long)size);
            if (offset + entry.Sizes.Sum(size => (long)size) > int.MaxValue)
            { error = "behavior-local-layout-overflow"; return false; }
            result[entry.Script] = new((int)offset, Array.AsReadOnly(entry.Sizes));
        }
        layouts = new ReadOnlyDictionary<int, MapBehaviorLocalLayout>(result);
        return true;
    }
}
