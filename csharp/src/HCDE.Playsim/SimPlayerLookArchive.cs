using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPlayerLookArchive
{
    internal static void Validate(SimSaveState state)
    {
        if ((state.PlayerLookRandomState.HasValue || state.CompatibilityInvisibility || state.Actors.Any(a => a.SeeInvisible))
            && state.GeometryHealth is null)
            throw new InvalidOperationException("Player look state requires geometry.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.PlayerLookRandomState.HasValue && !state.CompatibilityInvisibility && !state.Actors.Any(a => a.SeeInvisible)) return archive;
        var size = checked(24 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], (state.PlayerLookRandomState.HasValue ? 1 : 0) | (state.CompatibilityInvisibility ? 2 : 0));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[12..], state.PlayerLookRandomState ?? 0);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * 4)..], state.Actors[i].SeeInvisible ? 1 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 234);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 40) { error = "save-playerlook-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 24 || size > bytes.Length - 16) { error = "save-playerlook-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[8..]);
        if (prior < 15 || prior > 233 || count < 0 || (long)count * 4 + 24 != size || flags is < 0 or > 3)
        { error = "save-playerlook-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]) is < 0 or > 1)
            { error = "save-playerlook-value"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null || state.Actors.Count != count)
        { state = new(); error = "save-playerlook-state"; return false; }
        state.PlayerLookRandomState = (flags & 1) != 0 ? BinaryPrimitives.ReadUInt64LittleEndian(trailer[12..]) : null;
        state.CompatibilityInvisibility = (flags & 2) != 0;
        for (var i = 0; i < count; i++)
            state.Actors[i].SeeInvisible = BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 4)..]) == 1;
        return true;
    }
}
