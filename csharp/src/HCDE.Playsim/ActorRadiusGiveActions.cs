namespace HCDE.Playsim;

/// <summary>Native <c>A_RadiusGive</c> subset for represented player inventory.</summary>
public static class ActorRadiusGiveActions
{
    [Flags]
    public enum RadiusGiveFlags
    {
        GiveSelf = 1,
        Players = 2,
        Monsters = 4,
    }

    public static int RadiusGive(Actor actor, string itemClass, double distance, RadiusGiveFlags flags, int amount = 1,
        int limit = 0)
    {
        if (distance <= 0 || (flags & (RadiusGiveFlags.Players | RadiusGiveFlags.Monsters | RadiusGiveFlags.GiveSelf)) == 0)
            return 0;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Radius give requires a simulation.");
        var given = 0;
        var unlimited = limit <= 0;
        foreach (var thing in sim.Actors.ToArray())
        {
            if (!unlimited && given >= limit) break;
            if (thing.Destroyed) continue;
            if (ReferenceEquals(thing, actor))
            {
                if ((flags & RadiusGiveFlags.GiveSelf) == 0) continue;
            }
            else if (thing is PlayerPawn)
            {
                if ((flags & RadiusGiveFlags.Players) == 0) continue;
            }
            else if (thing.IsMonster)
            {
                if ((flags & RadiusGiveFlags.Monsters) == 0) continue;
            }
            else continue;
            var dist = RadiusDamageGeometry.Distance(actor, thing);
            if (dist >= distance || !CombatTrace.CheckSight(sim, thing, actor, ActorSightFlags.IgnoreVisibility)) continue;
            if (ActorInventoryActions.GiveInventory(thing, itemClass, amount)) given++;
        }
        return given;
    }
}
