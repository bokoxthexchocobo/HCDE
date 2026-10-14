using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerSearchArchive
{
    internal static void Validate(SimSaveState state)
    {
        if ((state.PlayerSearchRandomState.HasValue || state.Actors.Any(a => a.LastLookPlayerNumber != 0))
            && state.GeometryHealth is null)
            throw new InvalidOperationException("Player search state requires geometry.");
        if (state.Actors.Any(a => a.LastLookPlayerNumber is < 0 or > 63))
            throw new InvalidOperationException("Invalid player look cursor.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.PlayerSearchRandomState.HasValue && !state.Actors.Any(a => a.LastLookPlayerNumber != 0)) return archive;
        var size = checked(24 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], state.PlayerSearchRandomState.HasValue ? 1 : 0);
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[12..], state.PlayerSearchRandomState ?? 0);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * 4)..], state.Actors[i].LastLookPlayerNumber);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 235);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 40) { error = "save-playersearch-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 24 || size > bytes.Length - 16) { error = "save-playersearch-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[8..]);
        if (prior < 15 || prior > 234 || count < 0 || (long)count * 4 + 24 != size || flags is < 0 or > 1)
        { error = "save-playersearch-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]) is < 0 or > 63)
            { error = "save-playersearch-value"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null || state.Actors.Count != count)
        { state = new(); error = "save-playersearch-state"; return false; }
        state.PlayerSearchRandomState = (flags & 1) != 0 ? BinaryPrimitives.ReadUInt64LittleEndian(trailer[12..]) : null;
        for (var i = 0; i < count; i++)
            state.Actors[i].LastLookPlayerNumber = BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]);
        return true;
    }
}
