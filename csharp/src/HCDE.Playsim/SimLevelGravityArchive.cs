using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimLevelGravityArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.LevelGravityOverride is not { } control) return archive;
        if (!double.IsFinite(control)) throw new InvalidOperationException("Invalid saved level gravity.");
        var result = new byte[checked(archive.Length + 16)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt64LittleEndian(trailer[4..], BitConverter.DoubleToInt64Bits(control));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 121); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 32 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 16)
        { error = "save-levelgravity-size"; return false; }
        var trailer = bytes[^16..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var control = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(trailer[4..]));
        if (prior < 15 || prior > 120 || !double.IsFinite(control))
        { error = "save-levelgravity-value"; return false; }
        var legacy = bytes[..^16].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.LevelGravityOverride = control; return true;
    }
}
