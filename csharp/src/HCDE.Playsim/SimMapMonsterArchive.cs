using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimMapMonsterArchive
{
    internal static void Validate(SimSaveState state)
    {
        foreach (var pose in state.Actors.Where(pose => pose.MapMonsterThingIndex.HasValue))
        {
            var origin = pose.MapMonsterThingIndex!.Value;
            if (!state.IncludesBotIdentities || origin < 0 || pose.BotIdentity is not { SpawnedLostSoul: false }
                || pose.MapPickupIdentity.HasValue || pose.MapDecorationIdentity.HasValue || pose.DroppedPickupIdentity.HasValue
                || pose.PuffIdentity.HasValue || pose.ProjectileLifetime.HasValue || pose.IceChunkLifecycle.HasValue
                || state.Actors.Count(other => other.MapMonsterThingIndex == origin
                    || other.MapPickupIdentity?.ThingIndex == origin || other.MapDecorationIdentity?.ThingIndex == origin) != 1)
                throw new InvalidOperationException("Invalid saved map monster identity.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(pose => pose.MapMonsterThingIndex.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 4)..], state.Actors[i].MapMonsterThingIndex ?? -1);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 221);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-map-monster-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-map-monster-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 199 or > 220 || count < 0 || (long)count * 4 + 12 != size)
        { error = "save-map-monster-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]) < -1)
            { error = "save-map-monster-value"; return false; }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-map-monster-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var origin = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            if (origin >= 0) state.Actors[i].MapMonsterThingIndex = origin;
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-map-monster-state"; return false; }
        return true;
    }
}
