namespace HCDE.Playsim;

internal static class ThingDamage
{
    internal const int Special = 119;

    internal static bool? Execute(AuthoritySimulation sim, int special, Actor? activator,
        int tid, int amount, int mod)
    {
        if (special == 133)
        {
            if (tid == 0 && mod == 0)
            {
                sim.Massacre();
                return true;
            }
            var targets = sim.Actors.Where(actor => !actor.Destroyed && actor.Shootable
                && (tid == 0 || actor.ThingId == tid)
                && (mod == 0 || actor.SectorIndex >= 0 && actor.SectorIndex < sim.Level.Sectors.Count
                    && sim.Level.Sectors[actor.SectorIndex].MatchesTag(mod))).ToArray();
            foreach (var actor in targets)
                ActorDamage.Apply(actor, amount != 0 ? ActorDamage.TelefragDamage : actor.Health, activator);
            return true;
        }
        if (special != Special) return null;
        Apply(sim, activator, tid, amount, HealthActions.DamageType(mod));
        return true;
    }

    internal static bool Massacre(Actor actor)
    {
        if (actor.Health <= 0) return false;
        var shootable = actor.Shootable;
        var dormant = actor.Dormant;
        var invulnerable = actor.Invulnerable;
        actor.Shootable = true;
        actor.Dormant = actor.Invulnerable = false;
        int previous;
        do
        {
            previous = actor.Health;
            ActorDamage.Apply(actor, ActorDamage.TelefragDamage, damageType: "Massacre");
        } while (actor.Health > 0 && actor.Health < previous);
        if (actor.Health > 0)
        {
            actor.Shootable = shootable;
            actor.Dormant = dormant;
            actor.Invulnerable = invulnerable;
        }
        return actor.Health <= 0;
    }

    internal static int Apply(AuthoritySimulation sim, Actor? activator, int tid, int amount, string? damageType)
    {
        var count = 0;
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, tid).ToArray();
        foreach (var actor in targets)
        {
            if (!actor.Shootable) continue;
            if (amount > 0)
                ActorDamage.Apply(actor, amount, activator, damageType: damageType);
            else if (actor.Health < actor.ResurrectionHealth)
                actor.Health = (int)Math.Min((long)actor.Health - amount, actor.ResurrectionHealth);
            count++;
        }
        return count;
    }
}
