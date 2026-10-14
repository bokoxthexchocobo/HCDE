using System.Buffers.Binary;

namespace HCDE.Playsim;

/// <summary>Basic brain replay state; chase coordinates use signed 16.16 fixed-point values.</summary>
public readonly record struct SimBasicAttackState(MonsterMode Mode, int WindupTics, int Cooldown,
    int BlockedTics = 0, int PreviousX = 0, int PreviousY = 0, int LastSeenX = 0, int LastSeenY = 0,
    int RaiseTics = 0);

internal static class SimBasicAttackArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.BasicAttackState is { } saved &&
            (!state.IncludesBotIdentities || actor.BotIdentity?.BasicAttack is null
                || !Enum.IsDefined(saved.Mode) || saved.WindupTics < 0 || saved.Cooldown < 0 || saved.BlockedTics < 0
                || saved.RaiseTics < 0)))
            throw new InvalidOperationException("Invalid saved basic attack state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(actor => actor.BasicAttackState.HasValue)) return archive;
        var includeRaise = state.Actors.Any(actor => actor.BasicAttackState is { RaiseTics: not 0 });
        var includeChase = includeRaise || state.Actors.Any(actor => actor.BasicAttackState is { } saved
            && (saved.BlockedTics != 0 || saved.PreviousX != 0 || saved.PreviousY != 0
                || saved.LastSeenX != 0 || saved.LastSeenY != 0));
        var stride = includeRaise ? 40 : includeChase ? 36 : 16;
        var size = checked(12 + state.Actors.Count * stride);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * stride)..]; var saved = state.Actors[i].BasicAttackState;
            BinaryPrimitives.WriteInt32LittleEndian(item, saved.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], (int)(saved?.Mode ?? MonsterMode.Idle));
            BinaryPrimitives.WriteInt32LittleEndian(item[8..], saved?.WindupTics ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[12..], saved?.Cooldown ?? 0);
            if (includeChase)
            {
                BinaryPrimitives.WriteInt32LittleEndian(item[16..], saved?.BlockedTics ?? 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[20..], saved?.PreviousX ?? 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[24..], saved?.PreviousY ?? 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[28..], saved?.LastSeenX ?? 0);
                BinaryPrimitives.WriteInt32LittleEndian(item[32..], saved?.LastSeenY ?? 0);
                if (includeRaise) BinaryPrimitives.WriteInt32LittleEndian(item[36..], saved?.RaiseTics ?? 0);
            }
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), includeRaise ? (ushort)209 : includeChase ? (ushort)205 : (ushort)204);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-basic-attack-size"; return false; }
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var includeRaise = version == 209;
        var includeChase = includeRaise || version == 205;
        var stride = includeRaise ? 40 : includeChase ? 36 : 16;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-basic-attack-size"; return false; }
        var trailer = bytes[^size..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is not (202 or 203 or 210 or 212 or 213) || count < 0 || (long)count * stride + 12 != size)
        { error = "save-basic-attack-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * stride)..];
            var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var mode = BinaryPrimitives.ReadInt32LittleEndian(item[4..]);
            var windup = BinaryPrimitives.ReadInt32LittleEndian(item[8..]);
            var cooldown = BinaryPrimitives.ReadInt32LittleEndian(item[12..]);
            var blocked = includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[16..]) : 0;
            var raise = includeRaise ? BinaryPrimitives.ReadInt32LittleEndian(item[36..]) : 0;
            if (present is not (0 or 1) || !Enum.IsDefined((MonsterMode)mode) || windup < 0 || cooldown < 0
                || blocked < 0 || raise < 0 || present == 0 && (mode != 0 || windup != 0 || cooldown != 0 || raise != 0
                    || includeChase && item.Slice(16, 20).IndexOfAnyExcept((byte)0) >= 0))
            { error = "save-basic-attack-value"; return false; }
        }
        var legacy = bytes[..^size].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-basic-attack-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * stride)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 1)
                state.Actors[i].BasicAttackState = new((MonsterMode)BinaryPrimitives.ReadInt32LittleEndian(item[4..]),
                    BinaryPrimitives.ReadInt32LittleEndian(item[8..]), BinaryPrimitives.ReadInt32LittleEndian(item[12..]),
                    includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[16..]) : 0,
                    includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[20..]) : 0,
                    includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[24..]) : 0,
                    includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[28..]) : 0,
                    includeChase ? BinaryPrimitives.ReadInt32LittleEndian(item[32..]) : 0,
                    includeRaise ? BinaryPrimitives.ReadInt32LittleEndian(item[36..]) : 0);
        }
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-basic-attack-state"; return false; }
        return true;
    }
}
