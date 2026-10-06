using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimActorScale(double X, double Y);

internal static class SimScaleArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.Scale is { } scale && (!double.IsFinite(scale.X) || !double.IsFinite(scale.Y))))
            throw new InvalidOperationException("Invalid saved actor scale.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.Scale.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 20);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 20)..]; var scale = state.Actors[i].Scale;
            BinaryPrimitives.WriteInt32LittleEndian(item, scale.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt64LittleEndian(item[4..], BitConverter.DoubleToInt64Bits(scale?.X ?? 0));
            BinaryPrimitives.WriteInt64LittleEndian(item[12..], BitConverter.DoubleToInt64Bits(scale?.Y ?? 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 77); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-scale-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 76 || count < 0 || (long)count * 20 + 12 != size)
        { error = "save-scale-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var x = BinaryPrimitives.ReadInt64LittleEndian(item[4..]);
            var y = BinaryPrimitives.ReadInt64LittleEndian(item[12..]);
            if (present is not (0 or 1) || !double.IsFinite(BitConverter.Int64BitsToDouble(x))
                || !double.IsFinite(BitConverter.Int64BitsToDouble(y)) || present == 0 && (x != 0 || y != 0))
            { error = "save-scale-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-scale-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].Scale = new(BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(item[4..])),
                    BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(item[12..])));
        }
        return true;
    }
}
