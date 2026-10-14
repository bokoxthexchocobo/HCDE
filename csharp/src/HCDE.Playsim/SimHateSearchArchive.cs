using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimHateSearchArchive
{
    internal static void Validate(SimSaveState state)
    {
        if ((state.HateSearchRandomState.HasValue || state.Actors.Any(a => a.LastLookActorId.HasValue)) && state.GeometryHealth is null)
            throw new InvalidOperationException("Hate search state requires geometry.");
        if (state.Actors.Any(a => a.LastLookActorId == 0)) throw new InvalidOperationException("Invalid last-look actor ID.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.HateSearchRandomState.HasValue && !state.Actors.Any(a => a.LastLookActorId.HasValue)) return archive;
        var size = checked(24 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0); var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], state.HateSearchRandomState.HasValue ? 1 : 0);
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[12..], state.HateSearchRandomState ?? 0);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(trailer[(20 + i * 4)..], state.Actors[i].LastLookActorId ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 232); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 40) { error = "save-hatesearch-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 24 || size > bytes.Length - 16) { error = "save-hatesearch-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]); var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[8..]);
        if (prior < 15 || prior > 231 || count < 0 || (long)count * 4 + 24 != size || flags is < 0 or > 1)
        { error = "save-hatesearch-header"; return false; }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null || state.Actors.Count != count) { state = new(); error = "save-hatesearch-state"; return false; }
        state.HateSearchRandomState = flags == 1 ? BinaryPrimitives.ReadUInt64LittleEndian(trailer[12..]) : null;
        for (var i = 0; i < count; i++)
        {
            var id = BinaryPrimitives.ReadUInt32LittleEndian(trailer[(20 + i * 4)..]); state.Actors[i].LastLookActorId = id == 0 ? null : id;
        }
        return true;
    }
}
