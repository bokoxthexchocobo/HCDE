namespace HCDE.Playsim;

/// <summary>Weapon dispatch: paced melee/hitscan, seeded pellet spread and traveling projectiles.</summary>
public static class HitscanCombat
{
    public const double MeleeRange = 64;
    public const double HitscanRange = 8192; // PLAYERMISSILERANGE; monster hitscans use MISSILERANGE.

    public static bool Fire(AuthoritySimulation sim, PlayerPawn player)
    {
        if (player.IsDead || player.Destroyed || player.WeaponCooldown > 0 || !player.WeaponReady) return false;
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
            // GunShot / A_FireShotgun2 / A_Punch / A_Saw roll damage before spread.
            var damage = melee ? 2 * (1 + (int)(sim.NextCombatRandom() % 10))
                : 5 * (1 + (int)(sim.NextCombatRandom() % 3));
            var spreadScale = weapon == WeaponKind.Chainsaw ? 1 : 255.0 / 256;
            var spread = definition.SpreadDegrees == 0 ? 0 : sim.NextCombatSpread() * definition.SpreadDegrees * spreadScale;
            var pitchSpread = weapon == WeaponKind.SuperShotgun ? sim.NextCombatSpread() * (7.097 * 255 / 256) : 0;
            var target = CombatTrace.FindTarget(sim, player, melee ? MeleeRange : HitscanRange, spread, pitchSpread);
            if (target != null) ActorDamage.Apply(target, damage, player, inflictor: player);
        }
        return true;
    }
}
