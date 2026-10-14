using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimSectorSoundArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.SectorSoundTargets is not { Count: > 0 } targets) return;
        if (state.GeometryHealth is null || targets.Any(pair => pair.Key < 0 || pair.Key >= state.Sectors.Count
            || pair.Value == 0 || !state.Actors.Any(a => a.Id == pair.Value)))
            throw new InvalidOperationException("Invalid sector sound targets.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.SectorSoundTargets is not { Count: > 0 } targets) return archive;
        var size = checked(12 + targets.Count * 8); var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0); var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], targets.Count);
        var offset = 8;
        foreach (var pair in targets.OrderBy(pair => pair.Key))
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[offset..], pair.Key);
            BinaryPrimitives.WriteUInt32LittleEndian(trailer[(offset + 4)..], pair.Value); offset += 8;
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 247); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-sectorsound-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-sectorsound-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 246 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-sectorsound-header"; return false; }
        var targets = new Dictionary<int, uint>();
        for (var i = 0; i < count; i++)
            if (!targets.TryAdd(BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]),
                BinaryPrimitives.ReadUInt32LittleEndian(trailer[(12 + i * 8)..])))
            { error = "save-sectorsound-duplicate"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.SectorSoundTargets = targets;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-sectorsound-state"; return false; }
        return true;
    }
}
