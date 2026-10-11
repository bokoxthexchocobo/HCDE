using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimAmmoInventory(int Bullets, int Shells, int Rockets, int Cells,
    int MaxBullets, int MaxShells, int MaxRockets, int MaxCells, bool Backpack);
public readonly record struct SimWeaponInventory(WeaponKind Owned, WeaponKind Selected, WeaponKind? Pending,
    int OffsetY = PlayerPawn.WeaponTop, bool Lowering = false, bool InstantSwitch = false, bool AttackPressed = false,
    int? BobTimer = null, double ViewBob = 0, double MovementBob = 0, double WeaponBobX = 0, double WeaponBobY = 0,
    int TurnTicks = 0, bool TurnHeld = false);

internal static class SimAmmoArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.PlayerWeapons is { } weapons && !Valid(weapons)))
            throw new InvalidOperationException("Invalid saved player weapons.");
        if (state.Actors.Any(a => a.PlayerAmmo is { } ammo && !Valid(ammo)))
            throw new InvalidOperationException("Invalid saved player ammo.");
        if (state.GeometryHealth is null && state.Actors.Any(a => a.PlayerAmmo.HasValue || a.PlayerWeapons.HasValue))
            throw new InvalidOperationException("Player ammo requires a full simulation save.");
    }

    private static bool Valid(SimWeaponInventory weapons) => ((int)weapons.Owned & ~511) == 0
        && ValidWeapon((int)weapons.Selected) && (!weapons.Pending.HasValue || ValidWeapon((int)weapons.Pending.Value))
        && weapons.OffsetY is >= PlayerPawn.WeaponTop and <= PlayerPawn.WeaponBottom
        && double.IsFinite(weapons.ViewBob) && double.IsFinite(weapons.MovementBob)
        && double.IsFinite(weapons.WeaponBobX) && double.IsFinite(weapons.WeaponBobY)
        && weapons.TurnTicks is >= 0 and <= PlayerPawn.Turn180Ticks;
    private static bool ValidWeapon(int weapon) => weapon >= 0 && weapon <= 256 && (weapon & (weapon - 1)) == 0;

    private static bool Valid(SimAmmoInventory ammo, bool signedCapacity = true) => ammo.Bullets >= 0 && ammo.Shells >= 0
        && ammo.Rockets >= 0 && ammo.Cells >= 0 && (signedCapacity || !HasSignedCapacity(ammo));

    private static bool HasSignedCapacity(SimAmmoInventory ammo) => ammo.MaxBullets < 0 || ammo.MaxShells < 0
        || ammo.MaxRockets < 0 || ammo.MaxCells < 0;

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        var signedCapacity = state.Actors.Any(a => a.PlayerAmmo is { } ammo && HasSignedCapacity(ammo));
        // Version 170 uses the complete version 169 layout, even for default weapons.
        var weaponsPresent = signedCapacity || state.Actors.Any(a => a.PlayerWeapons.HasValue);
        var turnPresent = signedCapacity || state.Actors.Any(a => a.PlayerWeapons is { } w && (w.TurnTicks != 0 || w.TurnHeld));
        var bobPresent = turnPresent || state.Actors.Any(a => a.PlayerWeapons?.BobTimer.HasValue == true);
        var phasePresent = bobPresent || state.Actors.Any(a => a.PlayerWeapons is { } w
            && (w.OffsetY != PlayerPawn.WeaponTop || w.Lowering || w.InstantSwitch || w.AttackPressed));
        if (!weaponsPresent && !state.Actors.Any(a => a.PlayerAmmo.HasValue)) return archive;
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
        writer.Write((int)BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4))); writer.Write(state.Actors.Count);
        foreach (var actor in state.Actors)
        {
            if (weaponsPresent)
            {
                writer.Write(actor.PlayerWeapons.HasValue ? 1 : 0);
                if (actor.PlayerWeapons is { } weapons)
                {
                    writer.Write((int)weapons.Owned); writer.Write((int)weapons.Selected);
                    writer.Write(weapons.Pending.HasValue ? (int)weapons.Pending.Value : -1);
                    if (phasePresent)
                    {
                        writer.Write(weapons.OffsetY);
                        writer.Write((weapons.Lowering ? 1 : 0) | (weapons.InstantSwitch ? 2 : 0) | (weapons.AttackPressed ? 4 : 0));
                    }
                    if (bobPresent)
                    {
                        writer.Write(weapons.BobTimer ?? state.Tic);
                        writer.Write(weapons.ViewBob); writer.Write(weapons.MovementBob);
                        writer.Write(weapons.WeaponBobX); writer.Write(weapons.WeaponBobY);
                    }
                    if (turnPresent) { writer.Write(weapons.TurnTicks); writer.Write(weapons.TurnHeld ? 1 : 0); }
                }
            }
            writer.Write(actor.PlayerAmmo.HasValue ? 1 : 0);
            if (actor.PlayerAmmo is not { } ammo) continue;
            writer.Write(ammo.Bullets); writer.Write(ammo.Shells); writer.Write(ammo.Rockets); writer.Write(ammo.Cells);
            writer.Write(ammo.MaxBullets); writer.Write(ammo.MaxShells); writer.Write(ammo.MaxRockets); writer.Write(ammo.MaxCells);
            writer.Write(ammo.Backpack ? 1 : 0);
        }
        writer.Write(checked((int)stream.Length + 4));
        var trailer = stream.ToArray(); var result = new byte[checked(archive.Length + trailer.Length)];
        archive.CopyTo(result, 0); trailer.CopyTo(result, archive.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), signedCapacity ? (ushort)170 : turnPresent ? (ushort)169 : bobPresent ? (ushort)168 : phasePresent ? (ushort)167 : weaponsPresent ? (ushort)166 : (ushort)165); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-player-ammo-invalid";
        if (bytes.Length < 28) return false;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var weaponsPresent = version >= 166;
        var phasePresent = version >= 167;
        var bobPresent = version >= 168;
        var turnPresent = version >= 169;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var data = bytes.Slice(start, size - 4);
        var prior = BinaryPrimitives.ReadInt32LittleEndian(data); var count = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        if (prior is < 15 or > 164 || count < 0 || (long)count * (weaponsPresent ? 8 : 4) + 8 > data.Length) return false;
        var weaponEntries = new SimWeaponInventory?[count];
        var entries = new SimAmmoInventory?[count]; var offset = 8;
        for (var i = 0; i < count; i++)
        {
            if (weaponsPresent)
            {
                if (data.Length - offset < 4) return false;
                var hasWeapons = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
                if (hasWeapons is < 0 or > 1) return false;
                if (hasWeapons != 0)
                {
                    if (data.Length - offset < 12) return false;
                    var owned = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
                    var selected = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 4)..]);
                    var pending = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 8)..]); offset += 12;
                    if (pending < -1) return false;
                    var weapons = new SimWeaponInventory((WeaponKind)owned, (WeaponKind)selected, pending == -1 ? null : (WeaponKind)pending);
                    if (phasePresent)
                    {
                        if (data.Length - offset < 8) return false;
                        var height = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
                        var flags = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 4)..]); offset += 8;
                        if ((flags & ~7) != 0) return false;
                        weapons = weapons with { OffsetY = height, Lowering = (flags & 1) != 0,
                            InstantSwitch = (flags & 2) != 0, AttackPressed = (flags & 4) != 0 };
                    }
                    if (bobPresent)
                    {
                        if (data.Length - offset < 36) return false;
                        var timer = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
                        var bob = new double[4];
                        for (var j = 0; j < 4; j++)
                        {
                            bob[j] = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(data[offset..])); offset += 8;
                        }
                        weapons = weapons with { BobTimer = timer, ViewBob = bob[0], MovementBob = bob[1], WeaponBobX = bob[2], WeaponBobY = bob[3] };
                    }
                    if (turnPresent)
                    {
                        if (data.Length - offset < 8) return false;
                        var ticks = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
                        var held = BinaryPrimitives.ReadInt32LittleEndian(data[(offset + 4)..]); offset += 8;
                        if (held is < 0 or > 1) return false;
                        weapons = weapons with { TurnTicks = ticks, TurnHeld = held != 0 };
                    }
                    if (!Valid(weapons)) return false;
                    weaponEntries[i] = weapons;
                }
            }
            if (data.Length - offset < 4) return false;
            var present = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4;
            if (present is < 0 or > 1) return false;
            if (present == 0) continue;
            if (data.Length - offset < 36) return false;
            var values = new int[9];
            for (var j = 0; j < 9; j++) { values[j] = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]); offset += 4; }
            if (values[8] is < 0 or > 1) return false;
            var ammo = new SimAmmoInventory(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8] != 0);
            if (!Valid(ammo, signedCapacity: version >= 170)) return false;
            entries[i] = ammo;
        }
        if (offset != data.Length) return false;
        var inner = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(inner.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(inner, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-player-ammo-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].PlayerAmmo = entries[i]; state.Actors[i].PlayerWeapons = weaponEntries[i];
        }
        error = null; return true;
    }
}
