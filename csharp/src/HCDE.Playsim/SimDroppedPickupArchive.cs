using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimDroppedPickupIdentity(int DoomEdNum, int ThingId, bool JustSpawned, int StatNum);

internal static class SimDroppedPickupArchive
{
    internal static void Validate(SimSaveState state)
    {
        foreach (var pose in state.Actors.Where(pose => pose.DroppedPickupIdentity.HasValue))
        {
            var identity = pose.DroppedPickupIdentity!.Value;
            if (!state.IncludesBotIdentities || state.GeometryHealth is null || !pose.HasPhysics
                || pose.Id is 0 or uint.MaxValue || state.Actors.Count(other => other.Id == pose.Id) != 1
                || !PickupCatalog.IsPickup(identity.DoomEdNum, ravenHealth: true)
                || identity.ThingId is < 0 or > ushort.MaxValue || identity.StatNum is < 0 or > ThinkerStat.MaxStatNum
                || pose.BotIdentity.HasValue || pose.ProjectileLifetime.HasValue || pose.IceChunkLifecycle.HasValue
                || pose.BrainDisabled || pose.BasicAttackState.HasValue || pose.ProfileChaseHistory.HasValue
                || pose.PainDeath.HasValue || pose.RepeatAttack.HasValue || pose.ProfileAttack.HasValue
                || pose.RevenantAttack.HasValue || pose.ArchvileAttack.HasValue || pose.LostSoulAttack.HasValue
                || state.ThinkerOrder is { } order && order.Any(item => item.Id == pose.Id
                    && (item.StatNum != identity.StatNum || item.JustSpawned != identity.JustSpawned)))
                throw new InvalidOperationException("Invalid saved dropped pickup identity.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(pose => pose.DroppedPickupIdentity.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var identity = state.Actors[i].DroppedPickupIdentity;
            var entry = trailer[(8 + i * 12)..];
            BinaryPrimitives.WriteInt32LittleEndian(entry, identity?.DoomEdNum ?? -1);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], identity?.ThingId ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[8..], identity is { } item ? item.StatNum * 2 + (item.JustSpawned ? 1 : 0) : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 217);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-dropped-pickup-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-dropped-pickup-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 199 or > 216 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-dropped-pickup-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..]; var type = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var tid = BinaryPrimitives.ReadInt32LittleEndian(entry[4..]); var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[8..]);
            if (type != -1 && !PickupCatalog.IsPickup(type, ravenHealth: true) || tid is < 0 or > ushort.MaxValue
                || lifecycle is < 0 or > 255 || type == -1 && (tid != 0 || lifecycle != 0))
            { error = "save-dropped-pickup-value"; return false; }
        }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-dropped-pickup-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..]; var type = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[8..]);
            if (type != -1) state.Actors[i].DroppedPickupIdentity = new(type,
                BinaryPrimitives.ReadInt32LittleEndian(entry[4..]), (lifecycle & 1) != 0, lifecycle >> 1);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-dropped-pickup-state"; return false; }
        return true;
    }
}
