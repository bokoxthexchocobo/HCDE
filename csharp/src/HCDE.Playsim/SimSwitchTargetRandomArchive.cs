using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimSwitchTargetRandomArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.SwitchTargetRandomState is not { } random) return archive;
        var result = new byte[checked(archive.Length + 12)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt32LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 288);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 12)
        { error = "save-switchtargetrandom-size"; return false; }
        var trailer = bytes[^12..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 287) { error = "save-switchtargetrandom-header"; return false; }
        var legacy = bytes[..^12].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.SwitchTargetRandomState = BinaryPrimitives.ReadUInt32LittleEndian(trailer[4..]);
        if (state.GeometryHealth is null) { state = new(); error = "save-switchtargetrandom-state"; return false; }
        return true;
    }
}
