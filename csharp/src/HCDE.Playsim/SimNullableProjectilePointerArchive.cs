using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimNullableProjectilePointerArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.ProjectilePointers is { OwnerId: 0 })) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            var pointers = state.Actors[i].ProjectilePointers;
            BinaryPrimitives.WriteInt32LittleEndian(item, pointers.HasValue ? 1 : 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[4..], pointers?.OwnerId ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(item[8..], pointers?.TracerTargetId ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 190);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-nullable-pointer-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-nullable-pointer-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 24 || prior > 189 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-nullable-pointer-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            if (present is not (0 or 1) || present == 0 &&
                (BinaryPrimitives.ReadUInt32LittleEndian(item[4..]) != 0 || BinaryPrimitives.ReadUInt32LittleEndian(item[8..]) != 0))
            { error = "save-nullable-pointer-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-nullable-pointer-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            if (BinaryPrimitives.ReadInt32LittleEndian(item) == 0)
            {
                if (state.Actors[i].ProjectilePointers.HasValue)
                { state = new(); error = "save-nullable-pointer-state"; return false; }
                continue;
            }
            if (!state.Actors[i].ProjectileLifetime.HasValue)
            { state = new(); error = "save-nullable-pointer-state"; return false; }
            var owner = BinaryPrimitives.ReadUInt32LittleEndian(item[4..]);
            var tracer = BinaryPrimitives.ReadUInt32LittleEndian(item[8..]);
            state.Actors[i].ProjectilePointers = new(owner, tracer == 0 ? null : tracer);
        }
        return true;
    }
}
