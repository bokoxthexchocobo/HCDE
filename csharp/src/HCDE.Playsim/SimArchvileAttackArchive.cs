using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimArchvileAttackArchive
{
    private static bool Valid(SimRepeatAttack s) => s.NativeType == 64
        && s.AttackTic >= 0 && s.AttackTic < 94
        && s.TargetId != 0 && s.Mode is MonsterMode.Windup or MonsterMode.Recovery
        && s.WindupTics >= 0 && s.WindupTics <= 10 && s.Cooldown >= 0
        && s.FireExists == (s.AttackTic >= 10);
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.ArchvileAttack.HasValue)) return archive;
        if (state.GeometryHealth == null || state.Actors.Any(a => a.ArchvileAttack is { } s && !Valid(s)))
            throw new InvalidOperationException("Invalid repeat attack archive.");
        var size = checked(12 + state.Actors.Count * 28);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var trailer = bytes.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var s = state.Actors[i].ArchvileAttack;
            var entry = trailer.Slice(8 + i * 28, 28);
            BinaryPrimitives.WriteInt32LittleEndian(entry, s?.NativeType ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], s?.AttackTic ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], s?.TargetId ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[12..], (int)(s?.Mode ?? 0));
            BinaryPrimitives.WriteInt32LittleEndian(entry[16..], s?.WindupTics ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[20..], s?.Cooldown ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(entry[24..], s?.FireExists == true ? 1 : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 142); return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-archvileattack-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 28 != 0)
        { error = "save-archvileattack-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 141 || count != (size - 12) / 28)
        { error = "save-archvileattack-header"; return false; }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-archvileattack-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var e = trailer.Slice(8 + i * 28, 28);
            var type = BinaryPrimitives.ReadInt32LittleEndian(e);
            if (type == 0)
            {
                if (e[4..].IndexOfAnyExcept((byte)0) >= 0) { state = new(); error = "save-archvileattack-state"; return false; }
                continue;
            }
            var fire = BinaryPrimitives.ReadInt32LittleEndian(e[24..]);
            if (fire is < 0 or > 1) { state = new(); error = "save-archvileattack-state"; return false; }
            var target = BinaryPrimitives.ReadUInt32LittleEndian(e[8..]);
            var s = new SimRepeatAttack(type, BinaryPrimitives.ReadInt32LittleEndian(e[4..]),
                target == 0 ? null : target, (MonsterMode)BinaryPrimitives.ReadInt32LittleEndian(e[12..]),
                BinaryPrimitives.ReadInt32LittleEndian(e[16..]), BinaryPrimitives.ReadInt32LittleEndian(e[20..]), FireExists: fire == 1);
            if (!Valid(s)) { state = new(); error = "save-archvileattack-state"; return false; }
            state.Actors[i].ArchvileAttack = s;
        }
        return true;
    }
}
