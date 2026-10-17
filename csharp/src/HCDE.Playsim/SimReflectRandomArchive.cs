using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimReflectRandomArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.ReflectRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Reflect random archives require geometry state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.ReflectRandomState is not { } random) return archive;
        var result = new byte[checked(archive.Length + 16)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt64LittleEndian(trailer[4..], random);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[12..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 324);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 32 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 16)
        { error = "save-reflectrandom-size"; return false; }
        var trailer = bytes[^16..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        if (prior < 15 || prior > 323) { error = "save-reflectrandom-header"; return false; }
        var legacy = bytes[..^16].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null) { state = new(); error = "save-reflectrandom-state"; return false; }
        state.ReflectRandomState = BinaryPrimitives.ReadUInt64LittleEndian(trailer[4..]);
        return true;
    }
}
