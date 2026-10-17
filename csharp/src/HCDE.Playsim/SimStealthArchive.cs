using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimStealthArchive
{
    private static bool HasState(SimSaveState state) => state.Actors.Any(a => a.StealthFlag.HasValue || a.StealthAlpha != 0);

    internal static void Validate(SimSaveState state)
    {
        if (HasState(state) && state.GeometryHealth is null)
            throw new InvalidOperationException("Stealth archives require geometry state.");
        if (state.Actors.Any(a => a.StealthFlag is < 0 or > 1 || !double.IsFinite(a.StealthAlpha)))
            throw new InvalidOperationException("Invalid saved stealth state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!HasState(state)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, state.Actors[i].StealthFlag ?? -1);
            BinaryPrimitives.WriteDoubleLittleEndian(item[4..], state.Actors[i].StealthAlpha);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 331);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-stealth-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-stealth-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 330 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-stealth-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) is < -1 or > 1
                || !double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(item[4..])))
            { error = "save-stealth-value"; return false; }
        }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-stealth-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..]; var flag = BinaryPrimitives.ReadInt32LittleEndian(item);
            state.Actors[i].StealthFlag = flag < 0 ? null : flag;
            state.Actors[i].StealthAlpha = BinaryPrimitives.ReadDoubleLittleEndian(item[4..]);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-stealth-state"; return false; }
        return true;
    }
}
