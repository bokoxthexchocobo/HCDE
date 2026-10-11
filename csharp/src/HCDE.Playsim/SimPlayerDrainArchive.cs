using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerDrainArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => !double.IsFinite(actor.PlayerDrainStrength)))
            throw new InvalidOperationException("Invalid saved player drain strength.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.PlayerDrainStrength != 0)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var index = 0; index < state.Actors.Count; index++)
            BinaryPrimitives.WriteDoubleLittleEndian(trailer[(8 + index * 8)..], state.Actors[index].PlayerDrainStrength);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 153); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-drain-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 152 || count < 0 || (long)count * 8 + 12 != size) return false;
        for (var index = 0; index < count; index++)
            if (!double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(trailer[(8 + index * 8)..]))) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-drain-count"; return false; }
        for (var index = 0; index < count; index++)
            state.Actors[index].PlayerDrainStrength = BinaryPrimitives.ReadDoubleLittleEndian(trailer[(8 + index * 8)..]);
        error = null; return true;
    }
}
