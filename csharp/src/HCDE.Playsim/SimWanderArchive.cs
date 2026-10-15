using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimWanderArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.MoveDirection is < 0 or > 255))
            throw new InvalidOperationException("Invalid saved movement direction.");
        if ((state.NewChaseDirRandomState.HasValue || state.TryWalkRandomState.HasValue
            || state.Actors.Any(a => a.MoveDirection != 0 || a.MoveCount != 0)) && state.GeometryHealth is null)
            throw new InvalidOperationException("Wander archives require geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.NewChaseDirRandomState.HasValue && !state.TryWalkRandomState.HasValue
            && !state.Actors.Any(a => a.MoveDirection != 0 || a.MoveCount != 0)) return archive;
        var size = checked(32 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], (state.NewChaseDirRandomState.HasValue ? 1 : 0) | (state.TryWalkRandomState.HasValue ? 2 : 0));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[12..], state.NewChaseDirRandomState ?? 0);
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[20..], state.TryWalkRandomState ?? 0);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(28 + i * 8)..], state.Actors[i].MoveDirection);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(32 + i * 8)..], state.Actors[i].MoveCount);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 266);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 48) { error = "save-wander-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 32 || size > bytes.Length - 16) { error = "save-wander-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[8..]);
        if (prior < 15 || prior > 265 || count < 0 || flags is < 0 or > 3 || (long)count * 8 + 32 != size)
        { error = "save-wander-header"; return false; }
        var first = BinaryPrimitives.ReadUInt64LittleEndian(trailer[12..]);
        var second = BinaryPrimitives.ReadUInt64LittleEndian(trailer[20..]);
        if ((flags & 1) == 0 && first != 0 || (flags & 2) == 0 && second != 0)
        { error = "save-wander-random"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-wander-count"; return false; }
        state.NewChaseDirRandomState = (flags & 1) != 0 ? first : null;
        state.TryWalkRandomState = (flags & 2) != 0 ? second : null;
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].MoveDirection = BinaryPrimitives.ReadInt32LittleEndian(trailer[(28 + i * 8)..]);
            state.Actors[i].MoveCount = BinaryPrimitives.ReadInt32LittleEndian(trailer[(32 + i * 8)..]);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-wander-state"; return false; }
        return true;
    }
}
