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
        var range = melee ? player.MeleeRange.ToDouble() + 20
            + (weapon == WeaponKind.Chainsaw ? 1.0 / Fixed.Unit : 0) : HitscanRange;
        for (var pellet = 0; pellet < definition.Pellets; pellet++)
        {
            // GunShot / A_FireShotgun2 / A_Punch / A_Saw roll damage before spread.
            var damage = melee ? 2 * (1 + (int)(sim.NextCombatRandom() % 10))
                : 5 * (1 + (int)(sim.NextCombatRandom() % 3));
            var spreadScale = weapon == WeaponKind.Chainsaw ? 1 : 255.0 / 256;
            var spread = definition.SpreadDegrees == 0 ? 0 : sim.NextCombatSpread() * definition.SpreadDegrees * spreadScale;
            var pitchSpread = weapon == WeaponKind.SuperShotgun ? sim.NextCombatSpread() * (7.097 * 255 / 256) : 0;
            var hit = CombatTrace.TraceLineAttack(sim, player,
                BamAngle.FromDegrees(player.Angle.ToDegrees() + spread),
                BamAngle.FromDegrees(player.PitchDegrees + pitchSpread), range);
            if (hit.Victim is { } target)
            {
                var facing = BamAngle.FromDegrees(player.AngleTo(target));
                ActorDamage.Apply(target, damage, player, damageType: melee ? "Melee" : "Hitscan", inflictor: player);
                if (weapon == WeaponKind.Fist) player.Angle = facing;
                else if (weapon == WeaponKind.Chainsaw)
                {
                    var difference = Actor.Normalize180(facing.ToDegrees() - player.Angle.ToDegrees());
                    var angle = difference < 0
                        ? difference < -4.5 ? facing.ToDegrees() + 90.0 / 21 : player.Angle.ToDegrees() - 4.5
                        : difference > 4.5 ? facing.ToDegrees() - 90.0 / 21 : player.Angle.ToDegrees() + 4.5;
                    player.Angle = BamAngle.FromDegrees(angle);
                }
            }
            else GeometryLineAttack.Apply(sim, hit, damage);
        }
        return true;
    }
}
