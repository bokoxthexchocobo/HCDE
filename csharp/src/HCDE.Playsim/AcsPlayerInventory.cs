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
        if (activator is { Destroyed: false } actor && !string.IsNullOrEmpty(typeName)
            && typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
            return max ? actor.GetMaxHealth(false) : actor.Health;

        if (activator is { Destroyed: false } && !string.IsNullOrEmpty(typeName))
        {
            if (typeName.Equals("HealthBonus", StringComparison.OrdinalIgnoreCase))
                return max ? activator.Simulation?.DehackedHealthBonusCapPatched == true ? -1 : 200 : 0;
            if (typeName.Equals("Soulsphere", StringComparison.OrdinalIgnoreCase))
                return max ? activator.Simulation?.DehackedMaxSoulsphere ?? 200 : 0;
            if (typeName.Equals("MegasphereHealth", StringComparison.OrdinalIgnoreCase))
                return max ? activator.Simulation?.DehackedMegasphereHealth ?? 200 : 0;
            if (typeName.Equals("Megasphere", StringComparison.OrdinalIgnoreCase))
                return max ? 1 : 0;
            if (TryAmmoBox(typeName, out _, out var boxMaximum))
                return max ? boxMaximum : 0;
            if (max && activator is not PlayerPawn)
            {
                if (TryCapacityAmmoKind(typeName, out var ammoKind))
                    return DefaultAmmoMaximum(ammoKind);
                if (TryWeaponKind(typeName, out _) || TryKeyColor(typeName, out _)
                    || typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase)
                    || typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
                    || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
                    return 1;
            }
        }

        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return 0;

        if (typeName.Equals("PowerDamage", StringComparison.OrdinalIgnoreCase))
            return max || player.PowerDamageTics > 0 ? 1 : 0;
        if (typeName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase))
            return max || player.PowerBuddhaTics > 0 ? 1 : 0;
        if (typeName.Equals("PowerProtection", StringComparison.OrdinalIgnoreCase))
            return max || player.PowerProtectionTics > 0 ? 1 : 0;

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
            return max ? player.Inventory.ArmorMaximum : player.Inventory.Armor;

        if (typeName.Equals("BlueCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BlueSkull", StringComparison.OrdinalIgnoreCase))
            return max || player.Inventory.BlueKey ? 1 : 0;
        if (typeName.Equals("RedCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("RedSkull", StringComparison.OrdinalIgnoreCase))
            return max || player.Inventory.RedKey ? 1 : 0;
        if (typeName.Equals("YellowCard", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("YellowSkull", StringComparison.OrdinalIgnoreCase))
            return max || player.Inventory.YellowKey ? 1 : 0;

        if (TryCapacityAmmoKind(typeName, out var queryAmmo))
            return max ? MaxAmmo(player.Inventory, queryAmmo) : player.Inventory.Ammo(queryAmmo);

        if (TryWeaponKind(typeName, out var weaponKind))
            return max || player.Inventory.Owns(weaponKind) ? 1 : 0;
        if (typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase))
            return max || player.Inventory.HasBackpack ? 1 : 0;

        return max ? 0 : player.Inventory.SpareArmor.Count(spare =>
            spare.ArmorType.Equals(typeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Native <c>PCD_GETAMMOCAPACITY</c> for Doom ammo class names.</summary>
    public static int AmmoCapacity(Actor? activator, string[] stringTable, int stringId) =>
        AmmoCapacity(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int AmmoCapacity(Actor? activator, string? typeName)
    {
        if (activator is not { Destroyed: false } || string.IsNullOrEmpty(typeName))
            return 0;
        if (!TryCapacityAmmoKind(typeName, out var kind))
            return 0;
        return activator is PlayerPawn player ? MaxAmmo(player.Inventory, kind) : DefaultAmmoMaximum(kind);
    }

    /// <summary>Native <c>PCD_SETAMMOCAPACITY</c>. Non-ammo types are ignored.</summary>
    public static void SetAmmoCapacity(Actor? activator, string[] stringTable, int stringId, int capacity) =>
        SetAmmoCapacity(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId], capacity);

    public static void SetAmmoCapacity(Actor? activator, string? typeName, int capacity)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return;
        if (!TryCapacityAmmoKind(typeName, out var kind))
            return;
        SetMaxAmmo(player.Inventory, kind, capacity);
    }

    private static bool TryCapacityAmmoKind(string typeName, out AmmoKind kind)
    {
        if (typeName.Equals("Clip", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Bullets; return true; }
        if (typeName.Equals("Shell", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Shells; return true; }
        if (typeName.Equals("RocketAmmo", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Rockets; return true; }
        if (typeName.Equals("Cell", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Cells; return true; }
        kind = default; return false;
    }

    private static bool TryAmmoBox(string typeName, out AmmoKind kind, out int maximum)
    {
        if (typeName.Equals("ClipBox", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Bullets; maximum = 200; return true; }
        if (typeName.Equals("ShellBox", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Shells; maximum = 50; return true; }
        if (typeName.Equals("RocketBox", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Rockets; maximum = 50; return true; }
        if (typeName.Equals("CellPack", StringComparison.OrdinalIgnoreCase))
        { kind = AmmoKind.Cells; maximum = 300; return true; }
        kind = default; maximum = 0; return false;
    }

    private static int DefaultAmmoMaximum(AmmoKind kind) => kind switch
    {
        AmmoKind.Bullets => 200,
        AmmoKind.Shells or AmmoKind.Rockets => 50,
        _ => 300,
    };

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

    public static void ScriptClear(AuthoritySimulation sim, Actor? activator)
    {
        if (activator is not null) Clear(activator);
        else foreach (var player in sim.Players) Clear(player);
    }

    public static void Clear(Actor? activator)
    {
        if (activator is not PlayerPawn { Destroyed: false } player)
            return;
        player.Inventory.ClearInventory();
        player.PowerBuddhaTics = 0;
        player.DrainStrength = 0;
    }

    /// <summary>Native <c>UseInventory</c> / <c>DoUseInv</c> subset: weapon select only.</summary>
    public static int Use(Actor? activator, string[] stringTable, int stringId) =>
        Use(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    /// <summary>Native <c>PCD_CHECKWEAPON</c>: ready weapon class name match.</summary>
    public static int CheckWeapon(Actor? activator, string[] stringTable, int stringId) =>
        CheckWeapon(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int CheckWeapon(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return 0;
        if (!TryWeaponKind(typeName, out var weaponKind))
            return 0;
        return player.Inventory.Selected == weaponKind ? 1 : 0;
    }

    /// <summary>Native <c>PCD_SETWEAPON</c>: selecting the ready weapon cancels a pending switch.</summary>
    public static int SetWeapon(Actor? activator, string[] stringTable, int stringId) =>
        SetWeapon(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId]);

    public static int SetWeapon(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName)
            || !TryWeaponKind(typeName, out var weaponKind) || !player.Inventory.Owns(weaponKind))
            return 0;

        if (player.Inventory.Selected == weaponKind)
        {
            player.Inventory.Pending = null;
            return 1;
        }
        return QueueWeaponUse(player.Inventory, weaponKind);
    }

    public static int Use(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || player.Health <= 0 || string.IsNullOrEmpty(typeName))
            return 0;

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
            return 0;

        if (player.Inventory.TryUseStoredArmor(typeName))
            return 1;

        if (!TryWeaponKind(typeName, out var weaponKind))
            return 0;

        if (player.Inventory.Owns(weaponKind) && weaponKind != player.Inventory.Selected)
            player.Inventory.Pending = weaponKind;
        // Weapon.Use returns false to retain the weapon, even when it queued a switch.
        return 0;
    }

    private static int QueueWeaponUse(PlayerInventory inventory, WeaponKind weaponKind)
    {
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

    public static void ScriptGive(AuthoritySimulation sim, Actor? activator, string[] strings, int id, int amount)
    {
        if (activator is not null) Give(activator, strings, id, amount);
        else foreach (var player in sim.Players) Give(player, strings, id, amount);
    }

    public static void ScriptTake(AuthoritySimulation sim, Actor? activator, string[] strings, int id, int amount)
    {
        if (activator is not null) Take(activator, strings, id, amount);
        else foreach (var player in sim.Players) Take(player, strings, id, amount);
    }

    public static void Give(Actor? activator, string? typeName, int amount)
    {
        if (activator is not { Destroyed: false } || string.IsNullOrEmpty(typeName) || amount <= 0)
            return;

        if (activator is not PlayerPawn player)
        {
            if (TryHealthUse(typeName, out _, out _)
                || typeName.Equals("MegasphereHealth", StringComparison.OrdinalIgnoreCase))
                PickupCatalog.GiveHealth(activator, amount);
            return;
        }

        if (typeName.Equals("PowerDamage", StringComparison.OrdinalIgnoreCase))
        { player.GivePowerDamage(); return; }
        if (typeName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase))
        { player.GivePowerBuddha(); return; }
        if (typeName.Equals("PowerProtection", StringComparison.OrdinalIgnoreCase))
        { player.GivePowerProtection(); return; }
        var inventory = player.Inventory;
        if (TryAmmoBox(typeName, out var boxAmmo, out _))
        {
            GiveScriptAmmo(player, boxAmmo, amount);
            return;
        }
        if (typeName.Equals("MegasphereHealth", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.GiveHealth(player, amount, player.Simulation?.DehackedMegasphereHealth ?? 200);
            return;
        }
        if (typeName.Equals("BlueArmorForMegasphere", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.GiveArmor(inventory, PickupCatalog.ScaleArmor(checked(200 * amount), player), PlayerInventory.MegaSavePercent,
                "BlueArmorForMegasphere");
            return;
        }
        if (typeName.Equals("Megasphere", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.TryGive(player, PickupCatalog.Megasphere);
            return;
        }
        if (TryHealthUse(typeName, out _, out _) && !typeName.Equals("Health", StringComparison.OrdinalIgnoreCase)
            && PickupCatalog.TryEditorNumberForDropName(typeName, out var healthPickup))
        {
            PickupCatalog.TryGive(player, healthPickup, pickupAmount: amount);
            return;
        }
        if (typeName.Equals("ArmorBonus", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus, pickupAmount: amount);
            return;
        }
        if (typeName.Equals("GreenArmor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BlueArmor", StringComparison.OrdinalIgnoreCase))
        {
            var green = typeName.Equals("GreenArmor", StringComparison.OrdinalIgnoreCase);
            var armorClass = green ? player.Simulation?.DehackedGreenArmorClass ?? 1
                : player.Simulation?.DehackedBlueArmorClass ?? 2;
            var suitAmount = unchecked(100 * armorClass);
            PickupCatalog.TryGive(player, green ? PickupCatalog.GreenArmor : PickupCatalog.MegaArmor,
                pickupAmount: checked(suitAmount * amount));
            return;
        }
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.GiveHealth(player, amount);
            return;
        }

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
        {
            // Doom starts with BasicArmor: its HandlePickup rejects direct additions.
            // The Armor alias uses base BasicArmorPickup, whose SaveAmount is zero.
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

        if (typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase))
        {
            PickupCatalog.TryGive(player, PickupCatalog.Backpack);
            return;
        }

        if (TryCapacityAmmoKind(typeName, out var ammoKind))
        {
            // GiveInventory restores PendingWeapon after the pickup callbacks.
            GiveScriptAmmo(player, ammoKind, amount);
            return;
        }

        if (TryWeaponKind(typeName, out var weaponKind))
        {
            var owned = inventory.Owns(weaponKind);
            if (owned && player.Simulation?.NonDroppedWeaponsStay == true) return;
            inventory.Weapons |= weaponKind;
            if (!owned && inventory.Selected == 0 && !inventory.NeverAutoSwitch)
                inventory.Pending = weaponKind;
            var definition = WeaponCatalog.Find(weaponKind);
            if (definition?.Ammo is AmmoKind weaponAmmo)
            {
                // SetGiveAmount changes inventory count, not Weapon.AmmoGive.
                var ammoGive = weaponKind switch
                {
                    WeaponKind.Pistol or WeaponKind.Chaingun => 20,
                    WeaponKind.Shotgun or WeaponKind.SuperShotgun => 8,
                    WeaponKind.RocketLauncher => 2,
                    WeaponKind.Plasma or WeaponKind.Bfg => 40,
                    _ => 0,
                };
                var wasEmpty = inventory.Ammo(weaponAmmo) == 0;
                var added = inventory.TryAddAmmo(weaponAmmo,
                    PickupCatalog.ScaleWeaponAmmo(ammoGive, player, owned));
                if (added && wasEmpty && inventory.Selected == 0)
                    inventory.CheckAmmoPickupSwitch(weaponAmmo);
            }
        }
    }

    private static void GiveScriptAmmo(PlayerPawn player, AmmoKind kind, int amount)
    {
        var inventory = player.Inventory;
        var wasEmpty = inventory.Ammo(kind) == 0;
        var added = inventory.TryAddAmmo(kind, PickupCatalog.ScaleAmmo(amount, player));
        if (added && wasEmpty && inventory.Selected == 0)
            inventory.CheckAmmoPickupSwitch(kind);
    }

    public static void Take(Actor? activator, string[] stringTable, int stringId, int amount) =>
        Take(activator, stringId < 0 || stringId >= stringTable.Length ? null : stringTable[stringId], amount);

    /// <summary>Native <c>ACSF_DropInventory</c> subset with supported inventory tosses.</summary>
    public static bool Drop(Actor? activator, string? typeName)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName))
            return false;
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Fist", StringComparison.OrdinalIgnoreCase))
            return false;
        var amount = Count(activator, typeName, max: false);
        if (amount <= 0)
            return false;
        if (TryWeaponKind(typeName, out var tossedWeapon) && activator.Simulation is { } weaponSim
            && PickupCatalog.TryWeaponEdNum(tossedWeapon, out var weaponEditorNumber))
        {
            if (!weaponSim.SpawnDroppedPickup(activator, weaponEditorNumber, inventoryToss: true,
                suppressWeaponAmmo: true)) return false;
        }
        if (TryKeyColor(typeName, out _) && activator.Simulation is { } keySim
            && PickupCatalog.TryEditorNumberForDropName(typeName, out var keyEditorNumber))
        {
            if (!keySim.SpawnDroppedPickup(activator, keyEditorNumber, inventoryToss: true)) return false;
        }
        if (typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase)
            && activator.Simulation is { } backpackSim)
        {
            return backpackSim.DropBackpack(player) is not null;
        }
        if (TryCapacityAmmoKind(typeName, out _) && activator.Simulation is { } sim
            && PickupCatalog.TryEditorNumberForDropName(typeName, out var doomEdNum))
        {
            if (!sim.SpawnDroppedPickup(activator, doomEdNum, ignoreAmmoSkill: true, pickupAmount: 1,
                inventoryToss: true)) return false;
        }
        Take(activator, typeName, 1);
        return true;
    }

    public static void Take(Actor? activator, string? typeName, int amount)
    {
        if (activator is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName) || amount <= 0)
            return;

        if (typeName.Equals("PowerDamage", StringComparison.OrdinalIgnoreCase))
        { player.PowerDamageTics = 0; return; }
        if (typeName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase))
        { player.PowerBuddhaTics = 0; return; }
        if (typeName.Equals("PowerProtection", StringComparison.OrdinalIgnoreCase))
        { player.PowerProtectionTics = 0; return; }

        var inventory = player.Inventory;
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            // CheckInventory aliases Health to actor health; TakeInventory does not.
            return;
        }

        if (typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase))
        {
            inventory.Armor = inventory.Armor <= amount ? 0 : inventory.Armor - amount;
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

        if (typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase))
        {
            inventory.RemoveBackpack();
            return;
        }

        if (TryCapacityAmmoKind(typeName, out var ammoKind))
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

        if (TryWeaponKind(typeName, out var weaponKind))
        {
            var replaceReady = inventory.Owns(weaponKind) && inventory.Selected == weaponKind
                && (!inventory.Pending.HasValue || inventory.Pending == weaponKind);
            inventory.RemoveWeapon(weaponKind);
            if (replaceReady && inventory.Pending is { } replacement)
                player.BringUpWeapon(replacement);
            return;
        }
        inventory.TakeStoredArmor(typeName, amount);
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

    private static bool TryHealthUse(string typeName, out int amount, out int maximum)
    {
        if (typeName.Equals("Health", StringComparison.OrdinalIgnoreCase)
            || typeName.Equals("Stimpack", StringComparison.OrdinalIgnoreCase))
        {
            amount = 10;
            maximum = 100;
            return true;
        }
        if (typeName.Equals("Medikit", StringComparison.OrdinalIgnoreCase))
        {
            amount = 25;
            maximum = 100;
            return true;
        }
        if (typeName.Equals("HealthBonus", StringComparison.OrdinalIgnoreCase))
        {
            amount = 1;
            maximum = PlayerInventory.MaxHealthBonus;
            return true;
        }
        if (typeName.Equals("Soulsphere", StringComparison.OrdinalIgnoreCase))
        {
            amount = 100;
            maximum = PlayerInventory.MaxHealthBonus;
            return true;
        }
        amount = 0;
        maximum = 0;
        return false;
    }

    /// <summary>Native <c>ACSF_GetArmorType</c>: returns worn amount when the type name matches.</summary>
    public static int ArmorTypeAmount(AuthoritySimulation sim, string? typeName, int playerNumber)
    {
        if (string.IsNullOrEmpty(typeName) || playerNumber < 0)
            return 0;
        var player = sim.Players.FirstOrDefault(p => !p.Destroyed && p.PlayerNum == playerNumber);
        if (player is null)
            return 0;
        if (!typeName.Equals(player.Inventory.ArmorType, StringComparison.OrdinalIgnoreCase))
            return 0;
        return player.Inventory.Armor;
    }

    /// <summary>Native <c>ACSF_GetArmorInfo</c> subset for worn BasicArmor.</summary>
    public static int ArmorInfo(Actor? activator, int infoKind, AcsGlobalStrings globalStrings)
    {
        if (activator is not PlayerPawn { Destroyed: false } player)
            return 0;

        var inventory = player.Inventory;
        var hasArmor = inventory.Armor != 0;
        return infoKind switch
        {
            0 => globalStrings.Add(hasArmor ? inventory.ArmorType : "None"),
            1 => hasArmor ? inventory.ArmorMaximum : 0,
            2 => hasArmor ? DoubleToAcs(PlayerInventory.ArmorSaveFraction(inventory.ArmorSavePercent)) : 0,
            3 => hasArmor ? inventory.MaxAbsorb : 0,
            4 => hasArmor ? inventory.MaxFullAbsorb : 0,
            5 => hasArmor ? inventory.ArmorActualSaveAmount : 0,
            _ => 0,
        };
    }

    private static int DoubleToAcs(double value) => (int)Math.Floor(value * 65536.0);

    /// <summary>Native <c>ACSF_GetWeapon</c> ready-weapon class name.</summary>
    public static string ReadyWeaponClassName(Actor? activator)
    {
        if (activator is not PlayerPawn { Destroyed: false } player)
            return "None";
        return WeaponClassName(player.Inventory.Selected);
    }

    internal static string WeaponClassName(WeaponKind kind) => kind switch
    {
        WeaponKind.Pistol => "Pistol",
        WeaponKind.Fist => "Fist",
        WeaponKind.Shotgun => "Shotgun",
        WeaponKind.SuperShotgun => "SuperShotgun",
        WeaponKind.Chaingun => "Chaingun",
        WeaponKind.RocketLauncher => "RocketLauncher",
        WeaponKind.Plasma => "PlasmaRifle",
        WeaponKind.Bfg => "BFG9000",
        WeaponKind.Chainsaw => "Chainsaw",
        _ => "None",
    };

    private static bool TryWeaponKind(string typeName, out WeaponKind kind)
    {
        if (typeName.Equals("Shotgun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Shotgun; return true; }
        if (typeName.Equals("SuperShotgun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.SuperShotgun; return true; }
        if (typeName.Equals("Chaingun", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Chaingun; return true; }
        if (typeName.Equals("RocketLauncher", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.RocketLauncher; return true; }
        if (typeName.Equals("PlasmaRifle", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Plasma; return true; }
        if (typeName.Equals("BFG9000", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Bfg; return true; }
        if (typeName.Equals("Chainsaw", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Chainsaw; return true; }
        if (typeName.Equals("Fist", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Fist; return true; }
        if (typeName.Equals("Pistol", StringComparison.OrdinalIgnoreCase)) { kind = WeaponKind.Pistol; return true; }
        kind = default;
        return false;
    }
}
