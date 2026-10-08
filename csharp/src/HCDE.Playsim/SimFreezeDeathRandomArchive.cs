using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimFreezeDeathRandomArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.FreezeDeathRandomState is not { } random) return archive;
        var size = checked(20 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * 4)..], state.Actors[i].IceCorpseFlag == true ? 1 : 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 117);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 36)
        { error = "save-freezedeathrandom-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 20 || size > bytes.Length - 16)
        { error = "save-freezedeathrandom-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 116) { error = "save-freezedeathrandom-header"; return false; }
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[12..]);
        if (count < 0 || 20L + count * 4L != size)
        { error = "save-freezedeathrandom-size"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * 4)..]) is not (0 or 1))
            { error = "save-freezedeathrandom-flags"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (count != state.Actors.Count) { error = "save-freezedeathrandom-count"; return false; }
        state.FreezeDeathRandomState = BinaryPrimitives.ReadUInt64LittleEndian(trailer[4..]);
        for (var i = 0; i < count; i++)
            state.Actors[i].IceCorpseFlag = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * 4)..]) != 0;
        return true;
    }
}
