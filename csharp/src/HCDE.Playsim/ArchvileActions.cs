namespace HCDE.Playsim;

internal static class ArchvileActions
{
    internal static int RaiseDuration(int type) => type switch
    {
        3004 => 20, 9 or 84 => 25, 65 or 68 => 35, 3001 => 34,
        3002 or 58 or 66 => 30, 3003 or 69 => 56, 3005 or 71 => 48,
        67 => 40, _ => 0
    };

    internal static bool TryRaise(AuthoritySimulation sim, Actor archvile, Actor target)
    {
        var direction = Math.Atan2(target.Y.ToDouble() - archvile.Y.ToDouble(), target.X.ToDouble() - archvile.X.ToDouble());
        var speed = Math.Abs(archvile.MovementSpeed.ToDouble());
        var x = archvile.X.ToDouble() + Math.Cos(direction) * speed;
        var y = archvile.Y.ToDouble() + Math.Sin(direction) * speed;
        foreach (var corpse in sim.Actors.OrderBy(actor => actor.Id))
        {
            if (!corpse.IsBlockmapActor || !ActorRaise.HasRaiseState(corpse)) continue;
            var raiseState = corpse.GetRaiseState();
            var reach = (corpse.ResurrectionRadius ?? corpse.Radius).ToDouble() + archvile.Radius.ToDouble();
            if (Math.Abs(corpse.X.ToDouble() - x) > reach || Math.Abs(corpse.Y.ToDouble() - y) > reach) continue;
            corpse.VelocityX = corpse.VelocityY = default;
            if (!ActorRaise.CheckPosition(sim, corpse, heightOnly: true)) continue;
            if (!ActorRaise.CanResurrect(archvile, corpse)) continue;
            archvile.Angle = BamAngle.FromDegrees(Math.Atan2(corpse.Y.ToDouble() - archvile.Y.ToDouble(),
                corpse.X.ToDouble() - archvile.X.ToDouble()) * 180 / Math.PI);
            if (archvile.Friendly) archvile.Brain?.ClearResurrectionEnemy(corpse.Id);
            var ghostCompatibility = sim.Compat.HasFlag(CompatSurface.VileGhosts);
            if (ghostCompatibility)
                corpse.Height = new Fixed((int)Math.Clamp((long)corpse.Height.Raw * 4, int.MinValue, int.MaxValue));
            ActorRaise.ReviveSupported(corpse, restoreDimensions: !ghostCompatibility);
            ActorPropertyActions.CopySupportedFriendship(corpse, archvile);
            ActorRaise.EnterRaiseState(corpse, raiseState);
            return true;
        }
        return false;
    }

    internal static void Attack(AuthoritySimulation sim, Actor archvile, Actor target, bool fireExists,
        int initialDamage = 20, double thrust = 1, string? damageType = "Fire", int flags = 0,
        int blastDamage = 70, int blastRadius = 70)
    {
        if (!double.IsFinite(thrust)) throw new ArgumentOutOfRangeException(nameof(thrust));
        if (blastRadius <= 0) blastRadius = blastDamage;
        ActorOrientationActions.Face(archvile, target);
        if (!CombatTrace.HasLineOfSight(sim, archvile, target)) return;
        // VAF_DMGTYPEAPPLYTODIRECT applies the supplied type to the initial hit too.
        ActorDamage.Apply(target, initialDamage, archvile, damageType: (flags & 1) != 0 ? damageType : null, inflictor: archvile);
        if (fireExists && blastRadius > 0)
        {
            var position = target.Vec3Angle(-24, archvile.Angle.ToDegrees(), 0);
            var fire = new Actor
            {
                X = Fixed.FromDouble(position.X), Y = Fixed.FromDouble(position.Y),
                Z = Fixed.FromDouble(position.Z), Height = default, Solid = false, Shootable = false,
            };
            foreach (var victim in sim.Actors.ToArray())
            {
                if (ReferenceEquals(victim, archvile) || !victim.IsBlockmapActor || !victim.CanTakeDamage || victim.NoRadiusDamage || victim.SplashImmune(fire)) continue;
                var distance = RadiusDamageGeometry.Distance(fire, victim);
                if (distance < blastRadius && CombatTrace.HasLineOfSight(sim, fire, victim))
                    ActorDamage.Apply(victim, (int)(blastDamage * (1 - distance / blastRadius)), archvile, damageType: damageType, inflictor: fire);
            }
        }
        target.VelocityZ = Fixed.FromDouble(thrust * 1000.0 / Math.Max(1, target.Mass));
    }
}
