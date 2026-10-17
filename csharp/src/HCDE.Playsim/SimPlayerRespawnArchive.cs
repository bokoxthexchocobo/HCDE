using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerRespawnArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.RespawnEarliestTic != int.MaxValue || actor.RespawnArmed)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 8)..], state.Actors[i].RespawnEarliestTic);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * 8)..], state.Actors[i].RespawnArmed ? 1 : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 303);
        return result;
    }

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.RespawnEarliestTic < 0))
            throw new InvalidOperationException("Invalid saved player respawn deadline.");
        if (state.Actors.Any(actor => actor.RespawnEarliestTic != int.MaxValue || actor.RespawnArmed) && state.GeometryHealth is null)
            throw new InvalidOperationException("Player respawn archives require geometry state.");
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-playerrespawn-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-playerrespawn-size"; return false; }
        var trailer = bytes[(bytes.Length - size)..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 302 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-playerrespawn-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]) < 0
                || BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 8)..]) is < 0 or > 1)
            { error = "save-playerrespawn-value"; return false; }
        var legacy = bytes[..(bytes.Length - size)].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-playerrespawn-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].RespawnEarliestTic = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]);
            state.Actors[i].RespawnArmed = BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 8)..]) != 0;
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-playerrespawn-state"; return false; }
        return true;
    }
}
