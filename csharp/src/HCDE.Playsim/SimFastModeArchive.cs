using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimFastModeArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.FastModeFlags.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, state.Actors[i].FastModeFlags.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], state.Actors[i].FastModeFlags ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 108);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-fast-mode-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 107 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-fast-mode-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]) is < 0 or > 1
                || BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 8)..]) is < 0 or > 3)
            { error = "save-fast-mode-value"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-fast-mode-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            state.Actors[i].FastModeFlags = BinaryPrimitives.ReadInt32LittleEndian(item) == 1
                ? BinaryPrimitives.ReadInt32LittleEndian(item[4..]) : null;
        }
        return true;
    }
}
