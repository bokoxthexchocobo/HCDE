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

    // Default Doom slots from doomplayer.zs. Custom class/slot tables are not loaded yet.
    private static readonly WeaponKind[][] Slots = [[], [WeaponKind.Fist, WeaponKind.Chainsaw],
        [WeaponKind.Pistol], [WeaponKind.Shotgun, WeaponKind.SuperShotgun], [WeaponKind.Chaingun],
        [WeaponKind.RocketLauncher], [WeaponKind.Plasma], [WeaponKind.Bfg], [], []];
    private static readonly WeaponKind[] Cycle = Slots.SelectMany(slot => slot).ToArray();

    public void SelectSlot(byte slot)
    {
        // WST_NONE=10, WST_PREV=11, WST_NEXT=12 in g_game.h.
        if (slot is 11 or 12)
        {
            var current = Array.IndexOf(Cycle, Selected);
            if (current < 0) return;
            var direction = slot == 12 ? 1 : -1;
            for (var step = 1; step <= Cycle.Length; step++)
            {
                var weapon = Cycle[(current + step * direction + Cycle.Length) % Cycle.Length];
                if (CanSelect(weapon)) { Selected = weapon; return; }
            }
            return;
        }
        if (slot >= Slots.Length) return;
        var weapons = Slots[slot];
        var index = Array.IndexOf(weapons, Selected);
        // Repeated presses cycle backwards in the slot; entry prefers its last weapon.
        for (var step = 1; step <= weapons.Length; step++)
        {
            var weapon = weapons[((index < 0 ? 0 : index) - step + weapons.Length) % weapons.Length];
            if (CanSelect(weapon)) { Selected = weapon; return; }
        }
    }

    private bool CanSelect(WeaponKind weapon)
    {
        var definition = WeaponCatalog.Find(weapon);
        return Owns(weapon) && definition != null
            && (definition.Ammo is not { } ammo || Ammo(ammo) >= definition.AmmoUse);
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
