namespace HCDE.Playsim;

/// <summary>Managed damage subset of native A_RadiusDamageSelf.</summary>
public static class ActorRadiusSelfActions
{
    public static void Apply(Actor actor, int damage = 128, double distance = 128, int flags = 0,
        Func<Actor, Actor?>? spawnFlash = null)
    {
        if (!double.IsFinite(distance)) throw new ArgumentOutOfRangeException(nameof(distance));
        if ((flags & ~1) != 0) throw new NotSupportedException("Unknown radius self-damage flags.");
        var sim = actor.Simulation ?? throw new InvalidOperationException("Radius self-damage requires a simulation.");
        var target = actor is ProjectileActor projectile ? projectile.Owner : AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        if (target is null || target.Destroyed) return;
        var x = actor.X.ToDouble() - target.X.ToDouble();
        var y = actor.Y.ToDouble() - target.Y.ToDouble();
        var z = actor.Z.ToDouble() - target.Z.ToDouble();
        var actualDistance = Math.Sqrt(x * x + y * y + z * z);
        if (actualDistance >= distance || distance <= 0) return;
        var steps = checked(damage - (int)(damage * actualDistance / distance));
        var actualDamage = steps;
        if ((flags & 1) != 0)
        {
            actualDamage = 0;
            for (var i = 0; i < steps; i++) actualDamage = checked(actualDamage + 1 + (int)(sim.NextCombatRandom() & 7));
        }
        var flash = spawnFlash?.Invoke(target);
        var damageFlags = DamageFlags.None;
        var damageType = "BFGSplash";
        if (flash is not null)
        {
            flash.X = target.X; flash.Y = target.Y;
            flash.Z = Fixed.FromDouble(target.Z.ToDouble() + target.Height.ToDouble() / 4);
            if (flash.FoilInvul) damageFlags |= DamageFlags.FoilInvulnerability;
            if (flash.FoilBuddha) damageFlags |= DamageFlags.FoilBuddha;
            damageType = flash.DamageType;
        }
        ActorDamage.Apply(target, actualDamage, target, damageFlags, damageType, actor);
    }
}
