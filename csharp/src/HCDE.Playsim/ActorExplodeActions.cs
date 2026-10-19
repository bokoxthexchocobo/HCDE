namespace HCDE.Playsim;

/// <summary>Native radius explosion and thrust actions (ZScript <c>attacks.zs</c> subset).</summary>
public static class ActorExplodeActions
{
    public static void Explode(Actor actor, int damage, double distance, double fullDamageDistance = 0,
        string? damageType = null, bool hurtSource = false)
    {
        if (distance <= 0) distance = damage;
        if (fullDamageDistance < 0) fullDamageDistance = 0;
        fullDamageDistance = Math.Min(fullDamageDistance, distance - 1);
        var type = string.IsNullOrEmpty(damageType) || damageType.Equals("None", StringComparison.OrdinalIgnoreCase)
            ? "Unknown" : damageType;
        ApplyRadius(actor, damage, distance, fullDamageDistance, type, damageActors: true, thrustForce: 0, hurtSource);
    }

    public static void Detonate(Actor actor)
    {
        var damage = actor.GetMissileDamage(0, 1);
        Explode(actor, damage, damage, hurtSource: true);
    }

    public static void RadiusThrust(Actor actor, int force = 128, double distance = -1, bool affectSource = true,
        double fullThrustDistance = 0)
    {
        if (force == 0) force = 128;
        if (distance <= 0) distance = Math.Abs(force);
        ApplyRadius(actor, 0, distance, fullThrustDistance, actor.DamageType ?? "Unknown", damageActors: false,
            thrustForce: force, hurtSource: affectSource);
    }

    private static void ApplyRadius(Actor actor, int damage, double distance, double fullDamageDistance,
        string damageType, bool damageActors, int thrustForce, bool hurtSource)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Radius effects require a simulation.");
        foreach (var victim in sim.Actors.ToArray())
        {
            if (victim.Destroyed || !victim.IsBlockmapActor) continue;
            if (!hurtSource && ReferenceEquals(victim, actor)) continue;
            if (victim.NoRadiusDamage && damageActors) continue;
            if (victim.SplashImmune(actor)) continue;
            var dist = RadiusDamageGeometry.Distance(actor, victim);
            if (dist >= distance || !CombatTrace.CheckSight(sim, victim, actor, ActorSightFlags.IgnoreVisibility)) continue;
            var falloff = distance <= 0 ? 1 : (dist <= fullDamageDistance ? 1 : 1 - (dist - fullDamageDistance) / (distance - fullDamageDistance));
            if (damageActors && victim.CanTakeDamage)
            {
                var applied = (int)(damage * falloff);
                if (applied > 0)
                    ActorDamage.Apply(victim, applied, actor, damageType: damageType, inflictor: actor);
            }
            if (thrustForce != 0 && victim.Mass > 0)
            {
                var dx = victim.X.ToDouble() - actor.X.ToDouble();
                var dy = victim.Y.ToDouble() - actor.Y.ToDouble();
                var len = Math.Max(1e-6, Math.Sqrt(dx * dx + dy * dy));
                var push = thrustForce * falloff / victim.Mass;
                victim.VelocityX = Fixed.FromDouble(victim.VelocityX.ToDouble() + dx / len * push);
                victim.VelocityY = Fixed.FromDouble(victim.VelocityY.ToDouble() + dy / len * push);
            }
        }
    }
}
