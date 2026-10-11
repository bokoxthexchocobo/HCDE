using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerDamageCountArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.PlayerDamageCount != 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var index = 0; index < state.Actors.Count; index++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + index * 4)..], state.Actors[index].PlayerDamageCount);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 159); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-damage-count-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 158 || count < 0 || (long)count * 4 + 12 != size) return false;
        for (var index = 0; index < count; index++) if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + index * 4)..]) is < 0 or > 100) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-damage-count-count"; return false; }
        for (var index = 0; index < count; index++)
            state.Actors[index].PlayerDamageCount = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + index * 4)..]);
        error = null; return true;
    }
}
