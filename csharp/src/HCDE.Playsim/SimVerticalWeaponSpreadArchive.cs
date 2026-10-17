using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimVerticalWeaponSpreadArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.VerticalWeaponSpread && state.GeometryHealth is null)
            throw new InvalidOperationException("Vertical weapon spread requires geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.VerticalWeaponSpread) return archive;
        var result = new byte[checked(archive.Length + 12)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], 1);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 362);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 12)
        { error = "save-verticalspread-size"; return false; }
        var trailer = bytes[^12..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 361 || BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]) != 1)
        { error = "save-verticalspread-header"; return false; }
        var legacy = bytes[..^12].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null) { state = new(); error = "save-verticalspread-state"; return false; }
        state.VerticalWeaponSpread = true;
        return true;
    }
}
