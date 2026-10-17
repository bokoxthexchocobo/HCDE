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
        var fireSound = weapon switch
        {
            WeaponKind.Pistol => "weapons/pistol",
            WeaponKind.Shotgun => "weapons/shotgf",
            WeaponKind.Chaingun => "weapons/chngun",
            WeaponKind.SuperShotgun => "weapons/sshotf",
            _ => null,
        };
        if (fireSound is not null) ActorSoundActions.PlayWeaponSound(player, fireSound);
        var melee = weapon == WeaponKind.Fist;
        var range = melee ? player.MeleeRange.ToDouble() + 20 : HitscanRange;
        var gunSpread = weapon is WeaponKind.Pistol or WeaponKind.Chaingun
            ? player.WeaponRefiring ? 5.625 : 0 : definition.SpreadDegrees;
        for (var pellet = 0; pellet < definition.Pellets; pellet++)
        {
            // Native weapon actions roll damage before spread.
            var damage = melee ? sim.NextPunchDamage() : weapon == WeaponKind.SuperShotgun ? sim.NextFireSg2Damage()
                : sim.NextGunShotDamage();
            if (melee && player.HasPowerStrength) damage = unchecked(damage * 10);
            var spread = melee ? sim.NextPunchSpread() : weapon == WeaponKind.SuperShotgun ? sim.NextFireSg2Spread(11.25)
                : gunSpread == 0 ? 0 : sim.NextGunShotSpread(gunSpread);
            var pitchSpread = weapon == WeaponKind.SuperShotgun ? sim.NextFireSg2Spread(7.097)
                : !melee && gunSpread != 0 && sim.VerticalWeaponSpread ? sim.NextGunShotSpread(3.549) : 0;
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
