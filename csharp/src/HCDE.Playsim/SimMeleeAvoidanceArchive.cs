using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimMeleeAvoidanceArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.AvoidMeleeFlag is < 0 or > 1))
            throw new InvalidOperationException("Invalid saved AvoidMelee flag.");
        if ((state.EnemyStrafeRandomState.HasValue
            || state.Actors.Any(a => a.AvoidMeleeFlag != 0 || a.StrafeCount != 0)) && state.GeometryHealth is null)
            throw new InvalidOperationException("Melee avoidance archives require geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.EnemyStrafeRandomState.HasValue
            && !state.Actors.Any(a => a.AvoidMeleeFlag != 0 || a.StrafeCount != 0)) return archive;
        var size = checked(24 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], state.EnemyStrafeRandomState.HasValue ? 1 : 0);
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[12..], state.EnemyStrafeRandomState ?? 0);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * 8)..], state.Actors[i].AvoidMeleeFlag);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(24 + i * 8)..], state.Actors[i].StrafeCount);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 271);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 40) { error = "save-meleeavoidance-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 24 || size > bytes.Length - 16) { error = "save-meleeavoidance-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[8..]);
        if (prior < 15 || prior > 270 || count < 0 || flags is < 0 or > 1 || (long)count * 8 + 24 != size)
        { error = "save-meleeavoidance-header"; return false; }
        var first = BinaryPrimitives.ReadUInt64LittleEndian(trailer[12..]);
        if ((flags & 1) == 0 && first != 0)
        { error = "save-meleeavoidance-random"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-meleeavoidance-count"; return false; }
        state.EnemyStrafeRandomState = (flags & 1) != 0 ? first : null;
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].AvoidMeleeFlag = BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * 8)..]);
            state.Actors[i].StrafeCount = BinaryPrimitives.ReadInt32LittleEndian(trailer[(24 + i * 8)..]);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-meleeavoidance-state"; return false; }
        return true;
    }
}
