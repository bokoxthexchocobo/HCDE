using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimFloatBobSettings(bool Enabled, double Strength, double Factor);

internal static class SimFloatBobArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.FloatBobSettings.HasValue) && state.GeometryHealth is null)
            throw new InvalidOperationException("Float bob archives require geometry state.");
        if (state.Actors.Any(a => a.FloatBobSettings is { } b && (!double.IsFinite(b.Strength) || !double.IsFinite(b.Factor))))
            throw new InvalidOperationException("Invalid saved float bob settings.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.FloatBobSettings.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 20);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var bob = state.Actors[i].FloatBobSettings;
            var item = trailer[(8 + i * 20)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, bob.HasValue ? bob.Value.Enabled ? 1 : 0 : -1);
            BinaryPrimitives.WriteDoubleLittleEndian(item[4..], bob?.Strength ?? 1);
            BinaryPrimitives.WriteDoubleLittleEndian(item[12..], bob?.Factor ?? 1);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 326);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-floatbob-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-floatbob-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 325 || count < 0 || (long)count * 20 + 12 != size)
        { error = "save-floatbob-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) is < -1 or > 1
                || !double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(item[4..]))
                || !double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(item[12..])))
            { error = "save-floatbob-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-floatbob-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..];
            var enabled = BinaryPrimitives.ReadInt32LittleEndian(item);
            state.Actors[i].FloatBobSettings = enabled < 0 ? null : new SimFloatBobSettings(enabled == 1,
                BinaryPrimitives.ReadDoubleLittleEndian(item[4..]), BinaryPrimitives.ReadDoubleLittleEndian(item[12..]));
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-floatbob-state"; return false; }
        return true;
    }
}
