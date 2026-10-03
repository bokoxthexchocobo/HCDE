using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimProjectileLifetime(int Tics, ProjectileKind Kind);

internal static class SimProjectileLifetimeArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (!state.Actors.Any(a => a.ProjectileLifetime.HasValue)) return;
        if (state.GeometryHealth is null || state.Actors.Any(a => !a.ContactFlags.HasValue) ||
            state.Actors.Any(a => a.ProjectileLifetime is { } p && (p.Tics is < 1 or > 175 || !Enum.IsDefined(p.Kind))))
            throw new InvalidOperationException("Invalid projectile lifetime archive.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(a => a.ProjectileLifetime.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 8);
        var bytes = new byte[checked(archive.Length + size)]; archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var lifetime = state.Actors[i].ProjectileLifetime;
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 8), lifetime?.Tics ?? -1);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 12 + i * 8), lifetime.HasValue ? (int)lifetime.Value.Kind : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 23); return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 8 != 0)
        { error = "save-lifetime-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior is < 18 or > 22 || count < 0 || count != (size - 12) / 8)
        { error = "save-lifetime-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-lifetime-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var tics = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 8)..]);
            var kind = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 12 + i * 8)..]);
            if (tics != -1 && (tics is < 1 or > 175 || !Enum.IsDefined((ProjectileKind)kind)) || tics == -1 && kind != 0)
            { state = new(); error = "save-lifetime-state"; return false; }
            state.Actors[i].ProjectileLifetime = tics == -1 ? null : new(tics, (ProjectileKind)kind);
        }
        return true;
    }
}
