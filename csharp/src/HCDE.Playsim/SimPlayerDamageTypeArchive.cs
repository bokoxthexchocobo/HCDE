using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimPlayerDamageTypeArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.PlayerLastDamageType is not null)) return archive;
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            if (actor.PlayerLastDamageType is not { } type) { writer.Write(-1); continue; }
            var data = Encoding.GetBytes(type); writer.Write(data.Length); writer.Write(data);
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 125); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-damage-type-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 124 || count < 0 || (long)count * 4 + 8 > trailer.Length) return false;
        var names = new string?[count]; var offset = 8;
        for (var index = 0; index < count; index++)
        {
            if (trailer.Length - offset < 4) return false;
            var length = BinaryPrimitives.ReadInt32LittleEndian(trailer[offset..]); offset += 4;
            if (length == -1) continue;
            if (length < 0 || length > trailer.Length - offset) return false;
            try { names[index] = Encoding.GetString(trailer.Slice(offset, length)); }
            catch (DecoderFallbackException) { return false; }
            offset += length;
        }
        if (offset != trailer.Length) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-damage-type-count"; return false; }
        for (var index = 0; index < count; index++) state.Actors[index].PlayerLastDamageType = names[index];
        error = null; return true;
    }
}
