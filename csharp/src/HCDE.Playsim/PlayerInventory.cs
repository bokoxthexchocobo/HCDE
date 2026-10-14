namespace HCDE.Playsim;

[Flags]
public enum WeaponKind
{
    Fist = 1,
    Pistol = 2,
    Chainsaw = 4,
    Shotgun = 8,
    SuperShotgun = 16,
    Chaingun = 32,
    RocketLauncher = 64,
    Plasma = 128,
    Bfg = 256,
}

public enum AmmoKind
{
    Bullets,
    Shells,
    Rockets,
    Cells,
}

/// <summary>
/// Doom pistol-start inventory. Bullets begin at 50. Fist and pistol are already owned.
/// </summary>
/// <summary>One stored <c>BasicArmorPickup</c>. The worn suit is separate.</summary>
public readonly record struct SpareArmor(int SaveAmount, int SavePercent, int MaxAbsorb, int MaxFullAbsorb,
    string ArmorType = "BasicArmorPickup", bool IgnoreSkill = false);

public sealed class PlayerInventory
{
    private readonly List<HealthPickupStack> _healthPickups = new();
    public IReadOnlyList<HealthPickupStack> HealthPickups => _healthPickups;
    /// <summary>Use one numeric HealthPickup stack item; failed healing leaves its amount unchanged.</summary>
    public bool UseHealthPickup(int index)
    {
        if ((uint)index >= (uint)_healthPickups.Count) throw new ArgumentOutOfRangeException(nameof(index));
        var item = _healthPickups[index];
        if (_owner is null || _owner.Destroyed || !_owner.GiveBody(item.Health)) return false;
        if (_owner.Simulation?.InfiniteHealthItemUse == true) return true;
        if (item.Amount == 1) _healthPickups.RemoveAt(index);
        else _healthPickups[index] = item with { Amount = item.Amount - 1 };
        return true;
    }
    public int CountHealthPickup(string typeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        return _healthPickups.FirstOrDefault(item => string.Equals(item.TypeName, typeName, StringComparison.OrdinalIgnoreCase)).Amount;
    }

    public bool UseHealthPickup(string typeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        var index = _healthPickups.FindIndex(item => string.Equals(item.TypeName, typeName, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && UseHealthPickup(index);
    }

    public int TakeHealthPickup(string typeName, int amount)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        if (amount <= 0) return 0;
        var index = _healthPickups.FindIndex(item => string.Equals(item.TypeName, typeName, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return 0;
        var item = _healthPickups[index];
        var taken = Math.Min(amount, item.Amount);
        if (taken == item.Amount) _healthPickups.RemoveAt(index);
        else _healthPickups[index] = item with { Amount = item.Amount - taken };
        return taken;
    }

    /// <summary>Native Actor.SetInventory rules for supported named health items.</summary>
    public bool SetHealthPickup(string typeName, int amount, bool beyondMax = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        if (!HealthPickupCatalog.TryFind(typeName, out var definition, _owner?.Simulation?.DefaultInventoryMaximum ?? 25)) return false;
        var index = _healthPickups.FindIndex(item => string.Equals(item.TypeName, typeName, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return amount <= 0 || GiveHealthPickup(typeName, amount);
        var item = _healthPickups[index];
        if (amount == item.Amount) return false;
        if (amount <= 0) _healthPickups.RemoveAt(index);
        else if (amount < item.Amount) _healthPickups[index] = item with { Amount = amount };
        else _healthPickups[index] = item with { Amount = beyondMax ? amount : Math.Min(amount, definition.Maximum) };
        return true;
    }

    public bool GiveHealthPickup(string typeName, int amount, int? healthOverride = null, int? autoUseOverride = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        if (amount <= 0 || !HealthPickupCatalog.TryFind(typeName, out var definition, _owner?.Simulation?.DefaultInventoryMaximum ?? 25)) return false;
        var health = healthOverride ?? definition.Health;
        var mode = autoUseOverride ?? definition.AutoUseMode;
        if (mode is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(autoUseOverride));
        if (health == 0 || health < 0 && mode != 0)
            throw new ArgumentOutOfRangeException(nameof(healthOverride));
        var index = _healthPickups.FindIndex(item => item.Health == health
            && string.Equals(item.TypeName, definition.TypeName, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            var item = _healthPickups[index];
            var unlimited = _owner?.Simulation?.UnlimitedHealthPickups ?? false;
            if (!unlimited && item.Amount >= definition.Maximum) return false;
            _healthPickups[index] = item with { Amount = (int)Math.Min(unlimited ? int.MaxValue : definition.Maximum, (long)item.Amount + amount) };
        }
        else
            _healthPickups.Insert(0, new(health, Math.Min(amount, definition.Maximum), mode, definition.TypeName));
        return true;
    }

    public void AddHealthPickup(int health, int amount, int autoUseMode = 0, string? typeName = null)
    {
        if (health == 0 || health < 0 && autoUseMode != 0) throw new ArgumentOutOfRangeException(nameof(health));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (autoUseMode is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(autoUseMode));
        if (typeName is not null) ArgumentException.ThrowIfNullOrEmpty(typeName);
        _healthPickups.Add(new(health, amount, autoUseMode, typeName));
    }
    internal void RestoreHealthPickups(IReadOnlyList<HealthPickupStack>? items)
    {
        _healthPickups.Clear(); if (items is not null) _healthPickups.AddRange(items);
    }
    public int AutoUseStrifeHealth(int health, int maximum, double healthFactor = 1)
        => AutoUseStrifeHealthCore(health, maximum, healthFactor, health).Health;

    internal long AutoUseStrifePoisonHealth(int before, int damage, int maximum, double healthFactor)
    {
        var result = AutoUseStrifeHealthCore(before - damage, maximum, healthFactor, before);
        // GiveBody mirrors healed player health to the actor before P_PoisonDamage subtracts again.
        return result.Used ? (long)result.Health - damage : (long)before - damage;
    }

    private (int Health, bool Used) AutoUseStrifeHealthCore(int health, int maximum, double healthFactor, int actorHealth)
    {
        if (!double.IsFinite(healthFactor)) throw new ArgumentOutOfRangeException(nameof(healthFactor));
        var used = false;
        while (actorHealth > 0 && health < 50 && health < maximum)
        {
            var index = -1; var threshold = 0;
            for (var i = 0; i < _healthPickups.Count; i++)
                if (_healthPickups[i].AutoUseMode == 3 && _healthPickups[i].Health > threshold)
                {
                    index = i;
                    // Native AutoUseStrifeHealth compares healing against the previous stack's amount.
                    threshold = _healthPickups[i].Amount;
                }
            if (index < 0) break;
            while (actorHealth > 0 && health < 50 && health < maximum)
            {
                var item = _healthPickups[index];
                var gain = (int)Math.Clamp(Math.Min(item.Health, 65536) * healthFactor, 1, int.MaxValue);
                var count = (int)Math.Min(item.Amount, ((long)Math.Min(50, maximum) - health + gain - 1) / gain);
                // A first use that leaves mirrored actor health nonpositive prevents further GiveBody calls.
                if (health <= 0) count = 1;
                health = (int)Math.Min(maximum, health + (long)count * gain);
                actorHealth = health;
                used = true;
                if (_owner?.Simulation?.InfiniteHealthItemUse == true) continue;
                if (count == item.Amount)
                {
                    _healthPickups.RemoveAt(index);
                    break;
                }
                _healthPickups[index] = item with { Amount = item.Amount - count };
            }
        }
        return (health, used);
    }
    public int AutoUseHealth(int required, bool skillUse)
    {
        if (required <= 0) return 0;
        long Total(int mode) => _healthPickups.Where(i => i.AutoUseMode == mode).Sum(i => (long)i.Health * i.Amount);
        var first = Total(1); var second = Total(2);
        var useFirst = skillUse && first >= required;
        var useSecond = !useFirst && second >= required;
        var combine = !useFirst && !useSecond && skillUse && first + second >= required;
        if (!useFirst && !useSecond && !combine) return 0;
        long remaining = required; long saved = 0;
        void Consume(int mode)
        {
            while (remaining > 0)
            {
                var index = -1; var largest = 0;
                for (var i = 0; i < _healthPickups.Count; i++)
                    if (_healthPickups[i].AutoUseMode == mode && _healthPickups[i].Health > largest)
                    { index = i; largest = _healthPickups[i].Health; }
                if (index < 0) break;
                var item = _healthPickups[index];
                var count = (int)Math.Min(item.Amount, (remaining + largest - 1) / largest);
                var gain = (long)count * largest; remaining -= gain; saved += gain;
                if (count == item.Amount) _healthPickups.RemoveAt(index);
                else _healthPickups[index] = item with { Amount = item.Amount - count };
            }
        }
        if (useFirst || combine) Consume(1);
        if (useSecond || combine) Consume(2);
        return (int)Math.Min(int.MaxValue, saved);
    }
    private readonly PlayerPawn? _owner;
    public PlayerInventory() { }
    internal PlayerInventory(PlayerPawn owner) => _owner = owner;
    private double ArmorFactor => _owner?.Simulation?.ArmorFactor ?? 1;

    public const int MaxHealthBonus = 200;
    public const int GreenArmorAmount = 100;
    public const int MegaArmorAmount = 200;
    public const int GreenSavePercent = 33;
    internal static double ArmorSaveFraction(int percent) =>
        percent == GreenSavePercent ? 0.33335 : percent / 100.0;
    public const int MegaSavePercent = 50;

    public int Armor { get; set; }
    /// <summary>Native BasicArmor.MaxAmount, retained when armor is depleted.</summary>
    public int ArmorMaximum { get; set; } = 1;
    /// <summary>Native BasicArmor.ActualSaveAmount, independent of absorbed damage.</summary>
    public int ArmorActualSaveAmount { get; set; }
    public string ArmorType { get; set; } = "None";
    public int ArmorSavePercent { get; set; }
    /// <summary>BasicArmor total save cap. 0 means no cap.</summary>
    public int MaxAbsorb { get; set; }
    /// <summary>BasicArmor amount saved at 100% before the percent applies.</summary>
    public int MaxFullAbsorb { get; set; }
    /// <summary>Saved so far. A new suit does not clear it.</summary>
    public int AbsorbCount { get; set; }
    private readonly List<SpareArmor> _spareArmor = new();
    /// <summary>BasicArmorPickup items retained when pickup autoactivation fails.</summary>
    public IReadOnlyList<SpareArmor> SpareArmor => _spareArmor;
    internal void RestoreSpareArmor(IReadOnlyList<SpareArmor>? spares)
    {
        _spareArmor.Clear();
        if (spares is not null) _spareArmor.AddRange(spares);
    }
    public int Bullets { get; set; } = 50;
    private int StartingBullets => Math.Clamp(_owner?.Simulation?.DehackedInitialBullets ?? 50, 0, 200);
    public int Shells { get; set; }
    public int Rockets { get; set; }
    public int Cells { get; set; }
    public int MaxBullets { get; set; } = 200;
    public int MaxShells { get; set; } = 50;
    public int MaxRockets { get; set; } = 50;
    public int MaxCells { get; set; } = 300;
    /// <summary>Set by the first backpack. Later backpacks only add ammo.</summary>
    public bool HasBackpack { get; set; }
    public bool RedKey { get; set; }
    public bool BlueKey { get; set; }
    public bool YellowKey { get; set; }
    public WeaponKind Weapons { get; set; } = WeaponKind.Fist | WeaponKind.Pistol;
    public WeaponKind Selected { get; set; } = WeaponKind.Pistol;
    /// <summary>Native <c>PendingWeapon</c>. Null is <c>WP_NOCHANGE</c>. The ready weapon stays <see cref="Selected"/> until the lower finishes.</summary>
    public WeaponKind? Pending { get; set; }
    /// <summary>Converted player GetNeverSwitch preference for pickup-triggered switching.</summary>
    public bool NeverAutoSwitch { get; set; }

    internal void ClearInventory()
    {
        _healthPickups.Clear();
        _owner?.ClearDamagePowers();
        RemoveBackpack();
        Armor = 0;
        _spareArmor.Clear();
        Bullets = Shells = Rockets = Cells = 0;
        RedKey = BlueKey = YellowKey = false;
        Weapons = 0;
        Selected = 0;
        Pending = null;
    }

    /// <summary>Deathmatch pistol start. Cooperative respawn uses <see cref="FilterCoopRespawn"/>.</summary>
    public void ResetToPistolStart()
    {
        _healthPickups.Clear();
        _owner?.ClearDamagePowers();
        Armor = 0;
        ArmorMaximum = 1;
        ArmorActualSaveAmount = 0;
        ArmorType = "None";
        ArmorSavePercent = 0;
        MaxAbsorb = 0;
        MaxFullAbsorb = 0;
        AbsorbCount = 0;
        _spareArmor.Clear();
        Bullets = StartingBullets;
        Shells = 0;
        Rockets = 0;
        Cells = 0;
        MaxBullets = 200;
        MaxShells = 50;
        MaxRockets = 50;
        MaxCells = 300;
        HasBackpack = false;
        RedKey = false;
        BlueKey = false;
        YellowKey = false;
        Weapons = WeaponKind.Fist | WeaponKind.Pistol;
        Selected = WeaponKind.Pistol;
        Pending = null;
    }

    /// <summary>
    /// Keeps a <c>BasicArmorPickup</c> whose max amount is above 0. A worn amount
    /// at zero uses it now. Otherwise it stays, and the highest save
    /// percent is used when the worn suit reaches 0. Doom green and mega do not
    /// come through here.
    /// </summary>
    public bool TryKeepArmorPickup(int saveAmount, int savePercent, int maxAbsorb = 0, int maxFullAbsorb = 0,
        string armorType = "BasicArmorPickup", bool ignoreSkill = false)
    {
        if (saveAmount <= 0) return false;
        savePercent = Math.Clamp(savePercent, 0, 100);
        var spare = new SpareArmor(saveAmount, savePercent, maxAbsorb, maxFullAbsorb, armorType, ignoreSkill);
        var scaledAmount = GetSpareSaveAmount(spare);
        if (Armor <= 0 && Armor < scaledAmount)
        {
            EquipSpareArmor(spare, scaledAmount);
            return true;
        }
        _spareArmor.Add(spare);
        return true;
    }

    /// <summary>
    /// <c>BasicArmor.AbsorbDamage</c> when the amount reaches 0. The percent is
    /// cleared, then the spare with the highest save percent is used. An equal
    /// percent keeps the earlier spare. <see cref="AbsorbCount"/> stays.
    /// </summary>
    internal void PromoteSpareArmor()
    {
        if (Armor != 0) return;
        ArmorSavePercent = 0;
        ArmorType = "None";
        if (_spareArmor.Count == 0) return;
        var best = 0;
        for (var i = 1; i < _spareArmor.Count; i++)
            if (_spareArmor[i].SavePercent > _spareArmor[best].SavePercent)
                best = i;
        var spare = _spareArmor[best];
        var saveAmount = GetSpareSaveAmount(spare);
        if (Armor >= saveAmount) return;
        if (_owner?.Simulation?.InfiniteStoredInventoryUse != true) _spareArmor.RemoveAt(best);
        EquipSpareArmor(spare, saveAmount);
    }

    internal bool TryUseStoredArmor(string armorType)
    {
        var index = _spareArmor.FindIndex(spare => spare.ArmorType.Equals(armorType, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;
        var spare = _spareArmor[index];
        var saveAmount = GetSpareSaveAmount(spare);
        if (Armor >= saveAmount) return false;
        if (_owner?.Simulation?.InfiniteStoredInventoryUse != true) _spareArmor.RemoveAt(index);
        EquipSpareArmor(spare, saveAmount);
        return true;
    }

    private int GetSpareSaveAmount(SpareArmor spare) => spare.IgnoreSkill
        ? spare.SaveAmount : PickupCatalog.ScaleArmorAmount(spare.SaveAmount, ArmorFactor);

    private void EquipSpareArmor(SpareArmor spare, int saveAmount)
    {
        Armor = saveAmount;
        ArmorMaximum = saveAmount;
        ArmorActualSaveAmount = saveAmount;
        ArmorType = spare.ArmorType;
        ArmorSavePercent = spare.SavePercent;
        MaxAbsorb = spare.MaxAbsorb;
        MaxFullAbsorb = spare.MaxFullAbsorb;
    }

    internal int TakeStoredArmor(string armorType, int amount)
    {
        var taken = 0;
        for (var index = 0; index < _spareArmor.Count && amount > 0;)
        {
            if (_spareArmor[index].ArmorType.Equals(armorType, StringComparison.OrdinalIgnoreCase))
            {
                _spareArmor.RemoveAt(index);
                amount--;
                taken++;
            }
            else
                index++;
        }
        return taken;
    }

    /// <summary>
    /// <c>FilterCoopRespawnInventory</c> for this pistol-start inventory.
    /// Every flag defaults off, which keeps the pack. Lose-everything is a pistol start.
    /// Bullets are the only default ammo, at 50. Other pools have no default item, so
    /// lose-ammo zeroes them and halve-ammo divides them when the amount is above 1.
    /// A backpack stays unless everything is lost. PowerBuddha is cleared with a pistol
    /// start. Spare armor is cleared with the worn suit. Lose-keys is skipped by the
    /// caller when keys are shared. Losing everything still clears keys. Puzzle items
    /// are absent.
    /// </summary>
    public void FilterCoopRespawn(bool loseInventory, bool loseKeys, bool loseWeapons, bool loseArmor, bool loseAmmo, bool halveAmmo)
    {
        if (loseInventory)
        {
            ResetToPistolStart();
            return;
        }

        if (loseKeys)
        {
            RedKey = false;
            BlueKey = false;
            YellowKey = false;
        }
        if (loseWeapons)
            Weapons = WeaponKind.Fist | WeaponKind.Pistol;
        if (loseArmor)
        {
            Armor = 0;
            ArmorSavePercent = GreenSavePercent;
            _spareArmor.Clear();
        }
        if (loseAmmo)
        {
            Bullets = Math.Min(StartingBullets, MaxBullets);
            Shells = 0;
            Rockets = 0;
            Cells = 0;
        }
        else if (halveAmmo)
        {
            Bullets = HalveAmmo(Bullets, StartingBullets);
            Shells = HalveAmmo(Shells, 0);
            Rockets = HalveAmmo(Rockets, 0);
            Cells = HalveAmmo(Cells, 0);
        }

        if ((loseWeapons || loseAmmo || halveAmmo) && !CanSelect(Selected))
            Selected = CanSelect(WeaponKind.Pistol) ? WeaponKind.Pistol : WeaponKind.Fist;
    }

    private static int HalveAmmo(int amount, int starting)
    {
        if (amount <= 1)
            return amount;
        var halved = amount / 2;
        return starting > 0 ? Math.Max(halved, starting) : halved;
    }

    public bool Owns(WeaponKind weapon) => (Weapons & weapon) != 0;

    public bool HasKey(PickupCatalog.KeyColor color) => color switch
    {
        PickupCatalog.KeyColor.Blue => BlueKey,
        PickupCatalog.KeyColor.Red => RedKey,
        PickupCatalog.KeyColor.Yellow => YellowKey,
        _ => false,
    };

    /// <summary>
    /// Doom backpack. The first one lifts the caps to 400/100/100/600 and gives
    /// 10 bullets, 4 shells, 1 rocket, and 20 cells. Another one only gives ammo.
    /// The pickup is taken even when every pool is already full.
    /// A depleted first pack lifts the caps and gives nothing. A depleted pack
    /// still gives ammo when the player already has one.
    /// Amounts are already scaled by the caller.
    /// </summary>
    public bool GiveBackpack(int bullets = 10, int shells = 4, int rockets = 1, int cells = 20, bool depleted = false)
    {
        var first = !HasBackpack;
        if (first)
        {
            HasBackpack = true;
            MaxBullets = Math.Max(MaxBullets, 400);
            MaxShells = Math.Max(MaxShells, 100);
            MaxRockets = Math.Max(MaxRockets, 100);
            MaxCells = Math.Max(MaxCells, 600);
        }
        if (!(depleted && first))
        {
            TryAddAmmo(AmmoKind.Bullets, bullets);
            TryAddAmmo(AmmoKind.Shells, shells);
            TryAddAmmo(AmmoKind.Rockets, rockets);
            TryAddAmmo(AmmoKind.Cells, cells);
        }
        return true;
    }

    /// <summary>
    /// <c>DetachFromOwner</c> resets caps equal to the vanilla backpack maximum.
    /// Other caps stay unchanged. Ammo above a reset cap is cut.
    /// </summary>
    public bool RemoveBackpack()
    {
        if (!HasBackpack)
            return false;
        HasBackpack = false;
        if (MaxBullets == 400)
        {
            MaxBullets = 200;
            Bullets = Math.Min(Bullets, MaxBullets);
        }
        if (MaxShells == 100)
        {
            MaxShells = 50;
            Shells = Math.Min(Shells, MaxShells);
        }
        if (MaxRockets == 100)
        {
            MaxRockets = 50;
            Rockets = Math.Min(Rockets, MaxRockets);
        }
        if (MaxCells == 600)
        {
            MaxCells = 300;
            Cells = Math.Min(Cells, MaxCells);
        }
        return true;
    }

    // Default Doom slots from doomplayer.zs. Custom class/slot tables are not loaded yet.
    private static readonly WeaponKind[][] Slots = [[], [WeaponKind.Fist, WeaponKind.Chainsaw],
        [WeaponKind.Pistol], [WeaponKind.Shotgun, WeaponKind.SuperShotgun], [WeaponKind.Chaingun],
        [WeaponKind.RocketLauncher], [WeaponKind.Plasma], [WeaponKind.Bfg], [], []];
    private static readonly WeaponKind[] Cycle = Slots.SelectMany(slot => slot).ToArray();

    public void SelectSlot(byte slot)
    {
        // Weapon.Use sets PendingWeapon when the pick is not the ready weapon.
        // A press of the ready weapon leaves an existing pending switch alone.
        var choice = ChooseSlot(slot, Pending ?? Selected);
        if (choice is not { } weapon || weapon == Selected)
            return;
        Pending = weapon;
    }

    private WeaponKind? ChooseSlot(byte slot, WeaponKind recent)
    {
        // WST_NONE=10, WST_PREV=11, WST_NEXT=12 in g_game.h.
        // A switch in progress cycles from the pending weapon.
        if (slot is 11 or 12)
        {
            var current = Array.IndexOf(Cycle, recent);
            if (current < 0) return null;
            var direction = slot == 12 ? 1 : -1;
            for (var step = 1; step <= Cycle.Length; step++)
            {
                var weapon = Cycle[(current + step * direction + Cycle.Length) % Cycle.Length];
                if (CanSelect(weapon)) return weapon;
            }
            return null;
        }
        if (slot >= Slots.Length) return null;
        var weapons = Slots[slot];
        var index = Array.IndexOf(weapons, recent);
        // Repeated presses cycle backwards in the slot; entry prefers its last weapon.
        for (var step = 1; step <= weapons.Length; step++)
        {
            var weapon = weapons[((index < 0 ? 0 : index) - step + weapons.Length) % weapons.Length];
            if (CanSelect(weapon)) return weapon;
        }
        return null;
    }

    internal void RemoveWeapon(WeaponKind weapon)
    {
        if (!Owns(weapon)) return;
        Weapons &= ~weapon;
        if (Pending == weapon) Pending = null;
        if (Selected != weapon) return;
        Selected = 0;
        if (Pending.HasValue) return;
        var best = BestSelectableWeapon();
        if (best != 0) Pending = best;
    }

    internal WeaponKind BestSelectableWeapon()
    {
        WeaponKind best = 0;
        foreach (var candidate in Cycle)
            if (CanSelect(candidate) && (best == 0
                || WeaponCatalog.SelectionOrder(candidate) < WeaponCatalog.SelectionOrder(best)))
                best = candidate;
        return best;
    }

    private bool CanSelect(WeaponKind weapon)
    {
        var definition = WeaponCatalog.Find(weapon);
        return Owns(weapon) && definition != null
            && (definition.Ammo is not { } ammo || HasAmmoForUse(ammo, definition.AmmoUse));
    }

    internal void CheckAmmoPickupSwitch(AmmoKind ammo)
    {
        if (NeverAutoSwitch || Pending.HasValue || Selected is not (0 or WeaponKind.Fist or WeaponKind.Pistol)) return;
        var best = Selected;
        foreach (var weapon in Cycle)
            if (WeaponCatalog.Find(weapon)?.Ammo == ammo && CanSelect(weapon)
                && (best == 0 || WeaponCatalog.SelectionOrder(weapon) < WeaponCatalog.SelectionOrder(best))) best = weapon;
        if (best != Selected) Pending = best;
    }

    public int Ammo(AmmoKind kind) => kind switch
    {
        AmmoKind.Bullets => Bullets,
        AmmoKind.Shells => Shells,
        AmmoKind.Rockets => Rockets,
        _ => Cells,
    };

    public bool TryAddAmmo(AmmoKind kind, int amount)
    {
        if (amount < 0)
            return false;

        var (current, max) = kind switch
        {
            AmmoKind.Bullets => (Bullets, MaxBullets),
            AmmoKind.Shells => (Shells, MaxShells),
            AmmoKind.Rockets => (Rockets, MaxRockets),
            _ => (Cells, MaxCells),
        };
        if (current >= max)
            return false;

        var next = (int)Math.Min(max, (long)current + amount);
        switch (kind)
        {
            case AmmoKind.Bullets:
                Bullets = next;
                break;
            case AmmoKind.Shells:
                Shells = next;
                break;
            case AmmoKind.Rockets:
                Rockets = next;
                break;
            default:
                Cells = next;
                break;
        }

        return true;
    }

    public bool TrySpendAmmo(AmmoKind kind, int amount)
    {
        if (amount <= 0 || !HasAmmoForUse(kind, amount))
            return false;
        if (_owner?.HasInfiniteAmmo == true) return true;

        switch (kind)
        {
            case AmmoKind.Bullets:
                Bullets -= amount;
                break;
            case AmmoKind.Shells:
                Shells -= amount;
                break;
            case AmmoKind.Rockets:
                Rockets -= amount;
                break;
            default:
                Cells -= amount;
                break;
        }

        return true;
    }

    internal bool HasAmmoForUse(AmmoKind kind, int amount) =>
        _owner?.HasInfiniteAmmo == true || Ammo(kind) >= amount;
}
