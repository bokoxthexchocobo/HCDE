using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimGravityArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.GravityRaw.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, state.Actors[i].GravityRaw.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], state.Actors[i].GravityRaw ?? 65536);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 70);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-gravity-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 69 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-gravity-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 8)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            if (present is not (0 or 1) || present == 0 && BinaryPrimitives.ReadInt32LittleEndian(item[4..]) != 65536)
            { error = "save-gravity-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-gravity-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].GravityRaw = BinaryPrimitives.ReadInt32LittleEndian(item[4..]);
        }
        return true;
    }
}
