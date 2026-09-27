namespace HCDE.Playsim;

/// <summary>Weapon dispatch: paced melee/hitscan, seeded pellet spread and traveling projectiles.</summary>
public static class HitscanCombat
{
    public const int MeleeDamage = 10;
    public const int BulletDamage = 10;
    public const double MeleeRange = 64;
    public const double HitscanRange = 2048;

    public static bool Fire(AuthoritySimulation sim, PlayerPawn player)
    {
        if (player.IsDead || player.Destroyed || player.WeaponCooldown > 0) return false;
        var weapon = player.Inventory.Selected;
        var definition = WeaponCatalog.Find(weapon);
        if (definition == null || !player.Inventory.Owns(weapon)) return false;
        if (definition.Ammo is { } ammo && !player.Inventory.TrySpendAmmo(ammo, definition.AmmoUse)) return false;
        player.WeaponCooldown = definition.RefireTics;
        SoundPropagation.Alert(sim, player);
        if (definition.Projectile is { } projectile)
        {
            sim.SpawnProjectile(player, projectile);
            return true;
        }
        var melee = weapon is WeaponKind.Fist or WeaponKind.Chainsaw;
        for (var pellet = 0; pellet < definition.Pellets; pellet++)
        {
            var spread = definition.SpreadDegrees == 0 ? 0 : sim.NextCombatSpread() * definition.SpreadDegrees;
            var target = CombatTrace.FindTarget(sim, player, melee ? MeleeRange : HitscanRange, spread);
            var damage = definition.Pellets > 1 ? 5 * (1 + (int)(sim.NextCombatRandom() % 3)) : melee ? MeleeDamage : BulletDamage;
            if (target != null) ActorDamage.Apply(target, damage, player);
        }
        return true;
    }
}
