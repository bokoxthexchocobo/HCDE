using System.Buffers.Binary;
namespace HCDE.Playsim;
public readonly record struct SimPainDeath(int Tics, uint? TargetId);
internal static class SimPainDeathArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (!state.Actors.Any(a => a.PainDeath.HasValue)) return;
        if (state.GeometryHealth is null || state.Actors.Any(a => !a.ContactFlags.HasValue) ||
            state.Actors.Any(a => a.PainDeath is { } d && (d.Tics is < 0 or > 32 || d.TargetId == 0)))
            throw new InvalidOperationException("Invalid Pain Elemental death archive.");
    }
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.PainDeath.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var death = state.Actors[i].PainDeath;
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 8), death?.Tics ?? -1);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(start + 12 + i * 8), death?.TargetId ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 21); return bytes;
    }
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 8 != 0)
        { error = "save-pain-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior is < 18 or > 20 || count < 0 || count != (size - 12) / 8)
        { error = "save-pain-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-pain-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var tics = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 8)..]);
            var target = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 12 + i * 8)..]);
            if (tics is < -1 or > 32 || tics == -1 && target != 0)
            { state = new(); error = "save-pain-state"; return false; }
            state.Actors[i].PainDeath = tics == -1 ? null : new(tics, target == 0 ? null : target);
        }
        return true;
    }
}