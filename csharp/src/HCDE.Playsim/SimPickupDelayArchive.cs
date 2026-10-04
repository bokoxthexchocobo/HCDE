using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimPickupDelayArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.PickupDelay > 0 || a.SuppressWeaponPickupAmmo || a.Dropped || a.AlwaysPickupOverride.HasValue) && state.GeometryHealth is null)
            throw new InvalidOperationException("Pickup delays require a current archive.");
        if (state.Actors.Any(a => a.PickupDelay is < 0 or > 30
            || ((a.PickupDelay > 0 || a.SuppressWeaponPickupAmmo || a.AlwaysPickupOverride.HasValue) && !a.Pickup.HasValue)))
            throw new InvalidOperationException("Invalid pickup delay archive.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        var hasWeaponAmmoFlags = state.Actors.Any(a => a.SuppressWeaponPickupAmmo);
        var hasDroppedFlags = state.Actors.Any(a => a.Dropped);
        var hasAlwaysPickupFlags = state.Actors.Any(a => a.AlwaysPickupOverride.HasValue);
        if (!hasAlwaysPickupFlags && !hasDroppedFlags && !hasWeaponAmmoFlags && !state.Actors.Any(a => a.PickupDelay > 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4),
                state.Actors[i].PickupDelay | (state.Actors[i].SuppressWeaponPickupAmmo ? 64 : 0)
                | (state.Actors[i].Dropped ? 128 : 0)
                | (state.Actors[i].AlwaysPickupOverride.HasValue ? 256 : 0)
                | (state.Actors[i].AlwaysPickupOverride == true ? 512 : 0));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), hasAlwaysPickupFlags ? (ushort)41 : hasDroppedFlags ? (ushort)40 : hasWeaponAmmoFlags ? (ushort)39 : (ushort)38);
        return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var hasWeaponAmmoFlags = version >= 39;
        var hasDroppedFlags = version >= 40;
        var hasAlwaysPickupFlags = version >= 41;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
        { error = "save-pickup-delay-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 18 || prior >= version || count < 0 || count != (size - 12) / 4)
        { error = "save-pickup-delay-header"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count)
        { state = new(); error = "save-pickup-delay-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
            var delay = hasWeaponAmmoFlags ? value & 63 : value;
            var suppressAmmo = hasWeaponAmmoFlags && (value & 64) != 0;
            var alwaysPresent = hasAlwaysPickupFlags && (value & 256) != 0;
            if (delay is < 0 or > 30 || (hasWeaponAmmoFlags && (value & ~(hasAlwaysPickupFlags ? 1023 : hasDroppedFlags ? 255 : 127)) != 0)
                || (hasAlwaysPickupFlags && !alwaysPresent && (value & 512) != 0)
                || ((delay > 0 || suppressAmmo || alwaysPresent) && !state.Actors[i].Pickup.HasValue))
            { state = new(); error = "save-pickup-delay-value"; return false; }
            state.Actors[i].PickupDelay = delay;
            state.Actors[i].SuppressWeaponPickupAmmo = suppressAmmo;
            state.Actors[i].Dropped = hasDroppedFlags && (value & 128) != 0;
            state.Actors[i].AlwaysPickupOverride = alwaysPresent ? (value & 512) != 0 : null;
        }
        return true;
    }
}
