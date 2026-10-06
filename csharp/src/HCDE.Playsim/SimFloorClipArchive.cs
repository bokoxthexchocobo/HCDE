using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimFloorClipArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.FloorClip is { } range && !double.IsFinite(range)))
            throw new InvalidOperationException("Invalid saved actor floor clip.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.FloorClip.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 12)..]; var range = state.Actors[i].FloorClip;
            BinaryPrimitives.WriteInt32LittleEndian(item, range.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt64LittleEndian(item[4..], BitConverter.DoubleToInt64Bits(range ?? 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 88); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-floor-clip-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 87 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-floor-clip-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var bits = BinaryPrimitives.ReadInt64LittleEndian(item[4..]);
            if (present is not (0 or 1) || !double.IsFinite(BitConverter.Int64BitsToDouble(bits)) || present == 0 && bits != 0)
            { error = "save-floor-clip-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-floor-clip-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].FloorClip = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(item[4..]));
        }
        return true;
    }
}
