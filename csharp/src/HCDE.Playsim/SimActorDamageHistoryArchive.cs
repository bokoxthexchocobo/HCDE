using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimActorDamageHistory(int DeathCount, uint SourceId);

internal static class SimActorDamageHistoryArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.DamageHistory is { } history &&
            (!state.IncludesBotIdentities || state.GeometryHealth is null || history.DeathCount < 0)))
            throw new InvalidOperationException("Invalid saved actor damage history.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.DamageHistory.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 8)..]; var history = state.Actors[i].DamageHistory;
            BinaryPrimitives.WriteInt32LittleEndian(item, history?.DeathCount ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[4..], history?.SourceId ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 214);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-damage-history-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-damage-history-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 199 or > 213 || count < 0 || (long)count * 8 + 12 != size)
        { error = "save-damage-history-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 8)..]) < 0)
            { error = "save-damage-history-value"; return false; }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-damage-history-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 8)..];
            var deaths = BinaryPrimitives.ReadInt32LittleEndian(item);
            var source = BinaryPrimitives.ReadUInt32LittleEndian(item[4..]);
            if (deaths != 0 || source != 0) state.Actors[i].DamageHistory = new(deaths, source);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-damage-history-state"; return false; }
        return true;
    }
}
