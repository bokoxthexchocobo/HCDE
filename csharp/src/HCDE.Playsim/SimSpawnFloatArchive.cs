using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimSpawnFloatArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.SpawnFloatFlags.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 4)..], state.Actors[i].SpawnFloatFlags ?? -1);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 353);
        return result;
    }

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.SpawnFloatFlags.HasValue) && state.GeometryHealth is null)
            throw new InvalidOperationException("SpawnFloat archives require geometry state.");
        if (state.Actors.Any(a => a.SpawnFloatFlags is < 0 or > 3))
            throw new InvalidOperationException("Invalid saved spawnfloat flag.");
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-spawnfloat-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-spawnfloat-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 352 || count < 0 || (long)count * 4 + 12 != size)
        { error = "save-spawnfloat-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]) is < -1 or > 3)
            { error = "save-spawnfloat-value"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-spawnfloat-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            state.Actors[i].SpawnFloatFlags = value < 0 ? null : value;
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-spawnfloat-state"; return false; }
        return true;
    }
}
