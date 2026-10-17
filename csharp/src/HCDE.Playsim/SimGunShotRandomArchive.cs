using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimGunShotRandomArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.GunShotRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Gunshot randomness requires geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.GunShotRandomState is not { } random) return archive;
        var result = new byte[checked(archive.Length + 16)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 361);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 32 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 16)
        { error = "save-gunshotrandom-size"; return false; }
        var trailer = bytes[^16..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 360) { error = "save-gunshotrandom-header"; return false; }
        var legacy = bytes[..^16].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null) { state = new(); error = "save-gunshotrandom-state"; return false; }
        state.GunShotRandomState = BinaryPrimitives.ReadUInt64LittleEndian(trailer[4..]);
        return true;
    }
}
