using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimMapSpawnArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.MapSpawnRandomState.HasValue && !state.Actors.Any(a => a.SynchronizedFlag.HasValue)) return archive;
        var size = checked(24 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.MapSpawnRandomState.HasValue ? 1 : 0);
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[8..], state.MapSpawnRandomState ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[16..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * 4)..], state.Actors[i].SynchronizedFlag ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 112);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 24 || size > bytes.Length - 16) { error = "save-mapspawn-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var present = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[16..]);
        if (prior < 15 || prior > 111 || count < 0 || (long)count * 4 + 24 != size)
        { error = "save-mapspawn-header"; return false; }
        if (present is < 0 or > 1) { error = "save-mapspawn-value"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]) is < 0 or > 1)
            { error = "save-mapspawn-value"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-mapspawn-count"; return false; }
        state.MapSpawnRandomState = present == 1 ? BinaryPrimitives.ReadUInt64LittleEndian(trailer[8..]) : null;
        for (var i = 0; i < count; i++)
            state.Actors[i].SynchronizedFlag = BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]) == 1 ? 1 : null;
        return true;
    }
}
