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
        var x = archvile.X.ToDouble() + Math.Cos(direction) * 15;
        var y = archvile.Y.ToDouble() + Math.Sin(direction) * 15;
        foreach (var corpse in sim.Actors.OrderBy(actor => actor.Id))
        {
            if (!corpse.IsDead || corpse.Destroyed || corpse.RaiseDuration <= 0 || corpse.ResurrectionHealth <= 0
                || corpse.States.Current != ActorStateMachine.Corpse || corpse.Brain == null) continue;
            var reach = corpse.Radius.ToDouble() + archvile.Radius.ToDouble();
            if (Math.Abs(corpse.X.ToDouble() - x) > reach || Math.Abs(corpse.Y.ToDouble() - y) > reach
                || Math.Abs(corpse.Z.ToDouble() - archvile.Z.ToDouble()) > 64
                || !CombatTrace.HasLineOfSight(sim, archvile, corpse) || !ActorPhysics.CanOccupy(sim, corpse)) continue;
            archvile.Angle = BamAngle.FromDegrees(Math.Atan2(corpse.Y.ToDouble() - archvile.Y.ToDouble(),
                corpse.X.ToDouble() - archvile.X.ToDouble()) * 180 / Math.PI);
            corpse.Health = corpse.ResurrectionHealth;
            corpse.Solid = corpse.Shootable = true;
            corpse.Brain.Revive(corpse, target);
            return true;
        }
        return false;
    }

    internal static void Attack(AuthoritySimulation sim, Actor archvile, Actor target, bool fireExists)
    {
        ActorDamage.Apply(target, 20, archvile, inflictor: archvile);
        if (fireExists)
        {
            var radians = archvile.Angle.ToDegrees() * Math.PI / 180;
            var fire = new Actor
            {
                X = Fixed.FromDouble(target.X.ToDouble() - 24 * Math.Cos(radians)),
                Y = Fixed.FromDouble(target.Y.ToDouble() - 24 * Math.Sin(radians)),
                Z = target.Z, Height = default, Solid = false, Shootable = false,
            };
            foreach (var victim in sim.Actors.ToArray())
            {
                if (ReferenceEquals(victim, archvile) || !victim.CanTakeDamage || victim.NoRadiusDamage) continue;
                var horizontal = Math.Max(0, Math.Sqrt(Math.Pow(victim.X.ToDouble() - fire.X.ToDouble(), 2)
                    + Math.Pow(victim.Y.ToDouble() - fire.Y.ToDouble(), 2)) - victim.Radius.ToDouble());
                var vertical = Math.Max(0, Math.Max(victim.Z.ToDouble() - fire.Z.ToDouble(),
                    fire.Z.ToDouble() - victim.Z.ToDouble() - victim.Height.ToDouble()));
                var distance = Math.Sqrt(horizontal * horizontal + vertical * vertical);
                if (distance < 70 && CombatTrace.HasLineOfSight(sim, fire, victim))
                    ActorDamage.Apply(victim, 70 - (int)distance, archvile);
            }
        }
        target.VelocityZ = Fixed.FromDouble(1000.0 / Math.Max(1, target.Mass));
    }
}
