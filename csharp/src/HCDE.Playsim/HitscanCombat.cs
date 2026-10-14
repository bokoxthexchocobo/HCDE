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
        ActorNoiseActions.WeaponAlert(player);
        if (weapon == WeaponKind.Chainsaw)
        {
            ChainsawActions.Attack(player);
            return true;
        }
        if (definition.Projectile is { } projectile)
        {
            sim.SpawnProjectile(player, projectile);
            return true;
        }
        var melee = weapon == WeaponKind.Fist;
        var range = melee ? player.MeleeRange.ToDouble() + 20 : HitscanRange;
        for (var pellet = 0; pellet < definition.Pellets; pellet++)
        {
            // Native weapon actions roll damage before spread.
            var damage = melee ? sim.NextPunchDamage()
                : 5 * (1 + (int)(sim.NextCombatRandom() % 3));
            if (melee && player.HasPowerStrength) damage = unchecked(damage * 10);
            var spread = melee ? sim.NextPunchSpread()
                : definition.SpreadDegrees == 0 ? 0 : sim.NextCombatSpread() * definition.SpreadDegrees * (255.0 / 256);
            var pitchSpread = weapon == WeaponKind.SuperShotgun ? sim.NextCombatSpread() * (7.097 * 255 / 256) : 0;
            var angle = BamAngle.FromDegrees(player.Angle.ToDegrees() + spread);
            var pitch = BamAngle.FromDegrees(player.PitchDegrees + pitchSpread);
            var hit = !melee && sim.Rewind.TryTraceHistoricalHitscan(sim, player, angle, pitch, range, out var historicalHit)
                ? historicalHit : CombatTrace.TraceLineAttack(sim, player, angle, pitch, range);
            if (hit.Victim is { } target)
            {
                var facing = BamAngle.FromDegrees(player.AngleTo(target));
                LineAttackImpact.ApplyActorDamage(sim, player, target, hit, angle, damage,
                    melee ? "Melee" : "Hitscan", melee: weapon == WeaponKind.Fist);
                if (weapon == WeaponKind.Fist)
                {
                    ActorSoundActions.StartSound(player, "*fist", channel: 1);
                    player.Angle = facing;
                }
            }
            else
            {
                GeometryLineAttack.Apply(sim, hit, damage);
                LineAttackImpact.SpawnGeometryPuff(sim, player, hit, angle, pitch, melee: weapon == WeaponKind.Fist);
            }
        }
        return true;
    }
}
