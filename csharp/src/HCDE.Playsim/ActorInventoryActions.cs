namespace HCDE.Playsim;

/// <summary>Native inventory state actions for supported managed health, armor, key, ammo, and weapon classes.</summary>
public static class ActorInventoryActions
{
    /// <summary>Native Actor.ClearInventory for represented player inventory.</summary>
    public static void ClearInventory(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        AcsPlayerInventory.Clear(actor);
    }

    /// <summary>Native Actor.DropInventory for represented tossable inventory; reports spawn success.</summary>
    public static bool DropInventory(Actor actor, string? typeName, int amount = 1)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return AcsPlayerInventory.Drop(actor, typeName, amount);
    }

    /// <summary>Native Actor.UseInventory for represented named held items.</summary>
    public static bool UseInventory(Actor actor, string? typeName)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return AcsPlayerInventory.Use(actor, typeName) != 0;
    }

    /// <summary>Native Actor.GetAmmoCapacity, including defaults for unheld box classes.</summary>
    public static int GetAmmoCapacity(Actor actor, string? typeName)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.Destroyed || string.IsNullOrEmpty(typeName)) return 0;
        if (AcsPlayerInventory.TryAmmoBox(typeName, out _, out var maximum)) return maximum;
        return AcsPlayerInventory.AmmoCapacity(actor, typeName);
    }

    /// <summary>Native capacity assignment for represented parent ammo pools; amounts are unchanged.</summary>
    public static void SetAmmoCapacity(Actor actor, string? typeName, int capacity)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.Destroyed || string.IsNullOrEmpty(typeName)) return;
        if (AcsPlayerInventory.TryAmmoBox(typeName, out _, out _))
            throw new NotSupportedException("Ammo box capacity assignment requires exact-class inventory object support.");
        AcsPlayerInventory.SetAmmoCapacity(actor, typeName, capacity);
    }

    /// <summary>Native Actor.GiveAmmo for represented Doom ammo classes.</summary>
    public static bool GiveAmmo(Actor actor, string? typeName, int amount)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(typeName)) return false;
        if (!AcsPlayerInventory.TryCapacityAmmoKind(typeName, out var kind)
            && !AcsPlayerInventory.TryAmmoBox(typeName, out kind, out _)) return false;
        if (amount < 0)
            throw new NotSupportedException("Negative native ammo pickup amounts require signed inventory stack support.");
        return PickupCatalog.GiveAmmo(player.Inventory, kind, PickupCatalog.ScaleAmmo(amount, player));
    }

    /// <summary>Native CheckInventory thresholds for represented held inventory.</summary>
    public static bool CheckInventory(Actor actor, string? typeName, int amount, int pointerSelector = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrEmpty(typeName)) return false;
        var subject = Resolve(actor, pointerSelector);
        if (subject is not PlayerPawn { Destroyed: false } player) return false;
        // ACS exposes health as a synthetic count; native FindInventory does not.
        if (ImmediateHealthAmount(typeName) != 0) return false;
        var count = AcsPlayerInventory.Count(player, typeName, max: false);
        if (count <= 0) return false;
        return count >= (amount > 0 ? amount : AcsPlayerInventory.Count(player, typeName, max: true));
    }

    /// <summary>Native A_CheckReload for represented primary-fire Doom weapons.</summary>
    public static void CheckReload(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor is not PlayerPawn { Destroyed: false } player) return;
        var definition = WeaponCatalog.Find(player.Inventory.Selected);
        if (definition?.Ammo is not { } ammo || player.Inventory.HasAmmoForUse(ammo, definition.AmmoUse)) return;
        SelectWeapon(player, null, flags: 1);
    }

    public static void CheckRailReload(Actor actor) => CheckReload(actor);

    /// <summary>Native A_SelectWeapon named-weapon form; selection does not require ammo.</summary>
    public static bool SelectWeapon(Actor actor, string? typeName, int flags = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if ((flags & ~1) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        if (actor is not PlayerPawn { Destroyed: false } player) return false;
        if (!string.IsNullOrEmpty(typeName) && AcsPlayerInventory.TryWeaponKind(typeName, out var weapon)
            && player.Inventory.Owns(weapon))
        {
            if (player.Inventory.Selected != weapon) player.Inventory.Pending = weapon;
            return true;
        }
        if ((flags & 1) == 0) return false;
        var best = player.Inventory.BestSelectableWeapon();
        if (best != 0)
        {
            player.Inventory.Pending = best;
            if (player.Inventory.Selected == 0) player.BringUpWeapon(best);
            else player.WeaponLowering = true;
        }
        return true;
    }

    public static bool SetInventory(Actor actor, string? typeName, int amount, int pointerSelector = 0,
        bool beyondMax = false)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrEmpty(typeName)) return false;
        var subject = Resolve(actor, pointerSelector);
        if (subject is null || subject.Destroyed) return false;
        if (typeName.Equals("MaxHealthBonus", StringComparison.OrdinalIgnoreCase))
            return amount <= 0 || PickupCatalog.GiveMaxHealthBonus(subject, amount);
        if (typeName.Equals("UpgradeStamina", StringComparison.OrdinalIgnoreCase))
            return amount <= 0 || ActorPickupActions.UpgradeStamina(subject, amount);
        if (typeName.Equals("HealthFillup", StringComparison.OrdinalIgnoreCase))
            return amount <= 0 || ActorPickupActions.HealthFillup(subject);
        if (typeName.Equals("UpgradeAccuracy", StringComparison.OrdinalIgnoreCase))
            return amount <= 0 || ActorPickupActions.UpgradeAccuracy(subject);
        if (ActorPickupActions.IsDoorToken(typeName))
            return amount <= 0 || ActorPickupActions.UseDoorToken(subject, typeName);
        if (ImmediateHealthAmount(typeName) != 0)
        {
            // Immediate health never remains held; setter creation assigns Amount directly.
            if (amount <= 0) return true;
            var maximum = typeName.ToUpperInvariant() switch
            {
                "HEALTHBONUS" => PickupCatalog.DoomHealthDefaults(subject, PickupCatalog.HealthBonus).Maximum,
                "SOULSPHERE" => PickupCatalog.DoomHealthDefaults(subject, PickupCatalog.Soulsphere).Maximum,
                "MEGASPHEREHEALTH" => subject.Simulation?.DehackedMegasphereHealth ?? 200,
                _ => 0,
            };
            return subject.GiveBody(amount, maximum);
        }
        if (subject is not PlayerPawn player) return false;
        if (IsManagedPowerup(typeName))
        {
            if (AcsPlayerInventory.Count(player, typeName, max: false) == 0)
                return amount <= 0 || Give(player, typeName, amount, 0);
            if (amount == 1) return false;
            if (amount <= 0) return Take(player, typeName, 0, 0, 0);
            if (beyondMax)
                throw new NotSupportedException("Multiple held powerup amounts require native inventory stack support.");
            // Changing inventory Amount does not replenish EffectTics.
            return true;
        }
        var armorEditor = ArmorEditorNumber(typeName);
        if (armorEditor != 0)
            return amount <= 0 || PickupCatalog.TryGiveDoomClass(player, armorEditor, ignoreSkill: true);
        if (typeName.Equals("BlueArmorForMegasphere", StringComparison.OrdinalIgnoreCase))
            return amount <= 0 || PickupCatalog.GiveArmor(player.Inventory, 200,
                PlayerInventory.MegaSavePercent, "BlueArmorForMegasphere");
        var keyEditor = KeyEditorNumber(typeName);
        if (keyEditor != 0)
        {
            if (AcsPlayerInventory.Count(player, typeName, max: false) == 0)
                return amount <= 0 || PickupCatalog.TryGiveDoomClass(player, keyEditor, ignoreSkill: true);
            if (amount == 1) return false;
            if (amount <= 0) return Take(player, typeName, 0, 0, 0);
            if (beyondMax)
                throw new NotSupportedException("Multiple held key amounts require native inventory stack support.");
            return true;
        }
        if (AcsPlayerInventory.TryWeaponKind(typeName, out var weaponKind))
        {
            if (!player.Inventory.Owns(weaponKind))
            {
                if (amount <= 0) return true;
                if (weaponKind == WeaponKind.Fist)
                    return PickupCatalog.GiveWeapon(player.Inventory, weaponKind, AmmoKind.Bullets, 0, false);
                return PickupCatalog.TryWeaponEdNum(weaponKind, out var editor)
                    && PickupCatalog.TryGiveDoomClass(player, editor, ignoreSkill: true);
            }
            if (amount == 1) return false;
            if (amount <= 0) return Take(player, typeName, 0, 0, 0);
            if (beyondMax)
                throw new NotSupportedException("Multiple held weapon amounts require native inventory stack support.");
            return true;
        }
        if (IsBackpack(typeName))
        {
            var current = player.Inventory.HasBackpack ? 1 : 0;
            if (current == 0) return amount <= 0 || PickupCatalog.TryGiveDoomClass(player, PickupCatalog.Backpack, ignoreSkill: true);
            if (amount == current) return false;
            if (amount <= 0) return player.Inventory.RemoveBackpack();
            if (beyondMax && amount > 1)
                throw new NotSupportedException("Multiple held backpack amounts require native inventory stack support.");
            // Native growth clamps to MaxAmount=1 and still reports success.
            return true;
        }
        if (AcsPlayerInventory.TryCapacityAmmoKind(typeName, out var ammoKind))
        {
            var current = player.Inventory.Ammo(ammoKind);
            if (amount == current) return false;
            if (amount <= 0 || amount < current)
                return Take(player, typeName, amount <= 0 ? 0 : current - amount, 0, 0);
            var value = beyondMax ? amount : Math.Clamp(amount, 0, AcsPlayerInventory.AmmoCapacity(player, typeName));
            switch (ammoKind)
            {
                case AmmoKind.Bullets: player.Inventory.Bullets = value; break;
                case AmmoKind.Shells: player.Inventory.Shells = value; break;
                case AmmoKind.Rockets: player.Inventory.Rockets = value; break;
                case AmmoKind.Cells: player.Inventory.Cells = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(ammoKind));
            }
            return true;
        }
        if (IsWornArmor(typeName))
        {
            var inventory = player.Inventory;
            if (amount == inventory.Armor) return false;
            if (amount <= 0 || amount < inventory.Armor)
                return Take(player, typeName, amount <= 0 ? 0 : inventory.Armor - amount, 0, 0);
            inventory.Armor = beyondMax ? amount : Math.Clamp(amount, 0, inventory.ArmorMaximum);
            return true;
        }
        return player.Inventory.SetHealthPickup(typeName, amount, beyondMax);
    }

    public static bool GiveInventory(Actor actor, string? typeName, int amount = 0, int pointerSelector = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Give(actor, typeName, amount, pointerSelector);
    }

    public static bool GiveToTarget(Actor actor, string? typeName, int amount = 0, int pointerSelector = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrEmpty(typeName)) return false;
        return Give(Resolve(actor, AcsActorPointer.Target), typeName, amount, pointerSelector);
    }

    public static int GiveToChildren(Actor actor, string? typeName, int amount = 0)
        => ChangeFamily(actor, typeName, siblings: false, receiver => Give(receiver, typeName, amount, 0));

    public static int GiveToSiblings(Actor actor, string? typeName, int amount = 0)
        => ChangeFamily(actor, typeName, siblings: true, receiver => Give(receiver, typeName, amount, 0));

    private static bool Give(Actor? receiver, string? typeName, int amount, int pointerSelector)
    {
        if (receiver is null || string.IsNullOrEmpty(typeName)) return false;
        if (typeName.Equals("Berserk", StringComparison.OrdinalIgnoreCase))
            return Resolve(receiver, pointerSelector) is PlayerPawn berserkPlayer
                && PickupCatalog.TryGiveDoomClass(berserkPlayer, PickupCatalog.Berserk);
        if (typeName.Equals("MaxHealthBonus", StringComparison.OrdinalIgnoreCase))
            return Resolve(receiver, pointerSelector) is { Destroyed: false } bonusReceiver
                && PickupCatalog.GiveMaxHealthBonus(bonusReceiver, Math.Max(1, amount));
        if (typeName.Equals("UpgradeStamina", StringComparison.OrdinalIgnoreCase))
            return Resolve(receiver, pointerSelector) is { Destroyed: false } staminaReceiver
                && ActorPickupActions.UpgradeStamina(staminaReceiver, Math.Max(1, amount));
        if (typeName.Equals("HealthFillup", StringComparison.OrdinalIgnoreCase))
            return Resolve(receiver, pointerSelector) is { Destroyed: false } fillReceiver
                && ActorPickupActions.HealthFillup(fillReceiver);
        if (typeName.Equals("UpgradeAccuracy", StringComparison.OrdinalIgnoreCase))
            return Resolve(receiver, pointerSelector) is { Destroyed: false } accuracyReceiver
                && ActorPickupActions.UpgradeAccuracy(accuracyReceiver);
        if (ActorPickupActions.IsDoorToken(typeName))
            return Resolve(receiver, pointerSelector) is { Destroyed: false } doorReceiver
                && ActorPickupActions.UseDoorToken(doorReceiver, typeName);
        var healthAmount = ImmediateHealthAmount(typeName);
        var megaArmor = typeName.Equals("BlueArmorForMegasphere", StringComparison.OrdinalIgnoreCase);
        var armorEditor = ArmorEditorNumber(typeName);
        var keyEditor = KeyEditorNumber(typeName);
        var ammoEditor = AmmoEditorNumber(typeName);
        var isWeapon = AcsPlayerInventory.TryWeaponKind(typeName, out var weaponKind);
        var isBackpack = IsBackpack(typeName);
        if (healthAmount == 0 && !megaArmor && armorEditor == 0 && keyEditor == 0 && ammoEditor == 0 && !isWeapon && !isBackpack && !IsManagedPowerup(typeName) && !HealthPickupCatalog.TryFind(typeName, out _)) return false;
        var subject = Resolve(receiver, pointerSelector);
        if (subject is null || subject.Destroyed) return false;
        amount = Math.Max(1, amount);
        if (IsManagedPowerup(typeName))
        {
            if (subject is not PlayerPawn powerPlayer) return false;
            if (typeName.Equals("PowerStrength", StringComparison.OrdinalIgnoreCase))
            { powerPlayer.GivePowerStrength(); return true; }
            // Zero-duration incoming powers are accepted without refreshing an existing item's timer.
            if (typeName.Equals("PowerHighJump", StringComparison.OrdinalIgnoreCase))
            { powerPlayer.GivePowerHighJump(); return true; }
            var current = typeName.ToUpperInvariant() switch
            {
                "POWERDAMAGE" => powerPlayer.PowerDamageTics,
                "POWERPROTECTION" => powerPlayer.PowerProtectionTics,
                "POWERINFINITEAMMO" => powerPlayer.PowerInfiniteAmmoTics,
                "POWERDRAIN" => powerPlayer.PowerDrainTics,
                "POWERREGENERATION" => powerPlayer.PowerRegenerationTics,
                "POWERPROSPERITY" => powerPlayer.PowerProsperityTics,
                "POWERSPREAD" => powerPlayer.PowerSpreadTics,
                "POWERDOUBLEFIRINGSPEED" => powerPlayer.PowerDoubleFiringSpeedTics,
                _ => powerPlayer.PowerBuddhaTics,
            };
            if (current > PlayerPawn.PowerBuddhaBlinkThreshold) return false;
            AcsPlayerInventory.Give(powerPlayer, typeName, amount);
            return true;
        }
        if (isBackpack)
            return subject is PlayerPawn packPlayer && PickupCatalog.TryGiveDoomClass(packPlayer, PickupCatalog.Backpack);
        if (isWeapon)
        {
            if (subject is not PlayerPawn weaponPlayer) return false;
            // Weapon.Amount does not change AmmoGive; represented weapons have single ownership.
            if (weaponKind == WeaponKind.Fist)
                return PickupCatalog.GiveWeapon(weaponPlayer.Inventory, weaponKind, AmmoKind.Bullets, 0, false);
            return PickupCatalog.TryWeaponEdNum(weaponKind, out var weaponEditor)
                && PickupCatalog.TryGiveDoomClass(weaponPlayer, weaponEditor);
        }
        if (ammoEditor != 0)
            return subject is PlayerPawn ammoPlayer && PickupCatalog.TryGiveDoomClass(ammoPlayer, ammoEditor, pickupAmount: amount);
        if (keyEditor != 0)
            return subject is PlayerPawn keyPlayer && (PickupCatalog.TryGiveDoomClass(keyPlayer, keyEditor)
                || keyPlayer.Simulation?.GameMode == SpawnGameMode.Single);
        if (megaArmor)
            return subject is PlayerPawn armorPlayer && PickupCatalog.GiveArmor(armorPlayer.Inventory,
                PickupCatalog.ScaleArmor(200, armorPlayer), PlayerInventory.MegaSavePercent,
                "BlueArmorForMegasphere");
        // DoGiveInventory assigns Amount; only ACS SetGiveAmount multiplies armor SaveAmount.
        if (armorEditor != 0)
            return subject is PlayerPawn suitPlayer && PickupCatalog.TryGiveDoomClass(suitPlayer, armorEditor);
        if (healthAmount != 0)
        {
            var editorNumber = typeName.ToUpperInvariant() switch
            {
                "STIMPACK" => PickupCatalog.Stimpack,
                "MEDIKIT" => PickupCatalog.Medikit,
                "HEALTHBONUS" => PickupCatalog.HealthBonus,
                "SOULSPHERE" => PickupCatalog.Soulsphere,
                _ => 0,
            };
            var megaHealth = subject.Simulation?.DehackedMegasphereHealth ?? 200;
            var defaults = typeName.Equals("MegasphereHealth", StringComparison.OrdinalIgnoreCase)
                ? (Amount: megaHealth, Maximum: megaHealth)
                : editorNumber == 0 ? (Amount: 1, Maximum: 0) : PickupCatalog.DoomHealthDefaults(subject, editorNumber);
            return subject.GiveBody(unchecked(defaults.Amount * amount), defaults.Maximum);
        }
        return subject is PlayerPawn player && player.Inventory.GiveHealthPickup(typeName, amount);
    }

    private static int KeyEditorNumber(string typeName) =>
        PickupCatalog.TryEditorNumberForDropName(typeName, out var editor) && PickupCatalog.IsKey(editor) ? editor : 0;

    private static int AmmoEditorNumber(string typeName) =>
        (AcsPlayerInventory.TryCapacityAmmoKind(typeName, out _) || AcsPlayerInventory.TryAmmoBox(typeName, out _, out _))
        && PickupCatalog.TryEditorNumberForDropName(typeName, out var editor) ? editor : 0;

    private static int ArmorEditorNumber(string typeName) => typeName.ToUpperInvariant() switch
    {
        "GREENARMOR" => PickupCatalog.GreenArmor,
        "BLUEARMOR" => PickupCatalog.MegaArmor,
        "ARMORBONUS" => PickupCatalog.ArmorBonus,
        _ => 0,
    };

    private static int ImmediateHealthAmount(string typeName) => typeName.ToUpperInvariant() switch
    {
        "HEALTH" => 1,
        "STIMPACK" => 10,
        "MEDIKIT" => 25,
        "HEALTHBONUS" => 1,
        "MAXHEALTHBONUS" => 1,
        "UPGRADESTAMINA" => 1,
        "UPGRADEACCURACY" => 1,
        "HEALTHFILLUP" => 1,
        "SOULSPHERE" => 100,
        "MEGASPHEREHEALTH" => 200,
        _ => 0,
    };

    public static bool TakeInventory(Actor actor, string? typeName, int amount = 0, int flags = 0,
        int pointerSelector = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Take(actor, typeName, amount, flags, pointerSelector);
    }

    public static bool TakeFromTarget(Actor actor, string? typeName, int amount = 0, int flags = 0,
        int pointerSelector = 0)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrEmpty(typeName)) return false;
        return Take(Resolve(actor, AcsActorPointer.Target), typeName, amount, flags, pointerSelector);
    }

    public static int TakeFromChildren(Actor actor, string? typeName, int amount = 0)
        => ChangeFamily(actor, typeName, siblings: false, receiver => Take(receiver, typeName, amount, 0, 0));

    public static int TakeFromSiblings(Actor actor, string? typeName, int amount = 0)
        => ChangeFamily(actor, typeName, siblings: true, receiver => Take(receiver, typeName, amount, 0, 0));

    private static int ChangeFamily(Actor actor, string? typeName, bool siblings, Func<Actor, bool> change)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrEmpty(typeName)
            || !HealthPickupCatalog.TryFind(typeName, out _) && ImmediateHealthAmount(typeName) == 0
                && ArmorEditorNumber(typeName) == 0
                && KeyEditorNumber(typeName) == 0
                && !AcsPlayerInventory.TryCapacityAmmoKind(typeName, out _)
                && AmmoEditorNumber(typeName) == 0
                && !AcsPlayerInventory.TryWeaponKind(typeName, out _)
                && !ActorPickupActions.IsDoorToken(typeName)
                && !IsBackpack(typeName)
                && !IsManagedPowerup(typeName)
                && !IsWornArmor(typeName)
                && actor.Simulation?.Players.Any(player => HasStoredArmor(player, typeName)) != true
                && !typeName.Equals("BlueArmorForMegasphere", StringComparison.OrdinalIgnoreCase)) return 0;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Family inventory selection requires a simulation.");
        var master = siblings ? Resolve(actor, AcsActorPointer.Master) : actor;
        if (master is null) return 0;
        var count = 0;
        foreach (var receiver in sim.Actors.Where(candidate => !candidate.Destroyed && candidate.MasterId == master.Id
            && (!siblings || candidate != actor)).ToArray())
            if (change(receiver)) count++;
        return count;
    }

    private static bool Take(Actor? receiver, string? typeName, int amount, int flags, int pointerSelector)
    {
        if ((flags & ~1) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        if (receiver is null || string.IsNullOrEmpty(typeName)) return false;
        if (Resolve(receiver, pointerSelector) is not PlayerPawn { Destroyed: false } player) return false;
        // DepleteBy(0) removes the item; TIF_NOTAKEINFINITE only protects ammo.
        var take = amount == 0 ? int.MaxValue : (int)Math.Min(int.MaxValue, Math.Abs((long)amount));
        if (IsManagedPowerup(typeName))
        {
            var hadPowerup = AcsPlayerInventory.Count(player, typeName, max: false) > 0;
            AcsPlayerInventory.Take(player, typeName, take);
            return hadPowerup;
        }
        if (IsBackpack(typeName)) return player.Inventory.RemoveBackpack();
        if (AcsPlayerInventory.TryWeaponKind(typeName, out var weaponKind))
        {
            var hadWeapon = player.Inventory.Owns(weaponKind);
            AcsPlayerInventory.Take(player, typeName, take);
            return hadWeapon;
        }
        if (AcsPlayerInventory.TryCapacityAmmoKind(typeName, out var ammoKind))
        {
            var hadAmmo = player.Inventory.Ammo(ammoKind) > 0;
            if ((flags & 1) != 0 && player.HasInfiniteAmmo) return false;
            AcsPlayerInventory.Take(player, typeName, take);
            return hadAmmo;
        }
        if (KeyEditorNumber(typeName) != 0)
        {
            var hadKey = AcsPlayerInventory.Count(player, typeName, max: false) > 0;
            AcsPlayerInventory.Take(player, typeName, take);
            return hadKey;
        }
        if (IsWornArmor(typeName))
        {
            var hadArmor = player.Inventory.Armor > 0;
            // BasicArmor keeps its depleted inventory object and absorption metadata.
            player.Inventory.Armor = Math.Max(0, player.Inventory.Armor - take);
            return hadArmor;
        }
        if (HealthPickupCatalog.TryFind(typeName, out _))
            return player.Inventory.TakeHealthPickup(typeName, take) > 0;
        return player.Inventory.TakeStoredArmor(typeName, take) > 0;
    }

    private static bool HasStoredArmor(PlayerPawn player, string typeName) =>
        player.Inventory.SpareArmor.Any(spare => spare.ArmorType.Equals(typeName, StringComparison.OrdinalIgnoreCase));

    private static bool IsWornArmor(string typeName) =>
        typeName.Equals("Armor", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("BasicArmor", StringComparison.OrdinalIgnoreCase);

    private static bool IsBackpack(string typeName) => typeName.Equals("Backpack", StringComparison.OrdinalIgnoreCase);

    private static bool IsManagedPowerup(string typeName) =>
        typeName.Equals("PowerStrength", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerDamage", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerInfiniteAmmo", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerHighJump", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerDoubleFiringSpeed", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerProsperity", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerSpread", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerRegeneration", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerDrain", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("PowerProtection", StringComparison.OrdinalIgnoreCase);

    private static Actor? Resolve(Actor actor, int pointerSelector)
    {
        if (pointerSelector == AcsActorPointer.Default) return actor;
        if (pointerSelector == AcsActorPointer.Null) return null;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Inventory pointer selection requires a simulation.");
        return AcsActorPointer.Resolve(sim, actor, pointerSelector);
    }
}
