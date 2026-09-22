namespace HCDE.Playsim;

/// <summary>
/// Fixed-damage pistol, shotgun, plasma, and melee. No random spread and no rockets.
/// Armor on a player absorbs <c>damage * savePercent / 100</c>, limited by the armor pool.
/// </summary>
public static class HitscanCombat
{
    public const int MeleeDamage = 10;
    public const int BulletDamage = 10;
    public const int ShotgunDamage = 30;
    public const int PlasmaDamage = 20;
    public const double MeleeRange = 64;
    public const double HitscanRange = 2048;

    public static bool Fire(AuthoritySimulation sim, PlayerPawn player)
    {
        var weapon = player.Inventory.Selected;
        if (!player.Inventory.Owns(weapon))
            return false;

        if (IsMelee(weapon))
        {
            var target = FindTarget(sim, player, MeleeRange, melee: true);
            if (target != null)
                ApplyDamage(target, MeleeDamage);
            return target != null;
        }

        if (!TryAmmo(weapon, out var ammo, out var damage))
            return false;
        if (!player.Inventory.TrySpendAmmo(ammo, 1))
            return false;

        var hit = FindTarget(sim, player, HitscanRange, melee: false);
        if (hit != null)
            ApplyDamage(hit, damage);
        return true;
    }

    private static bool IsMelee(WeaponKind weapon) =>
        weapon is WeaponKind.Fist or WeaponKind.Chainsaw;

    private static bool TryAmmo(WeaponKind weapon, out AmmoKind ammo, out int damage)
    {
        switch (weapon)
        {
            case WeaponKind.Pistol:
            case WeaponKind.Chaingun:
                ammo = AmmoKind.Bullets;
                damage = BulletDamage;
                return true;
            case WeaponKind.Shotgun:
            case WeaponKind.SuperShotgun:
                ammo = AmmoKind.Shells;
                damage = ShotgunDamage;
                return true;
            case WeaponKind.Plasma:
            case WeaponKind.Bfg:
                ammo = AmmoKind.Cells;
                damage = PlasmaDamage;
                return true;
            default:
                ammo = default;
                damage = 0;
                return false;
        }
    }

    private static Actor? FindTarget(AuthoritySimulation sim, PlayerPawn player, double range, bool melee)
    {
        var radians = player.Angle.Raw * (Math.PI / 0x80000000u);
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        var originX = player.X.ToDouble();
        var originY = player.Y.ToDouble();
        Actor? best = null;
        var bestDistance = double.PositiveInfinity;
        foreach (var actor in sim.Actors)
        {
            if (ReferenceEquals(actor, player) || !actor.BlocksActors)
                continue;

            var rx = actor.X.ToDouble() - originX;
            var ry = actor.Y.ToDouble() - originY;
            var along = rx * dx + ry * dy;
            if (along <= 0)
                continue;

            if (melee)
            {
                var reach = range + actor.Radius.ToDouble();
                var distanceSquared = rx * rx + ry * ry;
                if (distanceSquared > reach * reach || along >= bestDistance)
                    continue;
                best = actor;
                bestDistance = along;
                continue;
            }

            if (along > range)
                continue;
            var sideX = rx - along * dx;
            var sideY = ry - along * dy;
            var radius = actor.Radius.ToDouble();
            if (sideX * sideX + sideY * sideY > radius * radius || along >= bestDistance)
                continue;
            best = actor;
            bestDistance = along;
        }

        return best;
    }

    private static void ApplyDamage(Actor target, int damage)
    {
        var healthDamage = damage;
        if (target is PlayerPawn player && player.Inventory.Armor > 0 && player.Inventory.ArmorSavePercent > 0)
        {
            var saved = Math.Min(player.Inventory.Armor, damage * player.Inventory.ArmorSavePercent / 100);
            player.Inventory.Armor -= saved;
            healthDamage = damage - saved;
        }

        target.Health = Math.Max(0, target.Health - healthDamage);
    }
}
