using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimPuffIdentity(int ThingId, bool JustSpawned, int StatNum, uint DamageSourceId);

internal static class SimPuffArchive
{
    internal static void Validate(SimSaveState state)
    {
        foreach (var pose in state.Actors.Where(pose => pose.PuffIdentity.HasValue))
        {
            var identity = pose.PuffIdentity!.Value;
            if (!state.IncludesBotIdentities || state.GeometryHealth is null || !pose.HasPhysics
                || pose.Id is 0 or uint.MaxValue || state.Actors.Count(other => other.Id == pose.Id) != 1
                || identity.ThingId is < 0 or > ushort.MaxValue || identity.StatNum is < 0 or > ThinkerStat.MaxStatNum
                || pose.State is < 0 or > 3 || pose.StateTics < -1
                || pose.BotIdentity.HasValue || pose.DroppedPickupIdentity.HasValue || pose.MapPickupIdentity.HasValue
                || pose.ProjectileLifetime.HasValue || pose.IceChunkLifecycle.HasValue
                || pose.BrainDisabled || pose.BasicAttackState.HasValue || pose.ProfileChaseHistory.HasValue
                || pose.PainDeath.HasValue || pose.RepeatAttack.HasValue || pose.ProfileAttack.HasValue
                || pose.RevenantAttack.HasValue || pose.ArchvileAttack.HasValue || pose.LostSoulAttack.HasValue
                || identity.DamageSourceId != 0 && !state.Actors.Any(other => other.Id == identity.DamageSourceId)
                || state.ThinkerOrder is { } order && order.Any(item => item.Id == pose.Id
                    && (item.StatNum != identity.StatNum || item.JustSpawned != identity.JustSpawned)))
                throw new InvalidOperationException("Invalid saved puff identity.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(pose => pose.PuffIdentity.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var identity = state.Actors[i].PuffIdentity; var entry = trailer[(8 + i * 12)..];
            BinaryPrimitives.WriteInt32LittleEndian(entry, identity?.ThingId ?? -1);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], identity is { } item ? item.StatNum * 2 + (item.JustSpawned ? 1 : 0) : 0);
            BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], identity?.DamageSourceId ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 219);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-puff-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-puff-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 199 or > 218 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-puff-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..]; var tid = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[4..]);
            if (tid is < -1 or > ushort.MaxValue || lifecycle is < 0 or > 255
                || tid == -1 && (lifecycle != 0 || BinaryPrimitives.ReadUInt32LittleEndian(entry[8..]) != 0))
            { error = "save-puff-value"; return false; }
        }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-puff-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..]; var tid = BinaryPrimitives.ReadInt32LittleEndian(entry);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(entry[4..]);
            if (tid >= 0) state.Actors[i].PuffIdentity = new(tid, (lifecycle & 1) != 0, lifecycle >> 1,
                BinaryPrimitives.ReadUInt32LittleEndian(entry[8..]));
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-puff-state"; return false; }
        return true;
    }
}
