using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimProjectilePointers(uint OwnerId, uint? TracerTargetId);

internal static class SimProjectilePointerArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (!state.Actors.Any(a => a.ProjectilePointers.HasValue)) return;
        if (state.Actors.Any(a => a.ProjectilePointers is { } p &&
            (p.OwnerId == 0 || p.TracerTargetId == 0 || !a.ProjectileLifetime.HasValue)))
            throw new InvalidOperationException("Invalid projectile pointer archive.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.ProjectilePointers.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var pointers = state.Actors[i].ProjectilePointers;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(start + 8 + i * 8), pointers?.OwnerId ?? 0);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(start + 12 + i * 8), pointers?.TracerTargetId ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 24); return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 8 != 0)
        { error = "save-pointer-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior != 23 || count < 0 || count != (size - 12) / 8)
        { error = "save-pointer-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-pointer-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var owner = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 8 + i * 8)..]);
            var tracer = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 12 + i * 8)..]);
            if (owner == 0 && tracer != 0 || owner != 0 &&
                !state.Actors[i].ProjectileLifetime.HasValue)
            { state = new(); error = "save-pointer-state"; return false; }
            state.Actors[i].ProjectilePointers = owner == 0 ? null : new(owner, tracer == 0 ? null : tracer);
        }
        return true;
    }
}
