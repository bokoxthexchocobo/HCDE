using System.Buffers.Binary;

namespace HCDE.MapLoader;

/// <summary>Reads JUMP from an already-delimited enhanced ACS chunk region.</summary>
public static class MapBehaviorJumpTableCodec
{
    public const int MaximumJumpPoints = 1_048_576;

    public static bool TryReadChunks(ReadOnlySpan<byte> chunks, out IReadOnlyList<uint>? targets, out string? error)
    {
        targets = null; error = null;
        uint[]? found = null;
        while (!chunks.IsEmpty)
        {
            if (chunks.Length < 8) { error = "behavior-jump-chunk-header-truncated"; return false; }
            var size = BinaryPrimitives.ReadInt32LittleEndian(chunks[4..]);
            if (size < 0 || size > chunks.Length - 8) { error = "behavior-jump-chunk-truncated"; return false; }
            if (chunks[..4].SequenceEqual("JUMP"u8) && found is null)
            {
                if (size % 4 != 0) { error = "behavior-jump-size-mismatch"; return false; }
                if (size / 4 > MaximumJumpPoints) { error = "behavior-jump-limit"; return false; }
                found = new uint[size / 4];
                for (var i = 0; i < found.Length; i++)
                    found[i] = BinaryPrimitives.ReadUInt32LittleEndian(chunks[(8 + i * 4)..]);
            }
            chunks = chunks[(8 + size)..];
        }
        targets = Array.AsReadOnly(found ?? Array.Empty<uint>());
        return true;
    }

    /// <summary>Relocates module offsets into a contained word-format code region; cross-region targets are unsupported.</summary>
    public static bool TryRelocate(IReadOnlyList<uint> targets, int codeOffset, int codeLength,
        out int[]? jumpPoints, out string? error)
    {
        ArgumentNullException.ThrowIfNull(targets);
        jumpPoints = null; error = null;
        if (codeOffset < 0 || codeLength < 0 || targets.Count > MaximumJumpPoints)
        { error = "behavior-jump-region-invalid"; return false; }
        var relocated = new int[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var relative = (long)targets[i] - codeOffset;
            if (relative < 0 || relative > (long)codeLength - 4)
            { error = "behavior-jump-target-outside-code"; return false; }
            relocated[i] = (int)relative;
        }
        jumpPoints = relocated;
        return true;
    }
}
