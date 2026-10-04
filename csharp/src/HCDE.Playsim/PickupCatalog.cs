namespace HCDE.Playsim;

/// <summary>
/// Vanilla Doom pickups. A gift that does not change the player leaves the actor on the map.
/// Skull keys set the same color as the matching card.
/// </summary>
public static class PickupCatalog
{
    public const int BlueCard = 5;
    public const int YellowCard = 6;
    public const int RedCard = 13;
    public const int RedSkull = 38;
    public const int YellowSkull = 39;
    public const int BlueSkull = 40;
    public const int Stimpack = 2011;
    public const int Medikit = 2012;
    public const int Soulsphere = 2013;
    public const int HealthBonus = 2014;
    public const int ArmorBonus = 2015;
    public const int GreenArmor = 2018;
    public const int MegaArmor = 2019;
    public const int Shotgun = 2001;
    public const int Pistol = 5010;
    public const int Chaingun = 2002;
    public const int RocketLauncher = 2003;
    public const int PlasmaRifle = 2004;
    public const int Chainsaw = 2005;
    public const int Bfg = 2006;
    public const int Clip = 2007;
    public const int Shells = 2008;
    public const int Rocket = 2010;
    public const int Cell = 2047;
    public const int CellPack = 17;
    public const int ClipBox = 2048;
    public const int ShellBox = 2049;
    public const int RocketBox = 2046;
    public const int SuperShotgun = 82;
    public const int Backpack = 8;
    public const int Megasphere = 83;

    public static bool IsPickup(int doomEdNum) => TryDescribe(doomEdNum, out _);
    internal static WeaponKind PickupWeapon(int doomEdNum) =>
        TryDescribe(doomEdNum, out var gift) && gift.Kind == GiftKind.Weapon ? gift.Weapon : 0;

    /// <summary>Vanilla Doom bonus items carry INVENTORY.ALWAYSPICKUP.</summary>
    internal static bool AlwaysPickup(int doomEdNum) => doomEdNum is HealthBonus or ArmorBonus or Megasphere;

    /// <summary>Native Actor defaults, with Doom Backpack's height override.</summary>
    internal static Fixed HeightOf(int doomEdNum) => Fixed.FromInt(doomEdNum == Backpack ? 26 : 16);

    /// <summary>Native PIT_CheckThing horizontal pickup contact uses strict axis bounds.</summary>
    internal static bool IsWithinHorizontalReach(Actor toucher, Actor pickup)
    {
        var reach = toucher.Radius.ToDouble() + pickup.Radius.ToDouble();
        return Math.Abs(toucher.X.ToDouble() - pickup.X.ToDouble()) < reach
            && Math.Abs(toucher.Y.ToDouble() - pickup.Y.ToDouble()) < reach;
    }

    /// <summary>Native P_TouchSpecialThing vertical reach, including both boundaries.</summary>
    internal static bool IsWithinVerticalReach(Actor toucher, Actor pickup)
    {
        var delta = pickup.Z.ToDouble() - toucher.Z.ToDouble();
        return delta <= toucher.Height.ToDouble() && delta >= Math.Min(-32.0, -pickup.Height.ToDouble());
    }

    /// <summary>Native <c>PClass::FindActor</c> subset for ACS <c>DropItem</c> on catalog pickups.</summary>
    public static bool TryEditorNumberForDropName(string? typeName, out int doomEdNum)
    {
        doomEdNum = 0;
        if (string.IsNullOrWhiteSpace(typeName))
            return false;
        doomEdNum = typeName.ToUpperInvariant() switch
        {
            "CLIP" => Clip,
            "CLIPBOX" => ClipBox,
            "SHELL" => Shells,
            "SHELLBOX" => ShellBox,
            "ROCKETAMMO" => Rocket,
            "ROCKETBOX" => RocketBox,
            "CELL" => Cell,
            "CELLPACK" => CellPack,
            "STIMPACK" => Stimpack,
            "MEDIKIT" => Medikit,
            "HEALTHBONUS" => HealthBonus,
            "SOULSPHERE" => Soulsphere,
            "MEGASPHERE" => Megasphere,
            "GREENARMOR" => GreenArmor,
            "BLUEARMOR" => MegaArmor,
            "ARMORBONUS" => ArmorBonus,
            "BACKPACK" => Backpack,
            "BLUECARD" => BlueCard,
            "BLUESKULL" => BlueSkull,
            "REDCARD" => RedCard,
            "REDSKULL" => RedSkull,
            "YELLOWCARD" => YellowCard,
            "YELLOWSKULL" => YellowSkull,
            "CHAINSAW" => Chainsaw,
            "PISTOL" => Pistol,
            "SHOTGUN" => Shotgun,
            "SUPERSHOTGUN" => SuperShotgun,
            "CHAINGUN" => Chaingun,
            "ROCKETLAUNCHER" => RocketLauncher,
            "PLASMARIFLE" => PlasmaRifle,
            "BFG9000" => Bfg,
            _ => 0,
        };
        return doomEdNum != 0 && IsPickup(doomEdNum);
    }

    /// <summary>Card and skull of one color are the same key in this port.</summary>
    public static bool IsKey(int doomEdNum) =>
        doomEdNum is BlueCard or BlueSkull or YellowCard or YellowSkull or RedCard or RedSkull;

    /// <summary>Supported Doom map thing for a weapon drop. Fist has no pickup.</summary>
    public static bool TryWeaponEdNum(WeaponKind weapon, out int doomEdNum)
    {
        doomEdNum = weapon switch
        {
            WeaponKind.Chainsaw => Chainsaw,
            WeaponKind.Pistol => Pistol,
            WeaponKind.Shotgun => Shotgun,
            WeaponKind.SuperShotgun => SuperShotgun,
            WeaponKind.Chaingun => Chaingun,
            WeaponKind.RocketLauncher => RocketLauncher,
            WeaponKind.Plasma => PlasmaRifle,
            WeaponKind.Bfg => Bfg,
            _ => 0,
        };
        return doomEdNum != 0;
    }

    public static bool TryGive(
        PlayerPawn player,
        int doomEdNum,
        bool ignoreSkill = false,
        bool depleted = false,
        int pickupAmount = 0,
        bool suppressWeaponAmmo = false)
    {
        if (!TryDescribe(doomEdNum, out var gift))
            return false;
        var giftAmount = pickupAmount > 0 ? pickupAmount : gift.Amount;
        var ammo = ScaleAmmo(giftAmount, player, ignoreSkill);
        var armor = ScaleArmor(giftAmount, player, ignoreSkill);

        return gift.Kind switch
        {
            GiftKind.Health => GiveHealth(player, giftAmount, gift.Maximum),
            GiftKind.Megasphere => GiveMegasphere(player),
            GiftKind.ArmorBonus => GiveArmorBonus(player.Inventory, armor),
            GiftKind.GreenArmor => GiveArmor(player.Inventory, armor, PlayerInventory.GreenSavePercent, "GreenArmor"),
            GiftKind.MegaArmor => GiveArmor(player.Inventory, armor, PlayerInventory.MegaSavePercent, "BlueArmor"),
            GiftKind.Ammo => GiveAmmo(player.Inventory, gift.Ammo, suppressWeaponAmmo ? 0 : ammo),
            GiftKind.Key => GiveKey(player.Inventory, gift.Key),
            GiftKind.Weapon => GiveWeapon(player.Inventory, gift.Weapon, gift.Ammo,
                ScaleWeaponAmmo(giftAmount, player, player.Inventory.Owns(gift.Weapon), ignoreSkill),
                gift.Amount > 0 && !suppressWeaponAmmo),
            GiftKind.Backpack => player.Inventory.GiveBackpack(
                ScaleAmmo(10, player, ignoreSkill),
                ScaleAmmo(4, player, ignoreSkill),
                ScaleAmmo(1, player, ignoreSkill),
                ScaleAmmo(20, player, ignoreSkill),
                depleted),
            _ => false,
        };
    }

    /// <summary>
    /// Baby and nightmare ammo factor 2. Other Doom skills stay 1.
    /// <c>sv_doubleammo</c> replaces that factor with 2. <c>sv_ammofactor</c> multiplies the result.
    /// Truncates toward zero.
    /// </summary>
    public static int ScaleAmmo(int amount, PlayerPawn player, bool ignoreSkill = false)
    {
        if (amount <= 0 || ignoreSkill)
            return amount;
        var skill = player.Simulation?.Skill ?? 2;
        var skillFactor = player.Simulation?.DoubleAmmo == true || skill is 0 or 4 ? 2.0 : 1.0;
        var extra = player.Simulation?.AmmoFactor ?? 1;
        var scaled = (int)(amount * skillFactor * extra);
        return scaled < 0 ? 0 : scaled;
    }

    internal static int ScaleWeaponAmmo(int amount, PlayerPawn player, bool owned, bool ignoreSkill = false)
    {
        if (!owned && player.Simulation is { GameMode: SpawnGameMode.Deathmatch, NoExtraAmmo: false })
            amount = (int)Math.Clamp((long)amount * 5 / 2, int.MinValue, int.MaxValue);
        return ScaleAmmo(amount, player, ignoreSkill);
    }

    internal static int ScaleArmor(int amount, PlayerPawn player, bool ignoreSkill = false) =>
        ignoreSkill ? amount : ScaleArmorAmount(amount, player.Simulation?.ArmorFactor ?? 1);

    internal static int ScaleArmorAmount(int amount, double factor) =>
        (int)Math.Clamp(amount * factor, int.MinValue, int.MaxValue);

    /// <summary>Vanilla Ammo/Weapon.ModifyDropAmount with the default drop factor.</summary>
    internal static int DropPickupAmount(int doomEdNum, int requestedAmount, double dropFactor = -1)
    {
        if (!TryDescribe(doomEdNum, out var gift))
            return requestedAmount;
        var factor = dropFactor == -1 ? 0.5 : dropFactor;
        int Scale(int amount) => (int)Math.Clamp(amount * factor, 0, int.MaxValue);
        return gift.Kind switch
        {
            GiftKind.Ammo => requestedAmount > 0
                ? dropFactor == -1 ? requestedAmount : Scale(requestedAmount)
                : Math.Max(1, Scale(gift.Amount)),
            // Weapon inventory amount does not override its ammo grant.
            GiftKind.Weapon => Scale(gift.Amount),
            _ => requestedAmount,
        };
    }

    private static bool GiveMegasphere(PlayerPawn player)
    {
        var armor = GiveArmor(player.Inventory, ScaleArmor(200, player), PlayerInventory.MegaSavePercent, "BlueArmorForMegasphere");
        var health = GiveHealth(player, 200, 200);
        return armor || health;
    }

    internal static bool GiveHealth(Actor actor, int amount)
    {
        if (actor is PlayerPawn player) return GiveHealth(player, amount);
        if (amount <= 0 || actor.Health <= 0 || actor.Health >= actor.ResurrectionHealth)
            return false;
        actor.Health += Math.Min(Math.Min(amount, 65536), actor.ResurrectionHealth - actor.Health);
        return true;
    }

    internal static bool GiveHealth(PlayerPawn player, int amount, int maximum = 0)
    {
        if (maximum <= 0) maximum = player.EffectiveMaxHealth;
        if (amount <= 0 || player.Health <= 0 || player.Health >= maximum)
            return false;
        var scaled = Math.Min(amount, 65536) * (player.Simulation?.HealthFactor ?? 1);
        var healing = (int)Math.Clamp(scaled, 1, int.MaxValue);
        player.Health += Math.Min(healing, maximum - player.Health);
        return true;
    }

    private static bool GiveArmorBonus(PlayerInventory inventory, int amount)
    {
        // Native bonus Use consumes a nonpositive scaled grant without changing armor.
        if (amount <= 0) return true;
        if (inventory.Armor >= PlayerInventory.MaxHealthBonus)
            return false;
        if (inventory.Armor <= 0)
        {
            inventory.Armor = 0;
            inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
            inventory.MaxAbsorb = 0;
            inventory.MaxFullAbsorb = 0;
            inventory.ArmorActualSaveAmount = PlayerInventory.MaxHealthBonus;
            inventory.ArmorType = "ArmorBonus";
        }
        inventory.Armor += Math.Min(amount, PlayerInventory.MaxHealthBonus - inventory.Armor);
        inventory.ArmorMaximum = Math.Max(inventory.ArmorMaximum, PlayerInventory.MaxHealthBonus);
        return true;
    }

    internal static bool GiveArmor(PlayerInventory inventory, int amount, int savePercent, string armorType)
    {
        if (inventory.Armor >= amount)
            return false;
        inventory.Armor = amount;
        inventory.ArmorMaximum = amount;
        inventory.ArmorActualSaveAmount = amount;
        inventory.ArmorType = armorType;
        inventory.ArmorSavePercent = savePercent;
        // Doom green and mega leave both caps at 0. AbsorbCount stays.
        inventory.MaxAbsorb = 0;
        inventory.MaxFullAbsorb = 0;
        return true;
    }

    private static bool GiveKey(PlayerInventory inventory, KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Red when inventory.RedKey:
            case KeyColor.Blue when inventory.BlueKey:
            case KeyColor.Yellow when inventory.YellowKey:
                return false;
            case KeyColor.Red:
                inventory.RedKey = true;
                return true;
            case KeyColor.Blue:
                inventory.BlueKey = true;
                return true;
            default:
                inventory.YellowKey = true;
                return true;
        }
    }

    private static bool GiveWeapon(PlayerInventory inventory, WeaponKind weapon, AmmoKind ammo, int amount, bool givesAmmo)
    {
        var owned = inventory.Owns(weapon);
        if (!owned)
        {
            inventory.Weapons |= weapon;
            if (!inventory.NeverAutoSwitch) inventory.Pending = weapon;
        }
        var added = givesAmmo && GiveAmmo(inventory, ammo, amount);
        return !owned || added;
    }

    private static bool GiveAmmo(PlayerInventory inventory, AmmoKind ammo, int amount)
    {
        var wasEmpty = inventory.Ammo(ammo) == 0;
        var added = inventory.TryAddAmmo(ammo, amount);
        if (added && wasEmpty) inventory.CheckAmmoPickupSwitch(ammo);
        return added;
    }

    private static bool TryDescribe(int doomEdNum, out Gift gift)
    {
        gift = doomEdNum switch
        {
            Stimpack => new Gift(GiftKind.Health, 10, 0),
            Medikit => new Gift(GiftKind.Health, 25, 0),
            HealthBonus => new Gift(GiftKind.Health, 1, PlayerInventory.MaxHealthBonus),
            Soulsphere => new Gift(GiftKind.Health, 100, PlayerInventory.MaxHealthBonus),
            ArmorBonus => new Gift(GiftKind.ArmorBonus, 1, PlayerInventory.MaxHealthBonus),
            GreenArmor => new Gift(GiftKind.GreenArmor, PlayerInventory.GreenArmorAmount, PlayerInventory.GreenSavePercent),
            MegaArmor => new Gift(GiftKind.MegaArmor, PlayerInventory.MegaArmorAmount, PlayerInventory.MegaSavePercent),
            Clip => new Gift(GiftKind.Ammo, AmmoKind.Bullets, 10),
            ClipBox => new Gift(GiftKind.Ammo, AmmoKind.Bullets, 50),
            Shells => new Gift(GiftKind.Ammo, AmmoKind.Shells, 4),
            ShellBox => new Gift(GiftKind.Ammo, AmmoKind.Shells, 20),
            Rocket => new Gift(GiftKind.Ammo, AmmoKind.Rockets, 1),
            RocketBox => new Gift(GiftKind.Ammo, AmmoKind.Rockets, 5),
            Cell => new Gift(GiftKind.Ammo, AmmoKind.Cells, 20),
            CellPack => new Gift(GiftKind.Ammo, AmmoKind.Cells, 100),
            BlueCard or BlueSkull => new Gift(GiftKind.Key, KeyColor.Blue),
            YellowCard or YellowSkull => new Gift(GiftKind.Key, KeyColor.Yellow),
            RedCard or RedSkull => new Gift(GiftKind.Key, KeyColor.Red),
            Chainsaw => new Gift(GiftKind.Weapon, WeaponKind.Chainsaw, AmmoKind.Bullets, 0),
            Pistol => new Gift(GiftKind.Weapon, WeaponKind.Pistol, AmmoKind.Bullets, 20),
            Shotgun => new Gift(GiftKind.Weapon, WeaponKind.Shotgun, AmmoKind.Shells, 8),
            SuperShotgun => new Gift(GiftKind.Weapon, WeaponKind.SuperShotgun, AmmoKind.Shells, 8),
            Chaingun => new Gift(GiftKind.Weapon, WeaponKind.Chaingun, AmmoKind.Bullets, 20),
            RocketLauncher => new Gift(GiftKind.Weapon, WeaponKind.RocketLauncher, AmmoKind.Rockets, 2),
            PlasmaRifle => new Gift(GiftKind.Weapon, WeaponKind.Plasma, AmmoKind.Cells, 40),
            Bfg => new Gift(GiftKind.Weapon, WeaponKind.Bfg, AmmoKind.Cells, 40),
            Backpack => new Gift(GiftKind.Backpack, 0, 0),
            Megasphere => new Gift(GiftKind.Megasphere, 1, 0),
            _ => default,
        };
        return gift.Kind != GiftKind.None;
    }

    private enum GiftKind
    {
        None,
        Health,
        ArmorBonus,
        GreenArmor,
        MegaArmor,
        Ammo,
        Key,
        Weapon,
        Backpack,
        Megasphere,
    }

    public enum KeyColor
    {
        Red,
        Blue,
        Yellow,
    }

    private readonly struct Gift
    {
        public Gift(GiftKind kind, int amount, int maximum)
        {
            Kind = kind;
            Amount = amount;
            Maximum = maximum;
        }

        public Gift(GiftKind kind, AmmoKind ammo, int amount)
        {
            Kind = kind;
            Ammo = ammo;
            Amount = amount;
        }

        public Gift(GiftKind kind, KeyColor key)
        {
            Kind = kind;
            Key = key;
        }

        public Gift(GiftKind kind, WeaponKind weapon, AmmoKind ammo, int amount)
        {
            Kind = kind;
            Weapon = weapon;
            Ammo = ammo;
            Amount = amount;
        }

        public GiftKind Kind { get; }
        public int Amount { get; }
        public int Maximum { get; }
        public AmmoKind Ammo { get; }
        public KeyColor Key { get; }
        public WeaponKind Weapon { get; }
    }
}
