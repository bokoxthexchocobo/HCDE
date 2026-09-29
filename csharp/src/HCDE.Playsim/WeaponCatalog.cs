namespace HCDE.Playsim;

public sealed record WeaponDefinition(AmmoKind? Ammo, int AmmoUse, int RefireTics,
    int Pellets = 1, double SpreadDegrees = 0, ProjectileKind? Projectile = null);

/// <summary>Managed firing cadence and ammo rules. Raise and lower gate firing; psprite sprites are not reproduced.</summary>
public static class WeaponCatalog
{
    public static WeaponDefinition? Find(WeaponKind weapon) => weapon switch
    {
        WeaponKind.Fist => new(null, 0, 22, SpreadDegrees: 5.625),
        WeaponKind.Chainsaw => new(null, 0, 4, SpreadDegrees: 2.8125),
        WeaponKind.Pistol => new(AmmoKind.Bullets, 1, 19),
        WeaponKind.Chaingun => new(AmmoKind.Bullets, 1, 4, SpreadDegrees: 5.625),
        WeaponKind.Shotgun => new(AmmoKind.Shells, 1, 35, 7, 5.625),
        WeaponKind.SuperShotgun => new(AmmoKind.Shells, 2, 57, 20, 11.25),
        WeaponKind.RocketLauncher => new(AmmoKind.Rockets, 1, 20, Projectile: ProjectileKind.Rocket),
        WeaponKind.Plasma => new(AmmoKind.Cells, 1, 3, Projectile: ProjectileKind.Plasma),
        WeaponKind.Bfg => new(AmmoKind.Cells, 40, 30, Projectile: ProjectileKind.Bfg),
        _ => null,
    };
}
