using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimProfileBrainState(MonsterMode Mode, int WindupTics, int Cooldown,
    int HealTics = 0, int RaiseTics = 0);

/// <summary>Class brain steering history with signed 16.16 coordinates.</summary>
public readonly record struct SimProfileChaseHistory(int BlockedTics, int PreviousX, int PreviousY,
    int LastSeenX, int LastSeenY, SimProfileBrainState? Runtime = null);

internal static class SimProfileChaseArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.ProfileChaseHistory is { } history
            && (!state.IncludesBotIdentities || actor.BotIdentity is not { BrainPresent: true, BasicAttack: null }
                || history.BlockedTics < 0 || history.Runtime is { } runtime
                    && (!Enum.IsDefined(runtime.Mode) || runtime.WindupTics < 0 || runtime.Cooldown < 0
                        || runtime.HealTics < 0 || runtime.RaiseTics < 0
                        || runtime.HealTics > 0 && runtime.RaiseTics > 0))))
            throw new InvalidOperationException("Invalid saved class brain chase history.");
        foreach (var actor in state.Actors)
        {
            var attack = actor.RepeatAttack ?? actor.ProfileAttack ?? actor.RevenantAttack
                ?? actor.ArchvileAttack ?? actor.LostSoulAttack;
            if (actor.ProfileChaseHistory?.Runtime is { } runtime && attack is { } saved
                && (runtime.Mode != saved.Mode || runtime.WindupTics != saved.WindupTics || runtime.Cooldown != saved.Cooldown
                    || runtime.HealTics > 0 || runtime.RaiseTics > 0))
                throw new InvalidOperationException("Class brain runtime conflicts with its saved attack sequence.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.ProfileChaseHistory.HasValue)) return archive;
        var includeRuntime = state.Actors.Any(actor => actor.ProfileChaseHistory?.Runtime is not null);
        var includeWaits = state.Actors.Any(actor => actor.ProfileChaseHistory?.Runtime is { } runtime
            && (runtime.HealTics != 0 || runtime.RaiseTics != 0));
        var stride = includeWaits ? 48 : includeRuntime ? 40 : 24;
        var size = checked(12 + state.Actors.Count * stride);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * stride)..]; var history = state.Actors[i].ProfileChaseHistory;
            BinaryPrimitives.WriteInt32LittleEndian(item, history.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], history?.BlockedTics ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[8..], history?.PreviousX ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[12..], history?.PreviousY ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[16..], history?.LastSeenX ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[20..], history?.LastSeenY ?? 0);
            if (includeRuntime)
            {
                var runtime = history?.Runtime;
                BinaryPrimitives.WriteInt32LittleEndian(item[24..], runtime.HasValue ? 1 : 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[28..], (int)(runtime?.Mode ?? MonsterMode.Idle));
                BinaryPrimitives.WriteInt32LittleEndian(item[32..], runtime?.WindupTics ?? 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[36..], runtime?.Cooldown ?? 0);
                if (includeWaits)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(item[40..], runtime?.HealTics ?? 0);
                    BinaryPrimitives.WriteInt32LittleEndian(item[44..], runtime?.RaiseTics ?? 0);
                }
            }
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), includeWaits ? (ushort)208 : includeRuntime ? (ushort)207 : (ushort)206);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-profile-chase-size"; return false; }
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var includeRuntime = version >= 207;
        var includeWaits = version == 208;
        var stride = includeWaits ? 48 : includeRuntime ? 40 : 24;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-profile-chase-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if ((prior < 199 || prior > 205 && prior is not (209 or 210 or 212 or 213)) || count < 0 || (long)count * stride + 12 != size)
        { error = "save-profile-chase-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer.Slice(8 + i * stride, stride);
            var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            if (present is not (0 or 1) || BinaryPrimitives.ReadInt32LittleEndian(item[4..]) < 0
                || present == 0 && item[4..].IndexOfAnyExcept((byte)0) >= 0)
            { error = "save-profile-chase-value"; return false; }
            if (includeRuntime)
            {
                var hasRuntime = BinaryPrimitives.ReadInt32LittleEndian(item[24..]);
                var mode = BinaryPrimitives.ReadInt32LittleEndian(item[28..]);
                var windup = BinaryPrimitives.ReadInt32LittleEndian(item[32..]);
                var cooldown = BinaryPrimitives.ReadInt32LittleEndian(item[36..]);
                var heal = includeWaits ? BinaryPrimitives.ReadInt32LittleEndian(item[40..]) : 0;
                var raise = includeWaits ? BinaryPrimitives.ReadInt32LittleEndian(item[44..]) : 0;
                if (hasRuntime is not (0 or 1) || !Enum.IsDefined((MonsterMode)mode) || windup < 0 || cooldown < 0
                    || heal < 0 || raise < 0 || heal > 0 && raise > 0
                    || hasRuntime == 0 && (mode != 0 || windup != 0 || cooldown != 0 || heal != 0 || raise != 0))
                { error = "save-profile-chase-value"; return false; }
            }
        }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-profile-chase-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * stride)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].ProfileChaseHistory = new(BinaryPrimitives.ReadInt32LittleEndian(item[4..]),
                    BinaryPrimitives.ReadInt32LittleEndian(item[8..]), BinaryPrimitives.ReadInt32LittleEndian(item[12..]),
                    BinaryPrimitives.ReadInt32LittleEndian(item[16..]), BinaryPrimitives.ReadInt32LittleEndian(item[20..]),
                    includeRuntime && BinaryPrimitives.ReadInt32LittleEndian(item[24..]) == 1
                        ? new((MonsterMode)BinaryPrimitives.ReadInt32LittleEndian(item[28..]),
                            BinaryPrimitives.ReadInt32LittleEndian(item[32..]), BinaryPrimitives.ReadInt32LittleEndian(item[36..]),
                            includeWaits ? BinaryPrimitives.ReadInt32LittleEndian(item[40..]) : 0,
                            includeWaits ? BinaryPrimitives.ReadInt32LittleEndian(item[44..]) : 0) : null);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-profile-chase-state"; return false; }
        return true;
    }
}
