using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimBlastEligibilityArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (!state.Actors.Any(a => a.BlastEligibilityFlags.HasValue)) return;
        if (state.Actors.Any(a => a.BlastEligibilityFlags is < 0 or > 3))
            throw new InvalidOperationException("Invalid blast-eligibility flag archive.");
        if (state.GeometryHealth is null || state.Actors.Any(a => !a.ContactFlags.HasValue || !a.BlastEligibilityFlags.HasValue))
            throw new InvalidOperationException("Incomplete blast-eligibility flag archive.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.BlastEligibilityFlags.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].BlastEligibilityFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 28); return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
        { error = "save-blast-eligibility-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior is < 18 or > 27 || count < 0 || count != (size - 12) / 4)
        { error = "save-blast-eligibility-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-blast-eligibility-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
            if (flags is < 0 or > 3) { state = new(); error = "save-blast-eligibility-flags"; return false; }
            state.Actors[i].BlastEligibilityFlags = flags;
        }
        return true;
    }
}
