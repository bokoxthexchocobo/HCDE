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
public sealed class PlayerInventory
{
    public const int MaxHealthBonus = 200;
    public const int GreenArmorAmount = 100;
    public const int MegaArmorAmount = 200;
    public const int GreenSavePercent = 33;
    public const int MegaSavePercent = 50;

    public int Armor { get; set; }
    public int ArmorSavePercent { get; set; }
    public int Bullets { get; set; } = 50;
    public int Shells { get; set; }
    public int Rockets { get; set; }
    public int Cells { get; set; }
    public int MaxBullets { get; set; } = 200;
    public int MaxShells { get; set; } = 50;
    public int MaxRockets { get; set; } = 50;
    public int MaxCells { get; set; } = 300;
    public bool RedKey { get; set; }
    public bool BlueKey { get; set; }
    public bool YellowKey { get; set; }
    public WeaponKind Weapons { get; set; } = WeaponKind.Fist | WeaponKind.Pistol;
    public WeaponKind Selected { get; set; } = WeaponKind.Pistol;

    public bool Owns(WeaponKind weapon) => (Weapons & weapon) != 0;

    public int Ammo(AmmoKind kind) => kind switch
    {
        AmmoKind.Bullets => Bullets,
        AmmoKind.Shells => Shells,
        AmmoKind.Rockets => Rockets,
        _ => Cells,
    };

    public bool TryAddAmmo(AmmoKind kind, int amount)
    {
        if (amount <= 0)
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

        var next = Math.Min(max, current + amount);
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
        if (amount <= 0 || Ammo(kind) < amount)
            return false;

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
}
