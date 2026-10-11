using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerGodMode2Archive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.PlayerGodMode2)) return archive;
        var size = checked(12 + state.Actors.Count);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var trailer = bytes.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var index = 0; index < state.Actors.Count; index++) trailer[8 + index] = state.Actors[index].PlayerGodMode2 ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 154);
        return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-god-mode2-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 153 || count < 0 || (long)count + 12 != size) return false;
        for (var index = 0; index < count; index++) if (trailer[8 + index] > 1) return false;
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-god-mode2-count"; return false; }
        for (var index = 0; index < count; index++) state.Actors[index].PlayerGodMode2 = trailer[8 + index] == 1;
        error = null; return true;
    }
}
