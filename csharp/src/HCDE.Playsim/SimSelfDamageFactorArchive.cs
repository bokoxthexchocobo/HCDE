using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimSelfDamageFactorArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.SelfDamageFactor != 65536)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var index = 0; index < state.Actors.Count; index++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + index * 4)..], state.Actors[index].SelfDamageFactor);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 151); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-self-damage-factor-invalid";
        if (bytes.Length < 28) return false;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 150 || count < 0 || (long)count * 4 + 12 != size) return false;
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-self-damage-factor-count"; return false; }
        for (var index = 0; index < count; index++)
            state.Actors[index].SelfDamageFactor = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + index * 4)..]);
        error = null; return true;
    }
}
