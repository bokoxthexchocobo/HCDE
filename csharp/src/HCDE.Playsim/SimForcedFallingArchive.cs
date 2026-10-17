using System.Buffers.Binary;
using HCDE.Gamedata;

namespace HCDE.Playsim;

internal static class SimForcedFallingArchive
{
    internal static void Validate(SimSaveState state)
    {
        if ((uint)state.ForcedFallingDamage > 3)
            throw new InvalidOperationException("Invalid forced falling damage style.");
        if (state.ForcedFallingDamage != FallingDamageStyle.None && state.GeometryHealth is null)
            throw new InvalidOperationException("Forced falling damage archives require geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.ForcedFallingDamage == FallingDamageStyle.None) return archive;
        var result = new byte[checked(archive.Length + 12)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], (int)state.ForcedFallingDamage);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 319);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 12)
        { error = "save-forcedfalling-size"; return false; }
        var trailer = bytes[^12..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var value = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 318 || value is < 0 or > 3)
        { error = "save-forcedfalling-header"; return false; }
        var legacy = bytes[..^12].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.ForcedFallingDamage = (FallingDamageStyle)value;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-forcedfalling-state"; return false; }
        return true;
    }
}
