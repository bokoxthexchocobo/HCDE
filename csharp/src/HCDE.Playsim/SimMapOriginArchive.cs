using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimMapOriginArchive
{
    internal static void Validate(SimSaveState state, bool decoration = false)
    {
        foreach (var pose in state.Actors.Where(pose => Get(pose, decoration).HasValue))
        {
            var identity = Get(pose, decoration)!.Value;
            if (!state.IncludesBotIdentities || state.GeometryHealth is null || !pose.HasPhysics
                || pose.Id is 0 or uint.MaxValue || state.Actors.Count(other => other.Id == pose.Id) != 1
                || identity.ThingIndex < 0 || state.Actors.Count(other => other.MapPickupIdentity?.ThingIndex == identity.ThingIndex || other.MapDecorationIdentity?.ThingIndex == identity.ThingIndex) != 1
                || !(decoration ? DoomDecorationDefaults.IsDecoration(identity.DoomEdNum) : PickupCatalog.IsPickup(identity.DoomEdNum, ravenHealth: true))
                || identity.ThingId is < 0 or > ushort.MaxValue || identity.StatNum is < 0 or > ThinkerStat.MaxStatNum
                || pose.MapPickupIdentity.HasValue && pose.MapDecorationIdentity.HasValue
                || pose.PuffIdentity.HasValue || pose.BotIdentity.HasValue || pose.DroppedPickupIdentity.HasValue || pose.ProjectileLifetime.HasValue || pose.IceChunkLifecycle.HasValue
                || pose.BrainDisabled || pose.BasicAttackState.HasValue || pose.ProfileChaseHistory.HasValue
                || pose.PainDeath.HasValue || pose.RepeatAttack.HasValue || pose.ProfileAttack.HasValue
                || pose.RevenantAttack.HasValue || pose.ArchvileAttack.HasValue || pose.LostSoulAttack.HasValue
                || state.ThinkerOrder is { } order && order.Any(item => item.Id == pose.Id
                    && (item.StatNum != identity.StatNum || item.JustSpawned != identity.JustSpawned)))
                throw new InvalidOperationException("Invalid saved map actor identity.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive, bool decoration = false)
    {
        Validate(state, decoration);
        if (!state.Actors.Any(pose => Get(pose, decoration).HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 16);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var identity = Get(state.Actors[i], decoration); var entry = trailer[(8 + i * 16)..];
            BinaryPrimitives.WriteInt32LittleEndian(entry, identity?.ThingIndex ?? -1);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], identity?.DoomEdNum ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[8..], identity?.ThingId ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[12..], identity is { } item ? item.StatNum * 2 + (item.JustSpawned ? 1 : 0) : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), (ushort)(decoration ? 220 : 218));
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error, bool decoration = false)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-map-pickup-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-map-pickup-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if ((prior < 199 || prior > (decoration ? 219 : 217)) || count < 0 || (long)count * 16 + 12 != size)
        { error = "save-map-pickup-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 16)..]; var origin = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var type = BinaryPrimitives.ReadInt32LittleEndian(entry[4..]); var tid = BinaryPrimitives.ReadInt32LittleEndian(entry[8..]);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[12..]);
            if (origin < -1 || tid is < 0 or > ushort.MaxValue || lifecycle is < 0 or > 255
                || origin >= 0 && !(decoration ? DoomDecorationDefaults.IsDecoration(type) : PickupCatalog.IsPickup(type, ravenHealth: true))
                || origin == -1 && (type != 0 || tid != 0 || lifecycle != 0))
            { error = "save-map-pickup-value"; return false; }
        }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-map-pickup-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 16)..]; var origin = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[12..]);
            if (origin >= 0) Set(state.Actors[i], decoration, new(origin,
                BinaryPrimitives.ReadInt32LittleEndian(entry[4..]), BinaryPrimitives.ReadInt32LittleEndian(entry[8..]),
                (lifecycle & 1) != 0, lifecycle >> 1));
        }
        try { Validate(state, decoration); }
        catch (InvalidOperationException) { state = new(); error = "save-map-pickup-state"; return false; }
        return true;
    }

    private static SimMapPickupIdentity? Get(SimActorPose pose, bool decoration)
        => decoration && pose.MapDecorationIdentity is { } item
            ? new(item.ThingIndex, item.DoomEdNum, item.ThingId, item.JustSpawned, item.StatNum)
            : decoration ? null : pose.MapPickupIdentity;

    private static void Set(SimActorPose pose, bool decoration, SimMapPickupIdentity identity)
    {
        if (decoration) pose.MapDecorationIdentity = new(identity.ThingIndex, identity.DoomEdNum, identity.ThingId, identity.JustSpawned, identity.StatNum);
        else pose.MapPickupIdentity = identity;
    }
}
