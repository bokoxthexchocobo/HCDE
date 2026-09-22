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

    public static bool IsPickup(int doomEdNum) => TryDescribe(doomEdNum, out _);

    public static bool TryGive(PlayerPawn player, int doomEdNum)
    {
        if (!TryDescribe(doomEdNum, out var gift))
            return false;

        return gift.Kind switch
        {
            GiftKind.Health => GiveHealth(player, gift.Amount, gift.Maximum),
            GiftKind.ArmorBonus => GiveArmorBonus(player.Inventory),
            GiftKind.GreenArmor => GiveArmor(player.Inventory, PlayerInventory.GreenArmorAmount, PlayerInventory.GreenSavePercent),
            GiftKind.MegaArmor => GiveArmor(player.Inventory, PlayerInventory.MegaArmorAmount, PlayerInventory.MegaSavePercent),
            GiftKind.Ammo => player.Inventory.TryAddAmmo(gift.Ammo, gift.Amount),
            GiftKind.Key => GiveKey(player.Inventory, gift.Key),
            GiftKind.Weapon => GiveWeapon(player.Inventory, gift.Weapon, gift.Ammo, gift.Amount),
            _ => false,
        };
    }

    private static bool GiveHealth(PlayerPawn player, int amount, int maximum)
    {
        if (player.Health >= maximum)
            return false;
        player.Health = Math.Min(maximum, player.Health + amount);
        return true;
    }

    private static bool GiveArmorBonus(PlayerInventory inventory)
    {
        if (inventory.Armor >= PlayerInventory.MaxHealthBonus)
            return false;
        inventory.Armor = Math.Min(PlayerInventory.MaxHealthBonus, inventory.Armor + 1);
        if (inventory.ArmorSavePercent == 0)
            inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        return true;
    }

    private static bool GiveArmor(PlayerInventory inventory, int amount, int savePercent)
    {
        if (inventory.Armor >= amount)
            return false;
        inventory.Armor = amount;
        inventory.ArmorSavePercent = savePercent;
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

    private static bool GiveWeapon(PlayerInventory inventory, WeaponKind weapon, AmmoKind ammo, int amount)
    {
        var owned = inventory.Owns(weapon);
        if (!owned)
            inventory.Weapons |= weapon;
        var added = amount > 0 && inventory.TryAddAmmo(ammo, amount);
        return !owned || added;
    }

    private static bool TryDescribe(int doomEdNum, out Gift gift)
    {
        gift = doomEdNum switch
        {
            Stimpack => new Gift(GiftKind.Health, 10, 100),
            Medikit => new Gift(GiftKind.Health, 25, 100),
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
            Shotgun => new Gift(GiftKind.Weapon, WeaponKind.Shotgun, AmmoKind.Shells, 8),
            SuperShotgun => new Gift(GiftKind.Weapon, WeaponKind.SuperShotgun, AmmoKind.Shells, 8),
            Chaingun => new Gift(GiftKind.Weapon, WeaponKind.Chaingun, AmmoKind.Bullets, 20),
            RocketLauncher => new Gift(GiftKind.Weapon, WeaponKind.RocketLauncher, AmmoKind.Rockets, 2),
            PlasmaRifle => new Gift(GiftKind.Weapon, WeaponKind.Plasma, AmmoKind.Cells, 40),
            Bfg => new Gift(GiftKind.Weapon, WeaponKind.Bfg, AmmoKind.Cells, 40),
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
    }

    private enum KeyColor
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
