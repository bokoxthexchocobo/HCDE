namespace HCDE.Playsim;

/// <summary>Native <c>CheckInventory</c> and skull-tag player key opcodes subset.</summary>
internal static class AcsPlayerInventory
{
    public static int HasKey(Actor? activator, PickupCatalog.KeyColor color) =>
        activator is PlayerPawn { Destroyed: false } player && player.Inventory.HasKey(color) ? 1 : 0;

    public static int HasMasterKeys(Actor? activator) =>
        activator is PlayerPawn { Destroyed: false } player
        && player.Inventory.BlueKey && player.Inventory.RedKey && player.Inventory.YellowKey
            ? 1 : 0;

    public static int Count(Actor? activator, string[] stringTable, int stringId, bool max)
    {
        if (stringId < 0 || stringId >= stringTable.Length)
            return 0;
        return Count(activator, stringTable[stringId], max);
    }

    public static int Count(Actor? activator, string typeName, bool max)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return 0;

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.Armor : player.Inventory.Armor;

        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
            return max ? 100 : player.Health;

        if (typeName.Equals("BlueCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BlueSkull", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.BlueKey ? 1 : 0;
        if (typeName.Equals("RedCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("RedSkull", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.RedKey ? 1 : 0;
        if (typeName.Equals("YellowCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("YellowSkull", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.YellowKey ? 1 : 0;

        if (typeName.Equals("Clip", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Bullet", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("AmmoClip", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.MaxBullets : player.Inventory.Bullets;
        if (typeName.Equals("Shell", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Shells", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.MaxShells : player.Inventory.Shells;
        if (typeName.Equals("RocketAmmo", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Rockets", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.MaxRockets : player.Inventory.Rockets;
        if (typeName.Equals("Cell", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Cells", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.MaxCells : player.Inventory.Cells;

        if (typeName.Equals("Shotgun", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.Shotgun) ? 1 : 0;
        if (typeName.Equals("SuperShotgun", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.SuperShotgun) ? 1 : 0;
        if (typeName.Equals("Chaingun", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.Chaingun) ? 1 : 0;
        if (typeName.Equals("RocketLauncher", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.RocketLauncher) ? 1 : 0;
        if (typeName.Equals("PlasmaRifle", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.Plasma) ? 1 : 0;
        if (typeName.Equals("BFG9000", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BFG", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.Bfg) ? 1 : 0;
        if (typeName.Equals("Chainsaw", StringComparison.OrdinalIgnoreCase))
            return player.Inventory.Owns(WeaponKind.Chainsaw) ? 1 : 0;

        return 0;
    }

    /// <summary>Native <c>PCD_GETAMMOCAPACITY</c> for Doom ammo class names.</summary>
    public static int AmmoCapacity(Actor? activator, string[] stringTable, int stringId) =>
        AmmoCapacity(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int AmmoCapacity(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return 0;
        if (!TryAmmoKind(typeName, out var kind))
            return 0;
        return MaxAmmo(player.Inventory, kind);
    }

    /// <summary>Native <c>PCD_SETAMMOCAPACITY</c>. Non-ammo types are ignored.</summary>
    public static void SetAmmoCapacity(Actor? activator, string[] stringTable, int stringId, int capacity) =>
        SetAmmoCapacity(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId], capacity);

    public static void SetAmmoCapacity(Actor? activator, string? typeName, int capacity)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return;
        if (!TryAmmoKind(typeName, out var kind))
            return;
        if (capacity < 0)
            capacity = 0;
        SetMaxAmmo(player.Inventory, kind, capacity);
    }

    private static int MaxAmmo(PlayerInventory inventory, AmmoKind kind) => kind switch
    {
        AmmoKind.Bullets => inventory.MaxBullets,
        AmmoKind.Shells => inventory.MaxShells,
        AmmoKind.Rockets => inventory.MaxRockets,
        _ => inventory.MaxCells,
    };

    private static void SetMaxAmmo(PlayerInventory inventory, AmmoKind kind, int capacity)
    {
        switch (kind)
        {
            case AmmoKind.Bullets: inventory.MaxBullets = capacity; break;
            case AmmoKind.Shells: inventory.MaxShells = capacity; break;
            case AmmoKind.Rockets: inventory.MaxRockets = capacity; break;
            default: inventory.MaxCells = capacity; break;
        }
    }

    public static void Clear(Actor? activator)
    {
        if (activator is not PlayerPawn { Destroyed: false } player)
            return;
        player.Inventory.ResetToPistolStart();
        player.PowerBuddhaTics = 0;
    }

    /// <summary>Native <c>UseInventory</c> / <c>DoUseInv</c> subset: weapon select only.</summary>
    public static int Use(Actor? activator, string[] stringTable, int stringId) =>
        Use(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int Use(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return 0;

        if (!TryWeaponKind(typeName, out var weaponKind))
            return 0;

        var inventory = player.Inventory;
        if (!inventory.Owns(weaponKind))
            return 0;

        var definition = WeaponCatalog.Find(weaponKind);
        if (definition?.Ammo is { } ammo && inventory.Ammo(ammo) < definition.AmmoUse)
            return 0;

        if (weaponKind != inventory.Selected)
            inventory.Pending = weaponKind;
        return 1;
    }

    /// <summary>Native <c>PCD_USEACTORINVENTORY</c>: tid 0 uses every in-game player.</summary>
    public static int UseByTid(AuthoritySimulation sim, int tid, string[] stringTable, int stringId) =>
        UseByTid(sim, tid, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int UseByTid(AuthoritySimulation sim, int tid, string? typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return 0;

        if (tid == 0)
        {
            var total = 0;
            foreach (var player in sim.Players)
            {
                if (player is { Destroyed: false })
                    total += Use(player, typeName);
            }
            return total;
        }

        var sum = 0;
        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            sum += Use(actor, typeName);
        return sum;
    }

    public static void Give(Actor? activator, string[] stringTable, int stringId, int amount) =>
        Give(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId], amount);

    public static void Give(Actor? activator, string? typeName, int amount)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName) || amount <= 0)
            return;

        var inventory = player.Inventory;
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            player.Health = Math.Min(PlayerInventory.MaxHealthBonus, player.Health + amount);
            return;
        }

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
        {
            inventory.Armor = Math.Min(PlayerInventory.MegaArmorAmount, inventory.Armor + amount);
            if (inventory.ArmorSavePercent == 0)
                inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
            return;
        }

        if (TryKeyColor(typeName, out var keyColor))
        {
            switch (keyColor)
            {
                case PickupCatalog.KeyColor.Blue: inventory.BlueKey = true; break;
                case PickupCatalog.KeyColor.Red: inventory.RedKey = true; break;
                case PickupCatalog.KeyColor.Yellow: inventory.YellowKey = true; break;
            }
            return;
        }

        if (TryAmmoKind(typeName, out var ammoKind))
        {
            inventory.TryAddAmmo(ammoKind, amount);
            return;
        }

        if (TryWeaponKind(typeName, out var weaponKind))
        {
            inventory.Weapons |= weaponKind;
            var definition = WeaponCatalog.Find(weaponKind);
            if (definition?.Ammo is AmmoKind weaponAmmo)
                inventory.TryAddAmmo(weaponAmmo, amount);
        }
    }

    public static void Take(Actor? activator, string[] stringTable, int stringId, int amount) =>
        Take(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId], amount);

    public static void Take(Actor? activator, string? typeName, int amount)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName) || amount <= 0)
            return;

        var inventory = player.Inventory;
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            player.Health = Math.Max(1, player.Health - amount);
            return;
        }

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
        {
            inventory.Armor = Math.Max(0, inventory.Armor - amount);
            if (inventory.Armor == 0)
                inventory.ArmorSavePercent = 0;
            return;
        }

        if (TryKeyColor(typeName, out var keyColor))
        {
            switch (keyColor)
            {
                case PickupCatalog.KeyColor.Blue: inventory.BlueKey = false; break;
                case PickupCatalog.KeyColor.Red: inventory.RedKey = false; break;
                case PickupCatalog.KeyColor.Yellow: inventory.YellowKey = false; break;
            }
            return;
        }

        if (TryAmmoKind(typeName, out var ammoKind))
        {
            var take = Math.Min(inventory.Ammo(ammoKind), amount);
            switch (ammoKind)
            {
                case AmmoKind.Bullets: inventory.Bullets -= take; break;
                case AmmoKind.Shells: inventory.Shells -= take; break;
                case AmmoKind.Rockets: inventory.Rockets -= take; break;
                default: inventory.Cells -= take; break;
            }
            return;
        }

        if (TryWeaponKind(typeName, out var weaponKind) && (inventory.Weapons & weaponKind) != 0)
            inventory.Weapons &= ~weaponKind;
    }

    private static bool TryKeyColor(string typeName, out PickupCatalog.KeyColor color)
    {
        if (typeName.Equals("BlueCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BlueSkull", StringComparison.OrdinalIgnoreCase))
        {
            color = PickupCatalog.KeyColor.Blue;
            return true;
        }
        if (typeName.Equals("RedCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("RedSkull", StringComparison.OrdinalIgnoreCase))
        {
            color = PickupCatalog.KeyColor.Red;
            return true;
        }
        if (typeName.Equals("YellowCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("YellowSkull", StringComparison.OrdinalIgnoreCase))
        {
            color = PickupCatalog.KeyColor.Yellow;
            return true;
        }
        color = default;
        return false;
    }

    private static bool TryAmmoKind(string typeName, out AmmoKind kind)
    {
        if (typeName.Equals("Clip", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Bullet", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("AmmoClip", StringComparison.OrdinalIgnoreCase))
        {
            kind = AmmoKind.Bullets;
            return true;
        }
        if (typeName.Equals("Shell", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Shells", StringComparison.OrdinalIgnoreCase))
        {
            kind = AmmoKind.Shells;
            return true;
        }
        if (typeName.Equals("RocketAmmo", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Rockets", StringComparison.OrdinalIgnoreCase))
        {
            kind = AmmoKind.Rockets;
            return true;
        }
        if (typeName.Equals("Cell", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Cells", StringComparison.OrdinalIgnoreCase))
        {
            kind = AmmoKind.Cells;
            return true;
        }
        kind = default;
        return false;
    }

    private static bool TryWeaponKind(string typeName, out WeaponKind kind)
    {
        if (typeName.Equals("Shotgun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Shotgun; return true; }
        if (typeName.Equals("SuperShotgun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.SuperShotgun; return true; }
        if (typeName.Equals("Chaingun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Chaingun; return true; }
        if (typeName.Equals("RocketLauncher", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.RocketLauncher; return true; }
        if (typeName.Equals("PlasmaRifle", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Plasma; return true; }
        if (typeName.Equals("BFG9000", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BFG", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Bfg; return true; }
        if (typeName.Equals("Chainsaw", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Chainsaw; return true; }
        if (typeName.Equals("Fist", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Fist; return true; }
        if (typeName.Equals("Pistol", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Pistol; return true; }
        kind = default;
        return false;
    }
}
