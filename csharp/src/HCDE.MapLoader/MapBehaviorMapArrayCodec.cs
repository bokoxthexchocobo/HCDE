using System.Buffers.Binary;

namespace HCDE.MapLoader;

public sealed record MapBehaviorMapArrayDeclaration(int MapVariable, uint Length);
public sealed record MapBehaviorMapArrayInitializer(int MapVariable, IReadOnlyList<int> Values);
public sealed record MapBehaviorMapVariableInitializer(int FirstVariable, IReadOnlyList<int> Values);
public sealed record MapBehaviorMapArrayMetadata(
    IReadOnlyList<MapBehaviorMapArrayDeclaration> Declarations,
    IReadOnlyList<MapBehaviorMapArrayInitializer> Initializers)
{
    public IReadOnlyList<MapBehaviorMapVariableInitializer> MapInitializers { get; init; } = Array.Empty<MapBehaviorMapVariableInitializer>();
}

/// <summary>Reads an already-delimited enhanced ACS chunk region, not an entire BEHAVIOR lump.</summary>
public static class MapBehaviorMapArrayCodec
{
    public static bool TryReadChunks(ReadOnlySpan<byte> chunks, out MapBehaviorMapArrayMetadata? metadata, out string? error)
    {
        metadata = null; error = null;
        var declarations = new List<MapBehaviorMapArrayDeclaration>();
        var initializers = new List<MapBehaviorMapArrayInitializer>();
        var mapInitializers = new List<MapBehaviorMapVariableInitializer>();
        var foundDeclarations = false;
        while (!chunks.IsEmpty)
        {
            if (chunks.Length < 8) { error = "behavior-array-chunk-header-truncated"; return false; }
            var size = BinaryPrimitives.ReadInt32LittleEndian(chunks[4..]);
            if (size < 0 || size > chunks.Length - 8) { error = "behavior-array-chunk-truncated"; return false; }
            var payload = chunks.Slice(8, size);
            if (chunks[..4].SequenceEqual("ARAY"u8) && !foundDeclarations)
            {
                foundDeclarations = true; // Native FindChunk uses the first ARAY chunk.
                if (size % 8 != 0) { error = "behavior-aray-size-mismatch"; return false; }
                for (var offset = 0; offset < size; offset += 8)
                {
                    var variable = BinaryPrimitives.ReadInt32LittleEndian(payload[offset..]);
                    if ((uint)variable >= 128) { error = "behavior-array-map-variable-out-of-range"; return false; }
                    declarations.Add(new(variable, BinaryPrimitives.ReadUInt32LittleEndian(payload[(offset + 4)..])));
                }
            }
            else if (chunks[..4].SequenceEqual("MINI"u8))
            {
                if (size < 4 || size % 4 != 0) { error = "behavior-mini-size-mismatch"; return false; }
                var first = BinaryPrimitives.ReadInt32LittleEndian(payload);
                var count = (size - 4) / 4;
                if ((uint)first > 128 || count > 128 - first)
                { error = "behavior-mini-range-invalid"; return false; }
                var values = new int[count];
                for (var i = 0; i < count; i++) values[i] = BinaryPrimitives.ReadInt32LittleEndian(payload[(4 + i * 4)..]);
                mapInitializers.Add(new(first, Array.AsReadOnly(values)));
            }
            else if (chunks[..4].SequenceEqual("AINI"u8))
            {
                if (size < 4 || size % 4 != 0) { error = "behavior-aini-size-mismatch"; return false; }
                var variable = BinaryPrimitives.ReadInt32LittleEndian(payload);
                if ((uint)variable >= 128) { error = "behavior-array-map-variable-out-of-range"; return false; }
                var values = new int[(size - 4) / 4];
                for (var i = 0; i < values.Length; i++)
                    values[i] = BinaryPrimitives.ReadInt32LittleEndian(payload[(4 + i * 4)..]);
                initializers.Add(new(variable, Array.AsReadOnly(values)));
            }
            chunks = chunks[(8 + size)..];
        }
        // Keep raw variable references and initializer order; module binding applies them later.
        metadata = new(declarations.AsReadOnly(), initializers.AsReadOnly()) { MapInitializers = mapInitializers.AsReadOnly() };
        return true;
    }
}
