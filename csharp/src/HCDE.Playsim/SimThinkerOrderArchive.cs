using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimThinkerPlacement(uint Id, int StatNum, bool JustSpawned);

internal static class SimThinkerOrderArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.ThinkerOrder is not { } order) return;
        if (!state.IncludesBotIdentities || state.GeometryHealth is null
            || order.Select(item => item.Id).Distinct().Count() != order.Count)
            throw new InvalidOperationException("Thinker order requires unique actors and identity state.");
        var poses = state.Actors.GroupBy(pose => pose.Id).ToDictionary(group => group.Key, group => group.ToArray());
        var previousBucket = -1;
        foreach (var item in order)
        {
            if (item.Id is 0 or uint.MaxValue || item.StatNum is < 0 or > ThinkerStat.MaxStatNum
                || !poses.TryGetValue(item.Id, out var matches) || matches.Length != 1
                || matches[0].BotIdentity is { } identity
                    && (identity.StatNum != item.StatNum || identity.JustSpawned != item.JustSpawned))
                throw new InvalidOperationException("Invalid saved thinker placement.");
            var bucket = item.StatNum * 2 + (item.JustSpawned && item.StatNum >= ThinkerStat.FirstThinking ? 1 : 0);
            if (bucket < previousBucket)
                throw new InvalidOperationException("Saved thinker groups are out of order.");
            previousBucket = bucket;
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.ThinkerOrder is not { } order) return archive;
        var size = checked(12 + order.Count * 12);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], order.Count);
        for (var i = 0; i < order.Count; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(trailer[(8 + i * 12)..], order[i].Id);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * 12)..], order[i].StatNum);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * 12)..], order[i].JustSpawned ? 1 : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 216);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-thinker-order-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-thinker-order-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 199 or > 215 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-thinker-order-header"; return false; }
        var order = new SimThinkerPlacement[count];
        for (var i = 0; i < count; i++)
        {
            var fresh = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * 12)..]);
            if (fresh is not (0 or 1)) { error = "save-thinker-order-value"; return false; }
            order[i] = new(BinaryPrimitives.ReadUInt32LittleEndian(trailer[(8 + i * 12)..]),
                BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 12)..]), fresh != 0);
        }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        state.ThinkerOrder = order;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-thinker-order-state"; return false; }
        return true;
    }
}
