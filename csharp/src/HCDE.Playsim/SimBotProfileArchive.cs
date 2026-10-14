using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimBotProfileArchive
{
    internal static void Validate(SimSaveState state)
    {
        foreach (var actor in state.Actors)
        {
            if (actor.BotIdentity is not { ProfileType: { } profile } identity) continue;
            var attack = actor.RepeatAttack ?? actor.ProfileAttack ?? actor.RevenantAttack
                ?? actor.ArchvileAttack ?? actor.LostSoulAttack;
            if (!state.IncludesBotIdentities || !identity.BrainPresent || identity.BasicAttack.HasValue
                || DoomMonsterAttacks.Find(profile) is null || attack is { } saved && saved.NativeType != profile
                || actor.PainDeath.HasValue && profile != 71
                || actor.ProfileChaseHistory?.Runtime is { HealTics: > 0 } && profile != 64)
                throw new InvalidOperationException("Invalid saved replacement bot attack profile.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.BotIdentity?.ProfileType is not null)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 4)..], state.Actors[i].BotIdentity?.ProfileType ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 211);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-bot-profile-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-bot-profile-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if ((prior < 199 || prior > 210 && prior is not (212 or 213)) || count < 0 || (long)count * 4 + 12 != size)
        { error = "save-bot-profile-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var type = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            if (type != 0 && DoomMonsterAttacks.Find(type) is null)
            { error = "save-bot-profile-value"; return false; }
        }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-bot-profile-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var type = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            if (type == 0) continue;
            if (state.Actors[i].BotIdentity is not { } identity)
            { state = new(); error = "save-bot-profile-state"; return false; }
            state.Actors[i].BotIdentity = identity with { ProfileType = type };
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-bot-profile-state"; return false; }
        return true;
    }
}
