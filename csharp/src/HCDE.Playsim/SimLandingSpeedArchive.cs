using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimLandingSpeedArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.LandingSpeed != Actor.DefaultLandingSpeed)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(trailer[(8 + i * 8)..], state.Actors[i].LandingSpeed);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 314);
        return result;
    }

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => !double.IsFinite(a.LandingSpeed)))
            throw new InvalidOperationException("Invalid saved landing speed.");
        if (state.Actors.Any(a => a.LandingSpeed != Actor.DefaultLandingSpeed) && state.GeometryHealth is null)
            throw new InvalidOperationException("LandingSpeed archives require geometry state.");
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-landingspeed-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-landingspeed-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 313 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-landingspeed-header"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-landingspeed-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var value = BinaryPrimitives.ReadDoubleLittleEndian(trailer[(8 + i * 8)..]);
            state.Actors[i].LandingSpeed = value;
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-landingspeed-state"; return false; }
        return true;
    }
}
