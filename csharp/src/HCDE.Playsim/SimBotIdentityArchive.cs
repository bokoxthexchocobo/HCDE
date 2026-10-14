using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimBotIdentity(int DoomEdNum, int ThingId, bool JustSpawned = false,
    bool BrainPresent = true, int StatNum = ThinkerStat.Default, MonsterAttack? BasicAttack = null,
    bool Dormant = false, int? ProfileType = null, bool SpawnedLostSoul = false, bool FailedLostSoulSpawn = false);

internal static class SimBotIdentityArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (!state.IncludesBotIdentities && state.Actors.Any(actor => actor.BotIdentity.HasValue))
            throw new InvalidOperationException("Bot identities require a complete bot identity table.");
        if (state.IncludesBotIdentities && state.GeometryHealth is null)
            throw new InvalidOperationException("Bot identity archives require geometry state.");
        if (state.Actors.Any(actor => actor.BotIdentity is { } identity &&
            (identity.DoomEdNum is < 1 or > ushort.MaxValue || identity.ThingId is < 0 or > ushort.MaxValue
                || identity.StatNum is < 0 or > ThinkerStat.MaxStatNum
                || identity.FailedLostSoulSpawn && !identity.SpawnedLostSoul
                || identity.BasicAttack is { } attack && (!Enum.IsDefined(attack) || !identity.BrainPresent
                    || actor.PainDeath.HasValue || actor.RepeatAttack.HasValue || actor.ProfileAttack.HasValue
                    || actor.RevenantAttack.HasValue || actor.ArchvileAttack.HasValue || actor.LostSoulAttack.HasValue)
                || actor.Id is 0 or uint.MaxValue || !actor.HasPhysics || actor.ProjectileLifetime.HasValue
                || actor.IceChunkLifecycle.HasValue || !identity.BrainPresent && (actor.BrainDisabled || actor.SkullChargeFlag == 1
                    || actor.PainDeath.HasValue || actor.RepeatAttack.HasValue || actor.ProfileAttack.HasValue
                    || actor.RevenantAttack.HasValue || actor.ArchvileAttack.HasValue || actor.LostSoulAttack.HasValue)
                || state.Actors.Count(other => other.Id == actor.Id) != 1)))
            throw new InvalidOperationException("Invalid saved bot identity.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.IncludesBotIdentities) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var includeBasicAttack = state.Actors.Any(actor => actor.BotIdentity?.BasicAttack is not null);
        var includeDormant = state.Actors.Any(actor => actor.BotIdentity is { Dormant: true });
        var includeSoul = state.Actors.Any(actor => actor.BotIdentity is { SpawnedLostSoul: true });
        var includeFailed = state.Actors.Any(actor => actor.BotIdentity is { FailedLostSoulSpawn: true });
        var includeStat = includeSoul || includeDormant || includeBasicAttack || state.Actors.Any(actor => actor.BotIdentity is { StatNum: not ThinkerStat.Default });
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 12)..], state.Actors[i].BotIdentity?.DoomEdNum ?? -1);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * 12)..], state.Actors[i].BotIdentity?.ThingId ?? 0);
            var identity = state.Actors[i].BotIdentity;
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * 12)..],
                (identity?.JustSpawned == true ? 1 : 0) | (identity is { BrainPresent: false } ? 2 : 0)
                    | (includeStat && identity.HasValue ? identity.Value.StatNum << 2 : 0)
                    | (identity?.BasicAttack is { } attack ? ((int)attack + 1) << 9 : 0)
                    | (identity?.Dormant == true ? 2048 : 0) | (identity?.SpawnedLostSoul == true ? 4096 : 0)
                    | (identity?.FailedLostSoulSpawn == true ? 8192 : 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4),
            includeFailed ? (ushort)213 : includeSoul ? (ushort)212 : includeDormant ? (ushort)210 : includeBasicAttack ? (ushort)203 : includeStat ? (ushort)201 : state.Actors.Any(actor => actor.BotIdentity is { BrainPresent: false }) ? (ushort)200 : (ushort)199);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-bot-identity-size"; return false; }
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var hasBrainPresence = version >= 200;
        var hasStat = version >= 201;
        var hasFailed = version == 213;
        var hasSoul = hasFailed || version == 212;
        var hasDormant = hasSoul || version == 210;
        var hasBasicAttack = hasDormant || version == 203;
        var maximumLifecycle = hasFailed ? 16383 : hasSoul ? 8191 : hasDormant ? 4095
            : hasBasicAttack ? 2047 : hasStat ? (ThinkerStat.MaxStatNum << 2) | 3 : hasBrainPresence ? 3 : 1;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-bot-identity-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 198 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-bot-identity-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var type = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 12)..]);
            var tid = BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 12)..]);
            var fresh = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * 12)..]);
            if (type != -1 && type is < 1 or > ushort.MaxValue || tid is < 0 or > ushort.MaxValue
                || fresh < 0 || fresh > maximumLifecycle
                || hasBasicAttack && ((fresh >> 9) & 3) != 0 && (fresh & 2) != 0
                || type == -1 && (tid != 0 || fresh != 0))
            { error = "save-bot-identity-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-bot-identity-count"; return false; }
        state.IncludesBotIdentities = true;
        for (var i = 0; i < count; i++)
        {
            var type = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 12)..]);
            var lifecycle = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * 12)..]);
            var attackCode = hasBasicAttack ? (lifecycle >> 9) & 3 : 0;
            if (type != -1) state.Actors[i].BotIdentity = new(type,
                BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * 12)..]),
                (lifecycle & 1) != 0, (lifecycle & 2) == 0,
                hasStat ? (lifecycle >> 2) & ThinkerStat.MaxStatNum : ThinkerStat.Default,
                attackCode > 0 ? (MonsterAttack)(attackCode - 1) : null,
                hasDormant && (lifecycle & 2048) != 0, null, hasSoul && (lifecycle & 4096) != 0,
                hasFailed && (lifecycle & 8192) != 0);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-bot-identity-state"; return false; }
        return true;
    }
}
