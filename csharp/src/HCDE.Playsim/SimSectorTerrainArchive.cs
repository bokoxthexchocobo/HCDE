using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

public readonly record struct SimSectorTerrain(string Floor, string Ceiling);

internal static class SimSectorTerrainArchive
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    internal static void Validate(SimSaveState state)
    {
        if (state.SectorTerrains is not { } terrains) return;
        if (state.GeometryHealth is null || terrains.Count != state.Sectors.Count
            || terrains.Any(t => t.Floor is null || t.Ceiling is null))
            throw new InvalidOperationException("Invalid saved sector terrains.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.SectorTerrains is not { } terrains) return archive;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding, leaveOpen: true);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        writer.Write(terrains.Count);
        foreach (var terrain in terrains)
        {
            WriteName(writer, terrain.Floor); WriteName(writer, terrain.Ceiling);
        }
        writer.Write(checked((int)stream.Length + 4)); writer.Flush();
        var result = new byte[checked(archive.Length + (int)stream.Length)];
        archive.CopyTo(result, 0); stream.ToArray().CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 364);
        return result;
    }

    private static void WriteName(BinaryWriter writer, string name)
    {
        var bytes = Encoding.GetBytes(name); writer.Write(bytes.Length); writer.Write(bytes);
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-sectorterrain-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-sectorterrain-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 363 || count < 0 || (long)count * 8 + 8 > trailer.Length)
        { error = "save-sectorterrain-header"; return false; }
        var terrains = new SimSectorTerrain[count]; var offset = 8;
        for (var i = 0; i < count; i++)
        {
            if (!ReadName(trailer, ref offset, out var floor) || !ReadName(trailer, ref offset, out var ceiling))
            { error = "save-sectorterrain-value"; return false; }
            terrains[i] = new(floor, ceiling);
        }
        if (offset != trailer.Length) { error = "save-sectorterrain-size"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.SectorTerrains = terrains;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-sectorterrain-state"; return false; }
        return true;
    }

    private static bool ReadName(ReadOnlySpan<byte> bytes, ref int offset, out string name)
    {
        name = "";
        if (bytes.Length - offset < 4) return false;
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]); offset += 4;
        if (length < 0 || length > bytes.Length - offset) return false;
        try { name = Encoding.GetString(bytes.Slice(offset, length)); }
        catch (DecoderFallbackException) { return false; }
        offset += length; return true;
    }
}
