using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimStateRandomArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.StateRandomState is not { } random) return archive;
        var result = new byte[checked(archive.Length + 16)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 111); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]) == 110 ? 12 : 16;
        if (bytes.Length < 16 + size || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != size)
        { error = "save-staterandom-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > (size == 12 ? 109 : 110)) { error = "save-staterandom-header"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.StateRandomState = size == 12 ? BinaryPrimitives.ReadUInt32LittleEndian(trailer[4..])
            : BinaryPrimitives.ReadUInt64LittleEndian(trailer[4..]); return true;
    }
}
