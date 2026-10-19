namespace HCDE.Playsim;

/// <summary>Native <c>p_map.cpp</c> <c>P_GetRadiusDamage</c> / <c>P_RadiusAttack</c> flat subset.</summary>
public static class MapRadiusAttack
{
    public static int GetRadiusDamage(Actor bombSpot, Actor victim, int damage, double distance, double fullDamageDistance,
        bool oldRadius, bool circular, bool fromAction = true, bool victimIsBombSource = false, double splashFactor = 1)
    {
        if (distance <= 0) distance = damage;
        fullDamageDistance = Math.Clamp(fullDamageDistance, 0, Math.Max(0, distance - 1));
        if (ReferenceEquals(bombSpot, victim)) return damage;
        var points = oldRadius
            ? GetOldRadiusDamage(bombSpot, victim, damage, distance, fullDamageDistance, fromAction, splashFactor)
            : GetModernRadiusDamage(bombSpot, victim, damage, distance, fullDamageDistance, circular, fromAction, victimIsBombSource, splashFactor);
        return points < 0 ? 0 : (int)points;
    }

    [Flags]
    public enum RadiusAttackFlags
    {
        None = 0,
        HurtSource = 1,
        OldRadiusDamage = 2,
        Circular = 4,
    }

    public static int Apply(AuthoritySimulation sim, Actor bombSpot, Actor? bombSource, int damage, double distance,
        string damageType, double fullDamageDistance = 0, bool hurtSource = false, bool oldRadius = false, bool circular = false)
        => Apply(sim, bombSpot, bombSource, damage, distance, damageType, fullDamageDistance,
            hurtSource ? RadiusAttackFlags.HurtSource : RadiusAttackFlags.None
            | (oldRadius ? RadiusAttackFlags.OldRadiusDamage : RadiusAttackFlags.None)
            | (circular ? RadiusAttackFlags.Circular : RadiusAttackFlags.None));

    public static int Apply(AuthoritySimulation sim, Actor bombSpot, Actor? bombSource, int damage, double distance,
        string damageType, double fullDamageDistance, RadiusAttackFlags flags)
    {
        if (distance <= 0) distance = damage;
        var hurtSource = (flags & RadiusAttackFlags.HurtSource) != 0;
        var oldRadius = (flags & RadiusAttackFlags.OldRadiusDamage) != 0;
        var circular = (flags & RadiusAttackFlags.Circular) != 0;
        bombSource ??= bombSpot;
        var applied = 0;
        foreach (var victim in sim.Actors.ToArray())
        {
            if (victim.Destroyed || !victim.IsBlockmapActor) continue;
            if (!hurtSource && (ReferenceEquals(victim, bombSource) || ReferenceEquals(victim, bombSpot))) continue;
            if (victim.NoRadiusDamage) continue;
            if (victim.SplashImmune(bombSpot)) continue;
            if (victim.RadiusSourceImmune(bombSource)) continue;
            int amount;
            if (oldRadius)
            {
                amount = GetOldRadiusDamage(sim, bombSpot, victim, damage, distance, fullDamageDistance, sim.SplashFactor);
                if (amount <= 0) continue;
            }
            else
            {
                amount = (int)GetModernRadiusDamage(bombSpot, victim, damage, distance, fullDamageDistance, circular,
                    fromAction: false, ReferenceEquals(victim, bombSource), sim.SplashFactor);
                if (amount <= 0) continue;
                if (!CombatTrace.CheckSight(sim, victim, bombSpot, ActorSightFlags.IgnoreVisibility)) continue;
            }
            if (victim.CanTakeDamage)
            {
                ActorDamage.Apply(victim, amount, bombSource, damageType: damageType, inflictor: bombSpot);
                applied++;
            }
        }
        return applied;
    }

    private static double GetModernRadiusDamage(Actor bombSpot, Actor victim, int damage, double distance,
        double fullDamageDistance, bool circular, bool fromAction, bool victimIsBombSource, double splashFactor)
    {
        var bombDistanceFloat = 1.0 / (distance - fullDamageDistance);
        double len;
        if (!circular)
        {
            var offset = bombSpot.Vec2To(victim);
            var dx = Math.Abs(offset.X);
            var dy = Math.Abs(offset.Y);
            var boxRadius = victim.Radius.ToDouble();
            len = Math.Max(dx, dy);
            var bombZ = bombSpot.Z.ToDouble();
            var top = victim.Top();
            if (bombZ < victim.Z.ToDouble() || bombZ >= top)
            {
                var dz = bombZ > victim.Z.ToDouble() ? bombZ - top : victim.Z.ToDouble() - bombZ;
                len = len <= boxRadius ? dz : Math.Sqrt(Math.Max(0, len - boxRadius) * Math.Max(0, len - boxRadius) + dz * dz);
            }
            else
            {
                len = Math.Max(0, len - boxRadius);
            }
        }
        else
        {
            len = bombSpot.Distance3D(victim);
        }
        len = Math.Clamp(len - fullDamageDistance, 0, len);
        var points = damage * (1.0 - len * bombDistanceFloat);
        if (!fromAction)
        {
            if (victimIsBombSource) points *= splashFactor;
            points *= victim.RadiusDamageFactor;
        }
        return points;
    }

    private static int GetOldRadiusDamage(Actor bombSpot, Actor victim, int damage, double distance,
        double fullDamageDistance, bool fromAction, double splashFactor)
    {
        var offset = bombSpot.Vec2To(victim);
        var dist = Math.Max(Math.Abs(offset.X), Math.Abs(offset.Y)) - victim.Radius.ToDouble();
        if (dist < 0) dist = 0;
        if (dist >= distance) return fromAction ? 0 : -1;
        dist = Math.Clamp(dist - fullDamageDistance, 0, dist);
        var scaled = (int)(damage * (distance - dist) / distance);
        if (!fromAction) scaled = (int)(scaled * splashFactor * victim.RadiusDamageFactor);
        return scaled;
    }

    private static int GetOldRadiusDamage(AuthoritySimulation sim, Actor bombSpot, Actor victim, int damage,
        double distance, double fullDamageDistance, double splashFactor)
    {
        var offset = bombSpot.Vec2To(victim);
        var dist = Math.Max(Math.Abs(offset.X), Math.Abs(offset.Y)) - victim.Radius.ToDouble();
        if (dist < 0) dist = 0;
        if (dist >= distance) return 0;
        if (!CombatTrace.CheckSight(sim, victim, bombSpot, ActorSightFlags.IgnoreVisibility)) return 0;
        dist = Math.Clamp(dist - fullDamageDistance, 0, dist);
        return (int)(damage * (distance - dist) / distance * splashFactor * victim.RadiusDamageFactor);
    }
}
