using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimIceChunkArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.IncludesIceChunkLifecycle && !state.Actors.Any(a => a.IceChunkLifecycle.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, state.Actors[i].IceChunkLifecycle.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], state.Actors[i].IceChunkLifecycle ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 114);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-icechunk-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 113 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-icechunk-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]) is < 0 or > 1
                || BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 8)..]) is < 0 or > 3
                || BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]) == 1
                    && BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 8)..]) is not (1 or 3))
            { error = "save-icechunk-value"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-icechunk-count"; return false; }
        state.IncludesIceChunkLifecycle = true;
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            state.Actors[i].IceChunkLifecycle = BinaryPrimitives.ReadInt32LittleEndian(item) == 1
                ? BinaryPrimitives.ReadInt32LittleEndian(item[4..]) : null;
        }
        return true;
    }
}
