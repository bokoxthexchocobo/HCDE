using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

internal static class SimPlayerPoisonArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.PoisonSourceId.HasValue || a.PoisonDamageType is not null || a.PoisonPainType is not null)) return archive;
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            writer.Write(actor.PoisonSourceId.HasValue); writer.Write(actor.PoisonSourceId ?? 0);
            WriteName(writer, actor.PoisonDamageType); WriteName(writer, actor.PoisonPainType);
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 161); return result;
    }
    private static void WriteName(BinaryWriter writer, string? name)
    {
        if (name is null) { writer.Write(-1); return; }
        var data = Encoding.GetBytes(name); writer.Write(data.Length); writer.Write(data);
    }
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-poison-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 160 || count < 0 || (long)count * 13 + 8 > trailer.Length) return false;
        var sources = new uint?[count]; var types = new string?[count]; var pains = new string?[count]; var offset = 8;
        for (var index = 0; index < count; index++)
        {
            if (trailer.Length - offset < 5 || trailer[offset] > 1) return false;
            var source = BinaryPrimitives.ReadUInt32LittleEndian(trailer[(offset + 1)..]);
            if (trailer[offset] == 0 && source != 0) return false;
            sources[index] = trailer[offset] == 1 ? source : null; offset += 5;
            if (!ReadName(trailer, ref offset, out types[index]) || !ReadName(trailer, ref offset, out pains[index])) return false;
        }
        if (offset != trailer.Length) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-poison-count"; return false; }
        for (var index = 0; index < count; index++)
        {
            state.Actors[index].PoisonSourceId = sources[index];
            state.Actors[index].PoisonDamageType = types[index]; state.Actors[index].PoisonPainType = pains[index];
        }
        error = null; return true;
    }
    private static bool ReadName(ReadOnlySpan<byte> data, ref int offset, out string? name)
    {
        name = null;
        if (data.Length - offset < 4) return false;
        var length = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
        if (length == -1) return true;
        if (length < 0 || length > data.Length - offset) return false;
        try { name = Encoding.GetString(data.Slice(offset, length)); }
        catch (DecoderFallbackException) { return false; }
        offset += length; return true;
    }
}
