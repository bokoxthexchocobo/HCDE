using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimActorTuning(int SpeedRaw, double FloatSpeed, int PainThreshold);

internal static class SimTuningArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.Tuning is { } tuning && !double.IsFinite(tuning.FloatSpeed)))
            throw new InvalidOperationException("Invalid saved actor float speed.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.Tuning.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 20);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 20)..]; var tuning = state.Actors[i].Tuning;
            BinaryPrimitives.WriteInt32LittleEndian(item, tuning.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], tuning?.SpeedRaw ?? 0);
            BinaryPrimitives.WriteInt64LittleEndian(item[8..], BitConverter.DoubleToInt64Bits(tuning?.FloatSpeed ?? 0));
            BinaryPrimitives.WriteInt32LittleEndian(item[16..], tuning?.PainThreshold ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 73); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-tuning-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 72 || count < 0 || (long)count * 20 + 12 != size)
        { error = "save-tuning-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var speed = BinaryPrimitives.ReadInt32LittleEndian(item[4..]);
            var floating = BinaryPrimitives.ReadInt64LittleEndian(item[8..]);
            var pain = BinaryPrimitives.ReadInt32LittleEndian(item[16..]);
            if (present is not (0 or 1) || !double.IsFinite(BitConverter.Int64BitsToDouble(floating))
                || present == 0 && (speed != 0 || floating != 0 || pain != 0))
            { error = "save-tuning-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-tuning-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 20)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].Tuning = new(BinaryPrimitives.ReadInt32LittleEndian(item[4..]),
                    BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(item[8..])),
                    BinaryPrimitives.ReadInt32LittleEndian(item[16..]));
        }
        return true;
    }
}
