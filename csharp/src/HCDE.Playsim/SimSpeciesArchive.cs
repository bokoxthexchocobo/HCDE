using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

public readonly record struct SimSpecies(string? Name);

internal static class SimSpeciesArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.SpeciesOverride.HasValue)) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            writer.Write(actor.SpeciesOverride.HasValue ? 1 : 0);
            WriteName(writer, actor.SpeciesOverride?.Name);
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 76); return result;
    }

    private static void WriteName(BinaryWriter writer, string? name)
    {
        if (name is null) { writer.Write(-1); return; }
        var bytes = Encoding.GetBytes(name); writer.Write(bytes.Length); writer.Write(bytes);
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-species-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 75 || count < 0 || (long)count * 8 + 8 > trailer.Length)
        { error = "save-species-header"; return false; }
        var records = new SimSpecies?[count]; var offset = 8;
        for (var i = 0; i < count; i++)
        {
            if (trailer.Length - offset < 4) { error = "save-species-value"; return false; }
            var present = BinaryPrimitives.ReadInt32LittleEndian(trailer[offset..]); offset += 4;
            if (present is not (0 or 1) || !ReadName(trailer, ref offset, out var source)
                || present == 0 && source is not null)
            { error = "save-species-value"; return false; }
            if (present == 1) records[i] = new(source);
        }
        if (offset != trailer.Length) { error = "save-species-size"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-species-count"; return false; }
        for (var i = 0; i < count; i++) state.Actors[i].SpeciesOverride = records[i];
        return true;
    }

    private static bool ReadName(ReadOnlySpan<byte> bytes, ref int offset, out string? name)
    {
        name = null;
        if (bytes.Length - offset < 4) return false;
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]); offset += 4;
        if (length == -1) return true;
        if (length < 0 || length > bytes.Length - offset) return false;
        try { name = Encoding.GetString(bytes.Slice(offset, length)); }
        catch (DecoderFallbackException) { return false; }
        offset += length; return true;
    }
}
