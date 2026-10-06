using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimTargetMemory(uint? Target, uint? LastEnemy, uint? LastHeard);

internal static class SimTargetMemoryArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.TargetMemory.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 16);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 16)..]; var memory = state.Actors[i].TargetMemory;
            BinaryPrimitives.WriteInt32LittleEndian(item, memory.HasValue ? 1 : 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[4..], memory?.Target ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[8..], memory?.LastEnemy ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[12..], memory?.LastHeard ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 69); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-target-memory-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 68 || count < 0 || (long)count * 16 + 12 != size)
        { error = "save-target-memory-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 16)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            if (present is not (0 or 1) || present == 0 &&
                (BinaryPrimitives.ReadUInt32LittleEndian(item[4..]) != 0
                || BinaryPrimitives.ReadUInt32LittleEndian(item[8..]) != 0
                || BinaryPrimitives.ReadUInt32LittleEndian(item[12..]) != 0))
            { error = "save-target-memory-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-target-memory-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 16)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 0) continue;
            state.Actors[i].TargetMemory = new(Id(item[4..]), Id(item[8..]), Id(item[12..]));
        }
        return true;
    }

    private static uint? Id(ReadOnlySpan<byte> bytes)
    { var id = BinaryPrimitives.ReadUInt32LittleEndian(bytes); return id == 0 ? null : id; }
}
