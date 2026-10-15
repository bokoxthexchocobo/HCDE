using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimMeleeThresholdArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.MeleeThresholdOverride is { } value && !double.IsFinite(value)))
            throw new InvalidOperationException("Invalid saved MeleeThreshold override.");
        if (state.Actors.Any(a => a.MeleeThresholdOverride.HasValue) && state.GeometryHealth is null)
            throw new InvalidOperationException("MeleeThreshold archives require geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.MeleeThresholdOverride.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var value = state.Actors[i].MeleeThresholdOverride;
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + 12 * i)..], value.HasValue ? 1 : 0);
            BinaryPrimitives.WriteDoubleLittleEndian(trailer[(12 + 12 * i)..], value.GetValueOrDefault());
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 285);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-meleethreshold-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-meleethreshold-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 284 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-meleethreshold-header"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-meleethreshold-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var present = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + 12 * i)..]);
            var value = BinaryPrimitives.ReadDoubleLittleEndian(trailer[(12 + 12 * i)..]);
            if (present is < 0 or > 1 || !double.IsFinite(value) || present == 0 && value != 0)
            { state = new(); error = "save-meleethreshold-state"; return false; }
            state.Actors[i].MeleeThresholdOverride = present == 0 ? null : value;
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-meleethreshold-state"; return false; }
        return true;
    }
}
