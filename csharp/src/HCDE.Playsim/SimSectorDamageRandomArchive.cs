using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimSectorDamageRandomArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.SectorDamageRandomState is not { } random) return archive;
        var result = new byte[checked(archive.Length + 16)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 297);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 32 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 16)
        { error = "save-sectordamagerandom-size"; return false; }
        var trailer = bytes[^16..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 296) { error = "save-sectordamagerandom-header"; return false; }
        var legacy = bytes[..^16].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.SectorDamageRandomState = BinaryPrimitives.ReadUInt64LittleEndian(trailer[4..]);
        if (state.GeometryHealth is null) { state = new(); error = "save-sectordamagerandom-state"; return false; }
        return true;
    }
}
