using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimLightArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Lights is { } lights && lights.Count != state.Sectors.Count)
            throw new InvalidOperationException("Saved sector light count is invalid.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.Lights is not { } lights) return archive;
        var size = checked(12 + lights.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], lights.Count);
        for (var i = 0; i < lights.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 4)..], lights[i]);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 80); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-light-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 79 || count < 0 || (long)count * 4 + 12 != size)
        { error = "save-light-header"; return false; }
        var lights = new List<short>(count);
        for (var i = 0; i < count; i++)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            if (value < short.MinValue || value > short.MaxValue) { error = "save-light-value"; return false; }
            lights.Add((short)value);
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Sectors.Count != count) { state = new(); error = "save-light-count"; return false; }
        state.Lights = lights; return true;
    }
}
