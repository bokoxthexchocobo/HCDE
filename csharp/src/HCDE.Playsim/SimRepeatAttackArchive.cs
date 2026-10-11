using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimRepeatAttack(int NativeType, int AttackTic, uint? TargetId,
    MonsterMode Mode, int WindupTics, int Cooldown, bool SeparateMelee = false, bool FireExists = false);

internal static class SimRepeatAttackArchive
{
    private static bool Valid(SimRepeatAttack s) => s.NativeType is 65 or 7 or 84
        && s.AttackTic >= 0 && s.AttackTic < (s.NativeType == 65 ? 19 : s.NativeType == 7 ? 29 : 35)
        && s.TargetId != 0 && s.Mode is MonsterMode.Windup or MonsterMode.Recovery
        && s.WindupTics >= 0 && s.WindupTics <= 30 && s.Cooldown >= 0;

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.RepeatAttack.HasValue)) return archive;
        if (state.GeometryHealth == null || state.Actors.Any(a => a.RepeatAttack is { } s && !Valid(s)))
            throw new InvalidOperationException("Invalid repeat attack archive.");
        var size = checked(12 + state.Actors.Count * 24);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var trailer = bytes.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var s = state.Actors[i].RepeatAttack;
            var entry = trailer.Slice(8 + i * 24, 24);
            BinaryPrimitives.WriteInt32LittleEndian(entry, s?.NativeType ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], s?.AttackTic ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], s?.TargetId ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[12..], (int)(s?.Mode ?? 0));
            BinaryPrimitives.WriteInt32LittleEndian(entry[16..], s?.WindupTics ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[20..], s?.Cooldown ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 138); return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-repeatattack-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 24 != 0)
        { error = "save-repeatattack-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 137 || count != (size - 12) / 24)
        { error = "save-repeatattack-header"; return false; }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-repeatattack-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var e = trailer.Slice(8 + i * 24, 24);
            var type = BinaryPrimitives.ReadInt32LittleEndian(e);
            if (type == 0)
            {
                if (e[4..].IndexOfAnyExcept((byte)0) >= 0) { state = new(); error = "save-repeatattack-state"; return false; }
                continue;
            }
            var target = BinaryPrimitives.ReadUInt32LittleEndian(e[8..]);
            var s = new SimRepeatAttack(type, BinaryPrimitives.ReadInt32LittleEndian(e[4..]),
                target == 0 ? null : target, (MonsterMode)BinaryPrimitives.ReadInt32LittleEndian(e[12..]),
                BinaryPrimitives.ReadInt32LittleEndian(e[16..]), BinaryPrimitives.ReadInt32LittleEndian(e[20..]));
            if (!Valid(s)) { state = new(); error = "save-repeatattack-state"; return false; }
            state.Actors[i].RepeatAttack = s;
        }
        return true;
    }
}
