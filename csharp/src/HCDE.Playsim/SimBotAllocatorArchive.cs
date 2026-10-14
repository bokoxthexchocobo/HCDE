using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimBotAllocatorArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.NextActorId is { } next && (!state.IncludesBotIdentities
            || state.GeometryHealth is null || next == 0 || state.Actors.Any(actor => actor.Id >= next)))
            throw new InvalidOperationException("Invalid saved actor ID allocator.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.NextActorId is not { } next) return archive;
        var result = new byte[checked(archive.Length + 12)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteUInt32LittleEndian(trailer[4..], next);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[8..], 12);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 202);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28 || BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]) != 12)
        { error = "save-bot-allocator-size"; return false; }
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[^12..]);
        if (prior is not (199 or 200 or 201 or 203 or 210 or 212 or 213))
        { error = "save-bot-allocator-header"; return false; }
        var legacy = bytes[..^12].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.NextActorId = BinaryPrimitives.ReadUInt32LittleEndian(bytes[^8..]);
        try { Validate(state); }
        catch (InvalidOperationException)
        { state = new(); error = "save-bot-allocator-state"; return false; }
        return true;
    }
}
